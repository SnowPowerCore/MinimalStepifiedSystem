using MinimalStepifiedSystem.Test.Context;

namespace MinimalStepifiedSystem.Test.Delegates;

public delegate Task TestDelegate(TestContext context, CancellationToken token = default);