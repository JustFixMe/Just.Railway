using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultMatchExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "Match";

    protected override void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        var templateArgNames = Enumerable.Range(1, argCount)
            .Select(i => $"T{i}")
            .ToImmutableArray();

        string resultTypeDef = GenerateResultTypeDef(templateArgNames);
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("R"));

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMatchExecutor)}}", "1.0.0.0")]
        public static R Match{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func{{methodTemplateDecl}} onSuccess, Func<Error, R> onFailure)
        {
            return result.State switch
            {
                ResultState.Success => onSuccess({{resultValueExpansion}}),
                ResultState.Error => onFailure(result.Error!),
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
        var methodTemplateArgNames = templateArgNames.Add("R");
        string methodTemplateDecl = GenerateTemplateDecl(methodTemplateArgNames);
        string asyncActionTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<R>"));

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMatchExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<R> Match{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{methodTemplateDecl}} onSuccess, Func<Error, R> onFailure)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => onSuccess({{resultValueExpansion}}),
                ResultState.Error => onFailure(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMatchExecutor)}}", "1.0.0.0")]
        public static {{taskType}}<R> Match{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func{{asyncActionTemplateDecl}} onSuccess, Func<Error, {{taskType}}<R>> onFailure)
        {
            return result.State switch
            {
                ResultState.Success => onSuccess({{resultValueExpansion}}),
                ResultState.Error => onFailure(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMatchExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<R> Match{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{asyncActionTemplateDecl}} onSuccess, Func<Error, {{taskType}}<R>> onFailure)
        {
            var result = await resultTask.ConfigureAwait(false);
            var matchTask = result.State switch
            {
                ResultState.Success => onSuccess({{resultValueExpansion}}),
                ResultState.Error => onFailure(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
            return await matchTask.ConfigureAwait(false);
        }
        """);
    }
}
