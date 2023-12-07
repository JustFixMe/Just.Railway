
namespace Just.Railway;

internal enum ResultState : byte
{
    Bottom = 0, Error = 0b01, Success = 0b11,
}

public readonly partial struct Result : IEquatable<Result>
{
    internal readonly Error? Error;
    internal readonly ResultState State;

    internal Result(Error? error)
    {
        Error = error;
        State = error is null ? ResultState.Success : ResultState.Error;
    }

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Success() => new(null);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Success<T>(T value) => new(value);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2)> Success<T1, T2>(T1 value1, T2 value2) => new((value1, value2));
    
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3)> Success<T1, T2, T3>(T1 value1, T2 value2, T3 value3) => new((value1, value2, value3));
    
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3, T4)> Success<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4) => new((value1, value2, value3, value4));
    
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<(T1, T2, T3, T4, T5)> Success<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5) => new((value1, value2, value3, value4, value5));


    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<T> Failure<T>(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<SuccessUnit>(Result result) => result.State switch
    {
        ResultState.Success => new(new SuccessUnit()),
        ResultState.Error => new(result.Error!),
        _ => throw new ResultNotInitializedException(nameof(result))
    };
    
    [Pure] public bool IsSuccess => Error is null;
    [Pure] public bool IsFailure => Error is not null;

    [Pure] public bool TryGetError([MaybeNullWhen(false)]out Error error)
    {
        if (IsSuccess)
        {
            error = default;
            return false;
        }
        if (IsFailure)
        {
            error = Error!;
            return true;
        }
        throw new ResultNotInitializedException();
    }

    [Pure] public override string ToString() => State switch
    {
        ResultState.Success => "",
        ResultState.Error => Error!.ToString(),
        _ => throw new ResultNotInitializedException()
    };

    [Pure] public override int GetHashCode() => State switch
    {
        ResultState.Success => 0,
        ResultState.Error => Error!.GetHashCode(),
        _ => throw new ResultNotInitializedException()
    };

    [Pure] public override bool Equals(object? obj) => obj is Result other && Equals(other);
    [Pure] public bool Equals(Result other)
    {
        if (State == ResultState.Bottom)
            throw new ResultNotInitializedException();
        
        return Error == other.Error;
    }
    [Pure] public static bool operator ==(Result left, Result right) => left.Equals(right);
    [Pure] public static bool operator !=(Result left, Result right) => !(left == right);
}

public readonly struct Result<T> : IEquatable<Result<T>>
{
    internal readonly Error? Error;
    internal readonly T Value;
    internal readonly ResultState State;

    internal Result(Error error)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error));
        State = ResultState.Error;
        Value = default!;
    }

    internal Result(T value)
    {
        Value = value;
        State = ResultState.Success;
    }

    [Pure] public static explicit operator Result(Result<T> result) => result.State switch
    {
        ResultState.Success => new(null),
        ResultState.Error => new(result.Error!),
        _ => throw new ResultNotInitializedException(nameof(result))
    };
    [Pure] public static implicit operator Result<T>(Error error) => new(error);
    [Pure] public static implicit operator Result<T>(T value) => new(value);
    [Pure] public bool IsSuccess => State == ResultState.Success;
    [Pure] public bool IsFailure => State == ResultState.Error;

    [Pure] public bool Unwrap([MaybeNullWhen(false)]out T value, [MaybeNullWhen(true)]out Error error)
    {
        switch (State)
        {
            case ResultState.Success:
                value = Value;
                error = default;
                return true;

            case ResultState.Error:
                value = default;
                error = Error!;
                return false;

            default: throw new ResultNotInitializedException();
        }
    }
    [Pure] public bool TryGetValue([MaybeNullWhen(false)]out T value)
    {
        switch (State)
        {
            case ResultState.Success:
                value = Value;
                return true;

            case ResultState.Error:
                value = default;
                return false;

            default: throw new ResultNotInitializedException();
        }
    }
    [Pure] public bool TryGetError([MaybeNullWhen(false)]out Error error)
    {
        switch (State)
        {
            case ResultState.Success:
                error = default;
                return false;

            case ResultState.Error:
                error = Error!;
                return true;

            default: throw new ResultNotInitializedException();
        }
    }

    [Pure] public Result<R> Cast<R>()
    {
        switch (State)
        {
            case ResultState.Error:
                return Error!;
            
            case ResultState.Success:
            {
                if (Value is R ret)
                    return ret;
                
                if (typeof(R).IsAssignableFrom(typeof(T)) && Value is null)
                    return default(R)!;

                return (R)(object)Value!;
            }

            default: throw new ResultNotInitializedException();
        }
    }

    [Pure] public override string ToString() => State switch
    {
        ResultState.Success => Value?.ToString() ?? "",
        ResultState.Error => Error!.ToString(),
        _ => throw new ResultNotInitializedException()
    };

    [Pure] public override int GetHashCode() => State switch
    {
        ResultState.Success => Value?.GetHashCode() ?? 0,
        ResultState.Error => Error!.GetHashCode(),
        _ => throw new ResultNotInitializedException()
    };

    [Pure] public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);
    [Pure] public bool Equals(Result<T> other)
    {
        if (State == ResultState.Bottom)
            throw new ResultNotInitializedException();
        
        if (IsSuccess != other.IsSuccess)
            return false;

        return IsSuccess
            ? ReflectionHelper.IsEqual(Value, other.Value)
            : Error == other.Error;
    }
    [Pure] public static bool operator ==(Result<T> left, Result<T> right) => left.Equals(right);
    [Pure] public static bool operator !=(Result<T> left, Result<T> right) => !(left == right);
}

public readonly struct SuccessUnit : IEquatable<SuccessUnit>
{
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SuccessUnit;
    public bool Equals(SuccessUnit other) => true;
    public override int GetHashCode() => 0;
    public override string ToString() => "success";

    public static bool operator ==(SuccessUnit left, SuccessUnit right) => left.Equals(right);

    public static bool operator !=(SuccessUnit left, SuccessUnit right) => !(left == right);
}

[Serializable]
public class ResultNotInitializedException(string variableName = "this") : InvalidOperationException("Result was not properly initialized.")
{
    public string VariableName { get; } = variableName;
}
