// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Relational.Tests.Update;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Update
{
    public class XuGuDbUpdateSqlGeneratorTest : UpdateSqlGeneratorTestBase
    {
        protected override IUpdateSqlGenerator CreateSqlGenerator()
            => new XuGuDbUpdateSqlGenerator(new XuGuDbSqlGenerationHelper(), new XuGuDbTypeMapper());

        //[Fact]
        //public void AppendBatchHeader_should_append_SET_NOCOUNT_ON()
        //{
        //    var sb = new StringBuilder();

        //    CreateSqlGenerator().AppendBatchHeader(sb);

        //    Assert.Equal("SET NOCOUNT ON;" + Environment.NewLine, sb.ToString());
        //}

        [Fact]
        public new void AppendDeleteOperation_creates_full_delete_command_text()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(false);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DELETE FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        protected void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            Assert.Equal(
                "INSERT INTO `SYSDBA`.`Ducks` (`Id`, `Name`, `Quacks`, `ConcurrencyToken`)" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2, :p3);" + Environment.NewLine +
                "SELECT `Computed`" + Environment.NewLine +
                "FROM `SYSDBA`.`Ducks`" + Environment.NewLine +
                "WHERE SQL%ROWCOUNT = 1 AND `Id` IS NULL;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        protected void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist_verification()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);
            Assert.Equal(
                "INSERT INTO `SYSDBA`.`Ducks` (`Name`, `Quacks`, `ConcurrencyToken`)" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2);" + Environment.NewLine +
                "SELECT `Id`, `Computed`" + Environment.NewLine +
                "FROM `SYSDBA`.`Ducks`" + Environment.NewLine +
                "WHERE SQL%ROWCOUNT = 1 AND `Id` = LAST_INSERT_ID();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendInsertOperation_appends_insert_and_select_rowcount_if_no_store_generated_columns_exist_or_conditions_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" +
                OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks"
                + CloseDelimeter + ", " + OpenDelimeter + "Concurrency" + "Token" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2, :p3);" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(false, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(stringBuilder);
        }

        protected override void AppendInsertOperation_appends_insert_and_select_store_generated_columns_but_no_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Id" +
                CloseDelimeter + ", " + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter +
                "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2, :p3);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendUpdateOperation_appends_select_for_computed_property()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(true, false);

            ((IXuGuDbUpdateSqlGenerator)CreateSqlGenerator()).UpdateOperation(stringBuilder, command, 0);

            AppendUpdateOperation_appends_select_for_computed_property_verification(stringBuilder);
        }

        protected override void AppendUpdateOperation_appends_select_for_computed_property_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = :p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = :p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = :p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public override void AppendDeleteOperation_creates_full_delete_command_text_with_concurrency_check()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateDeleteCommand(concurrencyToken: true);

            CreateSqlGenerator().AppendDeleteOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DELETE FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_single_identity_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO `SYSDBA`.`Ducks`" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT `Id`" + Environment.NewLine +
                "FROM `SYSDBA`.`Ducks`" + Environment.NewLine +
                "WHERE SQL%ROWCOUNT = 1 AND `Id` = LAST_INSERT_ID();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendInsertOperation_appends_insert_and_select_for_only_identity()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, false);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(stringBuilder);
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_only_identity_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO `SYSDBA`.`Ducks` (`Name`, `Quacks`, `ConcurrencyToken`)" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2);" + Environment.NewLine +
                "SELECT `Id`" + Environment.NewLine +
                "FROM `SYSDBA`.`Ducks`" + Environment.NewLine +
                "WHERE SQL%ROWCOUNT = 1 AND `Id` = LAST_INSERT_ID();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(true, true, true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(stringBuilder);
        }

        protected override void AppendInsertOperation_appends_insert_and_select_for_all_store_generated_columns_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "INSERT INTO `SYSDBA`.`Ducks`" + Environment.NewLine +
                "DEFAULT VALUES;" + Environment.NewLine +
                "SELECT `Id`, `Computed`" + Environment.NewLine +
                "FROM `SYSDBA`.`Ducks`" + Environment.NewLine +
                "WHERE SQL%ROWCOUNT = 1 AND `Id` = LAST_INSERT_ID();" + Environment.NewLine,
                stringBuilder.ToString());
        }

        //[Fact]
        protected void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(isComputed: true, concurrencyToken: true);

            CreateSqlGenerator().AppendUpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "DECLARE @inserted0 TABLE (`Computed` uniqueidentifier);" + Environment.NewLine +
                "UPDATE `SYSDBA`.`Ducks` SET `Name` = :p0, `Quacks` = :p1, `ConcurrencyToken` = :p2" + Environment.NewLine +
                "OUTPUT INSERTED.`Computed`" + Environment.NewLine +
                "INTO @inserted0" + Environment.NewLine +
                "WHERE `Id` IS NULL AND `ConcurrencyToken` IS NULL;" + Environment.NewLine +
                "SELECT `Computed` FROM @inserted0;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            var sqlGenerator = (IXuGuDbUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            var expectedText = "CREATE TEMPORARY TABLE IF NOT EXISTS `toInsert0` (`Name` varchar, `Quacks` integer, `ConcurrencyToken` binary, `_Position` integer);" + Environment.NewLine +
                "INSERT INTO `toInsert0`" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2, 0)," + Environment.NewLine +
                "(:p0, :p1, :p2, 1);" + Environment.NewLine + Environment.NewLine +
                "CREATE TEMPORARY TABLE IF NOT EXISTS `inserted0` (`Id` integer, `Name` varchar, `Quacks` integer, `Computed` guid, `ConcurrencyToken` binary, `_Position` integer);" + Environment.NewLine +
                "MERGE INTO `SYSDBA`.`Ducks` `i` USING (SELECT `Name`, `Quacks`, `ConcurrencyToken` FROM `toInsert0`) `s` ON(1=0)" + Environment.NewLine +
                "WHEN MATCHED THEN" + Environment.NewLine +
                "UPDATE SET `i`.`Name` = `s`.`Name`, `i`.`Quacks` = `s`.`Quacks`, `i`.`ConcurrencyToken` = `s`.`ConcurrencyToken`;" + Environment.NewLine +
                "WHEN NOT MATCHED THEN" + Environment.NewLine +
                "INSERT (`i`.`Name`, `i`.`Quacks`, `i`.`ConcurrencyToken`)" + Environment.NewLine +
                "VALUES (`s`.`Name`, `s`.`Quacks`, `s`.`ConcurrencyToken`);" + Environment.NewLine +
                "SELECT `Id`, `Computed` FROM `inserted0`" + Environment.NewLine +
                "ORDER BY `_Position`;" + Environment.NewLine;
            
            Assert.Equal(expectedText,stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
        }

        [Fact]
        public new void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(isComputed: true, concurrencyToken: true);

            ((IXuGuDbUpdateSqlGenerator)CreateSqlGenerator()).UpdateOperation(stringBuilder, command, 0);

            AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(stringBuilder);
        }

        protected override void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist_verification(StringBuilder stringBuilder)
        {
            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " + OpenDelimeter + "Name" + CloseDelimeter +
                " = :p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = :p1, " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = :p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL;" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendUpdateOperation_appends_where_for_concurrency_token()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, concurrencyToken: true);

            ((IXuGuDbUpdateSqlGenerator)CreateSqlGenerator()).UpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = :p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = :p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = :p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL AND " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public new void AppendUpdateOperation_appends_update_and_select_rowcount_if_store_generated_columns_dont_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateUpdateCommand(false, false);

            ((IXuGuDbUpdateSqlGenerator)CreateSqlGenerator()).UpdateOperation(stringBuilder, command, 0);

            Assert.Equal(
                "UPDATE " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " SET " +
                OpenDelimeter + "Name" + CloseDelimeter + " = :p0, " + OpenDelimeter + "Quacks" + CloseDelimeter + " = :p1, " +
                OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + " = :p2" + Environment.NewLine +
                "WHERE " + OpenDelimeter + "Id" + CloseDelimeter + " IS NULL;" + Environment.NewLine +
                "SELECT " + RowsAffected + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        [Fact]
        public override void GenerateNextSequenceValueOperation_returns_statement_with_sanatized_sequence()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("sequence", null);

            Assert.Equal(
                "SELECT " + OpenDelimeter + "sequence" + CloseDelimeter + ".NEXTVAL" + " FROM DUAL",
                statement);
        }

        [Fact]
        public override void GenerateNextSequenceValueOperation_correctly_handles_schemas()
        {
            var statement = CreateSqlGenerator().GenerateNextSequenceValueOperation("mysequence", "SYSDBA");

            Assert.Equal(
                "SELECT " + SchemaPrefix + OpenDelimeter + "mysequence" + CloseDelimeter + ".NEXTVAL" +" FROM DUAL",
                statement);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: false, isComputed: false);

            var sqlGenerator = (IXuGuDbUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            Assert.Equal(
                "INSERT INTO `SYSDBA`.`Ducks` (`Id`, `Name`, `Quacks`, `ConcurrencyToken`)" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2, :p3)," + Environment.NewLine +
                "(:p0, :p1, :p2, :p3);" + Environment.NewLine,
                stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NoResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_store_generated_columns_exist_default_values_only()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true, defaultsOnly: true);

            var sqlGenerator = (IXuGuDbUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            var expectedText =
                "CREATE TEMPORARY TABLE IF NOT EXISTS `toInsert0` (`Id` integer, `Computed` guid);" + Environment.NewLine +
                "INSERT INTO `SYSDBA`.`Ducks` (`Id`)" + Environment.NewLine +
                "VALUES (DEFAULT);" + Environment.NewLine +
                "MERGE INTO `toInsert0` `i` USING (SELECT `Id`, `Computed` FROM `SYSDBA`.`Ducks`) `s` ON(1=0)" + Environment.NewLine +
                "WHEN NOT MATCHED THEN" + Environment.NewLine +
                "INSERT (`i`.`Id`, `i`.`Computed`)" + Environment.NewLine +
                "VALUES (`s`.`Id`, `s`.`Computed`);" + Environment.NewLine +
                "SELECT `Id`, `Computed` FROM `toInsert0`;" + Environment.NewLine;
            Assert.Equal(expectedText, stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NotLastInResultSet, grouping);
        }

        [Fact]
        public void AppendBulkInsertOperation_appends_insert_if_no_store_generated_columns_exist_default_values_only()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: false, isComputed: false, defaultsOnly: true);

            var sqlGenerator = (IXuGuDbUpdateSqlGenerator)CreateSqlGenerator();
            var grouping = sqlGenerator.AppendBulkInsertOperation(stringBuilder, new[] { command, command }, 0);

            var expectedText = "INSERT INTO `SYSDBA`.`Ducks`" + Environment.NewLine +
                               "DEFAULT VALUES;" + Environment.NewLine;
            Assert.Equal(expectedText + expectedText,
                stringBuilder.ToString());
            Assert.Equal(ResultSetMapping.NoResultSet, grouping);
        }

        [Fact]
        public new void AppendInsertOperation_appends_insert_and_select_and_where_if_store_generated_columns_exist()
        {
            var stringBuilder = new StringBuilder();
            var command = CreateInsertCommand(identityKey: true, isComputed: true);

            CreateSqlGenerator().AppendInsertOperation(stringBuilder, command, 0);

            Assert.Equal(
                "INSERT INTO " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + " (" + OpenDelimeter + "Name" + CloseDelimeter + ", " + OpenDelimeter + "Quacks" + CloseDelimeter + ", " + OpenDelimeter + "ConcurrencyToken" + CloseDelimeter + ")" + Environment.NewLine +
                "VALUES (:p0, :p1, :p2);" + Environment.NewLine +
                "SELECT " + OpenDelimeter + "Id" + CloseDelimeter + ", " + OpenDelimeter + "Computed" + CloseDelimeter + "" + Environment.NewLine +
                "FROM " + SchemaPrefix + OpenDelimeter + "Ducks" + CloseDelimeter + "" + Environment.NewLine +
                "WHERE " + RowsAffected + " = 1 AND " + OpenDelimeter + "Id" + CloseDelimeter + " = " + Identity + ";" + Environment.NewLine,
                stringBuilder.ToString());
        }

        protected override string RowsAffected => "SQL%ROWCOUNT";

        protected override string Identity { get; } = "LAST_INSERT_ID()";

        protected override string OpenDelimeter => "`";

        protected override string CloseDelimeter => "`";

        protected override string Schema => "SYSDBA";
    }
}
