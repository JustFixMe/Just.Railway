namespace Just.Railway;

public static class Try
{
    public static Result Run(Action action)
    {
        try
        {
            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }
    public static async Task<Result> Run(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }
    public static async ValueTask<Result> Run(Func<ValueTask> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex);
        }
    }
    public static Result<T> Run<T>(Func<T> func)
    {
        try
        {
            return Result.Success(func());
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex);
        }
    }
    public static async Task<Result<T>> Run<T>(Func<Task<T>> func)
    {
        try
        {
            return Result.Success(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex);
        }
    }
    public static async ValueTask<Result<T>> Run<T>(Func<ValueTask<T>> func)
    {
        try
        {
            return Result.Success(await func().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex);
        }
    }

    public static Result<T> Run<T>(Func<Result<T>> func)
    {
        try
        {
            return func();
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex);
        }
    }
    public static async Task<Result<T>> Run<T>(Func<Task<Result<T>>> func)
    {
        try
        {
            return await func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex);
        }
    }
    public static async ValueTask<Result<T>> Run<T>(Func<ValueTask<Result<T>>> func)
    {
        try
        {
            return await func().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(ex);
        }
    }
}
