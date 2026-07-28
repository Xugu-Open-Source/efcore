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

        public override bool OuterApply => ServerVersion.Version >= new Version(12, 0);
        public override bool OuterReferenceInMultiLevelSubquery => ServerVersion.Version >= new Version(12, 0);
        public override bool Json => ServerVersion.Version >= new Version(12, 0);
        public override bool JsonTable => ServerVersion.Version >= new Version(12, 0);
        public override bool JsonValue => ServerVersion.Version >= new Version(12, 0);
        public override bool Values => ServerVersion.Version >= new Version(12, 0);
        public override bool ValuesWithRows => ServerVersion.Version >= new Version(12, 0);
    }
}
