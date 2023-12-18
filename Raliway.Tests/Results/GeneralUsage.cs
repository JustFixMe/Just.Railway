namespace Raliway.Tests.Results;

public class GeneralUsage
{
    [Fact]
    public void ChainedResultExtensions_WhenThereIsNoError()
    {
        // Given

        // When
        var result = Result.Success()
            .Append(() => Result.Success(1))
            .Append("test")
            .Map((i, s) => $"{s}_{i}")
            .Append("some")
            .Bind((s1, s2) => Result.Success(string.Join(';', s1, s2)))
            .Match(
                onSuccess: s => s.ToUpper(),
                onFailure: _ =>
                {
                    Assert.Fail();
                    return "";
                }
            );
        
        Assert.Equal("TEST_1;SOME", result);
    }

    [Fact]
    public void ChainedResultExtensions_WhenThereIsAnError()
    {
        // Given
        var error = Error.New("test");
        
        // When

        var result = Result.Success()
            .Append(() => Result.Failure<int>(error))
            .Append("test")
            .Map((i, s) =>
            {
                Assert.Fail();
                return "";
            })
            .Append("some")
            .Bind((s1, s2) => 
            {
                Assert.Fail();
                return Result.Success("");
            })
            .Match(
                onSuccess: _ => 
                {
                    Assert.Fail();
                    return "";
                },
                onFailure: err =>
                {
                    Assert.Equal(error, err);
                    return "satisfied";
                }
            );

        // Then
        Assert.Equal("satisfied", result);
    }

    [Fact]
    public async Task ChainedResultAsyncExtensions_WhenThereIsNoError()
    {
        // Given

        // When
        var result = await Result.Success()
            .Append(() => ValueTask.FromResult(Result.Success(1)))
            .Append("test")
            .Map((i, s) => $"{s}_{i}")
            .Append("some")
            .Bind(async (s1, s2) => await ValueTask.FromResult(Result.Success(string.Join(';', s1, s2))))
            .Match(
                onSuccess: s => s.ToUpper(),
                onFailure: _ =>
                {
                    Assert.Fail();
                    return "";
                }
            );
        
        Assert.Equal("TEST_1;SOME", result);
    }

    [Fact]
    public async Task ChainedResultAsyncExtensions_WhenThereIsAnError()
    {
        // Given
        var error = Error.New("test");
        
        // When

        var result = await Result.Success()
            .Append(() => Task.FromResult(Result.Failure<int>(error)))
            .Append("test")
            .Map((i, s) =>
            {
                Assert.Fail();
                return "";
            })
            .Append("some")
            .Bind(async (s1, s2) => 
            {
                Assert.Fail();
                await Task.CompletedTask;
                return Result.Success("");
            })
            .Match(
                onSuccess: _ => 
                {
                    Assert.Fail();
                    return "";
                },
                onFailure: err =>
                {
                    Assert.Equal(error, err);
                    return "satisfied";
                }
            );

        // Then
        Assert.Equal("satisfied", result);
    }

    [Fact]
    public void RecoverResultFromFailureState()
    {
        // Given
        Result<string> failed = new NotImplementedException();
        // When
        var result = failed.TryRecover(err => 
        {
            Assert.IsType<NotImplementedException>(err.ToException());

            if (err.Type == "System.NotImplementedException")
                return "recovered";

            Assert.Fail();
            return "";
        });
        // Then
        Assert.True(result.IsSuccess);
        Assert.Equal("recovered", result.Value);
    }

    [Fact]
    public void WhenCanNotRecoverResultFromFailureState()
    {
        // Given
        var error = Error.New("test");
        Result<string> failed = new NotImplementedException();
        // When
        var result = failed.TryRecover(err =>
        {
            if (err.Type == "System.NotImplementedException")
                return error;
            
            Assert.Fail();
            return "";
        });
        // Then
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
}
