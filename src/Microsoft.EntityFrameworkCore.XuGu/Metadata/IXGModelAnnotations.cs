// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IXGModelAnnotations : IRelationalModelAnnotations
    {
        IReadOnlyList<IXGExtension> XGExtensions { get; }
        string DatabaseTemplate { get; }
        XGValueGenerationStrategy? ValueGenerationStrategy { get; }
        string HiLoSequenceName { get; }
        string HiLoSequenceSchema { get; }
    }
}
