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
        string separatedTemplateArgs = string.Join(", ", templateArgNames);

        sb.AppendLine($"#region <{separatedTemplateArgs}>");

        string resultValueType = templateArgNames.Length == 1 ? separatedTemplateArgs : $"({separatedTemplateArgs})";
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static Result<R> Bind<{{separatedTemplateArgs}}, R>(this in Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, Result<R>> binding)
        {
            return result.State switch
            {
                ResultState.Success => binding({{resultValueExpansion}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static Task<Result<R>> Bind<{{separatedTemplateArgs}}, R>(this in Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, Task<Result<R>>> binding)
        {
            return result.State switch
            {
                ResultState.Success => binding({{resultValueExpansion}}),
                ResultState.Error => Task.FromResult<Result<R>>(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultBindExecutor)}}", "1.0.0.0")]
        public static async Task<Result<R>> Bind<{{separatedTemplateArgs}}, R>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, Result<R>> binding)
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
        public static async Task<Result<R>> Bind<{{separatedTemplateArgs}}, R>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, Task<Result<R>>> binding)
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

        sb.AppendLine("#endregion");
    }
}
