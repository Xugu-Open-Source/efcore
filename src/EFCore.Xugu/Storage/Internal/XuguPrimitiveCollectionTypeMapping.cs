using System.Data;
using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;

/// <summary>
///     Relational mapping for primitive collection parameters represented as JSON text.
/// </summary>
public sealed class XuguPrimitiveCollectionTypeMapping : RelationalTypeMapping
{
    public XuguPrimitiveCollectionTypeMapping(Type clrType)
        : base("JSON", clrType, System.Data.DbType.String)
    {
    }

    private XuguPrimitiveCollectionTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new XuguPrimitiveCollectionTypeMapping(parameters);

    public override DbParameter CreateParameter(
        DbCommand command,
        string name,
        object? value,
        bool? nullable = null,
        ParameterDirection direction = ParameterDirection.Input)
    {
        if (value is null)
        {
            value = "null";
        }
        else if (value is not string)
        {
            value = JsonSerializer.Serialize(value, value.GetType());
        }

        return base.CreateParameter(command, name, value, nullable, direction);
    }
}
