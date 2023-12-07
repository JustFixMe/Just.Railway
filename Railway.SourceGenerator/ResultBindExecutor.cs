using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultBindExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "Bind";

    protected override void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        var templateArgNames = Enumerable.Range(1, argCount)
            .Select(i => $"T{i}")
            .ToImmutableArray();

        string resultTypeDef = GenerateResultTypeDef(templateArgNames);
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("R"));
        string bindTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("Result<R>"));

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static Result<R> Bind{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func{{bindTemplateDecl}} binding)
        {
            return result.State switch
            {
                ResultState.Success => binding({{resultValueExpansion}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        GenerateAsyncMethods("Task", sb, templateArgNames, resultTypeDef, resultValueExpansion);
        GenerateAsyncMethods("ValueTask", sb, templateArgNames, resultTypeDef, resultValueExpansion);

        sb.AppendLine("#endregion");
    }

    private static void GenerateAsyncMethods(string taskType, StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string resultValueExpansion)
    {
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("R"));
        string bindTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("Result<R>"));
        string asyncActionTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<Result<R>>"));

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result<R>> Bind{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{bindTemplateDecl}} binding)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => binding({{resultValueExpansion}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static {{taskType}}<Result<R>> Bind{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func{{asyncActionTemplateDecl}} binding)
        {
            return result.State switch
            {
                ResultState.Success => binding({{resultValueExpansion}}),
                ResultState.Error => {{taskType}}.FromResult<Result<R>>(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result<R>> Bind{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{asyncActionTemplateDecl}} binding)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => await binding({{resultValueExpansion}}).ConfigureAwait(false),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);
    }
}
