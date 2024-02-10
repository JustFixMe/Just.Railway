using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Just.Railway.SourceGen;

public sealed class EnsureExtensionsExecutor : IGeneratorExecutor
{
    public void Execute(SourceProductionContext context, Compilation source)
    {
        var methods = GenerateMethods();
        var code = $$"""
        #nullable enable
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using System.Diagnostics.Contracts;
        using System.CodeDom.Compiler;

        namespace Just.Railway;

        public static partial class Ensure
        {
        {{methods}}
        }
        """;

        context.AddSource("Ensure.Extensions.g.cs", code);
    }

    private string GenerateMethods()
    {
        List<(string ErrorParameterDecl, string ErrorValueExpr)> errorGenerationDefinitions =
        [
            ("Error error = default!", "error"),
            ("ErrorFactory errorFactory", "errorFactory(ensure.ValueExpression)")
        ];

        var sb = new StringBuilder();

        sb.AppendLine("#region Satisfies");
        errorGenerationDefinitions.ForEach(def => GenerateSatisfiesExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region Null");
        errorGenerationDefinitions.ForEach(def => GenerateNullExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region NotNull");
        errorGenerationDefinitions.ForEach(def => GenerateNotNullExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region NotEmpty");
        errorGenerationDefinitions.ForEach(def => GenerateNotEmptyExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region NotWhitespace");
        errorGenerationDefinitions.ForEach(def => GenerateNotWhitespaceExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region True");
        errorGenerationDefinitions.ForEach(def => GenerateTrueExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region False");
        errorGenerationDefinitions.ForEach(def => GenerateFalseExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region EqualTo");
        errorGenerationDefinitions.ForEach(def => GenerateEqualToExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region NotEqualTo");
        errorGenerationDefinitions.ForEach(def => GenerateNotEqualToExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region LessThan");
        errorGenerationDefinitions.ForEach(def => GenerateLessThanExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region GreaterThan");
        errorGenerationDefinitions.ForEach(def => GenerateGreaterThanExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region LessThanOrEqualTo");
        errorGenerationDefinitions.ForEach(def => GenerateLessThanOrEqualToExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        sb.AppendLine("#region GreaterThanOrEqualTo");
        errorGenerationDefinitions.ForEach(def => GenerateGreaterThanOrEqualToExtensions(sb, def.ErrorParameterDecl, def.ErrorValueExpr));
        sb.AppendLine("#endregion");

        return sb.ToString();
    }

    private void GenerateNotWhitespaceExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is empty or consists exclusively of white-space characters.\")";

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<string> NotWhitespace(this in Ensure<string> ensure, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => string.IsNullOrWhiteSpace(ensure.Value)
                    ? new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression)
                    : new(ensure.Value!, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateNotEmptyExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is empty.\")";
        List<(string TemplateDef, string CollectionType, string NotEmptyTest)> typeOverloads =
        [
            ("<T>", "IEnumerable<T>", "ensure.Value?.Any() == true"),
            ("<T>", "ICollection<T>", "ensure.Value?.Count > 0"),
            ("<T>", "IReadOnlyCollection<T>", "ensure.Value?.Count > 0"),
            ("<T>", "IList<T>", "ensure.Value?.Count > 0"),
            ("<T>", "IReadOnlyList<T>", "ensure.Value?.Count > 0"),
            ("<T>", "ISet<T>", "ensure.Value?.Count > 0"),
            ("<T>", "IReadOnlySet<T>", "ensure.Value?.Count > 0"),
            ("<TKey,TValue>", "IDictionary<TKey,TValue>", "ensure.Value?.Count > 0"),
            ("<TKey,TValue>", "IReadOnlyDictionary<TKey,TValue>", "ensure.Value?.Count > 0"),
            ("<T>", "T[]", "ensure.Value?.Length > 0"),
            ("<T>", "List<T>", "ensure.Value?.Count > 0"),
            ("<T>", "Queue<T>", "ensure.Value?.Count > 0"),
            ("<T>", "HashSet<T>", "ensure.Value?.Count > 0"),
            ("", "string", "!string.IsNullOrEmpty(ensure.Value)"),
        ];

        typeOverloads.ForEach(def => sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<{{def.CollectionType}}> NotEmpty{{def.TemplateDef}}(this in Ensure<{{def.CollectionType}}> ensure, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => {{def.NotEmptyTest}}
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """));
    }

    private void GenerateNullExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is not null.\")";

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T?> Null<T>(this in Ensure<T?> ensure, {{errorParameterDecl}})
            where T : struct
        {
            return ensure.State switch
            {
                ResultState.Success => !ensure.Value.HasValue
                    ? new(default(T?)!, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T?> Null<T>(this in Ensure<T?> ensure, {{errorParameterDecl}})
            where T : class
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value is null
                    ? new(default(T?)!, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateNotNullExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is null.\")";

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> NotNull<T>(this in Ensure<T?> ensure, {{errorParameterDecl}})
            where T : struct
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value.HasValue
                    ? new(ensure.Value.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> NotNull<T>(this in Ensure<T?> ensure, {{errorParameterDecl}})
            where T : notnull
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value is not null
                    ? new(ensure.Value!, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateSatisfiesExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} does not satisfy the requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> Satisfies<T>(this in Ensure<T> ensure, Func<T, bool> requirement, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => requirement(ensure.Value)
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);

        GenerateSatisfiesAsyncExtensions(sb, "Task", errorParameterDecl, errorValueExpr, defaultErrorExpr);
        GenerateSatisfiesAsyncExtensions(sb, "ValueTask", errorParameterDecl, errorValueExpr, defaultErrorExpr);
    }

    private void GenerateSatisfiesAsyncExtensions(StringBuilder sb, string taskType, string errorParameterDecl, string errorValueExpr, string defaultErrorExpr)
    {
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Ensure<T>> Satisfies<T>(this {{taskType}}<Ensure<T>> ensureTask, Func<T, bool> requirement, {{errorParameterDecl}})
        {
            var ensure = await ensureTask.ConfigureAwait(false);
            return ensure.State switch
            {
                ResultState.Success => requirement(ensure.Value)
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensureTask))
            };
        }
        """);
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Ensure<T>> Satisfies<T>(this Ensure<T> ensure, Func<T, {{taskType}}<bool>> requirement, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => await requirement(ensure.Value).ConfigureAwait(false)
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static async {{taskType}}<Ensure<T>> Satisfies<T>(this {{taskType}}<Ensure<T>> ensureTask, Func<T, {{taskType}}<bool>> requirement, {{errorParameterDecl}})
        {
            var ensure = await ensureTask.ConfigureAwait(false);
            return ensure.State switch
            {
                ResultState.Success => await requirement(ensure.Value).ConfigureAwait(false)
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensureTask))
            };
        }
        """);
    }

    private void GenerateTrueExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is not true.\")";

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<bool> True(this in Ensure<bool> ensure, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value == true
                    ? new(ensure.Value!, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<bool> True(this in Ensure<bool?> ensure, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value == true
                    ? new(ensure.Value.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateFalseExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is not false.\")";

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<bool> False(this in Ensure<bool> ensure, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value == false
                    ? new(ensure.Value!, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);

        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<bool> False(this in Ensure<bool?> ensure, {{errorParameterDecl}})
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value == false
                    ? new(ensure.Value.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateEqualToExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is not equal to requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> EqualTo<T>(this in Ensure<T> ensure, T requirement, {{errorParameterDecl}})
            where T : IEquatable<T>
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value.Equals(requirement)
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateNotEqualToExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is equal to requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> NotEqualTo<T>(this in Ensure<T> ensure, T requirement, {{errorParameterDecl}})
            where T : IEquatable<T>
        {
            return ensure.State switch
            {
                ResultState.Success => !ensure.Value.Equals(requirement)
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateLessThanExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is not less than requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> LessThan<T>(this in Ensure<T> ensure, T requirement, {{errorParameterDecl}})
            where T : IComparable<T>
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value.CompareTo(requirement) < 0
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateGreaterThanExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is not greater than requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> GreaterThan<T>(this in Ensure<T> ensure, T requirement, {{errorParameterDecl}})
            where T : IComparable<T>
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value.CompareTo(requirement) > 0
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateLessThanOrEqualToExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is greater than requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> LessThanOrEqualTo<T>(this in Ensure<T> ensure, T requirement, {{errorParameterDecl}})
            where T : IComparable<T>
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value.CompareTo(requirement) <= 0
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }

    private void GenerateGreaterThanOrEqualToExtensions(StringBuilder sb, string errorParameterDecl, string errorValueExpr)
    {
        string defaultErrorExpr = "?? Error.New(DefaultErrorType, $\"Value {{{ensure.ValueExpression}}} is less than requirement.\")";
        sb.AppendLine($$"""
        [PureAttribute]
        [GeneratedCodeAttribute("{{nameof(EnsureExtensionsExecutor)}}", "1.0.0.0")]
        public static Ensure<T> GreaterThanOrEqualTo<T>(this in Ensure<T> ensure, T requirement, {{errorParameterDecl}})
            where T : IComparable<T>
        {
            return ensure.State switch
            {
                ResultState.Success => ensure.Value.CompareTo(requirement) >= 0
                    ? new(ensure.Value, ensure.ValueExpression)
                    : new({{errorValueExpr}} {{defaultErrorExpr}}, ensure.ValueExpression),
                ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
                _ => throw new EnsureNotInitializedException(nameof(ensure))
            };
        }
        """);
    }
}
