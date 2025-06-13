using AspectInjector.Broker;
using MinimalStepifiedSystem.Core.Utils;
using MinimalStepifiedSystem.Interfaces;
using MinimalStepifiedSystem.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace MinimalStepifiedSystem.Attributes;

[Aspect(Scope.Global)]
[Injection(typeof(StepifiedProcessAttribute))]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class StepifiedProcessAttribute : Attribute
{
    private const string FactoryMethodName = "Create";

    private static readonly DictionaryWithDefault<string, Delegate> _cachedDelegates =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new(defaultValue: default);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    private static readonly DictionaryWithDefault<string, Func<IServiceProvider, Delegate>> _factoryDelegates =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new(defaultValue: default);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    /// <summary>
    /// <para>• The order of types matters. They will be executed from top to bottom;</para>
    /// <para>• There must be an <see cref="IServiceProvider"/> instance registered;</para>
    /// <para>• You will need to implement <see cref="IStep{TDelegate, TContext, TReturn}"/> interface for each of these types with the correct signature;</para>
    /// <para>• You will need to register these types in your container.</para>
    /// </summary>
    [NotNull]
    public Type[] Steps { get; set; } = [];

    [Advice(Kind.Around, Targets = Target.Getter)]
    public object GetOrReturnCached([Argument(Source.ReturnType)] Type target,
                                    [Argument(Source.Name)] string name,
                                    [Argument(Source.Type)] Type targetClassType)
    {
        var currentDelegateKey = GetKey(targetClassType.FullName!, name);
        if (_cachedDelegates[currentDelegateKey] is object cachedDelegate)
        {
            return cachedDelegate;
        }

        // Use the generated factory by convention: {ContainingClass}_{PropertyName}_StepifiedFactory.Create(IServiceProvider)
        var factoryTypeName = $"{target.Namespace}.{targetClassType.Name}_{name}_StepifiedFactory, {target.Assembly.FullName}";
        if (!_factoryDelegates.TryGetValue(factoryTypeName, out var factoryDelegate))
        {
            var factoryType = Type.GetType(factoryTypeName, throwOnError: true)!;
            var createMethod = factoryType.GetMethod(FactoryMethodName, BindingFlags.Public | BindingFlags.Static)!;
            // The generated Create method always has signature: static {DelegateType} Create(IServiceProvider)
            factoryDelegate = (Func<IServiceProvider, Delegate>)Delegate.CreateDelegate(typeof(Func<IServiceProvider, Delegate>), createMethod);
            _factoryDelegates[factoryTypeName] = factoryDelegate;
        }
        var serviceProvider = ServiceProviderSupplier.Instance!.GetServiceProvider()!;
        var resultDelegate = factoryDelegate(serviceProvider);
        _cachedDelegates.Add(currentDelegateKey, resultDelegate);
        return resultDelegate!;
    }

    static string GetKey(string targetClassName, string memberName) =>
        $"{targetClassName}.{memberName}";
}