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
        string separatedTemplateArgs = string.Join(", ", templateArgNames);

        sb.AppendLine($"#region <{separatedTemplateArgs}>");

        string resultValueType = templateArgNames.Length == 1 ? separatedTemplateArgs : $"({separatedTemplateArgs})";
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTapExecutor)}}", "1.0.0.0")]
        public static ref readonly Result<{{resultValueType}}> Tap<{{separatedTemplateArgs}}>(this in Result<{{resultValueType}}> result, Action<{{separatedTemplateArgs}}>? onSuccess = null, Action<Error>? onFailure = null)
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

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTapExecutor)}}", "1.0.0.0")]
        public static async Task<Result<{{resultValueType}}>> Tap<{{separatedTemplateArgs}}>(this Task<Result<{{resultValueType}}>> resultTask, Action<{{separatedTemplateArgs}}>? onSuccess = null, Action<Error>? onFailure = null)
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
        public static async Task<Result<{{resultValueType}}>> Tap<{{separatedTemplateArgs}}>(this Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
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
        public static async Task<Result<{{resultValueType}}>> Tap<{{separatedTemplateArgs}}>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
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

        sb.AppendLine("#endregion");
    }
}
