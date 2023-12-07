using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultTapExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "Tap";

    protected override void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        var templateArgNames = Enumerable.Range(1, argCount)
            .Select(i => $"T{i}")
            .ToImmutableArray();

        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames);
        string resultTypeDef = GenerateResultTypeDef(templateArgNames);
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTapExecutor)}}", "1.0.0.0")]
        public static ref readonly {{resultTypeDef}} Tap{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Action{{methodTemplateDecl}}? onSuccess = null, Action<Error>? onFailure = null)
        {
            switch (result.State)
            {
                case ResultState.Success:
                    onSuccess?.Invoke({{resultValueExpansion}});
                    break;
                case ResultState.Error:
                    onFailure?.Invoke(result.Error!);
                    break;

                default: throw new ResultNotInitializedException(nameof(result));
            }
            return ref result;
        }
        """);

        GenerateAsyncMethods("Task", sb, templateArgNames, resultTypeDef, resultValueExpansion);
        GenerateAsyncMethods("ValueTask", sb, templateArgNames, resultTypeDef, resultValueExpansion);

        sb.AppendLine("#endregion");
    }

    private static void GenerateAsyncMethods(string taskType, StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string resultValueExpansion)
    {
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames);
        string asyncActionTemplateDecl = GenerateTemplateDecl(templateArgNames.Add(taskType));

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTapExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> Tap{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Action{{methodTemplateDecl}}? onSuccess = null, Action<Error>? onFailure = null)
        {
            var result = await resultTask.ConfigureAwait(false);
            switch (result.State)
            {
                case ResultState.Success:
                    onSuccess?.Invoke({{resultValueExpansion}});
                    break;
                case ResultState.Error:
                    onFailure?.Invoke(result.Error!);
                    break;

                default: throw new ResultNotInitializedException(nameof(resultTask));
            }
            return result;
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTapExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> Tap{{methodTemplateDecl}}(this {{resultTypeDef}} result, Func{{asyncActionTemplateDecl}}? onSuccess = null, Func<Error, {{taskType}}>? onFailure = null)
        {
            switch (result.State)
            {
                case ResultState.Success:
                    if (onSuccess is not null)
                        await onSuccess.Invoke({{resultValueExpansion}}).ConfigureAwait(false);
                    break;
                case ResultState.Error:
                    if (onFailure is not null)
                        await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                    break;

                default: throw new ResultNotInitializedException(nameof(result));
            }
            return result;
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTapExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> Tap{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{asyncActionTemplateDecl}}? onSuccess = null, Func<Error, {{taskType}}>? onFailure = null)
        {
            var result = await resultTask.ConfigureAwait(false);
            switch (result.State)
            {
                case ResultState.Success:
                    if (onSuccess is not null)
                        await onSuccess.Invoke({{resultValueExpansion}}).ConfigureAwait(false);
                    break;
                case ResultState.Error:
                    if (onFailure is not null)
                        await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                    break;

                default: throw new ResultNotInitializedException(nameof(resultTask));
            }
            return result;
        }
        """);
    }
}
