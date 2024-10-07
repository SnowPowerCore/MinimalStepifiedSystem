namespace MinimalStepifiedSystem.Core.Extensions;

public static class TaskExtensions
{
    public static Func<object, CancellationToken, Task<T>> ConvertFunc<T>(Func<object, CancellationToken, Task<object>> func) =>
        (arg, token) => func(arg, token).ContinueWith(t => (T)t.Result, TaskContinuationOptions.OnlyOnRanToCompletion);

    public static Func<object, CancellationToken, Delegate, Task<object>> ConvertFuncObj<T>(Func<object, CancellationToken, Delegate, Task<T>> func) =>
        (arg, token, del) => func(arg, token, del).ContinueWith(t => (object)t.Result!, TaskContinuationOptions.OnlyOnRanToCompletion);
}