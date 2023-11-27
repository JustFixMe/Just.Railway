namespace Just.Railway;

public static class Ensure
{
    public delegate Error ErrorFactory(string valueExpression);

    public const string DefaultErrorType = "EnsureFailed";

    [Pure] public static Ensure<T> That<T>(T value, [CallerArgumentExpression(nameof(value))]string valueExpression = "") => new(value, valueExpression);
    [Pure] public static async Task<Ensure<T>> That<T>(Task<T> value, [CallerArgumentExpression(nameof(value))]string valueExpression = "") => new(await value.ConfigureAwait(false), valueExpression);

    [Pure] public static Result<T> Result<T>(this in Ensure<T> ensure) => ensure.State switch
    {
        ResultState.Success => new(ensure.Value),
        ResultState.Error => new(ensure.Error!),
        _ => throw new EnsureNotInitializedException(nameof(ensure))
    };
    [Pure]
    public static async Task<Result<T>> Result<T>(this Task<Ensure<T>> ensureTask)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => new(ensure.Value),
            ResultState.Error => new(ensure.Error!),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }
    [Pure]
    public static Ensure<T> Satisfies<T>(this in Ensure<T> ensure, Func<T, bool> requirement, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => requirement(ensure.Value)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} does not satisfy the requirement."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<T> Satisfies<T>(this in Ensure<T> ensure, Func<T, bool> requirement, ErrorFactory errorFactory)
    {
        return ensure.State switch
        {
            ResultState.Success => requirement(ensure.Value)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(errorFactory(ensure.ValueExpression), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> Satisfies<T>(this Task<Ensure<T>> ensureTask, Func<T, bool> requirement, Error error = default!)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => requirement(ensure.Value)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} does not satisfy the requirement."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> Satisfies<T>(this Task<Ensure<T>> ensureTask, Func<T, bool> requirement, ErrorFactory errorFactory)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => requirement(ensure.Value)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(errorFactory(ensure.ValueExpression), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> Satisfies<T>(this Ensure<T> ensure, Func<T, Task<bool>> requirement, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => await requirement(ensure.Value).ConfigureAwait(false)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} does not satisfy the requirement."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> Satisfies<T>(this Ensure<T> ensure, Func<T, Task<bool>> requirement, ErrorFactory errorFactory)
    {
        return ensure.State switch
        {
            ResultState.Success => await requirement(ensure.Value).ConfigureAwait(false)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(errorFactory(ensure.ValueExpression), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> Satisfies<T>(this Task<Ensure<T>> ensureTask, Func<T, Task<bool>> requirement, Error error = default!)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => await requirement(ensure.Value).ConfigureAwait(false)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} does not satisfy the requirement."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> Satisfies<T>(this Task<Ensure<T>> ensureTask, Func<T, Task<bool>> requirement, ErrorFactory errorFactory)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => await requirement(ensure.Value).ConfigureAwait(false)
                ? new(ensure.Value, ensure.ValueExpression)
                : new(errorFactory(ensure.ValueExpression), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }

    [Pure]
    public static Ensure<T> NotNull<T>(this in Ensure<T?> ensure, Error error = default!)
        where T : struct
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value.HasValue
                ? new(ensure.Value.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is null."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> NotNull<T>(this Task<Ensure<T?>> ensureTask, Error error = default!)
        where T : struct
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => ensure.Value.HasValue
                ? new(ensure.Value.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is null."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }
    [Pure]
    public static Ensure<T> NotNull<T>(this in Ensure<T?> ensure, Error error = default!)
        where T : notnull
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is null."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static async Task<Ensure<T>> NotNull<T>(this Task<Ensure<T?>> ensureTask, Error error = default!)
        where T : notnull
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is null."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }

    [Pure]
    public static Ensure<T[]> NotEmpty<T>(this in Ensure<T[]> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Length > 0
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<List<T>> NotEmpty<T>(this in Ensure<List<T>> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Count > 0
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<IReadOnlyCollection<T>> NotEmpty<T>(this in Ensure<IReadOnlyCollection<T>> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Count > 0
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<ICollection<T>> NotEmpty<T>(this in Ensure<ICollection<T>> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Count > 0
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<IReadOnlyList<T>> NotEmpty<T>(this in Ensure<IReadOnlyList<T>> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Count > 0
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<IList<T>> NotEmpty<T>(this in Ensure<IList<T>> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Count > 0
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<IEnumerable<T>> NotEmpty<T>(this in Ensure<IEnumerable<T>> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => ensure.Value is not null && ensure.Value.Any()
                ? new(ensure.Value, ensure.ValueExpression)
                : new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
    [Pure]
    public static Ensure<string> NotEmpty(this in Ensure<string> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => string.IsNullOrEmpty(ensure.Value)
                ? new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty."), ensure.ValueExpression)
                : new(ensure.Value, ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }

    [Pure]
    public static Ensure<string> NotWhitespace(this in Ensure<string> ensure, Error error = default!)
    {
        return ensure.State switch
        {
            ResultState.Success => string.IsNullOrWhiteSpace(ensure.Value)
                ? new(error ?? Error.New(DefaultErrorType, $"Value {{{ensure.ValueExpression}}} is empty or consists exclusively of white-space characters."), ensure.ValueExpression)
                : new(ensure.Value, ensure.ValueExpression),
            ResultState.Error => new(ensure.Error!, ensure.ValueExpression),
            _ => throw new EnsureNotInitializedException(nameof(ensure))
        };
    }
}

public readonly struct Ensure<T>
{
    internal readonly ResultState State;
    internal readonly Error? Error;
    internal readonly T Value;
    internal readonly string ValueExpression;

    internal Ensure(T value, string valueExpression)
    {
        Value = value;
        ValueExpression = valueExpression;
        State = ResultState.Success;
    }

    internal Ensure(Error error, string valueExpression)
    {
        Error = error;
        ValueExpression = valueExpression;
        Value = default!;
        State = ResultState.Error;
    }
}

[Serializable]
public class EnsureNotInitializedException(string variableName = "this") : InvalidOperationException("Ensure was not properly initialized.")
{
    public string VariableName { get; } = variableName;
}
