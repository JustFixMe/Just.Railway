namespace Just.Railway;

public static partial class ResultExtensions
{
    #region Match (with fallback)
    
    public static T Match<T>(this in Result<T> result, Func<Error, T> fallback)
    {
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => fallback(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    
    public static async Task<T> Match<T>(this Result<T> result, Func<Error, Task<T>> fallback)
    {
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => await fallback(result.Error!).ConfigureAwait(false),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    public static async Task<T> Match<T>(this Task<Result<T>> resultTask, Func<Error, T> fallback)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => fallback(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    public static async Task<T> Match<T>(this Task<Result<T>> resultTask, Func<Error, Task<T>> fallback)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => await fallback(result.Error!).ConfigureAwait(false),
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    public static async ValueTask<T> Match<T>(this Result<T> result, Func<Error, ValueTask<T>> fallback)
    {
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => await fallback(result.Error!).ConfigureAwait(false),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    public static async ValueTask<T> Match<T>(this ValueTask<Result<T>> resultTask, Func<Error, T> fallback)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => fallback(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    public static async ValueTask<T> Match<T>(this ValueTask<Result<T>> resultTask, Func<Error, ValueTask<T>> fallback)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Value,
            ResultState.Error => await fallback(result.Error!).ConfigureAwait(false),
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion
    
    #region Merge

    public static Result Merge(this IEnumerable<Result> results)
    {
        List<Error>? errors = null;
        bool hasErrors = false;

        foreach (var result in results.OrderBy(x => x.State))
        {
            switch (result.State)
            {
                case ResultState.Error:
                    hasErrors = true;
                    errors ??= new(4);
                    errors.Add(result.Error!);
                    break;

                case ResultState.Success:
                    if (hasErrors) goto afterLoop;
                    break;
                    
                default: throw new ResultNotInitializedException(nameof(results));
            }
        }
        afterLoop:
        return hasErrors
            ? new(new ManyErrors(errors!))
            : new(null);
    }
    public static async Task<Result> Merge(this IEnumerable<Task<Result>> tasks)
    {
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Merge();
    }

    public static Result<IEnumerable<T>> Merge<T>(this IEnumerable<Result<T>> results)
    {
        List<T>? values = null;
        List<Error>? errors = null;
        bool hasErrors = false;

        foreach (var result in results.OrderBy(x => x.State))
        {
            switch (result.State)
            {
                case ResultState.Error:
                    hasErrors = true;
                    errors ??= new(4);
                    errors.Add(result.Error!);
                    break;

                case ResultState.Success:
                    if (hasErrors) goto afterLoop;
                    values ??= new(4);
                    values.Add(result.Value);
                    break;
                    
                default: throw new ResultNotInitializedException(nameof(results));
            }
        }
        afterLoop:
        return hasErrors
            ? new(new ManyErrors(errors!))
            : new((IEnumerable<T>?)values ?? Array.Empty<T>());
    }
    public static async Task<Result<IEnumerable<T>>> Merge<T>(this IEnumerable<Task<Result<T>>> tasks)
    {
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Merge();
    }

    #endregion
}
