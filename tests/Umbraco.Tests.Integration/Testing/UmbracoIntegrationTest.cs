using NUnit.Framework;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Abstract class for integration tests
/// </summary>
/// <remarks>
///     This will use a Host Builder to boot and install Umbraco ready for use
/// </remarks>
public abstract class UmbracoIntegrationTest : UmbracoIntegrationFixtureBase
{
    [SetUp]
    public void Setup()
    {
        BuildAndStartHost();
    }
    
    // Should be removed in a major - only here to silence ValidatePackage.target. It used to be non-async, but should not be called by implementors - it was marked with [TearDown] and executed anyway. And NUnit supports async teardown (now).
    public void TearDownAsync() {}

    [TearDown]
    public async Task TearDownActualAsync()
    {
        await StopHost();
    }

    [TearDown]
    public override void TearDown()
    {
        ExecuteTearDownQueue();
    }

    [SetUp]
    public override void SetUp_Logging() => TestContext.Out.Write($"Start test {TestCount++}: {TestContext.CurrentContext.Test.Name}");

    [TearDown]
    public override void TearDown_Logging() => TestContext.Out.Write($"  {TestContext.CurrentContext.Result.Outcome.Status}");
}
