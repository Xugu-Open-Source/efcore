// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EntityFrameworkCore.XuGu.Storage.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Query.Internal
{
    // Based on: System.Data.Jet.JetCommandParser (EntityFrameworkCore.Jet)
    public class XGQueryStringFactory : IRelationalQueryStringFactory
    {
        private static readonly Lazy<Regex> _limitExpressionParameterRegex = new Lazy<Regex>(
            () => new Regex(
                $@"(?<=\W)LIMIT\s+(?:(?<leading_offset>@?\w+),\s*)?(?<row_count>@?\w+)(?:\s*OFFSET\s*(?<trailing_offset>@?\w+))?",
                RegexOptions.Singleline | RegexOptions.IgnoreCase));

        private static readonly Lazy<Regex> _extractParameterRegex = new Lazy<Regex>(() => new Regex(@"@\w+"));

        private readonly IRelationalTypeMappingSource _typeMapper;

        public XGQueryStringFactory([NotNull] IRelationalTypeMappingSource typeMapper)
        {
            _typeMapper = typeMapper;
        }

        //public virtual string Create(DbCommand command)
        //{
        //    if (command.Parameters.Count == 0)
        //    {
        //        return command.CommandText;
        //    }

        //    // For parameter in LIMIT clauses and for string parameters, we need to inline the parameter values directly into the SQL.
        //    PrepareCommand(command);

        //    bool isNeedEnd = false;
        //    var builder = new StringBuilder();
        //    if (command.Parameters.Count > 0)
        //    {
        //        isNeedEnd = true;
        //        builder.AppendLine("DECLARE");
        //        foreach (DbParameter parameter in command.Parameters)
        //        {
        //            var typeName = TypeNameBuilder.CreateTypeName(parameter);
        //            //var typeMapping = _typeMapper.FindMapping(typeName);
        //            builder
        //                .Append(parameter.ParameterName.Replace(":", ""))
        //                .Append(' ')
        //                .Append(typeName)
        //                .Append(" := ")
        //                .Append(GetParameterValue(parameter))
        //                .AppendLine(";");
        //            command.CommandText= command.CommandText.Replace(parameter.ParameterName, parameter.ParameterName.Replace(":", ""));
        //        }
        //        builder.Append("BEGIN");
        //    }
        //    if (!command.CommandText.EndsWith(";"))
        //    {
        //        command.CommandText= command.CommandText + ";";
        //    }

        //    return isNeedEnd ? builder
        //        .AppendLine()
        //        .Append(command.CommandText).AppendLine("END;").ToString() : builder
        //        .AppendLine()
        //        .Append(command.CommandText).ToString();
        //}

        public virtual string Create(DbCommand command)
        {
            if (command.Parameters.Count == 0)
            {
                return command.CommandText;
            }

            var builder = new StringBuilder(command.CommandText);
            foreach (DbParameter parameter in command.Parameters)
            {
                builder = builder.Replace(parameter.ParameterName, GetParameterValue(parameter));
            }

            return builder.ToString();
        }

        private string GetParameterValue(DbParameter parameter)
        {
            var typeMapping = _typeMapper.FindMapping(parameter.Value.GetType());

            return (parameter.Value == DBNull.Value
                    || parameter.Value == null)
                ? "NULL"
                : typeMapping != null
                    ? typeMapping.GenerateSqlLiteral(parameter.Value)
                    : parameter.Value.ToString();
        }

        protected virtual void PrepareCommand(DbCommand command)
        {
            // XG does not support user variables in LIMIT statements.
            // (It does however support parameters in LIMIT statements since 2010. See https://bugs.XG.com/bug.php?id=11918)
            //
            // Because of that, we need to inline the parameter values as constants into the SQL command, in cases where they appear in a
            // LIMIT clause.
            //
            // Also, the rules for applying collation from user variables are different than the once for parameters.
            // We therefore inline all parameter values of type string as well.

            

            var stringParameterNames = command.Parameters.Cast<DbParameter>()
                .Where(p => p.Value is string)
                .Select(p => p.ParameterName)
                .ToList();

            if (!command.CommandText.Contains("LIMIT", StringComparison.OrdinalIgnoreCase) &&
                !stringParameterNames.Any())
            {
                return;
            }

            var matches = _limitExpressionParameterRegex.Value.Matches(command.CommandText);
            if (matches.Count <= 0 &&
                !stringParameterNames.Any())
            {
                return;
            }

            var limitGroupsWithParameter = matches.SelectMany(m => m.Groups["row_count"].Captures)
                .Concat(matches.SelectMany(m => m.Groups["leading_offset"].Captures))
                .Concat(matches.SelectMany(m => m.Groups["trailing_offset"].Captures))
                .ToList();

            if (!limitGroupsWithParameter.Any() &&
                !stringParameterNames.Any())
            {
                return;
            }

            var parser = new XGCommandParser(command.CommandText);
            var parameterPositions = parser.GetStateIndices(':');
            var parameters = parameterPositions
                .Select(
                    i => new
                    {
                        Index = i,
                        ParameterName = _extractParameterRegex.Value.Match(command.CommandText.Substring(i)).Value,
                    })
                .Where(t => !string.IsNullOrEmpty(t.ParameterName))
                .ToList();

            var validParameters = (limitGroupsWithParameter
                    .Where(c => parameterPositions.Contains(c.Index) &&
                                command.Parameters.Contains(c.Value))
                    .Select(c => new {Index = c.Index, ParameterName = c.Value}))
                .Concat(stringParameterNames.SelectMany(s => parameters.Where(p => p.ParameterName == s)))
                .Distinct()
                .OrderByDescending(c => c.Index)
                .ToList();

            foreach (var validParameter in validParameters)
            {
                var parameterIndex = validParameter.Index;
                var parameterName = validParameter.ParameterName;

                parameters.RemoveAt(
                    parameters.FindIndex(
                        t => t.Index == parameterIndex &&
                             t.ParameterName == parameterName));

                var parameter = command.Parameters[parameterName];
                var parameterValue = GetParameterValue(parameter);

                command.CommandText = command.CommandText.Substring(0, parameterIndex) +
                                      parameterValue +
                                      command.CommandText.Substring(parameterIndex + validParameter.ParameterName.Length);
            }

            foreach (var parameterName in validParameters
                .Select(c => c.ParameterName)
                .Distinct()
                .Where(s => parameters.FindIndex(t => t.ParameterName == s) == -1))
            {
                command.Parameters.RemoveAt(parameterName);
            }
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
                builder.Append("(max)");
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
                return (sqlParameter.m_DbType switch
                {
                    XGDbType.BigInt => builder.Append("bigint"),
                    XGDbType.Binary => builder.Append("binary").AppendSize(parameter),
                    XGDbType.Char => builder.Append("char").AppendSize(parameter),
                    XGDbType.Date => builder.Append("date"),
                    XGDbType.DateTime => builder.Append("datetime"),
                    XGDbType.DateTimeOffset => builder.Append("char").AppendPrecision(parameter),
                    XGDbType.Numeric => builder.Append("numeric").AppendPrecisionAndScale(parameter),
                    XGDbType.Real => builder.Append("float").AppendSize(parameter),
                    XGDbType.Int => builder.Append("int"),
                    XGDbType.SmallInt => builder.Append("smallint"),
                    XGDbType.Time => builder.Append("time").AppendPrecision(parameter),
                    XGDbType.TinyInt => builder.Append("tinyint"),
                    XGDbType.VarBinary => builder.Append("varbinary").AppendSizeOrMax(parameter),
                    XGDbType.VarChar => builder.Append("varchar").AppendSizeOrMax(parameter),
                    _ => builder.Append("char")
                }).ToString();
            }

            return "char";
        }
    }
}
