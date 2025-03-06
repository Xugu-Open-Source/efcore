// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDbTypeMapperTest
    {
        [Fact]
        public void Does_simple_XuGuDb_mappings_to_DDL_types()
        {

            Assert.Equal("integer", GetTypeMapping(typeof(int)).StoreType);
            Assert.Equal("datetime", GetTypeMapping(typeof(DateTime)).StoreType);
            Assert.Equal("char", GetTypeMapping(typeof(char)).StoreType);
            Assert.Equal("tinyint", GetTypeMapping(typeof(sbyte)).StoreType);
            Assert.Equal("float", GetTypeMapping(typeof(float)).StoreType);
            Assert.Equal("bool", GetTypeMapping(typeof(bool)).StoreType);
            Assert.Equal("smallint", GetTypeMapping(typeof(short)).StoreType);
            Assert.Equal("bigint", GetTypeMapping(typeof(long)).StoreType);
            Assert.Equal("float", GetTypeMapping(typeof(float)).StoreType);
        }


        [Fact]
        public void Does_simple_XuGuDb_mappings_for_nullable_CLR_types_to_DDL_types()
        {
            Assert.Equal("integer", GetTypeMapping(typeof(int?)).StoreType);
            Assert.Equal("datetime", GetTypeMapping(typeof(DateTime?)).StoreType);
            Assert.Equal("char", GetTypeMapping(typeof(char?)).StoreType);
            Assert.Equal("tinyint", GetTypeMapping(typeof(sbyte?)).StoreType);
            Assert.Equal("double", GetTypeMapping(typeof(double?)).StoreType);
            Assert.Equal("bool", GetTypeMapping(typeof(bool?)).StoreType);
            Assert.Equal("smallint", GetTypeMapping(typeof(short?)).StoreType);
            Assert.Equal("bigint", GetTypeMapping(typeof(long?)).StoreType);
            Assert.Equal("float", GetTypeMapping(typeof(float?)).StoreType);
        }

        [Fact]
        public void Does_simple_XuGuDb_mappings_for_enums_to_DDL_types()
        {
            var test = GetTypeMapping(typeof(IntEnum)).StoreType;
            Assert.Equal("integer", GetTypeMapping(typeof(IntEnum)).StoreType);
            Assert.Equal("tinyint", GetTypeMapping(typeof(SByteEnum)).StoreType);
            Assert.Equal("smallint", GetTypeMapping(typeof(ShortEnum)).StoreType);
            Assert.Equal("bigint", GetTypeMapping(typeof(LongEnum)).StoreType);
            Assert.Equal("integer", GetTypeMapping(typeof(IntEnum?)).StoreType);
            Assert.Equal("tinyint", GetTypeMapping(typeof(SByteEnum?)).StoreType);
            Assert.Equal("smallint", GetTypeMapping(typeof(ShortEnum?)).StoreType);
            Assert.Equal("bigint", GetTypeMapping(typeof(LongEnum?)).StoreType);
        }

        [Fact]
        public void Does_simple_XuGuDb_mappings_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int)).DbType);
            Assert.Null(GetTypeMapping(typeof(string)).DbType);
            Assert.Equal(DbType.Binary, GetTypeMapping(typeof(byte[])).DbType);
            //Assert.Null(GetTypeMapping(typeof(DateTime)).DbType);
            //Assert.Equal(DbType.Int32, GetTypeMapping(typeof(char)).DbType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(sbyte)).DbType);
            Assert.Null(GetTypeMapping(typeof(double)).DbType);
            Assert.Null(GetTypeMapping(typeof(bool)).DbType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short)).DbType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long)).DbType);
            Assert.Null(GetTypeMapping(typeof(float)).DbType);
        }

        [Fact]
        public void Does_simple_XuGuDb_mappings_for_nullable_CLR_types_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(int?)).DbType);
            Assert.Null(GetTypeMapping(typeof(string)).DbType);
            Assert.Equal(DbType.Binary, GetTypeMapping(typeof(byte[])).DbType);
            //Assert.Equal(DbType.Int32, GetTypeMapping(typeof(char?)).DbType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(sbyte?)).DbType);
            Assert.Null(GetTypeMapping(typeof(double?)).DbType);
            Assert.Null(GetTypeMapping(typeof(bool?)).DbType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(short?)).DbType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(long?)).DbType);
            Assert.Null(GetTypeMapping(typeof(float?)).DbType);
        }

        [Fact]
        public void Does_simple_XuGuDb_mappings_for_enums_to_DbTypes()
        {
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(IntEnum)).DbType);
            Assert.Equal(DbType.SByte, GetTypeMapping(typeof(SByteEnum)).DbType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(ShortEnum)).DbType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(LongEnum)).DbType);
            Assert.Equal(DbType.Int32, GetTypeMapping(typeof(IntEnum?)).DbType);
            //Assert.Equal(DbType.Byte, GetTypeMapping(typeof(ByteEnum?)).DbType);
            Assert.Equal(DbType.Int16, GetTypeMapping(typeof(ShortEnum?)).DbType);
            Assert.Equal(DbType.Int64, GetTypeMapping(typeof(LongEnum?)).DbType);
        }

        [Fact]
        public void Does_decimal_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(decimal));

            Assert.Null(typeMapping.DbType);
            Assert.Equal("numeric(18, 2)", typeMapping.StoreType);
        }

        [Fact]
        public void Does_decimal_mapping_for_nullable_CLR_types()
        {
            var typeMapping = GetTypeMapping(typeof(decimal?));

            Assert.Null(typeMapping.DbType);
            Assert.Equal("numeric(18, 2)", typeMapping.StoreType);
        }

        [Fact]
        public void Does_non_key_XuGuDb_string_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(string));

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar", typeMapping.StoreType);
            Assert.Equal(4000, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_string_mapping_with_max_length()
        {
            var typeMapping = GetTypeMapping(typeof(string), null, 3);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar(3)", typeMapping.StoreType);
            Assert.Equal(3, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_string_mapping_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(string));

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar", typeMapping.StoreType);
            Assert.Equal(4000, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new string('X', 4001)).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_string_mapping_with_max_length_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(string), null, 3);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar(3)", typeMapping.StoreType);
            Assert.Equal(3, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new string('X', 4001)).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_required_string_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(string), isNullable: false);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar", typeMapping.StoreType);
            Assert.Equal(4000, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(4000, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_key_XuGuDb_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string));
            property.IsNullable = false;
            property.DeclaringEntityType.SetPrimaryKey(property);

            var typeMapping = new XuGuDbTypeMapper().GetMapping(property);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar(450)", typeMapping.StoreType);
            Assert.Equal(450, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_foreign_key_XuGuDb_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string));
            property.IsNullable = false;
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(string));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);

            var typeMapping = new XuGuDbTypeMapper().GetMapping(fkProperty);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar(450)", typeMapping.StoreType);
            Assert.Equal(450, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_required_foreign_key_XuGuDb_string_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(string));
            property.IsNullable = false;
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(string));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);
            fkProperty.IsNullable = false;

            var typeMapping = new XuGuDbTypeMapper().GetMapping(fkProperty);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar(450)", typeMapping.StoreType);
            Assert.Equal(450, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_indexed_column_XuGuDb_string_mapping()
        {
            var entityType = CreateEntityType();
            var property = entityType.AddProperty("MyProp", typeof(string));
            entityType.AddIndex(property);

            var typeMapping = new XuGuDbTypeMapper().GetMapping(property);

            Assert.Null(typeMapping.DbType);
            Assert.Equal("varchar(450)", typeMapping.StoreType);
            Assert.Equal(450, typeMapping.Size);
            Assert.True(typeMapping.IsUnicode);
            Assert.Equal(450, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_binary_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary", typeMapping.StoreType);
            Assert.Equal(8000, typeMapping.Size);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_binary_mapping_with_max_length()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), null, 3);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(3)", typeMapping.StoreType);
            Assert.Equal(3, typeMapping.Size);
            Assert.Equal(3, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_binary_mapping_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]));

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary", typeMapping.StoreType);
            Assert.Equal(8000, typeMapping.Size);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_binary_mapping_with_max_length_with_long_string()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), null, 3);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(3)", typeMapping.StoreType);
            Assert.Equal(3, typeMapping.Size);
            Assert.Equal(-1, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[8001]).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_required_binary_mapping()
        {
            var typeMapping = GetTypeMapping(typeof(byte[]), isNullable: false);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary", typeMapping.StoreType);
            Assert.Equal(8000, typeMapping.Size);
            Assert.Equal(8000, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_non_key_XuGuDb_fixed_length_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyBinaryProp", typeof(byte[]));
            property.Relational().ColumnType = "binary(100)";

            var typeMapping = new XuGuDbTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(100)", typeMapping.StoreType);
        }

        [Fact]
        public void Does_key_XuGuDb_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsNullable = false;
            property.DeclaringEntityType.SetPrimaryKey(property);

            var typeMapping = new XuGuDbTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(900)", typeMapping.StoreType);
            Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_foreign_key_XuGuDb_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsNullable = false;
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(byte[]));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);

            var typeMapping = new XuGuDbTypeMapper().GetMapping(fkProperty);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(900)", typeMapping.StoreType);
            Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_required_foreign_key_XuGuDb_binary_mapping()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsNullable = false;
            var fkProperty = property.DeclaringEntityType.AddProperty("FK", typeof(byte[]));
            var pk = property.DeclaringEntityType.SetPrimaryKey(property);
            property.DeclaringEntityType.AddForeignKey(fkProperty, pk, property.DeclaringEntityType);
            fkProperty.IsNullable = false;

            var typeMapping = new XuGuDbTypeMapper().GetMapping(fkProperty);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(900)", typeMapping.StoreType);
            Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", new byte[3]).Size);
        }

        [Fact]
        public void Does_indexed_column_XuGuDb_binary_mapping()
        {
            var entityType = CreateEntityType();
            var property = entityType.AddProperty("MyProp", typeof(byte[]));
            entityType.AddIndex(property);

            var typeMapping = new XuGuDbTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary(900)", typeMapping.StoreType);
            Assert.Equal(900, typeMapping.CreateParameter(new TestCommand(), "Name", "Value").Size);
        }

        [Fact]
        public void Does_not_do_rowversion_mapping_for_non_computed_concurrency_tokens()
        {
            var property = CreateEntityType().AddProperty("MyProp", typeof(byte[]));
            property.IsConcurrencyToken = true;

            var typeMapping = (XuGuDbMaxLengthMapping)new XuGuDbTypeMapper().GetMapping(property);

            Assert.Equal(DbType.Binary, typeMapping.DbType);
            Assert.Equal("binary", typeMapping.StoreType);
        }

        private static RelationalTypeMapping GetTypeMapping(Type propertyType, bool? isNullable = null, int? maxLength = null)
        {
            var property = CreateEntityType().AddProperty("MyProp", propertyType);

            if (isNullable.HasValue)
            {
                property.IsNullable = isNullable.Value;
            }

            if (maxLength.HasValue)
            {
                property.SetMaxLength(maxLength);
            }

            return new XuGuDbTypeMapper().GetMapping(property);
        }

        private static EntityType CreateEntityType() => new Model().AddEntityType("MyType");

        [Fact]
        public void Does_default_mappings_for_sequence_types()
        {
            Assert.Equal("integer", new XuGuDbTypeMapper().GetMapping(typeof(int)).StoreType);
            Assert.Equal("smallint", new XuGuDbTypeMapper().GetMapping(typeof(short)).StoreType);
            Assert.Equal("bigint", new XuGuDbTypeMapper().GetMapping(typeof(long)).StoreType);
            Assert.Equal("tinyint", new XuGuDbTypeMapper().GetMapping(typeof(sbyte)).StoreType);
        }

        [Fact]
        public void Does_default_mappings_for_strings_and_byte_arrays()
        {
            Assert.Equal("varchar", new XuGuDbTypeMapper().GetMapping(typeof(string)).StoreType);
            Assert.Equal("binary", new XuGuDbTypeMapper().GetMapping(typeof(byte[])).StoreType);
        }

        [Fact]
        public void Does_default_mappings_for_values()
        {
            Assert.Equal("varchar", new XuGuDbTypeMapper().GetMappingForValue("Cheese").StoreType);
            Assert.Equal("binary", new XuGuDbTypeMapper().GetMappingForValue(new byte[1]).StoreType);
            Assert.Equal("datetime", new XuGuDbTypeMapper().GetMappingForValue(new DateTime()).StoreType);
        }

        [Fact]
        public void Does_default_mappings_for_null_values()
        {
            Assert.Equal("NULL", new XuGuDbTypeMapper().GetMappingForValue(null).StoreType);
            Assert.Equal("NULL", new XuGuDbTypeMapper().GetMappingForValue(DBNull.Value).StoreType);
            Assert.Equal("NULL", RelationalTypeMapperExtensions.GetMappingForValue(null, "Itz").StoreType);
        }

        [Fact]
        public void Throws_for_unrecognized_types()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => new XuGuDbTypeMapper().GetMapping("magic"));
            Assert.Equal(RelationalStrings.UnsupportedType("magic"), ex.Message);
        }

        [Theory]
        [InlineData("bigint", typeof(long), null, false)]
        [InlineData("binary", typeof(byte[]), 8000, false)]
        [InlineData("binary(333)", typeof(byte[]), 333, false)]
        [InlineData("bool", typeof(bool), null, false)]
        [InlineData("char", typeof(string), 8000, false)]
        [InlineData("char(333)", typeof(string), 333, false)]
        [InlineData("date", typeof(DateTime), null, false)]
        [InlineData("datetime", typeof(DateTime), null, false)]
        [InlineData("double", typeof(double), null, false)]
        [InlineData("double(10, 8)", typeof(double), null, false)]
        //[InlineData("image", typeof(byte[]), 8000, false)]
        [InlineData("integer", typeof(int), null, false)]
        //[InlineData("money", typeof(decimal), null, false)]
        [InlineData("numeric", typeof(decimal), null, false)]
        [InlineData("float", typeof(float), null, false)]
        [InlineData("smallint", typeof(short), null, false)]
        [InlineData("time", typeof(DateTime), null, false)]
        [InlineData("tinyint", typeof(sbyte), null, false)]
        [InlineData("varchar", typeof(string), 8000, false)]
        [InlineData("varchar(333)", typeof(string), 333, false)]
        public void Can_map_by_type_name(string typeName, Type clrType, int? size, bool unicode)
        {
            var mapping = new XuGuDbTypeMapper().GetMapping(typeName);

            Assert.Equal(clrType, mapping.ClrType);
            Assert.Equal(size, mapping.Size);
            Assert.Equal(unicode, mapping.IsUnicode);
            Assert.Equal(typeName.ToLowerInvariant(), mapping.StoreType);
        }

        private enum LongEnum : long
        {
        }

        private enum IntEnum
        {
        }

        private enum ShortEnum : short
        {
        }

        private enum ByteEnum : byte
        {
        }

        private enum SByteEnum : sbyte
        {
        }

        private class TestParameter : DbParameter
        {
            public override void ResetDbType()
            {
            }

            public override DbType DbType { get; set; }
            public override ParameterDirection Direction { get; set; }
            public override bool IsNullable { get; set; }
            public override string ParameterName { get; set; }
            public override string SourceColumn { get; set; }
#if NET451
            public override DataRowVersion SourceVersion { get; set; }
#endif
            public override object Value { get; set; }
            public override bool SourceColumnNullMapping { get; set; }
            public override int Size { get; set; }
        }

        private class TestCommand : DbCommand
        {
            public override void Prepare()
            {
            }

            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection { get; }
            protected override DbTransaction DbTransaction { get; set; }
            public override bool DesignTimeVisible { get; set; }

            public override void Cancel()
            {
            }

            protected override DbParameter CreateDbParameter()
            {
                return new TestParameter();
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                throw new NotImplementedException();
            }

            public override int ExecuteNonQuery()
            {
                throw new NotImplementedException();
            }

            public override object ExecuteScalar()
            {
                throw new NotImplementedException();
            }
        }
    }
}
