using AspectInjector.Broker;
using Microsoft.Extensions.DependencyInjection;
using MinimalStepifiedSystem.Base;
using MinimalStepifiedSystem.Interfaces;
using MinimalStepifiedSystem.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace MinimalStepifiedSystem.Attributes;

[Aspect(Scope.Global)]
[Injection(typeof(StepifiedProcessAttribute))]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class StepifiedProcessAttribute : Attribute
{
    private const string InvokeMethodName = "Invoke";
    private const string InvokeAsyncMethodName = "InvokeAsync";
    private const string ContextIdentifier = "context";
    private const string CancellationTokenIdentifier = "token";
    private const string DelegateIdentifier = "delegate";

    private readonly DictionaryWithDefault<string, object> _cachedDelegates =
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        new(defaultValue: default);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    /// <summary>
    /// <para>• The order of types matters. They will be executed from top to bottom;</para>
    /// <para>• There must be an <see cref="IServiceProvider"/> instance registered with the help of <see cref="ServiceProviderSupplierAttribute"/>. This attribute should be applied inside of the target instance and to the constructor;</para>
    /// <para>• You will need to implement <see cref="IStep{TDelegate, TContext}"/> interface for each of these types with the correct signature;</para>
    /// <para>• You will need to register these types in your container.</para>
    /// </summary>
    [NotNull]
    public Type[] Steps { get; set; } = [];

    [Advice(Kind.Around, Targets = Target.Getter)]
    public object GetOrReturnCached([Argument(Source.ReturnType)] Type target,
                                    [Argument(Source.Name)] string name,
                                    [Argument(Source.Instance)] object targetClass,
                                    [Argument(Source.Type)] Type targetClassType,
                                    [Argument(Source.Triggers)] Attribute[] triggers)
    {
        var currentDelegateKey = GetKey(targetClassType.FullName!, name);
        if (_cachedDelegates[currentDelegateKey] is object cachedDelegate)
        {
            return cachedDelegate;
        }

        var serviceProviderSupplier = (IServiceProviderSupplier)targetClass;
        var trigger = triggers.OfType<StepifiedProcessAttribute>().First();
        var delegateMethod = target.GetMethod(InvokeMethodName);
        var contextParamType = delegateMethod!.GetParameters().FirstOrDefault()?.ParameterType;

        using var builder = new GenericStepifiedBuilder();

        foreach (var step in trigger.Steps)
        {
            UseStep(builder, serviceProviderSupplier.ServiceProvider, target, contextParamType!, step);
        }

        var item = builder.Build();
        var resultDelegate = Delegate.CreateDelegate(target, item.Target, item.Method);
        _cachedDelegates.Add(currentDelegateKey, resultDelegate);
        return resultDelegate;

        static GenericStepifiedBuilder UseStep(
            GenericStepifiedBuilder stepifiedBuilder, IServiceProvider serviceProvider,
            Type delegateType, Type contextType, Type stepType)
        {
            var stepInterface = typeof(IStep<,>);
            var stepGenericInterface = stepInterface.MakeGenericType(delegateType, contextType);
            if (stepGenericInterface.GetTypeInfo().IsAssignableFrom(stepType.GetTypeInfo()))
            {
                return stepifiedBuilder.Use(next =>
                    (object context, CancellationToken token = default) =>
                    {
                        var step = serviceProvider.GetRequiredService(stepType)
                            ?? throw new InvalidOperationException(
                                $"Couldn't get an instance of {stepType.FullName} from the container.");
                        var stepDelegate = GetStepDowncastedFunc(step);
                        return stepDelegate(context, token, Delegate.CreateDelegate(delegateType, next.Target, next.Method))!;
                    });
            }

            throw new InvalidOperationException($"Step should inherit IStep");
        }

        static Func<object, CancellationToken, Delegate, Task> GetStepDowncastedFunc(object step)
        {
            var stepType = step.GetType();
            var methodInfo = stepType.GetMethod(InvokeAsyncMethodName);

            var instance = Expression.Constant(step, stepType);
            var obj = Expression.Parameter(typeof(object), ContextIdentifier);
            var ct = Expression.Parameter(typeof(CancellationToken), CancellationTokenIdentifier);
            var del = Expression.Parameter(typeof(Delegate), DelegateIdentifier);

            var parametersInfo = methodInfo!.GetParameters();
            var convert1 = Expression.Convert(obj, parametersInfo.First().ParameterType);
            var convert2 = Expression.Convert(del, parametersInfo.ElementAtOrDefault(parametersInfo.Length - 2).ParameterType);

            var call = Expression.Call(instance, methodInfo, convert1, convert2, ct);
            return Expression.Lambda<Func<object, CancellationToken, Delegate, Task>>(call, obj, ct, del).Compile();
        }

        static string GetKey(string targetClassName, string memberName) =>
            $"{targetClassName}.{memberName}";
    }
}