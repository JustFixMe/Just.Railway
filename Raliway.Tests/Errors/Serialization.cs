using System.Collections.Immutable;

namespace Railway.Tests.Errors;

public class Serialization
{
    [Fact]
    public void WhenSerializingManyErrors()
    {
        // Given
        Error many_errors = new ManyErrors(new Error[]{
            new ExpectedError("err1", "msg1"){
                ExtensionData = ImmutableDictionary<string, string>.Empty
                    .Add("ext", "ext_value"),
            },
            new ExceptionalError(new Exception("msg2")),
        });
        // When
        var result = JsonSerializer.Serialize(many_errors);
        // Then
        Assert.Equal(
            expected: "[{\"$$err\":0,\"Type\":\"err1\",\"Message\":\"msg1\",\"ExtensionData\":{\"ext\":\"ext_value\"}},{\"$$err\":1,\"Type\":\"Exception\",\"Message\":\"msg2\"}]",
            result);
    }

    [Fact]
    public void WhenDeserializingManyErrors()
    {
        // Given
        var json = "[{\"$$err\":0,\"Type\":\"err1\",\"Message\":\"msg1\",\"ExtensionData\":{\"ext1\":\"ext_value1\",\"ext2\":\"ext_value2\"}},{\"$$err\":1,\"Type\":\"Exception\",\"Message\":\"msg2\"}]";
        // When
        var result = JsonSerializer.Deserialize<Error[]>(json);
        // Then
        Assert.True(result?.Length == 2);
        Assert.Equal(
            expected: new ManyErrors(new Error[]{
                new ExpectedError("err1", "msg1"),
                new ExceptionalError(new Exception("msg2")),
            }),
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
