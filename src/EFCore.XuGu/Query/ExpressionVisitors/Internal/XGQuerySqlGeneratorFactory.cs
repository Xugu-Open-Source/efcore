// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;

namespace EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
    {
        private readonly QuerySqlGeneratorDependencies _dependencies;
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly IXGOptions _options;

        public XGQuerySqlGeneratorFactory(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGOptions options)
        {
            _dependencies = dependencies;
            _sqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            _options = options;
        }

        public virtual QuerySqlGenerator Create()
            => new XGQuerySqlGenerator(_dependencies, _sqlExpressionFactory, _options);
    }
}
