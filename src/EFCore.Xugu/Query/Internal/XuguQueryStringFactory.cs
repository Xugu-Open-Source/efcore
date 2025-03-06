// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguQueryStringFactory : IRelationalQueryStringFactory
    {
        private readonly IRelationalTypeMappingSource _typeMapper;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguQueryStringFactory(IRelationalTypeMappingSource typeMapper)
        {
            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Create(DbCommand command)
        {
            if (command.Parameters.Count == 0)
            {
                return command.CommandText;
            }

            var builder = new StringBuilder();
            foreach (DbParameter parameter in command.Parameters)
            {
                var typeName = TypeNameBuilder.CreateTypeName(parameter);
                var typeMapping = _typeMapper.FindMapping(typeName);

                builder
                    .Append("DECLARE ")
                    .Append(parameter.ParameterName)
                    .Append(' ')
                    .Append(typeName)
                    .Append(" = ")
                    .Append(
                        (parameter.Value == DBNull.Value
                            || parameter.Value == null)
                            ? "NULL"
                            : parameter.Value is SqlBytes sqlBytes
                                ? new XuguByteArrayTypeMapping(typeName).GenerateSqlLiteral(sqlBytes.Value)
                                : typeMapping != null
                                    ? typeMapping.GenerateSqlLiteral(parameter.Value)
                                    : parameter.Value.ToString())
                    .AppendLine(";");
            }

            return builder
                .AppendLine()
                .Append(command.CommandText).ToString();
        }
    }

    internal static class TypeNameBuilder
    {
        private static StringBuilder AppendSize(this StringBuilder builder, DbParameter parameter)
        {
            if (parameter.Size > 0)
            {
                builder
                    .Append('(')
                    .Append(parameter.Size.ToString(CultureInfo.InvariantCulture))
                    .Append(')');
            }

            return builder;
        }

        private static StringBuilder AppendSizeOrMax(this StringBuilder builder, DbParameter parameter)
        {
            if (parameter.Size > 0)
            {
                builder.AppendSize(parameter);
            }
            else if (parameter.Size == -1)
            {
                builder.Append("");
            }

            return builder;
        }

        private static StringBuilder AppendPrecision(this StringBuilder builder, DbParameter parameter)
        {
            if (parameter.Precision > 0)
            {
                builder
                    .Append('(')
                    .Append(parameter.Precision.ToString(CultureInfo.InvariantCulture))
                    .Append(')');
            }

            return builder;
        }

        private static StringBuilder AppendPrecisionAndScale(this StringBuilder builder, DbParameter parameter)
        {
            if (parameter.Precision > 0
                && parameter.Scale > 0)
            {
                builder
                    .Append('(')
                    .Append(parameter.Precision.ToString(CultureInfo.InvariantCulture))
                    .Append(',')
                    .Append(parameter.Scale.ToString(CultureInfo.InvariantCulture))
                    .Append(')');
            }

            return builder.AppendPrecision(parameter);
        }

        public static string CreateTypeName(DbParameter parameter)
        {
            if (parameter is XGParameters sqlParameter)
            {
                var builder = new StringBuilder();
                return ((SqlDbType)sqlParameter.DbType switch
                {
                    SqlDbType.BigInt => builder.Append("bigint"),
                    SqlDbType.Binary => builder.Append("binary").AppendSize(parameter),
                    SqlDbType.Bit => builder.Append("bit"),
                    SqlDbType.Char => builder.Append("char").AppendSize(parameter),
                    SqlDbType.Date => builder.Append("date"),
                    SqlDbType.DateTime => builder.Append("datetime"),
                    SqlDbType.DateTime2 => builder.Append("datetime").AppendPrecision(parameter),
                    SqlDbType.DateTimeOffset => builder.Append("timestamp").AppendPrecision(parameter),
                    SqlDbType.Decimal => builder.Append("numeric").AppendPrecisionAndScale(parameter),
                    SqlDbType.Float => builder.Append("float").AppendSize(parameter),
                    SqlDbType.Int => builder.Append("int"),
                    SqlDbType.NChar => builder.Append("char").AppendSize(parameter),
                    SqlDbType.NVarChar => builder.Append("varchar"),
                    SqlDbType.Real => builder.Append("real"),
                    SqlDbType.SmallDateTime => builder.Append("smalldatetime"),
                    SqlDbType.SmallInt => builder.Append("smallint"),
                    SqlDbType.SmallMoney => builder.Append("smallmoney"),
                    SqlDbType.Structured => builder.Append("structured"),
                    SqlDbType.Text => builder.Append("clob"),
                    SqlDbType.Time => builder.Append("time").AppendPrecision(parameter),
                    SqlDbType.Timestamp => builder.Append("timestamp"),
                    SqlDbType.TinyInt => builder.Append("tinyint"),
                    SqlDbType.VarBinary => builder.Append("binary"),
                    SqlDbType.VarChar => builder.Append("varchar").AppendSizeOrMax(parameter),
                    SqlDbType.Xml => builder.Append("xml"),
                    _ => builder.Append("sql_variant")
                }).ToString();
            }

            return "sql_variant";
        }
    }
}
