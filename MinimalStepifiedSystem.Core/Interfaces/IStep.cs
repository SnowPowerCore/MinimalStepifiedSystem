namespace MinimalStepifiedSystem.Interfaces;

/// <summary>
/// A unit of logic with its own dependencies. Its context can be any object (define it in the custom delegate). Its delegate represents the next delegate in the chain.
/// </summary>
/// <typeparam name="TDelegate">Controllable call to the next delegate.</typeparam>
/// <typeparam name="TContext">Context object that may contain necessary data between different steps.</typeparam>
public interface IStep<TDelegate, TContext> where TDelegate : Delegate
                                            where TContext : class
{
    Task InvokeAsync(TContext context, TDelegate next, CancellationToken token = default);
}