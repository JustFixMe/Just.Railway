namespace Raliway.Tests.Results;

public class Combine
{
    [Fact]
    public void TwoResultCombination_WhenThereIsAnError()
    {
        // Given
        var result1 = Result.Success(1);
        var result2 = Result.Failure(Error.New("some error"));
        // When
        var result = Result.Combine(result1, result2);
        // Then
        Assert.True(result.IsFailure);
        Assert.Equal(result2.Error, result.Error);
    }
    [Fact]
    public void TwoResultCombination_WhenThereAreTwoErrors()
    {
        // Given
        var result1 = Result.Failure<byte>(Error.New("1"));
        var result2 = Result.Failure(Error.New("2"));
        // When
        var result = Result.Combine(result1, result2);
        // Then
        Assert.True(result.IsFailure);
        Assert.Equal(result1.Error + result2.Error, result.Error);
    }
    [Fact]
    public void TwoResultCombination_WhenThereIsNoError()
    {
        // Given
        var result1 = Result.Success(1);
        var result2 = Result.Success(3.14);
        // When
        var result = Result.Combine(result1, result2);
        // Then
        Assert.True(result.IsSuccess);
    }
    [Fact]
    public void ThreeResultCombination_WhenThereIsAnError()
    {
        // Given
        var result1 = Result.Success(1);
        var result2 = Result.Success(3.14);
        var result3 = Result.Failure(Error.New("some error"));
        // When
        Result<(int, double)> result = Result.Combine(result1, result2, result3);
        // Then
        Assert.True(result.IsFailure);
        Assert.Equal(result3.Error, result.Error);
    }
    [Fact]
    public void ThreeResultCombination_WhenThereAreTwoErrors()
    {
        // Given
        var result1 = Result.Failure<int?>(Error.New("1"));
        var result2 = Result.Success(3.14);
        var result3 = Result.Failure(Error.New("3"));
        // When
        Result<(int?, double)> result = Result.Combine(result1, result2, result3);
        // Then
        Assert.True(result.IsFailure);
        Assert.Equal(result1.Error + result3.Error, result.Error);
    }
    [Fact]
    public void ThreeResultCombination_WhenThereIsNoError()
    {
        // Given
        var result1 = Result.Success(1);
        var result2 = Result.Success(3.14);
        var result3 = Result.Success();
        // When
        var result = Result.Combine(result1, result2, result3);
        // Then
        Assert.True(result.IsSuccess);
    }
}
