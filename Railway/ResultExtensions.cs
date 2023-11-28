namespace Just.Railway;

public static partial class ResultExtensions
{
    #region Match<>

    [Pure]
    public static R Match<R>(this in Result result, Func<R> onSuccess, Func<Error, R> onFailure)
    {
        return result.State switch
        {
            ResultState.Success => onSuccess(),
            ResultState.Error => onFailure(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }

    [Pure]
    public static Task<R> Match<R>(this in Result result, Func<Task<R>> onSuccess, Func<Error, Task<R>> onFailure)
    {
        return result.State switch
        {
            ResultState.Success => onSuccess(),
            ResultState.Error => onFailure(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }

    [Pure] public static async Task<R> Match<R>(this Task<Result> resultTask, Func<R> onSuccess, Func<Error, R> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => onSuccess(),
            ResultState.Error => onFailure(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<R> Match<R>(this Task<Result> resultTask, Func<Task<R>> onSuccess, Func<Error, Task<R>> onFailure)
    {
        var result = await resultTask.ConfigureAwait(false);
        var matchTask = result.State switch
        {
            ResultState.Success => onSuccess(),
            ResultState.Error => onFailure(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
        return await matchTask.ConfigureAwait(false);
    }

    #endregion

    #region Map<>

    [Pure]
    public static Result<R> Map<R>(this in Result result, Func<R> mapping)
    {
        return result.State switch
        {
            ResultState.Success => mapping(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }

    [Pure]
    public static async Task<Result<R>> Map<R>(this Result result, Func<Task<R>> mapping)
    {
        return result.State switch
        {
            ResultState.Success => await mapping().ConfigureAwait(false),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }

    [Pure]
    public static async Task<Result<R>> Map<R>(this Task<Result> resultTask, Func<R> mapping)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => mapping(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    [Pure]
    public static async Task<Result<R>> Map<R>(this Task<Result> resultTask, Func<Task<R>> mapping)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => await mapping().ConfigureAwait(false),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion

    #region Bind<>

    [Pure]
    public static Result Bind(this in Result result, Func<Result> binding)
    {
        return result.State switch
        {
            ResultState.Success => binding(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static Task<Result> Bind(this in Result result, Func<Task<Result>> binding)
    {
        return result.State switch
        {
            ResultState.Success => binding(),
            ResultState.Error => Task.FromResult<Result>(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Result> binding)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => binding(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Task<Result>> binding)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => await binding().ConfigureAwait(false),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    [Pure]
    public static Result<R> Bind<R>(this in Result result, Func<Result<R>> binding)
    {
        return result.State switch
        {
            ResultState.Success => binding(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static Task<Result<R>> Bind<R>(this in Result result, Func<Task<Result<R>>> binding)
    {
        return result.State switch
        {
            ResultState.Success => binding(),
            ResultState.Error => Task.FromResult<Result<R>>(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<R>> Bind<R>(this Task<Result> resultTask, Func<Result<R>> binding)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => binding(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<Result<R>> Bind<R>(this Task<Result> resultTask, Func<Task<Result<R>>> binding)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => await binding().ConfigureAwait(false),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion

    #region Append

    #region <>

    [Pure] public static Result Append(this in Result result, Result next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new(null)
            : new(error);
    }

    #endregion

    #region <T>

    [Pure] public static Result<T> Append<T>(this in Result result, T value)
    {
        return result.State switch
        {
            ResultState.Success => value,
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure] public static Result<T> Append<T>(this in Result result, Result<T> next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new(next.Value)
            : new(error);
    }
    [Pure] public static Result<T> Append<T>(this in Result<T> result, Result next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new(result.Value)
            : new(error);
    }
    [Pure]
    public static Result<T> Append<T>(this in Result result, Func<Result<T>> next)
    {
        return result.State switch
        {
            ResultState.Success => next(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }

    [Pure]
    public static Task<Result<T>> Append<T>(this in Result result, Func<Task<Result<T>>> next)
    {
        return result.State switch
        {
            ResultState.Success => next(),
            ResultState.Error => Task.FromResult<Result<T>>(result.Error!),
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<T>> Append<T>(this Task<Result> resultTask, Func<Task<Result<T>>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => await next().ConfigureAwait(false),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<Result<T>> Append<T>(this Task<Result> resultTask, Func<Result<T>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => next(),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion

    #region <T1, T2>

    [Pure] public static Result<(T1, T2)> Append<T1, T2>(this in Result<T1> result, T2 value)
    {
        return result.State switch
        {
            ResultState.Success => (result.Value, value),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure] public static Result<(T1, T2)> Append<T1, T2>(this in Result<T1> result, Result<T2> next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new((result.Value, next.Value))
            : new(error);
    }
    [Pure] public static Result<(T1, T2)> Append<T1, T2>(this in Result<(T1, T2)> result, Result next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new(result.Value)
            : new(error);
    }
    [Pure]
    public static Result<(T1, T2)> Append<T1, T2>(this in Result<T1> result, Func<Result<T2>> next)
    {
        return result.State switch
        {
            ResultState.Success => result.Append(next()),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2)>> Append<T1, T2>(this Result<T1> result, Func<Task<Result<T2>>> next)
    {
        return result.State switch
        {
            ResultState.Success => result.Append(await next().ConfigureAwait(false)),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2)>> Append<T1, T2>(this Task<Result<T1>> resultTask, Func<Task<Result<T2>>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Append(await next().ConfigureAwait(false)),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2)>> Append<T1, T2>(this Task<Result<T1>> resultTask, Func<Result<T2>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Append(next()),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion

    #region <T1, T2, T3>

    [Pure] public static Result<(T1, T2, T3)> Append<T1, T2, T3>(this in Result<(T1, T2)> result, T3 value)
    {
        return result.State switch
        {
            ResultState.Success => (result.Value.Item1, result.Value.Item2, value),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure] public static Result<(T1, T2, T3)> Append<T1, T2, T3>(this in Result<(T1, T2)> result, Result<T3> next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new((result.Value.Item1, result.Value.Item2, next.Value))
            : new(error);
    }
    [Pure] public static Result<(T1, T2, T3)> Append<T1, T2, T3>(this in Result<(T1, T2, T3)> result, Result next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new(result.Value)
            : new(error);
    }
    [Pure]
    public static Result<(T1, T2, T3)> Append<T1, T2, T3>(this in Result<(T1, T2)> result, Func<Result<T3>> next)
    {
        return result.State switch
        {
            ResultState.Success => result.Append(next()),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3)>> Append<T1, T2, T3>(this Result<(T1, T2)> result, Func<Task<Result<T3>>> next)
    {
        return result.State switch
        {
            ResultState.Success => result.Append(await next().ConfigureAwait(false)),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3)>> Append<T1, T2, T3>(this Task<Result<(T1, T2)>> resultTask, Func<Task<Result<T3>>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Append(await next().ConfigureAwait(false)),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3)>> Append<T1, T2, T3>(this Task<Result<(T1, T2)>> resultTask, Func<Result<T3>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Append(next()),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion

    #region <T1, T2, T3, T4>

    [Pure] public static Result<(T1, T2, T3, T4)> Append<T1, T2, T3, T4>(this in Result<(T1, T2, T3)> result, T4 value)
    {
        return result.State switch
        {
            ResultState.Success => (result.Value.Item1, result.Value.Item2, result.Value.Item3, value),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure] public static Result<(T1, T2, T3, T4)> Append<T1, T2, T3, T4>(this in Result<(T1, T2, T3)> result, Result<T4> next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new((result.Value.Item1, result.Value.Item2, result.Value.Item3, next.Value))
            : new(error);
    }
    [Pure] public static Result<(T1, T2, T3, T4)> Append<T1, T2, T3, T4>(this in Result<(T1, T2, T3, T4)> result, Result next)
    {
        Error? error = null;

        if ((result.State & next.State) == ResultState.Bottom)
        {
            throw new ResultNotInitializedException(string.Join(';', GetBottom(result.State, next.State)));

            static IEnumerable<string> GetBottom(ResultState r1, ResultState r2)
            {
                if (r1 == ResultState.Bottom)
                    yield return nameof(result);
                if (r2 == ResultState.Bottom)
                    yield return nameof(next);
            }
        }
        
        if (result.IsFailure)
        {
            error += result.Error;
        }
        if (next.IsFailure)
        {
            error += next.Error;
        }
        return error is null
            ? new(result.Value)
            : new(error);
    }
    [Pure]
    public static Result<(T1, T2, T3, T4)> Append<T1, T2, T3, T4>(this in Result<(T1, T2, T3)> result, Func<Result<T4>> next)
    {
        return result.State switch
        {
            ResultState.Success => result.Append(next()),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3, T4)>> Append<T1, T2, T3, T4>(this Result<(T1, T2, T3)> result, Func<Task<Result<T4>>> next)
    {
        return result.State switch
        {
            ResultState.Success => result.Append(await next().ConfigureAwait(false)),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(result))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3, T4)>> Append<T1, T2, T3, T4>(this Task<Result<(T1, T2, T3)>> resultTask, Func<Task<Result<T4>>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Append(await next().ConfigureAwait(false)),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3, T4)>> Append<T1, T2, T3, T4>(this Task<Result<(T1, T2, T3)>> resultTask, Func<Result<T4>> next)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.State switch
        {
            ResultState.Success => result.Append(next()),
            ResultState.Error => result.Error!,
            _ => throw new ResultNotInitializedException(nameof(resultTask))
        };
    }

    #endregion

    #endregion

    #region Tap
    [Pure]
    public static ref readonly Result Tap(this in Result result, Action? onSuccess = null, Action<Error>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke();
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }

        return ref result;
    }
    [Pure]
    public static async Task<Result> Tap(this Task<Result> resultTask, Action? onSuccess = null, Action<Error>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke();
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }

        return result;
    }
    [Pure]
    public static async Task<Result> Tap(this Result result, Func<Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke().ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }

        return result;
    }
    [Pure]
    public static async Task<Result> Tap(this Task<Result> resultTask, Func<Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke().ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }

        return result;
    }

    [Pure]
    public static ref readonly Result<T> Tap<T>(this in Result<T> result, Action<T>? onSuccess = null, Action<Error>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value!);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return ref result;
    }
    [Pure]
    public static async Task<Result<T>> Tap<T>(this Task<Result<T>> resultTask, Action<T>? onSuccess = null, Action<Error>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value!);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<T>> Tap<T>(this Result<T> result, Func<T, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value!).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<T>> Tap<T>(this Task<Result<T>> resultTask, Func<T, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value!).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }

    [Pure]
    public static ref readonly Result<(T1, T2)> Tap<T1, T2>(this in Result<(T1, T2)> result, Action<T1, T2>? onSuccess = null, Action<Error>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value.Item1, result.Value.Item2);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return ref result;
    }
    [Pure]
    public static async Task<Result<(T1, T2)>> Tap<T1, T2>(this Task<Result<(T1, T2)>> resultTask, Action<T1, T2>? onSuccess = null, Action<Error>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value.Item1, result.Value.Item2);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<(T1, T2)>> Tap<T1, T2>(this Result<(T1, T2)> result, Func<T1, T2, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value.Item1, result.Value.Item2).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<(T1, T2)>> Tap<T1, T2>(this Task<Result<(T1, T2)>> resultTask, Func<T1, T2, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value.Item1, result.Value.Item2).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }

    [Pure]
    public static ref readonly Result<(T1, T2, T3)> Tap<T1, T2, T3>(this in Result<(T1, T2, T3)> result, Action<T1, T2, T3>? onSuccess = null, Action<Error>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return ref result;
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3)>> Tap<T1, T2, T3>(this Task<Result<(T1, T2, T3)>> resultTask, Action<T1, T2, T3>? onSuccess = null, Action<Error>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3)>> Tap<T1, T2, T3>(this Result<(T1, T2, T3)> result, Func<T1, T2, T3, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3)>> Tap<T1, T2, T3>(this Task<Result<(T1, T2, T3)>> resultTask, Func<T1, T2, T3, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }

    [Pure]
    public static ref readonly Result<(T1, T2, T3, T4)> Tap<T1, T2, T3, T4>(this in Result<(T1, T2, T3, T4)> result, Action<T1, T2, T3, T4>? onSuccess = null, Action<Error>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return ref result;
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3, T4)>> Tap<T1, T2, T3, T4>(this Task<Result<(T1, T2, T3, T4)>> resultTask, Action<T1, T2, T3, T4>? onSuccess = null, Action<Error>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                onSuccess?.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4);
                break;
            case ResultState.Error:
                onFailure?.Invoke(result.Error!);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3, T4)>> Tap<T1, T2, T3, T4>(this Result<(T1, T2, T3, T4)> result, Func<T1, T2, T3, T4, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(result));
        }
        return result;
    }
    [Pure]
    public static async Task<Result<(T1, T2, T3, T4)>> Tap<T1, T2, T3, T4>(this Task<Result<(T1, T2, T3, T4)>> resultTask, Func<T1, T2, T3, T4, Task>? onSuccess = null, Func<Error, Task>? onFailure = null)
    {
        var result = await resultTask.ConfigureAwait(false);
        switch (result.State)
        {
            case ResultState.Success:
                if (onSuccess is not null)
                    await onSuccess.Invoke(result.Value.Item1, result.Value.Item2, result.Value.Item3, result.Value.Item4).ConfigureAwait(false);
                break;
            case ResultState.Error:
                if (onFailure is not null)
                    await onFailure.Invoke(result.Error!).ConfigureAwait(false);
                break;

            default: throw new ResultNotInitializedException(nameof(resultTask));
        }
        return result;
    }
#endregion

    #region Merge

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
