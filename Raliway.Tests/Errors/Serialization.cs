namespace Railway.Tests.Errors;

public class Serialization
{
    [Fact]
    public void WhenSerializingManyErrors()
    {
        // Given
        Error many_errors = new ManyErrors(
        [
            Error.New("err1", "msg1", new KeyValuePair<string, string>[]
            {
                new("ext", "ext_value"),
            }),
            Error.New(new Exception("msg2")),
        ]);
        // When
        var result = JsonSerializer.Serialize(many_errors);
        // Then
        Assert.Equal(
            expected: "[{\"type\":\"err1\",\"msg\":\"msg1\",\"ext\":\"ext_value\"},{\"type\":\"System.Exception\",\"msg\":\"msg2\"}]",
            result);
    }

    [Fact]
    public void WhenDeserializingManyErrorsAsError()
    {
        // Given
        var json = "[{\"type\":\"err1\",\"msg\":\"msg1\",\"ext1\":\"ext_value1\",\"ext2\":\"ext_value2\"},{\"type\":\"System.Exception\",\"msg\":\"msg2\"}]";
        // When
        var result = JsonSerializer.Deserialize<Error>(json);
        // Then
        Assert.IsType<ManyErrors>(result);
        ManyErrors manyErrors = (ManyErrors)result;

        Assert.True(manyErrors.Count == 2);
        Assert.Equal(
            expected: Error.Many(
                Error.New("err1", "msg1"),
                Error.New(new Exception("msg2"))
            ).ToEnumerable(),
            manyErrors
        );
        Assert.Equal(
            expected: "ext_value1",
            manyErrors[0]["ext1"]);
        Assert.Equal(
            expected: "ext_value2",
            manyErrors[0]["ext2"]);
    }

    [Fact]
    public void WhenDeserializingManyErrorsAsManyErrors()
    {
        // Given
        var json = "[{\"type\":\"err1\",\"msg\":\"msg1\",\"ext1\":\"ext_value1\",\"ext2\":\"ext_value2\"},{\"type\":\"System.Exception\",\"msg\":\"msg2\"}]";
        // When
        var result = JsonSerializer.Deserialize<ManyErrors>(json);
        // Then
        Assert.NotNull(result);
        Assert.True(result.Count == 2);
        Assert.Equal(
            expected: Error.Many(
                Error.New("err1", "msg1"),
                Error.New(new Exception("msg2"))
            ).ToEnumerable(),
            result
        );
        Assert.Equal(
            expected: "ext_value1",
            result[0]["ext1"]);
        Assert.Equal(
            expected: "ext_value2",
            result[0]["ext2"]);
    }
}
