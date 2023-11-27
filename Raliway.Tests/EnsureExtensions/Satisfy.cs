namespace Raliway.Tests.EnsureExtensions;

public class Satisfy
{
    [Fact]
    public void WhenRequirementWasSatisfied_ShouldBeSuccessful()
    {
        var result = Ensure.That(69)
            .Satisfies(i => i < 100)
            .Result();
        
        Assert.True(result.IsSuccess);
        Assert.Equal(69, result.Value);
    }
    [Fact]
    public void WhenRequirementWasNotSatisfied_ShouldBeFailureWithDefaultError()
    {
        var error = Error.New(Ensure.DefaultErrorType, "Value {69} does not satisfy the requirement.");
        var result = Ensure.That(69)
            .Satisfies(i => i > 100)
            .Result();
        
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void WhenAllRequirementsWasSatisfied_ShouldBeSuccessful()
    {
        var result = Ensure.That<string?>("69")
            .NotNull()
            .NotEmpty()
            .NotWhitespace()
            .Satisfies(s => s == "69")
            .Result();
        
        Assert.True(result.IsSuccess);
        Assert.Equal("69", result.Value);
    }

    [Fact]
    public void WhenAnyRequirementWasNotSatisfied_ShouldBeFailureWithFirstError()
    {
        var error = Error.New(Ensure.DefaultErrorType, "Value {(string?)\"   \"} is empty or consists exclusively of white-space characters.");
        var result = Ensure.That((string?)"   ")
            .NotNull()
            .NotEmpty()
            .NotWhitespace()
            .Satisfies(s => s == "69")
            .Result();
        
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
}
