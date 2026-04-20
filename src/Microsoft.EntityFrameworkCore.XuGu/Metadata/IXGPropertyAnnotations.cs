// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IXGPropertyAnnotations : IRelationalPropertyAnnotations
    {

        XGValueGenerationStrategy? ValueGenerationStrategy { get; }
        string HiLoSequenceName { get; }
        string HiLoSequenceSchema { get; }
        ISequence FindHiLoSequence();
    }
}
