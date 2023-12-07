namespace Just.Railway;

public static partial class ResultExtensions
{
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
                    errors ??= [];
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
                    errors ??= [];
                    errors.Add(result.Error!);
                    break;

                case ResultState.Success:
                    if (hasErrors) goto afterLoop;
                    values ??= [];
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
