// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGParametersBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
    {
        private readonly RelationalParameterBasedSqlProcessorDependencies _dependencies;
        [NotNull] private readonly IXGOptions _options;

        public XGParametersBasedSqlProcessorFactory(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            [NotNull] IXGOptions options)
        {
            _dependencies = dependencies;
            _options = options;
        }

        public virtual RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
            => new XGParametersBasedSqlProcessor(_dependencies, useRelationalNulls, _options);
    }
}
