using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal abstract class ResultExtensionsExecutor : IGeneratorExecutor
{
    public void Execute(SourceProductionContext context, Compilation source)
    {
        var methods = GenerateMethods();
        var code = $$"""
        #nullable enable
        using System;
        using System.Collections.Generic;
        using System.Diagnostics.Contracts;
        using System.CodeDom.Compiler;

        namespace Just.Railway;

        public static partial class ResultExtensions
        {
        {{methods}}
        }
        """;

        context.AddSource($"ResultExtensions.{ExtensionType}.g.cs", code);
    }

    private string GenerateMethods()
    {
        var sb = new StringBuilder();

        GenerateHelperMethods(sb);
        for (int i = 0; i <= Constants.MaxResultTupleSize; i++)
        {
            GenerateMethodsForArgCount(sb, argCount: i);
        }

        return sb.ToString();
    }

    protected static string GenerateTemplateDecl(ImmutableArray<string> templateArgNames) => templateArgNames.Length > 0
        ? $"<{string.Join(", ", templateArgNames)}>"
        : string.Empty;

    protected static string GenerateResultTypeDef(ImmutableArray<string> templateArgNames) => templateArgNames.Length switch
        {
            0 => "Result",
            1 => $"Result<{string.Join(", ", templateArgNames)}>",
            _ => $"Result<({string.Join(", ", templateArgNames)})>",
        };
    protected static string JoinArguments(string arg1, string arg2) => (arg1, arg2) switch
    {
        ("", "") => "",
        (string arg, "") => arg,
        ("", string arg) => arg,
        _ => $"{arg1}, {arg2}"
    };

    protected static string GenerateResultValueExpansion(ImmutableArray<string> templateArgNames)
    {
        string resultExpansion;

        switch (templateArgNames.Length)
        {
            case 0:
            resultExpansion = string.Empty;
            break;

            case 1:
            resultExpansion = "result.Value";
            break;

            default:
            var resultExpansionBuilder = new StringBuilder();
            for (int i = 1; i <= templateArgNames.Length; i++)
            {
                resultExpansionBuilder.Append($"result.Value.Item{i}, ");
            }
            resultExpansionBuilder.Remove(resultExpansionBuilder.Length - 2, 2);
            resultExpansion = resultExpansionBuilder.ToString();
            break;
        }

        return resultExpansion;
    }

    protected abstract string ExtensionType { get; }
    protected abstract void GenerateMethodsForArgCount(StringBuilder sb, int argCount);
    protected virtual void GenerateHelperMethods(StringBuilder sb) {}
}
