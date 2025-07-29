// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Sql.Internal
{
    public class XGQuerySqlGeneratorFactory : QuerySqlGeneratorFactoryBase
    {
        private readonly IXGOptions _options;

        public XGQuerySqlGeneratorFactory([NotNull] QuerySqlGeneratorDependencies dependencies, IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        public override IQuerySqlGenerator CreateDefault(SelectExpression selectExpression)
            => new XGQuerySqlGenerator(
                Dependencies,
                Check.NotNull(selectExpression, nameof(selectExpression)), _options);
    }
}
