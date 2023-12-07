using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultCombineExecutor : IGeneratorExecutor
{
    public void Execute(SourceProductionContext context, Compilation source)
    {
        var methods = GenerateCombineMethods();
        var code = $$"""
        #nullable enable
        using System;
        using System.Collections.Generic;
        using System.Diagnostics.Contracts;
        using System.CodeDom.Compiler;

        namespace Just.Railway;

        public readonly partial struct Result
        {
        {{methods}}
        }
        """;

        context.AddSource("Result.Combine.g.cs", code);
    }


    private string GenerateCombineMethods()
    {
        var sb = new StringBuilder();

        for (int i = 2; i <= Constants.MaxResultTupleSize; i++)
        {
            GenerateCombineMethodsForArgCount(sb, argCount: i);
        }

        return sb.ToString();
    }

    private void GenerateCombineMethodsForArgCount(StringBuilder sb, int argCount)
    {
        sb.AppendLine($"#region Combine {argCount} Results");

        GenerateGetBottomMethod(sb, argCount);

        var permutations = 1 << argCount;
        var argsResultTupleSizes = new ImmutableArray<int>[permutations];

        Span<int> templateCounts = stackalloc int[argCount];
        for (int i = 0; i < permutations; i++)
        {
            templateCounts.Fill(0);
            for (int j = 0; j < argCount; j++)
            {
                templateCounts[j] = (i & (1 << j)) > 0 ? 1 : 0;
            }
            argsResultTupleSizes[i] = templateCounts.ToImmutableArray();
        }

        foreach (var argResultTupleSizes in argsResultTupleSizes)
        {
            sb.AppendLine(GenerateCombineMethodBody(argResultTupleSizes));
        }

        sb.AppendLine("#endregion");

    }

    private static void GenerateGetBottomMethod(StringBuilder sb, int argCount)
    {
        var args = Enumerable.Range(1, argCount)
            .Select(i => $"result{i}")
            .ToImmutableArray();
        var argsDecl = string.Join(", ", args.Select(x => $"ResultState {x}"));
        sb.AppendLine($"[GeneratedCodeAttribute(\"{nameof(ResultCombineExecutor)}\", \"1.0.0.0\")]");
        sb.AppendLine($"private static IEnumerable<string> GetBottom({argsDecl})");
        sb.AppendLine("{");
        foreach (var arg in args)
        {
            sb.AppendLine($"    if ({arg} == ResultState.Bottom) yield return \"{arg}\";");
        }
        sb.AppendLine("}");
    }

    private string GenerateCombineMethodBody(ImmutableArray<int> argResultTupleSizes)
    {
        var resultTupleSize = argResultTupleSizes.Sum();

        var paramNames = Enumerable.Range(1, argResultTupleSizes.Length)
            .Select(i => $"result{i}")
            .ToImmutableArray();
        var templateArgNames = Enumerable.Range(1, resultTupleSize)
            .Select(i => $"T{i}")
            .ToImmutableArray();

        string templateDecl = templateArgNames.IsEmpty 
            ? string.Empty
            : $"<{string.Join(", ", templateArgNames)}>";
        string resultTypeDecl = GetResultTypeDecl(templateArgNames);
        string paramDecl;
        {
            var paramDeclBuilder = new StringBuilder();
            int currentTemplateArg = 0;
            for (int i = 0; i < argResultTupleSizes.Length; i++)
            {
                var argResultTupleSize = argResultTupleSizes[i];
                string currentParamType = GetResultTypeDecl(templateArgNames.Slice(currentTemplateArg, argResultTupleSize));
                currentTemplateArg += argResultTupleSize;
                paramDeclBuilder.Append($"in {currentParamType} {paramNames[i]}, ");
            }
            paramDeclBuilder.Remove(paramDeclBuilder.Length-2, 2);
            paramDecl = paramDeclBuilder.ToString();
        }

        var paramNameStates = paramNames.Select(x => $"{x}.State")
            .ToImmutableArray();
        string bottomStateCheck = string.Join(" & ", paramNameStates);
        string statesSeparatedList = string.Join(", ", paramNameStates);

        string failureChecks;
        {
            var failureChecksBuilder = new StringBuilder();
            foreach (var paramName in paramNames)
            {
                failureChecksBuilder.AppendLine($"    if ({paramName}.IsFailure) error += {paramName}.Error;");
            }
            failureChecks = failureChecksBuilder.ToString();
        }
        string resultExpansion;
        switch (resultTupleSize)
        {
            case 0:
            resultExpansion = "null";
            break;

            case 1:
            resultExpansion = $"{paramNames[argResultTupleSizes.IndexOf(1)]}.Value";
            break;
            
            default:
            var resultExpansionBuilder = new StringBuilder();
            resultExpansionBuilder.Append("(");
            for (int i = 0; i < argResultTupleSizes.Length; i++)
            {
                if (argResultTupleSizes[i] == 0) continue;
                if (argResultTupleSizes[i] == 1)
                {
                    resultExpansionBuilder.Append($"{paramNames[i]}.Value, ");
                    continue;
                }
                
                for (int valueIndex = 1; valueIndex <= argResultTupleSizes[i]; valueIndex++)
                {
                    resultExpansionBuilder.Append($"{paramNames[i]}.Value.Item{valueIndex}, ");
                }
            }
            resultExpansionBuilder.Remove(resultExpansionBuilder.Length - 2, 2);
            resultExpansionBuilder.Append(")");
            resultExpansion = resultExpansionBuilder.ToString();
            break;
        }
        
        string returnExpr = $"return error is null ? new({resultExpansion}) : new(error);";
        var method = $$"""
        [GeneratedCodeAttribute("{{nameof(ResultCombineExecutor)}}", "1.0.0.0")]
        [PureAttribute]
        public static {{resultTypeDecl}} Combine{{templateDecl}}({{paramDecl}})
        {
            if (({{bottomStateCheck}}) == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(string.Join(';', GetBottom({{statesSeparatedList}})));
            }
            Error? error = null;
        {{failureChecks}}
            {{returnExpr}}
        }
        """;
        return method;

        static string GetResultTypeDecl(IReadOnlyList<string> templateArgNames)
        {
            return templateArgNames.Count switch
            {
                0 => "Result",
                1 => $"Result<{templateArgNames[0]}>",
                _ => $"Result<({string.Join(", ", templateArgNames)})>"
            };
        }
    }
}
