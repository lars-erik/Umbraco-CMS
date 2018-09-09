using NUnit.Framework;
using Umbraco.Core.Composing;
using Current = Umbraco.Web.Composing.Current;

namespace Our.Umbraco.Container.Castle.Tests
{
    [TestFixture]
    public class NPocoSqlTests : global::Umbraco.Tests.Persistence.NPocoTests.NPocoSqlTests
    {
        public override void PreSetUp()
        {
            Current.Container.RegisterInstance(Current.Container);
        }

    }
}
