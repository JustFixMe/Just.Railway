using System.Collections;
using System.Collections.Immutable;
using System.Text;

namespace Just.Railway;

[JsonConverter(typeof(ErrorJsonConverter))]
public abstract class Error : IEquatable<Error>, IComparable<Error>
{
    protected internal Error(){}

    /// <summary>
    /// Create an <see cref="ExceptionalError"/>
    /// </summary>
    /// <param name="thisException">Exception</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(Exception thisException) => new ExceptionalError(thisException);
    /// <summary>
    /// Create a <see cref="ExceptionalError"/> with an overriden detail. This can be useful for sanitising the display message
    /// when internally we're carrying the exception. 
    /// </summary>
    /// <param name="message">Error detail</param>
    /// <param name="thisException">Exception</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string message, Exception thisException) =>
        new ExceptionalError(message, thisException);
    /// <summary>
    /// Create an <see cref="ExpectedError"/>
    /// </summary>
    /// <param name="message">Error detail</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string message, IEnumerable<KeyValuePair<string, string>>? extensionData = null) =>
        new ExpectedError("error", message)
        {
            ExtensionData = extensionData?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty
        };
    /// <summary>
    /// Create an <see cref="ExpectedError"/>
    /// </summary>
    /// <param name="type">Error code</param>
    /// <param name="message">Error detail</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string type, string message, IEnumerable<KeyValuePair<string, string>>? extensionData = null) =>
        new ExpectedError(type, message)
        {
            ExtensionData = extensionData?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty
        };
    /// <summary>
    /// Create a <see cref="ManyErrors"/>
    /// </summary>
    /// <remarks>Collects many errors into a single <see cref="Error"/> type, called <see cref="ManyErrors"/></remarks>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Many(Error error1, Error error2) => (error1, error2) switch
    {
        (null, null) => new ManyErrors(ImmutableArray<Error>.Empty),
        (Error err, null) => err,
        (Error err, { IsEmpty: true }) => err,
        (null, Error err) => err,
        ({ IsEmpty: true }, Error err) => err,
        (Error err1, Error err2) => new ManyErrors(err1, err2)
    };
    /// <summary>
    /// Create a <see cref="ManyErrors"/>
    /// </summary>
    /// <remarks>Collects many errors into a single <see cref="Error"/> type, called <see cref="ManyErrors"/></remarks>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Many(params Error[] errors) => errors.Length switch
    {
        1 => errors[0],
        _ => new ManyErrors(errors)
    };
    /// <summary>
    /// Create a <see cref="ManyErrors"/>
    /// </summary>
    /// <remarks>Collects many errors into a single <see cref="Error"/> type, called <see cref="ManyErrors"/></remarks>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Many(IEnumerable<Error> errors) => new ManyErrors(errors);

    [Pure] public abstract string Type { get; }
    [Pure] public abstract string Message { get; }

    [Pure] public ImmutableDictionary<string, string> ExtensionData { get; internal init; } = ImmutableDictionary<string, string>.Empty;
    [Pure] public string? this[string key] => ExtensionData.TryGetValue(key, out var value) == true ? value : null;

    [Pure] public abstract int Count { get; }
    [Pure] public abstract bool IsEmpty { get; }
    [Pure] public abstract bool IsExpected { get; }
    [Pure] public abstract bool IsExeptional { get; }

    [Pure] public Error Append(Error? next)
    {
        if (next is null || next.IsEmpty)
            return this;

        if (this.IsEmpty)
            return next;
        
        return new ManyErrors(this, next);
    }
    [Pure]
    [return: NotNullIfNotNull(nameof(lhs))]
    [return: NotNullIfNotNull(nameof(rhs))]
    public static Error? operator +(Error? lhs, Error? rhs) => lhs is null ? rhs : lhs.Append(rhs);

    [Pure] public abstract IEnumerable<Error> ToEnumerable();
    
    /// <summary>
    /// Gets the <see cref="Exception"/>
    /// </summary>
    /// <returns>New <see cref="ErrorException"/> constructed from current error</returns>
    [Pure] public virtual Exception ToException() => new ErrorException(Type, Message);

