using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Just.Railway.SourceGen;

internal sealed class ResultTryRecoverExecutor : ResultExtensionsExecutor
{
    protected override string ExtensionType => "TryRecover";
    protected override void GenerateMethodsForArgCount(StringBuilder sb, int argCount)
    {
        if (argCount > 1) return;
        
        var templateArgNames = Enumerable.Repeat("T", argCount)
            .ToImmutableArray();

        string methodTemplateDecl = GenerateTemplateDecl(templateArgNames);
        string resultTypeDef = GenerateResultTypeDef(templateArgNames);

        sb.AppendLine($"#region {resultTypeDef}");

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTryRecoverExecutor)}}", "1.0.0.0")]
        public static {{resultTypeDef}} TryRecover{{methodTemplateDecl}}(this in {{resultTypeDef}} result, Func<Error, {{resultTypeDef}}> recover)
        {
            return result.State switch
            {
                ResultState.Success => ({{resultTypeDef}})result.Value,
                ResultState.Error => recover(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);

        GenerateAsyncMethods("Task", sb, templateArgNames, resultTypeDef, methodTemplateDecl);
        GenerateAsyncMethods("ValueTask", sb, templateArgNames, resultTypeDef, methodTemplateDecl);

        sb.AppendLine("#endregion");
    }
    private static void GenerateAsyncMethods(string taskType, StringBuilder sb, ImmutableArray<string> templateArgNames, string resultTypeDef, string methodTemplateDecl)
    {
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTryRecoverExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> TryRecover{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<Error, {{resultTypeDef}}> recover)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => ({{resultTypeDef}})result.Value,
                ResultState.Error => recover(result.Error!),
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTryRecoverExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> TryRecover{{methodTemplateDecl}}(this {{resultTypeDef}} result, Func<Error, {{taskType}}<{{resultTypeDef}}>> recover)
        {
            return result.State switch
            {
                ResultState.Success => ({{resultTypeDef}})result.Value,
                ResultState.Error => await recover(result.Error!).ConfigureAwait(false),
                _ => throw new ResultNotInitializedException(nameof(result))
            };
        }
        """);
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(ResultTryRecoverExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<{{resultTypeDef}}> TryRecover{{methodTemplateDecl}}(this {{taskType}}<{{resultTypeDef}}> resultTask, Func<Error, {{taskType}}<{{resultTypeDef}}>> recover)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.State switch
            {
                ResultState.Success => ({{resultTypeDef}})result.Value,
                ResultState.Error => await recover(result.Error!).ConfigureAwait(false),
                _ => throw new ResultNotInitializedException(nameof(resultTask))
            };
        }
        """);
    }
}
