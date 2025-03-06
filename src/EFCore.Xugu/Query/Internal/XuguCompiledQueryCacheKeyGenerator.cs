// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
    {
        private readonly IXuguConnection _xuguConnection;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguCompiledQueryCacheKeyGenerator(
            CompiledQueryCacheKeyGeneratorDependencies dependencies,
            RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies,
            IXuguConnection xuguConnection)
            : base(dependencies, relationalDependencies)
        {
            Check.NotNull(xuguConnection, nameof(xuguConnection));

            _xuguConnection = xuguConnection;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override object GenerateCacheKey(Expression query, bool async)
            => new XuguCompiledQueryCacheKey(
                GenerateCacheKeyCore(query, async),
                _xuguConnection.IsMultipleActiveResultSetsEnabled);

        private readonly struct XuguCompiledQueryCacheKey : IEquatable<XuguCompiledQueryCacheKey>
        {
            private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
            private readonly bool _multipleActiveResultSetsEnabled;

            public XuguCompiledQueryCacheKey(
                RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey,
                bool multipleActiveResultSetsEnabled)
            {
                _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
                _multipleActiveResultSetsEnabled = multipleActiveResultSetsEnabled;
            }

            public override bool Equals(object? obj)
                => obj is XuguCompiledQueryCacheKey xuguCompiledQueryCacheKey
                    && Equals(xuguCompiledQueryCacheKey);

            public bool Equals(XuguCompiledQueryCacheKey other)
                => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
                    && _multipleActiveResultSetsEnabled == other._multipleActiveResultSetsEnabled;

            public override int GetHashCode()
                => HashCode.Combine(_relationalCompiledQueryCacheKey, _multipleActiveResultSetsEnabled);
        }
    }
}
