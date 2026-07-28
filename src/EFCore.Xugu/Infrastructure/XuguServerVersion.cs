namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

/// <summary>
/// Default server version for XuguDB 12.x.
/// </summary>
public sealed class XuguServerVersion : ServerVersion
{
    public static XuguServerVersion Default { get; } = new(new Version(12, 0));

    public override ServerVersionSupport Supports { get; }

    public XuguServerVersion(Version version)
        : base(version)
    {
        Supports = new XuguServerVersionSupport(this);
    }

    private sealed class XuguServerVersionSupport : ServerVersionSupport
    {
        internal XuguServerVersionSupport(ServerVersion serverVersion)
            : base(serverVersion)
        {
        }

        // Live XuguDB 12 supports JSON scalar functions (JSON_VALUE/LENGTH/CONTAINS/EXTRACT)
        // but NOT JSON_TABLE / unnest as a rowset source (docs have no JSON_TABLE; probe → E19132).
        public override bool OuterApply => false;
        public override bool OuterReferenceInMultiLevelSubquery => ServerVersion.Version >= new Version(12, 0);
        public override bool Json => ServerVersion.Version >= new Version(12, 0);
        public override bool JsonTable => false;
        public override bool JsonValue => ServerVersion.Version >= new Version(12, 0);
        // VALUES as SELECT source is limited; GenerateValues rewrites to UNION ALL SELECT.
        public override bool Values => ServerVersion.Version >= new Version(12, 0);
        public override bool ValuesWithRows => false;
    }
}
