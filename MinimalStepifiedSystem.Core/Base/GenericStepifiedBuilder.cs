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
        Delegate e = (object context) => Task.CompletedTask;

        for (var c = _components.Count - 1; c >= 0; c--)
            e = _components[c](e);

        return e;
    }

    public void Dispose()
    {
        _components?.Clear();
    }
}