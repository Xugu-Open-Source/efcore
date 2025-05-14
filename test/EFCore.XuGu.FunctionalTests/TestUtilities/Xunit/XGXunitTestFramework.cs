// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit
{
    public class XGXunitTestFramework : XunitTestFramework
    {
        public XGXunitTestFramework(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
            => new XGXunitTestFrameworkDiscoverer(assemblyInfo, SourceInformationProvider, DiagnosticMessageSink);
    }
}
