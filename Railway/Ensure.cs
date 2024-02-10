namespace Just.Railway;

public static partial class Ensure
{
    public delegate Error ErrorFactory(string valueExpression);

    public const string DefaultErrorType = "EnsureFailed";

    [Pure] public static Ensure<T> That<T>(T value, [CallerArgumentExpression(nameof(value))]string valueExpression = "") => new(value, valueExpression);

    [Pure] public static Result<T> Result<T>(this in Ensure<T> ensure) => ensure;
    [Pure] public static async Task<Result<T>> Result<T>(this Task<Ensure<T>> ensureTask) => await ensureTask.ConfigureAwait(false);
    [Pure] public static async ValueTask<Result<T>> Result<T>(this ValueTask<Ensure<T>> ensureTask) => await ensureTask.ConfigureAwait(false);
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

    [Pure]
    public static implicit operator Result<T>(in Ensure<T> ensure) => ensure.State switch
    {
        ResultState.Success => new(ensure.Value),
        ResultState.Error => new(ensure.Error!),
        _ => throw new EnsureNotInitializedException(nameof(ensure))
    };

    [Pure]
    public static explicit operator Result(in Ensure<T> ensure) => ensure.State switch
    {
        ResultState.Success => new(null),
        ResultState.Error => new(ensure.Error!),
        _ => throw new EnsureNotInitializedException(nameof(ensure))
    };
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
