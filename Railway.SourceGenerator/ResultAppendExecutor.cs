using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultAppendExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "Append";

    protected override void GenerateHelperMethods(StringBuilder sb)
    {
        sb.AppendLine("""
        private static IEnumerable<string> GetBottom(ResultState r1, ResultState r2, string firstArg = "result", string secondArg = "next")
        {
            if (r1 == ResultState.Bottom)
                yield return firstArg;
            if (r2 == ResultState.Bottom)
                yield return secondArg;
        }
        """);
    }

    protected override void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        var templateArgNames = Enumerable.Range(1, argCount)
            .Select(i => $"T{i}")
            .ToImmutableArray();

        string resultTypeDef = GenerateResultTypeDef(templateArgNames);
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames);

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static {{resultTypeDef}} Append{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Result next)
        {
            Error? error = null;
            if ((result.State & next.State) == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));
            }

            if (result.IsFailure)
            {
                error += result.Error;
            }
            if (next.IsFailure)
            {
                error += next.Error;
            }

            return error is null
                ? Result.Success({{resultValueExpansion}})
                : error;
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static {{resultTypeDef}} Append{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func<Result> nextFunc)
        {
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = nextFunc();
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{resultValueExpansion}});
        }
        """);

        GenerateAsyncMethods("Task", sb, templateArgNames, resultTypeDef, resultValueExpansion);
        GenerateAsyncMethods("ValueTask", sb, templateArgNames, resultTypeDef, resultValueExpansion);

        if (argCount < Constants.MaxResultTupleSize)
        {
            GenerateExpandedMethods(sb, templateArgNames, resultTypeDef, resultValueExpansion);
            GenerateExpandedAsyncMethods("Task", sb, templateArgNames, resultTypeDef, resultValueExpansion);
            GenerateExpandedAsyncMethods("ValueTask", sb, templateArgNames, resultTypeDef, resultValueExpansion);
        }

        sb.AppendLine("#endregion");
    }

    private void GenerateAsyncMethods(string taskType, StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string resultValueExpansion)
    {
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> Append{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<Result> nextFunc)
        {
            var result = await resultTask.ConfigureAwait(false);
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = nextFunc();
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{resultValueExpansion}});
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> Append{{methodTemplateDecl}}(this {{resultTypeDef}} result, Func<{{taskType}}<Result>> nextFunc)
        {
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = await nextFunc().ConfigureAwait(false);
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{resultValueExpansion}});
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> Append{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<{{taskType}}<Result>> nextFunc)
        {
            var result = await resultTask.ConfigureAwait(false);
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(resultTask));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = await nextFunc().ConfigureAwait(false);
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{resultValueExpansion}});
        }
        """);
    }

    private void GenerateExpandedAsyncMethods(string taskType, StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string resultValueExpansion)
    {
        var expandedTemplateArgNames = templateArgNames.Add("TNext");
        string resultExpandedTypeDef = GenerateResultTypeDef(expandedTemplateArgNames);
        string methodExpandedTemplateDecl = GenerateTemplateDecl(expandedTemplateArgNames);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, TNext next)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => Result.Success({{JoinArguments(resultValueExpansion, "next")}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Result<TNext> next)
        {
            var result = await resultTask.ConfigureAwait(false);
            if ((result.State & next.State) == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));
            }

            Error? error = null;
            if (result.IsFailure)
            {
                error += result.Error;
            }
            if (next.IsFailure)
            {
                error += next.Error;
            }

            return error is null
                ? Result.Success({{JoinArguments(resultValueExpansion, "next.Value")}})
                : error;
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<TNext> nextFunc)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => Result.Success({{JoinArguments(resultValueExpansion, "nextFunc()")}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{resultTypeDef}} result, Func<{{taskType}}<TNext>> nextFunc)
        {
            return result.State switch
            {
                ResultState.Success => Result.Success({{JoinArguments(resultValueExpansion, "await nextFunc().ConfigureAwait(false)")}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<{{taskType}}<TNext>> nextFunc)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => Result.Success({{JoinArguments(resultValueExpansion, "await nextFunc().ConfigureAwait(false)")}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);


        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<Result<TNext>> nextFunc)
        {
            var result = await resultTask.ConfigureAwait(false);
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(resultTask));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = nextFunc();
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "next.Value")}});
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{resultTypeDef}} result, Func<{{taskType}}<Result<TNext>>> nextFunc)
        {
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = await nextFunc().ConfigureAwait(false);
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "next.Value")}});
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Append{{methodExpandedTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<{{taskType}}<Result<TNext>>> nextFunc)
        {
            var result = await resultTask.ConfigureAwait(false);
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(resultTask));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = await nextFunc().ConfigureAwait(false);
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "next.Value")}});
        }
        """);
    }

    private static void GenerateExpandedMethods(StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string resultValueExpansion)
    {
        var expandedTemplateArgNames = templateArgNames.Add("TNext");
        string resultExpandedTypeDef = GenerateResultTypeDef(expandedTemplateArgNames);
        string methodExpandedTemplateDecl = GenerateTemplateDecl(expandedTemplateArgNames);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static {{resultExpandedTypeDef}} Append{{methodExpandedTemplateDecl}}(this in {{resultTypeDef}} result, TNext next)
        {
            return result.State switch
            {
                ResultState.Success => Result.Success({{JoinArguments(resultValueExpansion, "next")}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static {{resultExpandedTypeDef}} Append{{methodExpandedTemplateDecl}}(this in {{resultTypeDef}} result, Func<TNext> nextFunc)
        {
            return result.State switch
            {
                ResultState.Success => Result.Success({{JoinArguments(resultValueExpansion, "nextFunc()")}}),
                ResultState.Error => result.Error!,
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static {{resultExpandedTypeDef}} Append{{methodExpandedTemplateDecl}}(this in {{resultTypeDef}} result, Result<TNext> next)
        {
            Error? error = null;
            if ((result.State & next.State) == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));
            }

            if (result.IsFailure)
            {
                error += result.Error;
            }
            if (next.IsFailure)
            {
                error += next.Error;
            }

            return error is null
                ? Result.Success({{JoinArguments(resultValueExpansion, "next.Value")}})
                : error;
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultAppendExecutor)}}", "1.0.0.0")]
        public static {{resultExpandedTypeDef}} Append{{methodExpandedTemplateDecl}}(this in {{resultTypeDef}} result, Func<Result<TNext>> nextFunc)
        {
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var next = nextFunc();
            if (next.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(nextFunc));
            }
            else if (next.IsFailure)
            {
                return next.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "next.Value")}});
        }
        """);
    }

    internal static string JoinArguments(string arg1, string arg2) => (arg1, arg2) switch
    {
        ("", "") => "",
        (string arg, "") => arg,
        ("", string arg) => arg,
        _ => $"{arg1}, {arg2}"
    };
}
