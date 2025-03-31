using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit
{
    public class XGConditionalFactDiscoverer : ConditionalFactDiscoverer
    {
        public XGConditionalFactDiscoverer(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        protected override IXunitTestCase CreateTestCase(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
            => new XGConditionalFactTestCase(
                DiagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod);
    }
}