    /// <summary>
    /// Compares error types
    /// </summary>
    /// <returns><see cref="true"/> when other error has the same type</returns>
    [Pure] public virtual bool IsSimilarTo([NotNullWhen(true)] Error? other) => Type == other?.Type;
    [Pure] public virtual bool Equals([NotNullWhen(true)] Error? other) => IsSimilarTo(other) && Message == other.Message;
    [Pure] public static bool operator ==(Error? lhs, Error? rhs) => lhs is null ? rhs is null : lhs.Equals(rhs);
    [Pure] public static bool operator !=(Error? lhs, Error? rhs) => !(lhs == rhs);
    [Pure] public sealed override bool Equals(object? obj) => Equals(obj as Error);
    [Pure] public override int GetHashCode() => HashCode.Combine(Type, Message);

    [Pure] public virtual int CompareTo(Error? other)
    {
        if (other is null)
        {
            return -1;
        }
        
        var compareResult = string.Compare(Type, other.Type);
        if (compareResult != 0)
        {
            return compareResult;
        }

        return string.Compare(Message, other.Message);
    }

    [Pure] public sealed override string ToString() => Message;
    [Pure] public void Deconstruct(out string type, out string message)
    {
        type = Type;
        message = Message;
    }

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)] internal virtual Error AccessUnsafe(int position) => this;
}

[JsonConverter(typeof(ExpectedErrorJsonConverter))]
public sealed class ExpectedError : Error
{
    public ExpectedError(string type, string message)
    {
        Type = type;
        Message = message;
    }
    public ExpectedError(string message)
        : this("error", message)
    {
    }

    [Pure] public override string Type { get; }
    [Pure] public override string Message { get; }

    [Pure] public override int Count => 1;
    [Pure] public override bool IsEmpty => false;
    [Pure] public override bool IsExpected => true;
    [Pure] public override bool IsExeptional => false;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IEnumerable<Error> ToEnumerable()
    {
        yield return this;
    }
}

[JsonConverter(typeof(ExceptionalErrorJsonConverter))]
public sealed class ExceptionalError : Error
{
    internal readonly Exception? Exception;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string ToErrorType(Type exceptionType) => exceptionType.FullName ?? exceptionType.Name;

    internal ExceptionalError(Exception exception)
        : this(ToErrorType(exception.GetType()), exception.Message)
    {
        Exception = exception;
        ExtensionData = ExtractExtensionData(exception);
    }
    internal ExceptionalError(string message, Exception exception)
        : this(ToErrorType(exception.GetType()), message)
    {
        Exception = exception;
        ExtensionData = ExtractExtensionData(exception);
    }

    public ExceptionalError(string type, string message)
    {
        Type = type;
        Message = message;
    }

    [Pure] public override string Type { get; }
    [Pure] public override string Message { get; }

    [Pure] public override int Count => 1;
    [Pure] public override bool IsEmpty => false;
    [Pure] public override bool IsExpected => false;
    [Pure] public override bool IsExeptional => true;

    [Pure] public override Exception ToException() => Exception ?? base.ToException();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IEnumerable<Error> ToEnumerable()
    {
        yield return this;
    }

    private static ImmutableDictionary<string, string> ExtractExtensionData(Exception exception)
    {
        if (!(exception.Data?.Count > 0))
        return ImmutableDictionary<string, string>.Empty;

        ImmutableDictionary<string, string>.Builder? values = null;

        foreach (var key in exception.Data.Keys)
        {
            if (key is null) continue;

            var value = exception.Data[key];
            if (value is null) continue;

            var keyString = key.ToString();
            var valueString = value.ToString();
            if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(valueString)) continue;

            values ??= ImmutableDictionary.CreateBuilder<string, string>();
            values.Add(keyString, valueString);
        }
        return values is not null ? values.ToImmutable() : ImmutableDictionary<string, string>.Empty;
    }
}

[JsonConverter(typeof(ManyErrorsJsonConverter))]
public sealed class ManyErrors : Error, IEnumerable<Error>, IReadOnlyList<Error>
{
    private readonly ImmutableArray<Error> _errors;
    [Pure] public IEnumerable<Error> Errors { get => _errors; }

