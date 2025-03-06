// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Design.FunctionalTests.ReverseEngineering
{
    public class XuGuDbE2EFixture
    {
        public XuGuDbE2EFixture()
        {
            XuGuDbTestStore.CreateDatabase(
                "XuGuDbReverseEngineerTestE2E",
                scriptPath: "ReverseEngineering/E2E.sql",
                nonMasterScript: false,
                recreateIfAlreadyExists: true);
        }
    }
}
