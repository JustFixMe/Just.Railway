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

        for (int i = 1; i <= Constants.MaxResultTupleSize; i++)
        {
            GenerateMethodsForArgCount(sb, argCount: i);
        }

        return sb.ToString();
    }

    protected string GenerateResultValueExpansion(ImmutableArray<string> templateArgNames)
    {
        string resultExpansion;
        if (templateArgNames.Length > 1)
        {
            var resultExpansionBuilder = new StringBuilder();
            for (int i = 1; i <= templateArgNames.Length; i++)
            {
                resultExpansionBuilder.Append($"result.Value.Item{i}, ");
            }
            resultExpansionBuilder.Remove(resultExpansionBuilder.Length - 2, 2);
            resultExpansion = resultExpansionBuilder.ToString();
        }
        else
        {
            resultExpansion = "result.Value";
        }

        return resultExpansion;
    }

    protected abstract string ExtensionType { get; }
    protected abstract void GenerateMethodsForArgCount(StringBuilder sb, int argCount);
}
