using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultMapExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "Map";
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
        [GeneratedCodeAttribute("{{nameof(ResultMapExecutor)}}", "1.0.0.0")]
        public static Result<R> Map<{{separatedTemplateArgs}}, R>(this in Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, R> mapping)
        {
            return result.State switch
            {
                ResultState.Success => mapping({{resultValueExpansion}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMapExecutor)}}", "1.0.0.0")]
        public static async Task<Result<R>> Map<{{separatedTemplateArgs}}, R>(this Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, Task<R>> mapping)
        {
            return result.State switch
            {
                ResultState.Success => await mapping({{resultValueExpansion}}).ConfigureAwait(false),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMapExecutor)}}", "1.0.0.0")]
        public static async Task<Result<R>> Map<{{separatedTemplateArgs}}, R>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, R> mapping)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => mapping({{resultValueExpansion}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMapExecutor)}}", "1.0.0.0")]
        public static async Task<Result<R>> Map<{{separatedTemplateArgs}}, R>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, Task<R>> mapping)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => await mapping({{resultValueExpansion}}).ConfigureAwait(false),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);

        sb.AppendLine("#endregion");
    }
}
