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
    private readonly DictionaryWithDefault<string, Delegate> _cachedDelegates =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new(defaultValue: default);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    /// <summary>
    /// <para>• The order of types matters. They will be executed from top to bottom;</para>
    /// <para>• There must be an <see cref="IServiceProvider"/> instance registered with the help of <see cref="ServiceProviderSupplierAttribute"/>. This attribute should be applied inside of the target instance and to the constructor;</para>
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
        var factoryType = Type.GetType(factoryTypeName, throwOnError: true)!;
        var createMethod = factoryType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)!;
        var serviceProvider = ServiceProviderSupplier.Instance!.GetServiceProvider()!;
        var resultDelegate = createMethod.Invoke(default, [serviceProvider]);
        _cachedDelegates.Add(currentDelegateKey, (Delegate)resultDelegate!);
        return resultDelegate!;
    }

    static string GetKey(string targetClassName, string memberName) =>
        $"{targetClassName}.{memberName}";
}