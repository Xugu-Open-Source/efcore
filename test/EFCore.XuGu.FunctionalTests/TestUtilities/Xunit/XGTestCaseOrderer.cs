using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit
{
    public class XGTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
            => testCases.OrderBy(c => c.TestMethod.Method.Name, StringComparer.OrdinalIgnoreCase);
    }
}