    internal ManyErrors(ImmutableArray<Error> errors) => _errors = errors;
    internal ManyErrors(Error head, Error tail)
    {
        var headCount = head.Count;
        var tailCount = tail.Count;
        var errors = ImmutableArray.CreateBuilder<Error>(headCount + tailCount);

        if (headCount > 0)
            AppendSanitized(errors, head);

        if (tailCount > 0)
            AppendSanitized(errors, tail);

        _errors = errors.MoveToImmutable();
    }
    public ManyErrors(IEnumerable<Error> errors)
    {
        var unpackedErrors = ImmutableArray.CreateBuilder<Error>();

        foreach (var err in errors)
        {
            if (err.IsEmpty) continue;

            AppendSanitized(unpackedErrors, err);
        }

        _errors = unpackedErrors.ToImmutable();
    }

    [Pure] public override string Type => "many_errors";

    private string? _lazyMessage = null;
    [Pure] public override string Message => _lazyMessage ??= ToFullArrayString(_errors);

    [Pure] private static string ToFullArrayString(in ImmutableArray<Error> errors)
    {
        var separator = Environment.NewLine;

        var sb = new StringBuilder();
        for (int i = 0; i < errors.Length; i++)
        {
            sb.Append(errors[i]);
            sb.Append(separator);
        }
        sb.Remove(sb.Length - separator.Length, separator.Length);

        return sb.ToString();
    }

    [Pure] public override int Count => _errors.Length;
    [Pure] public override bool IsEmpty => _errors.IsEmpty;
    [Pure] public override bool IsExpected => _errors.All(static x => x.IsExpected);
    [Pure] public override bool IsExeptional => _errors.Any(static x => x.IsExeptional);

    [Pure] public Error this[int index] => _errors[index];

    [Pure] public override Exception ToException() => new AggregateException(_errors.Select(static x => x.ToException()));
    [Pure] public override IEnumerable<Error> ToEnumerable() => _errors;

    [Pure] public override int CompareTo(Error? other)
    {
        if (other is null)
            return -1;
        if (other.Count != _errors.Length)
            return _errors.Length.CompareTo(other.Count);

        for (int i = 0; i < _errors.Length; i++)
        {
            var compareResult = _errors[i].CompareTo(other.AccessUnsafe(i));
            if (compareResult != 0)
            {
                return compareResult;
            }
        }

        return 0;
    }
    [Pure] public override bool IsSimilarTo([NotNullWhen(true)] Error? other)
    {
        if (other is null)
        {
            return false;
        }
        if (_errors.Length != other.Count)
        {
            return false;
        }
        for (int i = 0; i < _errors.Length; i++)
        {
            if (!_errors[i].IsSimilarTo(other.AccessUnsafe(i)))
            {
                return false;
            }
        }
        return true;
    }
    [Pure] public override bool Equals([NotNullWhen(true)] Error? other)
    {
        if (other is null)
        {
            return false;
        }
        if (_errors.Length != other.Count)
        {
            return false;
        }
        for (int i = 0; i < _errors.Length; i++)
        {
            if (!_errors[i].Equals(other.AccessUnsafe(i)))
            {
                return false;
            }
        }
        return true;
    }

    private int? _lazyHashCode = null;
    [Pure] public override int GetHashCode() => _lazyHashCode ??= CalcHashCode(_errors);
    private static int CalcHashCode(in ImmutableArray<Error> errors)
    {
        if (errors.IsEmpty)
            return 0;

        var hash = new HashCode();
        foreach (var err in errors)
        {
            hash.Add(err);
        }
        return hash.ToHashCode();
    }


    [Pure] public ImmutableArray<Error>.Enumerator GetEnumerator() => _errors.GetEnumerator();
    [Pure] IEnumerator<Error> IEnumerable<Error>.GetEnumerator() => Errors.GetEnumerator();
    [Pure] IEnumerator IEnumerable.GetEnumerator() => Errors.GetEnumerator();

    internal static void AppendSanitized(ImmutableArray<Error>.Builder errors, Error error)
    {
        if (error is ManyErrors many)
            errors.AddRange(many._errors);
        else
            errors.Add(error);
    }

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)] internal override Error AccessUnsafe(int position) => _errors[position];
}

[Serializable]
public sealed class ErrorException : Exception
{
    public ErrorException(string type, string message) : base(message)
    {
        Type = type ?? nameof(ErrorException);
    }
    public string Type { get; }
}
