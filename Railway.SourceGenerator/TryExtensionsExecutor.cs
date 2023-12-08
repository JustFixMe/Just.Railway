using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

public sealed class TryExtensionsExecutor : IGeneratorExecutor
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

        public static partial class Try
        {
        {{methods}}
        }
        """;

        context.AddSource("Try.Run.g.cs", code);
    }

    private string GenerateMethods()
    {
        var sb = new StringBuilder();

        for (int i = 0; i <= Constants.MaxResultTupleSize; i++)
        {
            GenerateMethodsForArgCount(sb, argCount: i);
        }

        return sb.ToString();
    }

    private void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        var templateArgNames = Enumerable.Range(1, argCount)
            .Select(i => $"T{i}")
            .ToImmutableArray();
        var argNames = Enumerable.Range(1, argCount)
            .Select(i => $"arg{i}")
            .ToImmutableArray();

        string actionTemplateDecl = GenerateTemplateDecl(templateArgNames);
        string resultActionTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("Result"));
        string funcTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("TResult"));
        string resultFuncTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("Result<TResult>"));
        string argumentsDeclExpansion = string.Join(", ", templateArgNames.Zip(argNames, (t, n) => $"{t} {n}"));
        string argumentsExpansion = string.Join(", ", argNames);

        sb.AppendLine($"#region <{string.Join(", ", templateArgNames)}>");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static Result Run{{actionTemplateDecl}}(Action{{actionTemplateDecl}} action{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                action({{argumentsExpansion}});
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static Result Run{{actionTemplateDecl}}(Func{{resultActionTemplateDecl}} action{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                return action({{argumentsExpansion}});
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static Result<TResult> Run{{funcTemplateDecl}}(Func{{funcTemplateDecl}} func{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                return Result.Success(func({{argumentsExpansion}}));
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static Result<TResult> Run{{funcTemplateDecl}}(Func{{resultFuncTemplateDecl}} func{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                return func({{argumentsExpansion}});
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        GenerateAsyncMethods(sb, templateArgNames, actionTemplateDecl, funcTemplateDecl, argumentsDeclExpansion, argumentsExpansion, "Task");
        GenerateAsyncMethods(sb, templateArgNames, actionTemplateDecl, funcTemplateDecl, argumentsDeclExpansion, argumentsExpansion, "ValueTask");

        sb.AppendLine("#endregion");
    }

    private static void GenerateAsyncMethods(StringBuilder sb, ImmutableArray<string> templateArgNames, string actionTemplateDecl, string funcTemplateDecl, string argumentsDeclExpansion, string argumentsExpansion, string taskType)
    {
        string actionTaskTemplateDecl = GenerateTemplateDecl(templateArgNames.Add(taskType));
        string resultActionTaskTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<Result>"));
        string funcTaskTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<TResult>"));
        string resultFuncTaskTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<Result<TResult>>"));
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result> Run{{actionTemplateDecl}}(Func{{actionTaskTemplateDecl}} action{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                await action({{argumentsExpansion}}).ConfigureAwait(false);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result> Run{{actionTemplateDecl}}(Func{{resultActionTaskTemplateDecl}} action{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                return await action({{argumentsExpansion}}).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result<TResult>> Run{{funcTemplateDecl}}(Func{{funcTaskTemplateDecl}} func{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                return Result.Success(await func({{argumentsExpansion}}).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(TryExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result<TResult>> Run{{funcTemplateDecl}}(Func{{resultFuncTaskTemplateDecl}} func{{TrailingArguments(argumentsDeclExpansion)}})
        {
            try
            {
                return await func({{argumentsExpansion}}).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Error.New(ex);
            }
        }
        """);
    }

    private static string TrailingArguments(string argumentsExpansion) => string.IsNullOrEmpty(argumentsExpansion)
        ? string.Empty
        : $", {argumentsExpansion}";
    private static string GenerateTemplateDecl(ImmutableArray<string> templateArgNames) => templateArgNames.Length > 0
        ? $"<{string.Join(", ", templateArgNames)}>"
        : string.Empty;
}
