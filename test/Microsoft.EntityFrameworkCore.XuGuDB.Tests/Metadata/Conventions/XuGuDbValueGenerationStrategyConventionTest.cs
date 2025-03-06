// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Metadata.Conventions
{
    public class XuGuDbValueGenerationStrategyConventionTest
    {
        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used()
        {
            var model = XuGuDbTestHelpers.Instance.CreateConventionBuilder().Model;

            Assert.Equal(1, model.GetAnnotations().Count());

            Assert.Equal(XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy, model.GetAnnotations().Single().Name);
            Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, model.GetAnnotations().Single().Value);
        }

        [Fact]
        public void Annotations_are_added_when_conventional_model_builder_is_used_with_sequences()
        {
            var model = XuGuDbTestHelpers.Instance.CreateConventionBuilder()
                .ForXuGuDbUseSequenceHiLo()
                .Model;

            var annotations = model.GetAnnotations().OrderBy(a => a.Name);
            Assert.Equal(3, annotations.Count());

            Assert.Equal(XuGuDbFullAnnotationNames.Instance.HiLoSequenceName, annotations.ElementAt(0).Name);
            Assert.Equal(XuGuDbModelAnnotations.DefaultHiLoSequenceName, annotations.ElementAt(0).Value);

            Assert.Equal(
                XuGuDbFullAnnotationNames.Instance.SequencePrefix +
                "." +
                XuGuDbModelAnnotations.DefaultHiLoSequenceName,
                annotations.ElementAt(1).Name);
            Assert.NotNull(annotations.ElementAt(1).Value);

            Assert.Equal(XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy, annotations.ElementAt(2).Name);
            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, annotations.ElementAt(2).Value);
        }
    }
}
