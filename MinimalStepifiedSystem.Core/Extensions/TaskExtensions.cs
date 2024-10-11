namespace MinimalStepifiedSystem.Core.Extensions;

public static class TaskExtensions
{
    public static Func<TContext, CancellationToken, Task<TReturn>> ConvertFunc<TContext, TReturn>(Func<object, CancellationToken, Task<object>> func) =>
        async (arg, token) => (TReturn)await func(arg!, token);

    public static Func<object, TDelegate, CancellationToken, Task<object>> ConvertFuncObj<TContext, TDelegate, TOriginalReturn>(Func<TContext, TDelegate, CancellationToken, Task<TOriginalReturn>> func) =>
        async (arg, del, token) => await func((TContext)arg, del, token);
}