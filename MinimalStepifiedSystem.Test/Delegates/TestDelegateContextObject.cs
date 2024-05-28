namespace MinimalStepifiedSystem.Test.Delegates;

//It is possible to create a custom delegate with simple object. It doesn't have to be a specific type.
public delegate Task TestDelegateContextObject(object dumbObject, CancellationToken token = default);