using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

internal sealed class ResultExtendExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "Extend";

    protected override void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        if (argCount == 0 || argCount == Constants.MaxResultTupleSize)
        {
            return;
        }

        var templateArgNames = Enumerable.Range(1, argCount)
            .Select(i => $"T{i}")
            .ToImmutableArray();

        var expandedTemplateArgNames = templateArgNames.Add("R");
        string resultTypeDef = GenerateResultTypeDef(templateArgNames);
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);
        string resultExpandedTypeDef = GenerateResultTypeDef(expandedTemplateArgNames);
        string methodTemplateDecl = GenerateTemplateDecl(expandedTemplateArgNames);
        string bindTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("Result<R>"));

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultExtendExecutor)}}", "1.0.0.0")]
        public static {{resultExpandedTypeDef}} Extend{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func{{bindTemplateDecl}} extensionFunc)
        {
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var extension = extensionFunc({{resultValueExpansion}});
            if (extension.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(extensionFunc));
            }
            else if (extension.IsFailure)
            {
                return extension.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "extension.Value")}});
        }
        """);

        GenerateAsyncMethods("Task", sb, templateArgNames, resultTypeDef, resultValueExpansion);
        GenerateAsyncMethods("ValueTask", sb, templateArgNames, resultTypeDef, resultValueExpansion);

        sb.AppendLine("#endregion");
    }

    private static void GenerateAsyncMethods(string taskType, StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string resultValueExpansion)
    {
        var expandedTemplateArgNames = templateArgNames.Add("R");
        string resultExpandedTypeDef = GenerateResultTypeDef(expandedTemplateArgNames);
        string methodTemplateDecl = GenerateTemplateDecl(expandedTemplateArgNames);
        string bindTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("Result<R>"));
        string asyncActionTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<Result<R>>"));

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultExtendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Extend{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{bindTemplateDecl}} extensionFunc)
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

            var extension = extensionFunc({{resultValueExpansion}});
            if (extension.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(extensionFunc));
            }
            else if (extension.IsFailure)
            {
                return extension.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "extension.Value")}});
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultExtendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Extend{{methodTemplateDecl}}(this {{resultTypeDef}} result, Func{{asyncActionTemplateDecl}} extensionFunc)
        {
            if (result.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(result));
            }
            else if (result.IsFailure)
            {
                return result.Error!;
            }

            var extension = await extensionFunc({{resultValueExpansion}}).ConfigureAwait(false);
            if (extension.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(extensionFunc));
            }
            else if (extension.IsFailure)
            {
                return extension.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "extension.Value")}});
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultExtendExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultExpandedTypeDef}}> Extend{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{asyncActionTemplateDecl}} extensionFunc)
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

            var extension = await extensionFunc({{resultValueExpansion}}).ConfigureAwait(false);
            if (extension.State == ResultState.Bottom)
            {
                throw new ResultNotInitializedException(nameof(extensionFunc));
            }
            else if (extension.IsFailure)
            {
                return extension.Error!;
            }

            return Result.Success({{JoinArguments(resultValueExpansion, "extension.Value")}});
        }
        """);
    }
}
