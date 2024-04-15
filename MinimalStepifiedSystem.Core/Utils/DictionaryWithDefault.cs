using System.Diagnostics.CodeAnalysis;

namespace MinimalStepifiedSystem.Utils;

public class DictionaryWithDefault<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
    public TValue DefaultValue { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public DictionaryWithDefault() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


    public DictionaryWithDefault(TValue defaultValue) =>
        DefaultValue = defaultValue;

    public new TValue this[[NotNull] TKey key]
    {
        get => TryGetValue(key!, out var t) ? t : DefaultValue;
        set => base[key!] = value;
    }
}