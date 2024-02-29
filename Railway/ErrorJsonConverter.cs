using System.Collections.Immutable;

namespace Just.Railway;

public sealed class ErrorJsonConverter : JsonConverter<Error>
{
    public override Error? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartObject => ToExpectedError(ReadOne(ref reader)),
            JsonTokenType.StartArray => ReadMany(ref reader),
            JsonTokenType.None => null,
            JsonTokenType.Null => null,
            _ => throw new JsonException("Unexpected JSON token.")
        };
    }

    public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
    {
        if (value is ManyErrors manyErrors)
        {
            writer.WriteStartArray();
            foreach (var err in manyErrors)
            {
                WriteOne(writer, err);
            }
            writer.WriteEndArray();
        }
        else
        {
            WriteOne(writer, value);
        }
    }

    internal static ManyErrors ReadMany(ref Utf8JsonReader reader)
    {
        var errors = ImmutableArray.CreateBuilder<Error>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                errors.Add(ToExpectedError(ReadOne(ref reader)));
            }
        }
        return new ManyErrors(errors.ToImmutable());
    }
    internal static ExpectedError ToExpectedError(in (string Type, string Message, ImmutableDictionary<string, string> ExtensionData) errorInfo)
        => new(errorInfo.Type, errorInfo.Message) { ExtensionData = errorInfo.ExtensionData };
    internal static (string Type, string Message, ImmutableDictionary<string, string> ExtensionData) ReadOne(ref Utf8JsonReader reader)
    {
        ImmutableDictionary<string, string>.Builder? extensionData = null;
        string type = "error";
        string message = "";
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                {
                    var propname = reader.GetString();
                    reader.Read();

                    if (reader.TokenType == JsonTokenType.Null)
                        break;

                    while (reader.TokenType == JsonTokenType.Comment) reader.Read();

                    if (!(reader.TokenType == JsonTokenType.String))
                        throw new JsonException("Unable to deserialize Error type.");
                    
                    var propvalue = reader.GetString();
                    if (string.IsNullOrEmpty(propvalue))
                        break;

                    if (propname == "type" || string.Equals(propname, "type", StringComparison.InvariantCultureIgnoreCase))
                    {
                        type = propvalue;
                    }
                    else if (propname == "msg" || string.Equals(propname, "msg", StringComparison.InvariantCultureIgnoreCase))
                    {
                        message = propvalue;
                    }
                    else if (!string.IsNullOrEmpty(propname))
                    {
                        extensionData ??= ImmutableDictionary.CreateBuilder<string, string>();
                        extensionData.Add(propname, propvalue);
                    }

                    break;
                }
                case JsonTokenType.Comment: break;
                case JsonTokenType.EndObject: goto endLoop;
                default: throw new JsonException("Unable to deserialize Error type.");
            }
        }
        endLoop:
        return (type, message, extensionData?.ToImmutable() ?? ImmutableDictionary<string, string>.Empty);
    }
    internal static void WriteOne(Utf8JsonWriter writer, Error value)
    {
        writer.WriteStartObject();

        writer.WriteString("type", value.Type);
        writer.WriteString("msg", value.Message);

        if (value.ExtensionData?.Count > 0)
        {
            foreach (var (key, val) in value.ExtensionData)
            {
                writer.WriteString(key, val);
            }
        }

        writer.WriteEndObject();
    }
}

public sealed class ExpectedErrorJsonConverter : JsonConverter<ExpectedError>
{
    public override ExpectedError? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartObject => ErrorJsonConverter.ToExpectedError(ErrorJsonConverter.ReadOne(ref reader)),
            JsonTokenType.None => null,
            JsonTokenType.Null => null,
            _ => throw new JsonException("Unexpected JSON token.")
        };
    }

    public override void Write(Utf8JsonWriter writer, ExpectedError value, JsonSerializerOptions options)
    {
        ErrorJsonConverter.WriteOne(writer, value);
    }
}

public sealed class ExceptionalErrorJsonConverter : JsonConverter<ExceptionalError>
{
    public override ExceptionalError? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartObject => ToExceptionalError(ErrorJsonConverter.ReadOne(ref reader)),
            JsonTokenType.None => null,
            JsonTokenType.Null => null,
            _ => throw new JsonException("Unexpected JSON token.")
        };
    }

    public override void Write(Utf8JsonWriter writer, ExceptionalError value, JsonSerializerOptions options)
    {
        ErrorJsonConverter.WriteOne(writer, value);
    }

    private static ExceptionalError ToExceptionalError(in (string Type, string Message, ImmutableDictionary<string, string> ExtensionData) errorInfo)
        => new(errorInfo.Type, errorInfo.Message) { ExtensionData = errorInfo.ExtensionData };
}

public sealed class ManyErrorsJsonConverter : JsonConverter<ManyErrors>
{
    public override ManyErrors? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.StartArray => ErrorJsonConverter.ReadMany(ref reader),
            JsonTokenType.None => null,
            JsonTokenType.Null => null,
            _ => throw new JsonException("Unexpected JSON token.")
        };
    }

    public override void Write(Utf8JsonWriter writer, ManyErrors value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var err in value)
        {
            ErrorJsonConverter.WriteOne(writer, err);
        }
        writer.WriteEndArray();
    }
}
