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

        string resultTypeDef = GenerateResultTypeDef(templateArgNames);
        string resultValueExpansion = GenerateResultValueExpansion(templateArgNames);
        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames.Add("R"));

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMapExecutor)}}", "1.0.0.0")]
        public static Result<R> Map{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func{{methodTemplateDecl}} mapping)
        {
            return result.State switch
            {
                ResultState.Success => mapping({{resultValueExpansion}}),
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
        var methodTemplateArgNames = templateArgNames.Add("R");
        string methodTemplateDecl = GenerateTemplateDecl(methodTemplateArgNames);
        string asyncActionTemplateDecl = GenerateTemplateDecl(templateArgNames.Add($"{taskType}<R>"));

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultMapExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Result<R>> Map{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{methodTemplateDecl}} mapping)
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
        public static async {{taskType}}<Result<R>> Map{{methodTemplateDecl}}(this {{resultTypeDef}} result, Func{{asyncActionTemplateDecl}} mapping)
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
        public static async {{taskType}}<Result<R>> Map{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func{{asyncActionTemplateDecl}} mapping)
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
    }
}
