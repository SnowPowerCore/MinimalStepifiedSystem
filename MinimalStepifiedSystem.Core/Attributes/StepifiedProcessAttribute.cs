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

    private MethodInvoker? _convertFuncMethodInvoker;

    private readonly DictionaryWithDefault<string, object> _cachedDelegates =
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
        var returnParamType = delegateMethod!.ReturnParameter.ParameterType.GetGenericArguments().FirstOrDefault();

        using var builder = new GenericStepifiedBuilder();

        foreach (var step in trigger.Steps)
        {
            UseStep(builder, serviceProviderSupplier.ServiceProvider, target, contextParamType!, returnParamType!, step);
        }

        var item = builder.Build();
        _convertFuncMethodInvoker ??= MethodInvoker.Create(typeof(Core.Extensions.TaskExtensions)
            .GetMethod(nameof(Core.Extensions.TaskExtensions.ConvertFunc), BindingFlags.Static | BindingFlags.Public)!
            .MakeGenericMethod(returnParamType!));
        var convertedItem = (Delegate)_convertFuncMethodInvoker.Invoke(default, [item])!;
        var resultDelegate = Delegate.CreateDelegate(target, convertedItem.Target, convertedItem.Method);
        _cachedDelegates.Add(currentDelegateKey, resultDelegate);
        return resultDelegate;

        static GenericStepifiedBuilder UseStep(
            GenericStepifiedBuilder stepifiedBuilder, IServiceProvider serviceProvider,
            Type delegateType, Type contextType, Type returnType, Type stepType)
        {
            var stepInterface = typeof(IStep<,,>);
            var stepGenericInterface = stepInterface.MakeGenericType(delegateType, contextType, returnType);
            if (stepGenericInterface.GetTypeInfo().IsAssignableFrom(stepType.GetTypeInfo()))
            {
                var convertFuncMethodInvoker = default(MethodInvoker);
                var convertFuncObjMethodInvoker = default(MethodInvoker);
                var convertFuncFinalMethodInvoker = default(MethodInvoker);
                return stepifiedBuilder.Use(next =>
                    (Func<object, CancellationToken, Task<object>>)((object context, CancellationToken token = default) =>
                    {
                        var step = serviceProvider.GetRequiredService(stepType)
                            ?? throw new InvalidOperationException(
                                $"Couldn't get an instance of {stepType.FullName} from the container.");
                        var stepDelegate = GetStepDowncastedFunc(step, returnType);
                        convertFuncObjMethodInvoker ??= MethodInvoker.Create(typeof(Core.Extensions.TaskExtensions)
                            .GetMethod(nameof(Core.Extensions.TaskExtensions.ConvertFuncObj), BindingFlags.Static | BindingFlags.Public)!
                            .MakeGenericMethod(returnType));
                        var convertedItem = convertFuncObjMethodInvoker.Invoke(default, [stepDelegate]);
                        convertFuncMethodInvoker ??= MethodInvoker.Create(typeof(Core.Extensions.TaskExtensions)
                            .GetMethod(nameof(Core.Extensions.TaskExtensions.ConvertFunc), BindingFlags.Static | BindingFlags.Public)!
                            .MakeGenericMethod(returnType));
                        var nextDelConverted = (Delegate)convertFuncMethodInvoker.Invoke(default, [next])!;
                        convertFuncFinalMethodInvoker ??= MethodInvoker.Create(convertedItem!.GetType()
                            .GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public)!);
                        return (Task<object>)convertFuncFinalMethodInvoker
                            .Invoke(convertedItem, [context, token, Delegate.CreateDelegate(delegateType, nextDelConverted.Target, nextDelConverted.Method)])!;
                    }));
            }

            throw new InvalidOperationException($"Step should inherit IStep");
        }

        static Delegate GetStepDowncastedFunc(object step, Type returnType)
        {
            var stepType = step.GetType();
            var methodInfo = stepType.GetMethod(InvokeAsyncMethodName);

            var instance = Expression.Constant(step, stepType);
            var obj = Expression.Parameter(typeof(object), ContextIdentifier);
            var ct = Expression.Parameter(typeof(CancellationToken), CancellationTokenIdentifier);
            var del = Expression.Parameter(typeof(Delegate), DelegateIdentifier);

            var parametersInfo = methodInfo!.GetParameters();
            var convert1 = Expression.Convert(obj, parametersInfo.First().ParameterType);
            var convert2 = Expression.Convert(del, parametersInfo.ElementAtOrDefault(parametersInfo.Length - 2)!.ParameterType);

            var call = Expression.Call(instance, methodInfo, convert1, convert2, ct);
            var taskType = typeof(Task<>).MakeGenericType(returnType);
            var fullFuncType = typeof(Func<,,,>)
                .MakeGenericType(typeof(object), typeof(CancellationToken), typeof(Delegate), taskType);
            return Expression.Lambda(fullFuncType, call, obj, ct, del).Compile();
        }

        static string GetKey(string targetClassName, string memberName) =>
            $"{targetClassName}.{memberName}";
    }
}