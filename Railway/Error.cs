using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace Just.Railway;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$$err")]
[JsonDerivedType(typeof(ExpectedError), typeDiscriminator: 0)]
[JsonDerivedType(typeof(ExceptionalError), typeDiscriminator: 1)]
[JsonDerivedType(typeof(ManyErrors))]
public abstract class Error : IEquatable<Error>, IComparable<Error>
{
    private IDictionary<string, object>? _extensionData;

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
    public static Error New(string message) =>
        new ExpectedError("error", message);
    /// <summary>
    /// Create an <see cref="ExpectedError"/>
    /// </summary>
    /// <param name="type">Error code</param>
    /// <param name="message">Error detail</param>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error New(string type, string message) =>
        new ExpectedError(type, message);
    /// <summary>
    /// Create a <see cref="ManyErrors"/>
    /// </summary>
    /// <remarks>Collects many errors into a single <see cref="Error"/> type, called <see cref="ManyErrors"/></remarks>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Many(Error error1, Error error2) => (error1, error2) switch
    {
        (null, null) => new ManyErrors(Enumerable.Empty<Error>()),
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
    [Pure, JsonExtensionData] public IDictionary<string, object> ExtensionData
    {
        get => _extensionData ??= new Dictionary<string, object>();
        init => _extensionData = value ?? new Dictionary<string, object>();
    }
    [Pure] public object? this[string name]
    {
        get => _extensionData?.TryGetValue(name, out var val) == true ? val : null;

        set
        {
            if (value is null)
            {
                _extensionData?.Remove(name);
            }
            else
            {
                _extensionData ??= new Dictionary<string, object>();
                _extensionData[name] = value;
            }
        }
    }

    [Pure, JsonIgnore] public abstract int Count { get; }
    [Pure, JsonIgnore] public abstract bool IsEmpty { get; }
    [Pure, JsonIgnore] public abstract bool IsExpected { get; }
    [Pure, JsonIgnore] public abstract bool IsExeptional { get; }

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

    [Pure] public override string ToString() => Message;
    [Pure] public void Deconstruct(out string type, out string message)
    {
        type = Type;
        message = Message;
    }
}

public sealed class ExpectedError : Error
{
    [JsonConstructor]
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

    [Pure, JsonIgnore] public override int Count => 1;
    [Pure, JsonIgnore] public override bool IsEmpty => false;
    [Pure, JsonIgnore] public override bool IsExpected => true;
    [Pure, JsonIgnore] public override bool IsExeptional => false;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IEnumerable<Error> ToEnumerable()
    {
        yield return this;
    }
}

public sealed class ExceptionalError : Error
{
    internal readonly Exception? Exception;

    internal ExceptionalError(Exception exception)
        : this(exception.GetType().Name, exception.Message)
    {
        Exception = exception;
        FillExtensionData(exception);
    }
    internal ExceptionalError(string message, Exception exception)
        : this(exception.GetType().Name, message)
    {
        Exception = exception;
        FillExtensionData(exception);
    }

    [JsonConstructor]
    public ExceptionalError(string type, string message)
    {
        Type = type;
        Message = message;
    }

    [Pure] public override string Type { get; }
    [Pure] public override string Message { get; }

    [Pure, JsonIgnore] public override int Count => 1;
    [Pure, JsonIgnore] public override bool IsEmpty => false;
    [Pure, JsonIgnore] public override bool IsExpected => false;
    [Pure, JsonIgnore] public override bool IsExeptional => true;

    [Pure] public override Exception ToException() => Exception ?? base.ToException();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IEnumerable<Error> ToEnumerable()
    {
        yield return this;
    }

    private void FillExtensionData(Exception exception)
    {
        foreach (var key in exception.Data.Keys)
        {
            var value = exception.Data[key];
            if (key is null || value is null)
                continue;
            this.ExtensionData[key.ToString() ?? string.Empty] = value;
        }
    }
}

[DataContract]
public sealed class ManyErrors : Error, IEnumerable<Error>
{
    private readonly List<Error> _errors;
    [Pure, DataMember] public IEnumerable<Error> Errors { get => _errors; }

    internal ManyErrors(Error head, Error tail)
    {
        _errors = new List<Error>(head.Count + tail.Count);

        if (head.Count == 1)
            _errors.Add(head);
        else if (head.Count > 1)
            _errors.AddRange(head.ToEnumerable());

        if (tail.Count == 1)
            _errors.Add(tail);
        else if (tail.Count > 1)
            _errors.AddRange(tail.ToEnumerable());
    }
    public ManyErrors(IEnumerable<Error> errors)
    {
        _errors = errors.SelectMany(x => x.ToEnumerable())
            .Where(x => !x.IsEmpty)
            .ToList();
    }

    [Pure] public override string Type => "many_errors";
    [Pure] public override string Message => ToFullArrayString();
    [Pure] public override string ToString() => ToFullArrayString();

    [Pure] private string ToFullArrayString()
    {
        var separator = Environment.NewLine;
        var lastIndex = _errors.Count - 1;

        var sb = new StringBuilder();
        for (int i = 0; i < _errors.Count; i++)
        {
            sb.Append(_errors[i]);
            if (i < lastIndex)
                sb.Append(separator);
        }

        return sb.ToString();
    }

    [Pure] public override int Count => _errors.Count;
    [Pure, JsonIgnore] public override bool IsEmpty => _errors.Count == 0;
    [Pure, JsonIgnore] public override bool IsExpected => _errors.All(static x => x.IsExpected);
    [Pure, JsonIgnore] public override bool IsExeptional => _errors.Any(static x => x.IsExeptional);

    [Pure] public override Exception ToException() => new AggregateException(_errors.Select(static x => x.ToException()));
    [Pure] public override IEnumerable<Error> ToEnumerable() => _errors;

    [Pure] public override int CompareTo(Error? other)
    {
        if (other is null)
            return -1;
        if (other.Count != _errors.Count)
            return _errors.Count.CompareTo(other.Count);
        
        var compareResult = 0;
        int i = 0;
        foreach (var otherErr in other.ToEnumerable())
        {
            var thisErr = _errors[i++];
            compareResult = thisErr.CompareTo(otherErr);
            if (compareResult != 0)
            {
                return compareResult;
            }
        }

        return compareResult;
    }
    [Pure] public override bool IsSimilarTo([NotNullWhen(true)] Error? other)
    {
        if (other is null)
        {
            return false;
        }
        if (_errors.Count != other.Count)
        {
            return false;
        }
        int i = 0;
        foreach (var otherErr in other.ToEnumerable())
        {
            var thisErr = _errors[i++];
            if (!thisErr.IsSimilarTo(otherErr))
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
        if (_errors.Count != other.Count)
        {
            return false;
        }
        int i = 0;
        foreach (var otherErr in other.ToEnumerable())
        {
            var thisErr = _errors[i++];
            if (!thisErr.Equals(otherErr))
            {
                return false;
            }
        }
        return true;
    }
    [Pure] public override int GetHashCode()
    {
        if (_errors.Count == 0)
            return 0;
        
        var hash = new HashCode();
        foreach (var err in _errors)
        {
            hash.Add(err);
        }
        return hash.ToHashCode();
    }

    [Pure] public IEnumerator<Error> GetEnumerator() => _errors.GetEnumerator();
    [Pure] IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[Serializable]
public sealed class ErrorException(string type, string message) : Exception(message)
{
    public string Type { get; } = type ?? nameof(ErrorException);
}
