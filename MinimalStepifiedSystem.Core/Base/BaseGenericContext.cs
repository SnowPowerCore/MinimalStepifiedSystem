using MinimalStepifiedSystem.Utils;

namespace MinimalStepifiedSystem.Base;

public class BaseGenericContext
{
    private DictionaryWithDefault<string, object> Data { get; } = new(defaultValue: false);

    public T? GetFromData<T>(string key) =>
        Data.TryGetValue(key, out var item) ? (T)item : default;

    public void SetDataWith(string key, object data) =>
        Data[key] = data;
}