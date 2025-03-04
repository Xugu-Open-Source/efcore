// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using JetBrains.Annotations;
using EntityFrameworkCore.XuGu.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Represents a <see cref="ServerVersion"/> for XG database servers.
    /// For MariaDB database servers, use <see cref="MariaDbServerVersion"/> instead.
    /// </summary>
    public class XGServerVersion : ServerVersion
    {
        public static readonly string XGTypeIdentifier = nameof(ServerType.XuGu).ToLowerInvariant();
        public static readonly ServerVersion LatestSupportedServerVersion = new XGServerVersion(new Version(8, 0, 21));

        public override ServerVersionSupport Supports { get; }

        public XGServerVersion(Version version)
            : base(version, ServerType.XuGu)
        {
            Supports = new XGServerVersionSupport(this);
        }

        public XGServerVersion(string versionString)
            : this(Parse(versionString, ServerType.XuGu))
        {
        }

        public XGServerVersion(ServerVersion serverVersion)
            : base(serverVersion.Version, serverVersion.Type, serverVersion.TypeIdentifier)
        {
            if (Type != ServerType.XuGu ||
                !string.Equals(TypeIdentifier, XGTypeIdentifier))
            {
                throw new ArgumentException($"{nameof(XGServerVersion)} is not compatible with the supplied server type.");
            }

            Supports = new XGServerVersionSupport(this);
        }

        public class XGServerVersionSupport : ServerVersionSupport
        {
            internal XGServerVersionSupport([NotNull] ServerVersion serverVersion)
                : base(serverVersion)
            {
            }

            public override bool DateTimeCurrentTimestamp => ServerVersion.Version >= new Version(5, 6, 5);
            public override bool DateTime6 => ServerVersion.Version >= new Version(5, 6, 4);
            public override bool LargerKeyLength => ServerVersion.Version >= new Version(5, 7, 7);
            public override bool RenameIndex => ServerVersion.Version >= new Version(5, 7, 0);
            public override bool RenameColumn => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool WindowFunctions => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool FloatCast => false; // The implemented support drops some decimal places and rounds.
            public override bool DoubleCast => ServerVersion.Version >= new Version(8, 0, 17);
            public override bool OuterApply => ServerVersion.Version >= new Version(8, 0, 14);
            public override bool CrossApply => ServerVersion.Version >= new Version(8, 0, 14);
            public override bool OuterReferenceInMultiLevelSubquery => ServerVersion.Version >= new Version(8, 0, 14);
            public override bool Json => ServerVersion.Version >= new Version(5, 7, 8);
            public override bool GeneratedColumns => ServerVersion.Version >= new Version(5, 7, 6);
            public override bool NullableGeneratedColumns => ServerVersion.Version >= new Version(5, 7, 0);
            public override bool ParenthesisEnclosedGeneratedColumnExpressions => GeneratedColumns;
            public override bool DefaultCharSetUtf8Mb4 => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool DefaultExpression => ServerVersion.Version >= new Version(8, 0, 13);
            public override bool AlternativeDefaultExpression => false;
            public override bool SpatialIndexes => ServerVersion.Version >= new Version(5, 7, 5);
            //public override bool SpatialReferenceSystemRestrictedColumns => ServerVersion.Version >= new Version(8, 0, 3);
            public override bool SpatialFunctionAdditions => false;
            public override bool SpatialSupportFunctionAdditions => ServerVersion.Version >= new Version(5, 7, 6);
            public override bool SpatialSetSridFunction => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool SpatialDistanceFunctionImplementsAndoyer => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool SpatialDistanceSphereFunction => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool SpatialGeographic => ServerVersion.Version >= new Version(8, 0, 0);
            public override bool ExceptIntercept => false;
            public override bool ExceptInterceptPrecedence => false;
            public override bool JsonDataTypeEmulation => false;
            public override bool ImplicitBoolCheckUsesIndex => ServerVersion.Version >= new Version(8, 0, 0); // Exact version has not been verified yet
            public override bool XGBug96947Workaround => ServerVersion.Version >= new Version(5, 7, 0) &&
                                                            ServerVersion.Version < new Version(8, 0, 25); // Exact version has not been verified yet, but it is 5.7.x and could very well be 5.7.0
            public override bool XGBug104294Workaround => ServerVersion.Version >= new Version(8, 0, 0); // Exact version has not been determined yet
            public override bool FullTextParser => ServerVersion.Version >= new Version(5, 7, 3);
            public override bool InformationSchemaCheckConstraintsTable => ServerVersion.Version >= new Version(8, 0, 16); // XG is missing the explicit TABLE_NAME column that MariaDB supports, so always join the TABLE_CONSTRAINTS table when accessing CHECK_CONSTRAINTS for any database server that supports CHECK_CONSTRAINTS.
        }
    }
}
