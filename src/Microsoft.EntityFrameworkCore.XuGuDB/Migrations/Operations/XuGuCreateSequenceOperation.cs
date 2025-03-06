// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Migrations.Operations
{
    public class XuGuCreateSequenceOperation : CreateSequenceOperation
    {
        public virtual int? Cache { get; set; } = 1;
    }
}
