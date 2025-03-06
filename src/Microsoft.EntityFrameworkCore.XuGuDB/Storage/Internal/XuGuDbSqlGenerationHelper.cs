// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{

    public class XuGuDbSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        private const string DateTimeFormatConst = "yyyy-MM-dd HH:mm:ss.fffK";
        private const string DateTimeFormatStringConst = "'{0:" + DateTimeFormatConst + "}'";
        private const string DateTimeOffsetFormatConst = "yyyy-MM-dd HH:mm:ss.fffzzz";
        private const string DateTimeOffsetFormatStringConst = "'{0:" + DateTimeOffsetFormatConst + "}'";

        protected override string DateTimeFormat => DateTimeFormatConst;

        protected override string DateTimeFormatString => DateTimeFormatStringConst;

        protected override string DateTimeOffsetFormat => DateTimeOffsetFormatConst;

        protected override string DateTimeOffsetFormatString => DateTimeOffsetFormatStringConst;

        public override string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier));

        public override void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            builder.Append(identifier);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string DelimitIdentifier(string identifier)
            => $"`{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}`";


        public override string DelimitIdentifier(string name, string schema)
            => (!string.IsNullOrEmpty(schema)
                ? "`"+schema + "`."
                : string.Empty)
               + "`"+Check.NotEmpty(name, nameof(name))+"`";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            builder.Append('`');
            EscapeIdentifier(builder, identifier);
            builder.Append('`');
        }


        public override void DelimitIdentifier(StringBuilder builder, string name, string schema)
        {
            if (!string.IsNullOrEmpty(schema))
            {
                builder.Append("`")
                    .Append(schema)
                    .Append("`")
                    .Append(".")
                    .Append("`")
                    .Append(name)
                    .Append("`");
            }
            else
            {
                builder
                    .Append("`")
                    .Append(name)
                    .Append("`");
            }
        }

        protected override void GenerateLiteralValue(StringBuilder builder, byte[] value)
        {
            Check.NotNull(value, nameof(value));

            builder.Append("0x");

            foreach (var @byte in value)
            {
                builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        protected override string GenerateLiteralValue(string value, RelationalTypeMapping typeMapping = null)
            => $"'{EscapeLiteral(Check.NotNull(value, nameof(value)))}'";

        protected override void GenerateLiteralValue(StringBuilder builder, string value, RelationalTypeMapping typeMapping = null)
        {
            builder.Append(typeMapping.IsUnicode ? "N'" : "'");
            EscapeLiteral(builder, value);
            builder.Append("'");
        }

        protected override string GenerateLiteralValue(DateTime value)
            => $"'{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'";

        protected override string GenerateLiteralValue(DateTimeOffset value)
            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'";

        /// <summary>
        ///     Generates a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="name">The candidate name for the parameter.</param>
        /// <returns>
        ///     A valid name based on the candidate name.
        /// </returns>
        public override string GenerateParameterName(string name)
            => ":" + name;

        /// <summary>
        ///     Writes a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to write generated string to.</param>
        /// <param name="name">The candidate name for the parameter.</param>
        public override void GenerateParameterName(StringBuilder builder, string name)
            => builder.Append(":").Append(name);
    }
}
