namespace Just.Railway;

public static partial class Ensure
{
    public delegate Error ErrorFactory(string valueExpression);

    public const string DefaultErrorType = "EnsureFailed";

    [Pure] public static Ensure<T> That<T>(T value, [CallerArgumentExpression(nameof(value))]string valueExpression = "") => new(value, valueExpression);

    [Pure] public static Result<T> Result<T>(this in Ensure<T> ensure) => ensure.State switch
    {
        ResultState.Success => new(ensure.Value),
        ResultState.Error => new(ensure.Error!),
        _ => throw new EnsureNotInitializedException(nameof(ensure))
    };
    [Pure] public static async Task<Result<T>> Result<T>(this Task<Ensure<T>> ensureTask)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => new(ensure.Value),
            ResultState.Error => new(ensure.Error!),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }
    [Pure] public static async ValueTask<Result<T>> Result<T>(this ValueTask<Ensure<T>> ensureTask)
    {
        var ensure = await ensureTask.ConfigureAwait(false);
        return ensure.State switch
        {
            ResultState.Success => new(ensure.Value),
            ResultState.Error => new(ensure.Error!),
            _ => throw new EnsureNotInitializedException(nameof(ensureTask))
        };
    }

    [Pure] public static Ensure<string> NotWhitespace(this in Ensure<string> ensure, Error error = default!)
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
        Error = default;
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
public class EnsureNotInitializedException : InvalidOperationException
{
    public EnsureNotInitializedException(string variableName = "this")
        : base("Ensure was not properly initialized.")
    {
        VariableName = variableName;
    }
    public string VariableName { get; }
}
