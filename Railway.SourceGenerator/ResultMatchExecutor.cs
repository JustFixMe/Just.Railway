using System;
using System.Collections.Generic;
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
        string separatedTemplateArgs = string.Join(", ", templateArgNames);

        sb.AppendLine($"#region <{separatedTemplateArgs}>");

        string resultValueType = templateArgNames.Length == 1 ? separatedTemplateArgs : $"({separatedTemplateArgs})";
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMatchExecutor)}}", "1.0.0.0")]
        public static R Match<{{separatedTemplateArgs}}, R>(this in Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, R> onSuccess, Func<Error, R> onFailure)
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
        public static Task<R> Match<{{separatedTemplateArgs}}, R>(this in Result<{{resultValueType}}> result, Func<{{separatedTemplateArgs}}, Task<R>> onSuccess, Func<Error, Task<R>> onFailure)
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
        public static async Task<R> Match<{{separatedTemplateArgs}}, R>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, R> onSuccess, Func<Error, R> onFailure)
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
        public static async Task<R> Match<{{separatedTemplateArgs}}, R>(this Task<Result<{{resultValueType}}>> resultTask, Func<{{separatedTemplateArgs}}, Task<R>> onSuccess, Func<Error, Task<R>> onFailure)
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

        sb.AppendLine("#endregion");
    }
}
