namespace MinimalStepifiedSystem.Base;

internal sealed class GenericStepifiedBuilder : IDisposable
{
    private readonly List<Func<Delegate, Delegate>> _components = [];

    public GenericStepifiedBuilder Use(Func<Delegate, Delegate> step)
    {
        _components.Add(step);
        return this;
    }

    public Delegate Build()
    {
        Delegate e = (Func<object, CancellationToken, Task<object>>)(static (object context, CancellationToken token = default) =>
        {
            if (token != CancellationToken.None && token.IsCancellationRequested)
            {
                Console.WriteLine(
                    $"Operation was cancelled for the {context.GetType().FullName}. "
                    + "Please check steps which operate this context.");
                return Task.FromCanceled<object>(token);
            }
            return Task.FromResult(context);
        });

        for (var c = _components.Count - 1; c >= 0; c--)
            e = _components[c](e);

        return e;
    }

    public void Dispose()
    {
        _components?.Clear();
    }
}