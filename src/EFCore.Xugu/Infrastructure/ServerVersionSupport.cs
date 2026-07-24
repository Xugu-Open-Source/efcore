using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

/// <summary>
/// Feature flags keyed by XuguDB server version for functional/spec tests.
/// </summary>
public class ServerVersionSupport
{
    public virtual ServerVersion ServerVersion { get; }

    public ServerVersionSupport(ServerVersion serverVersion)
    {
        ServerVersion = serverVersion ?? throw new ArgumentNullException(nameof(serverVersion));
    }

    public virtual bool Version(string versionString)
        => Version(ServerVersion.Parse(NormalizeVersionString(versionString)));

    public virtual bool Version(ServerVersion serverVersion)
        => ServerVersion.Version >= serverVersion.Version;

    public virtual bool PropertyOrVersion(string propertyNameOrServerVersion)
    {
        if (ServerVersion.TryParse(propertyNameOrServerVersion, out var serverVersion))
        {
            return Version(serverVersion);
        }

        var property = typeof(ServerVersionSupport).GetRuntimeProperty(propertyNameOrServerVersion);
        if (property is { PropertyType: var propertyType } && propertyType == typeof(bool))
        {
            return (bool)property.GetValue(this)!;
        }

        // Unknown feature flags from shared EF/Pomelo attributes: treat as unsupported
        // instead of crashing the test host (was ArgumentException → process abort).
        return false;
    }

    private static string NormalizeVersionString(string versionString)
    {
        if (versionString.EndsWith("-xg", StringComparison.OrdinalIgnoreCase))
        {
            return versionString[..^3];
        }

        return versionString;
    }

    public virtual bool OuterApply => false;
    public virtual bool OuterReferenceInMultiLevelSubquery => false;
    public virtual bool Json => false;
    public virtual bool JsonTable => false;
    public virtual bool JsonValue => false;
    public virtual bool LimitWithinInAllAnySomeSubquery => false;
    public virtual bool LimitWithNonConstantValue => false;
    public virtual bool OffsetReferencesOuterQuery => false;
    public virtual bool Values => false;
    public virtual bool ValuesWithRows => false;

    public virtual bool JsonTableImplementationStable => JsonTable;
    public virtual bool JsonTableImplementationWithoutXGBugs => JsonTable;
}
