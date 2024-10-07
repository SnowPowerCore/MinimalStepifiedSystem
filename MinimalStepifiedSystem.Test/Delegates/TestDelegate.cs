using MinimalStepifiedSystem.Test.Context;

namespace MinimalStepifiedSystem.Test.Delegates;

public delegate Task<TestContext> TestDelegate(TestContext context, CancellationToken token = default);