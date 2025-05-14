// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public partial class XGMigrationsSqlGeneratorTest : MigrationsSqlGeneratorTestBase
    {
        public XGMigrationsSqlGeneratorTest()
            : base(
                XGTestHelpers.Instance,
                new ServiceCollection().AddEntityFrameworkXGNetTopologySuite(),
                XGTestHelpers.Instance.AddProviderOptions(
                    ((IRelationalDbContextOptionsBuilderInfrastructure)
                        new XGDbContextOptionsBuilder(new DbContextOptionsBuilder()).UseNetTopologySuite())
                    .OptionsBuilder).Options)
        {
        }

        protected /*override*/ virtual string Schema { get; } = null;

        public override void AddColumnOperation_with_unicode_overridden()
        {
            base.AddColumnOperation_with_unicode_overridden();

            AssertSql(
                @"ALTER TABLE `Person` ADD `Name` varchar NULL;");
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            base.AddColumnOperation_with_unicode_no_model();

            AssertSql(
                @"ALTER TABLE `Person` ADD `Name` varchar NULL;");
        }

        public override void AddColumnOperation_with_fixed_length_no_model()
        {
            base.AddColumnOperation_with_fixed_length_no_model();

            AssertSql(
                @"ALTER TABLE `Person` ADD `Name` varchar(100) NULL;");
        }

        public override void AddColumnOperation_with_maxLength_no_model()
        {
            base.AddColumnOperation_with_maxLength_no_model();

            AssertSql(
                @"ALTER TABLE `Person` ADD `Name` varchar(30) NULL;");
        }

        public override void AddColumnOperation_with_precision_and_scale_overridden()
        {
            base.AddColumnOperation_with_precision_and_scale_overridden();

            AssertSql(
                @"ALTER TABLE `Person` ADD `Pi` decimal(15,10) NOT NULL;");
        }

        public override void AddColumnOperation_with_precision_and_scale_no_model()
        {
            base.AddColumnOperation_with_precision_and_scale_no_model();

            AssertSql(
                @"ALTER TABLE `Person` ADD `Pi` decimal(20,7) NOT NULL;");
        }

        public override void AddForeignKeyOperation_without_principal_columns()
        {
            base.AddForeignKeyOperation_without_principal_columns();

            AssertSql(
                @"ALTER TABLE `People` ADD FOREIGN KEY (`SpouseId`) REFERENCES `People`;");
        }

        public override void AlterColumnOperation_without_column_type()
        {
            base.AlterColumnOperation_without_column_type();

            AssertSql(
                @"ALTER TABLE `People` MODIFY COLUMN `LuckyNumber` int NOT NULL;");
        }

        public override void RenameTableOperation_legacy()
        {
            base.RenameTableOperation_legacy();

            AssertSql(
                @"ALTER TABLE `People` RENAME TO `Person`;");
        }

        public override void RenameTableOperation()
        {
            base.RenameTableOperation();

            AssertSql(
                @"ALTER TABLE `People` RENAME TO `Person`;");
        }

        public override void InsertDataOperation_all_args_spatial()
        {
            base.InsertDataOperation_all_args_spatial();

            AssertSql(
                @"INSERT INTO `People` (`Id`, `Full Name`, `Geometry`)
VALUES (0, NULL, NULL),
(1, 'Daenerys Targaryen', NULL),
(2, 'John Snow', NULL),
(3, 'Arya Stark', NULL),
(4, 'Harry Strickland', NULL),
(5, 'The Imp', NULL),
(6, 'The Kingslayer', NULL),
(7, 'Aemon Targaryen', X'E61000000107000000080000000102000000040000009A9999999999F13F9A999999999901409A999999999901409A999999999901409A999999999901409A9999999999F13F6666666666661C40CDCCCCCCCCCC1C400102000000040000006666666666661C40CDCCCCCCCCCC1C403333333333333440333333333333344033333333333334409A9999999999F13F6666666666865140CDCCCCCCCC8C514001040000000300000001010000009A9999999999F13F9A9999999999014001010000009A999999999901409A9999999999014001010000009A999999999901409A9999999999F13F010300000001000000040000009A9999999999F13F9A999999999901409A999999999901409A999999999901409A999999999901409A9999999999F13F9A9999999999F13F9A99999999990140010300000001000000040000003333333333332440333333333333344033333333333334403333333333333440333333333333344033333333333324403333333333332440333333333333344001010000009A9999999999F13F9A999999999901400105000000020000000102000000040000009A9999999999F13F9A999999999901409A999999999901409A999999999901409A999999999901409A9999999999F13F6666666666661C40CDCCCCCCCCCC1C400102000000040000006666666666661C40CDCCCCCCCCCC1C403333333333333440333333333333344033333333333334409A9999999999F13F6666666666865140CDCCCCCCCC8C51400106000000020000000103000000010000000400000033333333333324403333333333333440333333333333344033333333333334403333333333333440333333333333244033333333333324403333333333333440010300000001000000040000009A9999999999F13F9A999999999901409A999999999901409A999999999901409A999999999901409A9999999999F13F9A9999999999F13F9A99999999990140');");
        }

        public override void InsertDataOperation_required_args()
        {
            base.InsertDataOperation_required_args();

            AssertSql(
                @"INSERT INTO `People` (`First Name`)
VALUES ('John');");
        }

        public override void InsertDataOperation_required_args_composite()
        {
            base.InsertDataOperation_required_args_composite();

            AssertSql(
                @"INSERT INTO `People` (`First Name`, `Last Name`)
VALUES ('John', 'Snow');");
        }

        public override void InsertDataOperation_required_args_multiple_rows()
        {
            base.InsertDataOperation_required_args_multiple_rows();

            AssertSql(
                @"INSERT INTO `People` (`First Name`)
VALUES ('John');
INSERT INTO `People` (`First Name`)
VALUES ('Daenerys');");
        }

        public override void InsertDataOperation_throws_for_unsupported_column_types()
        {
            base.InsertDataOperation_throws_for_unsupported_column_types();
        }

        public override void DeleteDataOperation_all_args()
        {
            base.DeleteDataOperation_all_args();

            if (AppConfig.ServerVersion.Supports.Returning)
            {
                AssertSql(
"""
DELETE FROM `People`
WHERE `First Name` = 'Hodor'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'Daenerys'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'John'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'Arya'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'Harry'
RETURNING 1;
""");
            }
            else
            {
                AssertSql(
                    @"DELETE FROM `People`
WHERE `First Name` = 'Hodor';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'Daenerys';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'John';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'Arya';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'Harry';
SELECT ROWNUM;
--GO");
            }
        }

        public override void DeleteDataOperation_all_args_composite()
        {
            base.DeleteDataOperation_all_args_composite();

            if (AppConfig.ServerVersion.Supports.Returning)
            {
                AssertSql(
"""
DELETE FROM `People`
WHERE `First Name` = 'Hodor' AND `Last Name` IS NULL
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'Daenerys' AND `Last Name` = 'Targaryen'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'John' AND `Last Name` = 'Snow'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'Arya' AND `Last Name` = 'Stark'
RETURNING 1;
DELETE FROM `People`
WHERE `First Name` = 'Harry' AND `Last Name` = 'Strickland'
RETURNING 1;
""");
            }
            else
            {
                AssertSql(
                    @"DELETE FROM `People`
WHERE `First Name` = 'Hodor' AND `Last Name` IS NULL;
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'Daenerys' AND `Last Name` = 'Targaryen';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'John' AND `Last Name` = 'Snow';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'Arya' AND `Last Name` = 'Stark';
SELECT ROWNUM;
--GO
DELETE FROM `People`
WHERE `First Name` = 'Harry' AND `Last Name` = 'Strickland';
SELECT ROWNUM;
--GO");
            }
        }

        public override void DeleteDataOperation_required_args()
        {
            base.DeleteDataOperation_required_args();

            if (AppConfig.ServerVersion.Supports.Returning)
            {
                AssertSql(
"""
DELETE FROM `People`
WHERE `Last Name` = 'Snow'
RETURNING 1;
""");
            }
            else
            {
                AssertSql(
                    @"DELETE FROM `People`
WHERE `Last Name` = 'Snow';
SELECT ROWNUM;
--GO");
            }
        }

        public override void DeleteDataOperation_required_args_composite()
        {
            base.DeleteDataOperation_required_args_composite();

            if (AppConfig.ServerVersion.Supports.Returning)
            {
                AssertSql(
"""
DELETE FROM `People`
WHERE `First Name` = 'John' AND `Last Name` = 'Snow'
RETURNING 1;
""");
            }
            else
            {
                AssertSql(
                    @"DELETE FROM `People`
WHERE `First Name` = 'John' AND `Last Name` = 'Snow';
SELECT ROWNUM;
--GO");
            }

        }

        public override void UpdateDataOperation_all_args()
        {
            base.UpdateDataOperation_all_args();

            AssertSql(
                @"UPDATE `People` SET `Birthplace` = 'Winterfell', `House Allegiance` = 'Stark', `Culture` = 'Northmen'
WHERE `First Name` = 'Hodor';
SELECT ROWNUM;
--GO
UPDATE `People` SET `Birthplace` = 'Dragonstone', `House Allegiance` = 'Targaryen', `Culture` = 'Valyrian'
WHERE `First Name` = 'Daenerys';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_all_args_composite()
        {
            base.UpdateDataOperation_all_args_composite();

            AssertSql(
                @"UPDATE `People` SET `House Allegiance` = 'Stark'
WHERE `First Name` = 'Hodor' AND `Last Name` IS NULL;
SELECT ROWNUM;
--GO
UPDATE `People` SET `House Allegiance` = 'Targaryen'
WHERE `First Name` = 'Daenerys' AND `Last Name` = 'Targaryen';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_all_args_composite_multi()
        {
            base.UpdateDataOperation_all_args_composite_multi();

            AssertSql(
                @"UPDATE `People` SET `Birthplace` = 'Winterfell', `House Allegiance` = 'Stark', `Culture` = 'Northmen'
WHERE `First Name` = 'Hodor' AND `Last Name` IS NULL;
SELECT ROWNUM;
--GO
UPDATE `People` SET `Birthplace` = 'Dragonstone', `House Allegiance` = 'Targaryen', `Culture` = 'Valyrian'
WHERE `First Name` = 'Daenerys' AND `Last Name` = 'Targaryen';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_all_args_multi()
        {
            base.UpdateDataOperation_all_args_multi();

            AssertSql(
                @"UPDATE `People` SET `Birthplace` = 'Dragonstone', `House Allegiance` = 'Targaryen', `Culture` = 'Valyrian'
WHERE `First Name` = 'Daenerys';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_required_args()
        {
            base.UpdateDataOperation_required_args();

            AssertSql(
                @"UPDATE `People` SET `House Allegiance` = 'Targaryen'
WHERE `First Name` = 'Daenerys';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_required_args_multiple_rows()
        {
            base.UpdateDataOperation_required_args_multiple_rows();

            AssertSql(
                @"UPDATE `People` SET `House Allegiance` = 'Stark'
WHERE `First Name` = 'Hodor';
SELECT ROWNUM;
--GO
UPDATE `People` SET `House Allegiance` = 'Targaryen'
WHERE `First Name` = 'Daenerys';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_required_args_composite()
        {
            base.UpdateDataOperation_required_args_composite();

            AssertSql(
                @"UPDATE `People` SET `House Allegiance` = 'Targaryen'
WHERE `First Name` = 'Daenerys' AND `Last Name` = 'Targaryen';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_required_args_composite_multi()
        {
            base.UpdateDataOperation_required_args_composite_multi();

            AssertSql(
                @"UPDATE `People` SET `Birthplace` = 'Dragonstone', `House Allegiance` = 'Targaryen', `Culture` = 'Valyrian'
WHERE `First Name` = 'Daenerys' AND `Last Name` = 'Targaryen';
SELECT ROWNUM;
--GO");
        }

        public override void UpdateDataOperation_required_args_multi()
        {
            base.UpdateDataOperation_required_args_multi();

            AssertSql(
                @"UPDATE `People` SET `Birthplace` = 'Dragonstone', `House Allegiance` = 'Targaryen', `Culture` = 'Valyrian'
WHERE `First Name` = 'Daenerys';
SELECT ROWNUM;
--GO");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public override void DefaultValue_with_line_breaks(bool isUnicode)
        {
            base.DefaultValue_with_line_breaks(isUnicode);

            AssertSql(
                @"CREATE TABLE `dbo`.`TestLineBreaks` (
    `TestDefaultValue` varchar DEFAULT CONCAT('', CHR(13) || CHR(10), 'Various Line', CHR(13), 'Breaks', CHR(10), '') NOT NULL
);");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public override void DefaultValue_with_line_breaks_2(bool isUnicode)
        {
            base.DefaultValue_with_line_breaks_2(isUnicode);

        AssertSql(
            @"CREATE TABLE `dbo`.`TestLineBreaks` (
    `TestDefaultValue` varchar DEFAULT CONCAT('0', CHR(13) || CHR(10), '1', CHR(13) || CHR(10), '2', CHR(13) || CHR(10), '3', CHR(13) || CHR(10), '4', CHR(13) || CHR(10), '5', CHR(13) || CHR(10), '6', CHR(13) || CHR(10), '7', CHR(13) || CHR(10), '8', CHR(13) || CHR(10), '9', CHR(13) || CHR(10), '10', CHR(13) || CHR(10), '11', CHR(13) || CHR(10), '12', CHR(13) || CHR(10), '13', CHR(13) || CHR(10), '14', CHR(13) || CHR(10), '15', CHR(13) || CHR(10), '16', CHR(13) || CHR(10), '17', CHR(13) || CHR(10), '18', CHR(13) || CHR(10), '19', CHR(13) || CHR(10), '20', CHR(13) || CHR(10), '21', CHR(13) || CHR(10), '22', CHR(13) || CHR(10), '23', CHR(13) || CHR(10), '24', CHR(13) || CHR(10), '25', CHR(13) || CHR(10), '26', CHR(13) || CHR(10), '27', CHR(13) || CHR(10), '28', CHR(13) || CHR(10), '29', CHR(13) || CHR(10), '30', CHR(13) || CHR(10), '31', CHR(13) || CHR(10), '32', CHR(13) || CHR(10), '33', CHR(13) || CHR(10), '34', CHR(13) || CHR(10), '35', CHR(13) || CHR(10), '36', CHR(13) || CHR(10), '37', CHR(13) || CHR(10), '38', CHR(13) || CHR(10), '39', CHR(13) || CHR(10), '40', CHR(13) || CHR(10), '41', CHR(13) || CHR(10), '42', CHR(13) || CHR(10), '43', CHR(13) || CHR(10), '44', CHR(13) || CHR(10), '45', CHR(13) || CHR(10), '46', CHR(13) || CHR(10), '47', CHR(13) || CHR(10), '48', CHR(13) || CHR(10), '49', CHR(13) || CHR(10), '50', CHR(13) || CHR(10), '51', CHR(13) || CHR(10), '52', CHR(13) || CHR(10), '53', CHR(13) || CHR(10), '54', CHR(13) || CHR(10), '55', CHR(13) || CHR(10), '56', CHR(13) || CHR(10), '57', CHR(13) || CHR(10), '58', CHR(13) || CHR(10), '59', CHR(13) || CHR(10), '60', CHR(13) || CHR(10), '61', CHR(13) || CHR(10), '62', CHR(13) || CHR(10), '63', CHR(13) || CHR(10), '64', CHR(13) || CHR(10), '65', CHR(13) || CHR(10), '66', CHR(13) || CHR(10), '67', CHR(13) || CHR(10), '68', CHR(13) || CHR(10), '69', CHR(13) || CHR(10), '70', CHR(13) || CHR(10), '71', CHR(13) || CHR(10), '72', CHR(13) || CHR(10), '73', CHR(13) || CHR(10), '74', CHR(13) || CHR(10), '75', CHR(13) || CHR(10), '76', CHR(13) || CHR(10), '77', CHR(13) || CHR(10), '78', CHR(13) || CHR(10), '79', CHR(13) || CHR(10), '80', CHR(13) || CHR(10), '81', CHR(13) || CHR(10), '82', CHR(13) || CHR(10), '83', CHR(13) || CHR(10), '84', CHR(13) || CHR(10), '85', CHR(13) || CHR(10), '86', CHR(13) || CHR(10), '87', CHR(13) || CHR(10), '88', CHR(13) || CHR(10), '89', CHR(13) || CHR(10), '90', CHR(13) || CHR(10), '91', CHR(13) || CHR(10), '92', CHR(13) || CHR(10), '93', CHR(13) || CHR(10), '94', CHR(13) || CHR(10), '95', CHR(13) || CHR(10), '96', CHR(13) || CHR(10), '97', CHR(13) || CHR(10), '98', CHR(13) || CHR(10), '99', CHR(13) || CHR(10), '100', CHR(13) || CHR(10), '101', CHR(13) || CHR(10), '102', CHR(13) || CHR(10), '103', CHR(13) || CHR(10), '104', CHR(13) || CHR(10), '105', CHR(13) || CHR(10), '106', CHR(13) || CHR(10), '107', CHR(13) || CHR(10), '108', CHR(13) || CHR(10), '109', CHR(13) || CHR(10), '110', CHR(13) || CHR(10), '111', CHR(13) || CHR(10), '112', CHR(13) || CHR(10), '113', CHR(13) || CHR(10), '114', CHR(13) || CHR(10), '115', CHR(13) || CHR(10), '116', CHR(13) || CHR(10), '117', CHR(13) || CHR(10), '118', CHR(13) || CHR(10), '119', CHR(13) || CHR(10), '120', CHR(13) || CHR(10), '121', CHR(13) || CHR(10), '122', CHR(13) || CHR(10), '123', CHR(13) || CHR(10), '124', CHR(13) || CHR(10), '125', CHR(13) || CHR(10), '126', CHR(13) || CHR(10), '127', CHR(13) || CHR(10), '128', CHR(13) || CHR(10), '129', CHR(13) || CHR(10), '130', CHR(13) || CHR(10), '131', CHR(13) || CHR(10), '132', CHR(13) || CHR(10), '133', CHR(13) || CHR(10), '134', CHR(13) || CHR(10), '135', CHR(13) || CHR(10), '136', CHR(13) || CHR(10), '137', CHR(13) || CHR(10), '138', CHR(13) || CHR(10), '139', CHR(13) || CHR(10), '140', CHR(13) || CHR(10), '141', CHR(13) || CHR(10), '142', CHR(13) || CHR(10), '143', CHR(13) || CHR(10), '144', CHR(13) || CHR(10), '145', CHR(13) || CHR(10), '146', CHR(13) || CHR(10), '147', CHR(13) || CHR(10), '148', CHR(13) || CHR(10), '149', CHR(13) || CHR(10), '150', CHR(13) || CHR(10), '151', CHR(13) || CHR(10), '152', CHR(13) || CHR(10), '153', CHR(13) || CHR(10), '154', CHR(13) || CHR(10), '155', CHR(13) || CHR(10), '156', CHR(13) || CHR(10), '157', CHR(13) || CHR(10), '158', CHR(13) || CHR(10), '159', CHR(13) || CHR(10), '160', CHR(13) || CHR(10), '161', CHR(13) || CHR(10), '162', CHR(13) || CHR(10), '163', CHR(13) || CHR(10), '164', CHR(13) || CHR(10), '165', CHR(13) || CHR(10), '166', CHR(13) || CHR(10), '167', CHR(13) || CHR(10), '168', CHR(13) || CHR(10), '169', CHR(13) || CHR(10), '170', CHR(13) || CHR(10), '171', CHR(13) || CHR(10), '172', CHR(13) || CHR(10), '173', CHR(13) || CHR(10), '174', CHR(13) || CHR(10), '175', CHR(13) || CHR(10), '176', CHR(13) || CHR(10), '177', CHR(13) || CHR(10), '178', CHR(13) || CHR(10), '179', CHR(13) || CHR(10), '180', CHR(13) || CHR(10), '181', CHR(13) || CHR(10), '182', CHR(13) || CHR(10), '183', CHR(13) || CHR(10), '184', CHR(13) || CHR(10), '185', CHR(13) || CHR(10), '186', CHR(13) || CHR(10), '187', CHR(13) || CHR(10), '188', CHR(13) || CHR(10), '189', CHR(13) || CHR(10), '190', CHR(13) || CHR(10), '191', CHR(13) || CHR(10), '192', CHR(13) || CHR(10), '193', CHR(13) || CHR(10), '194', CHR(13) || CHR(10), '195', CHR(13) || CHR(10), '196', CHR(13) || CHR(10), '197', CHR(13) || CHR(10), '198', CHR(13) || CHR(10), '199', CHR(13) || CHR(10), '200', CHR(13) || CHR(10), '201', CHR(13) || CHR(10), '202', CHR(13) || CHR(10), '203', CHR(13) || CHR(10), '204', CHR(13) || CHR(10), '205', CHR(13) || CHR(10), '206', CHR(13) || CHR(10), '207', CHR(13) || CHR(10), '208', CHR(13) || CHR(10), '209', CHR(13) || CHR(10), '210', CHR(13) || CHR(10), '211', CHR(13) || CHR(10), '212', CHR(13) || CHR(10), '213', CHR(13) || CHR(10), '214', CHR(13) || CHR(10), '215', CHR(13) || CHR(10), '216', CHR(13) || CHR(10), '217', CHR(13) || CHR(10), '218', CHR(13) || CHR(10), '219', CHR(13) || CHR(10), '220', CHR(13) || CHR(10), '221', CHR(13) || CHR(10), '222', CHR(13) || CHR(10), '223', CHR(13) || CHR(10), '224', CHR(13) || CHR(10), '225', CHR(13) || CHR(10), '226', CHR(13) || CHR(10), '227', CHR(13) || CHR(10), '228', CHR(13) || CHR(10), '229', CHR(13) || CHR(10), '230', CHR(13) || CHR(10), '231', CHR(13) || CHR(10), '232', CHR(13) || CHR(10), '233', CHR(13) || CHR(10), '234', CHR(13) || CHR(10), '235', CHR(13) || CHR(10), '236', CHR(13) || CHR(10), '237', CHR(13) || CHR(10), '238', CHR(13) || CHR(10), '239', CHR(13) || CHR(10), '240', CHR(13) || CHR(10), '241', CHR(13) || CHR(10), '242', CHR(13) || CHR(10), '243', CHR(13) || CHR(10), '244', CHR(13) || CHR(10), '245', CHR(13) || CHR(10), '246', CHR(13) || CHR(10), '247', CHR(13) || CHR(10), '248', CHR(13) || CHR(10), '249', CHR(13) || CHR(10), '250', CHR(13) || CHR(10), '251', CHR(13) || CHR(10), '252', CHR(13) || CHR(10), '253', CHR(13) || CHR(10), '254', CHR(13) || CHR(10), '255', CHR(13) || CHR(10), '256', CHR(13) || CHR(10), '257', CHR(13) || CHR(10), '258', CHR(13) || CHR(10), '259', CHR(13) || CHR(10), '260', CHR(13) || CHR(10), '261', CHR(13) || CHR(10), '262', CHR(13) || CHR(10), '263', CHR(13) || CHR(10), '264', CHR(13) || CHR(10), '265', CHR(13) || CHR(10), '266', CHR(13) || CHR(10), '267', CHR(13) || CHR(10), '268', CHR(13) || CHR(10), '269', CHR(13) || CHR(10), '270', CHR(13) || CHR(10), '271', CHR(13) || CHR(10), '272', CHR(13) || CHR(10), '273', CHR(13) || CHR(10), '274', CHR(13) || CHR(10), '275', CHR(13) || CHR(10), '276', CHR(13) || CHR(10), '277', CHR(13) || CHR(10), '278', CHR(13) || CHR(10), '279', CHR(13) || CHR(10), '280', CHR(13) || CHR(10), '281', CHR(13) || CHR(10), '282', CHR(13) || CHR(10), '283', CHR(13) || CHR(10), '284', CHR(13) || CHR(10), '285', CHR(13) || CHR(10), '286', CHR(13) || CHR(10), '287', CHR(13) || CHR(10), '288', CHR(13) || CHR(10), '289', CHR(13) || CHR(10), '290', CHR(13) || CHR(10), '291', CHR(13) || CHR(10), '292', CHR(13) || CHR(10), '293', CHR(13) || CHR(10), '294', CHR(13) || CHR(10), '295', CHR(13) || CHR(10), '296', CHR(13) || CHR(10), '297', CHR(13) || CHR(10), '298', CHR(13) || CHR(10), '299', CHR(13) || CHR(10), '') NOT NULL
);");
        }

        [ConditionalFact]
        [SupportedServerVersionLessThanCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public virtual void DefaultValue_not_generated_for_unlimited_text_column_missing_default_expression_support()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "History",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Table = "History",
                            Name = "Event",
                            ClrType = typeof(string),
                            DefaultValue = "The Battle of Waterloo"
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `History` (
    `Event` varchar NOT NULL
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public virtual void DefaultValue_generated_for_unlimited_text_column()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "History",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Table = "History",
                            Name = "Event",
                            ClrType = typeof(string),
                            DefaultValue = "The Battle of Waterloo"
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`History` (
    `Event` varchar DEFAULT 'The Battle of Waterloo' NOT NULL
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void DefaultValue_generated_for_limited_text_column()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "History",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Table = "History",
                            Name = "Event",
                            ClrType = typeof(string),
                            MaxLength = 128,
                            DefaultValue = "The Battle of Waterloo"
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`History` (
    `Event` varchar(128) DEFAULT 'The Battle of Waterloo' NOT NULL
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void DefaultValue_formats_literal_correctly()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "History",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Event",
                            ClrType = typeof(string),
                            ColumnType = "VARCHAR(255)",
                            DefaultValue = new DateTime(2015, 4, 12, 17, 5, 0)
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`History` (
    `Event` VARCHAR(255) DEFAULT '2015-04-12 17:05:00' NOT NULL
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new XGCreateDatabaseOperation { Name = "Northwind" });

            Assert.Equal(
                @"CREATE DATABASE `Northwind`;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation_with_charset()
        {
            Generate(new XGCreateDatabaseOperation { Name = "Northwind", CharSet = "latin1"});

            Assert.Equal(
                @"CREATE DATABASE `Northwind` CHARACTER SET latin1;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateDatabaseOperation_with_collation()
        {
            Generate(new XGCreateDatabaseOperation { Name = "Northwind", Collation = "latin1_general_ci"});

            Assert.Equal(
                @"CREATE DATABASE `Northwind`;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AlterDatabaseOperation_with_charset()
        {
            Generate(
                new XGCreateDatabaseOperation {Name = "Northwind", CharSet = "latin1"},
                new AlterDatabaseOperation {[XGAnnotationNames.CharSet] = "utf8mb4"});

            Assert.Equal(
                @"CREATE DATABASE `Northwind` CHARACTER SET latin1;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AlterDatabaseOperation_with_collation()
        {
            Generate(
                new XGCreateDatabaseOperation {Name = "Northwind", Collation = "latin1_general_ci"},
                new AlterDatabaseOperation {Collation = "latin1_swedish_ci"});

            Assert.Equal(
                @"CREATE DATABASE `Northwind`;"+ EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateTableUlongAutoincrement()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "TestUlongAutoIncrement",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Id",
                            Table = "TestUlongAutoIncrement",
                            ClrType = typeof(ulong),
                            ColumnType = "bigint unsigned",
                            IsNullable = false,
                            [XGAnnotationNames.ValueGenerationStrategy] = XGValueGenerationStrategy.IdentityColumn
                        }
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Id" }
                    }
                });

            Assert.Equal(
                "CREATE TABLE `SYSDBA`.`TestUlongAutoIncrement` (" + EOL +
                "    `Id` bigint unsigned IDENTITY NOT NULL," + EOL +
                "    PRIMARY KEY (`Id`)" + EOL +
                ");" + EOL,
                Sql);
        }

        [ConditionalTheory]
        [InlineData(false, false, "Latin1")]
        [InlineData(false, false, null)]
        [InlineData(false, true, "Latin1")]
        [InlineData(false, true, null)]
        [InlineData(null, false, "Latin1")]
        [InlineData(null, false, "Utf8Mb4")]
        [InlineData(null, false, null)]
        [InlineData(null, true, "Latin1")]
        [InlineData(null, true, "Utf8Mb4")]
        [InlineData(null, true, null)]
        [InlineData(true, false, "Latin1")]
        [InlineData(true, false, null)]
        [InlineData(true, true, "Latin1")]
        [InlineData(true, true, null)]
        public virtual void AddColumnOperation_with_charset_implicit(bool? isUnicode, bool isIndex, string charSetName)
        {
            var charSet = CharSet.GetCharSetFromName(charSetName);

            Generate(
                modelBuilder =>
                {
                    modelBuilder.HasCharSet(charSet);
                    modelBuilder.Entity(
                        "Person", eb =>
                        {
                            eb.Property<int>("Id");

                            var pb = eb.Property<string>("Name");

                            if (isUnicode.HasValue)
                            {
                                pb.IsUnicode(isUnicode.Value);
                            }

                            if (isIndex)
                            {
                                eb.HasIndex("Name");
                            }
                        });
                },
                modelBuilder =>
                {
                    var addColumn = modelBuilder.AddColumn<string>(name: "Name", table: "Person", nullable: true, unicode: isUnicode);

                    if (charSet != null)
                    {
                        addColumn.Annotation(XGAnnotationNames.CharSet, charSet);
                    }
                }
            );

            var columnType = /*isIndex
                ? $"varchar({XGTestHelpers.Instance.GetIndexedStringPropertyDefaultLength})"
                : */"varchar";

            Assert.Equal(
                $"ALTER TABLE `Person` ADD `Name` {columnType} NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public override void AddColumnOperation_without_column_type()
        {
            base.AddColumnOperation_without_column_type();

            Assert.Equal(
                @"ALTER TABLE `People` ADD `Alias` varchar NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_datetime6()
        {
            Generate(new AddColumnOperation
            {
                Table = "People",
                Name = "Birthday",
                ClrType = typeof(DateTime),
                ColumnType = "timestamp(6)",
                IsNullable = false,
                DefaultValue = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
            });

            Assert.Equal(
                "ALTER TABLE `People` ADD `Birthday` timestamp(6) DEFAULT '" +
                new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified).ToString("yyyy-MM-dd HH:mm:ss.FFFFFF") +
                "' NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public override void AddColumnOperation_with_maxLength_overridden()
        {
            base.AddColumnOperation_with_maxLength_overridden();

            Assert.Equal(
                @"ALTER TABLE `Person` ADD `Name` varchar(32) NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_computed_column()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "timestamp",
                    IsNullable = true,
                    [XGAnnotationNames.ValueGenerationStrategy] = XGValueGenerationStrategy.ComputedColumn
                });

            Assert.Equal(
                @"ALTER TABLE `People` ADD `Birthday` timestamp DEFAULT CURRENT_TIMESTAMP NULL;" +
                EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_serial()
        {
            Generate(new AddColumnOperation
            {
                Table = "People",
                Name = "foo",
                ClrType = typeof(int),
                ColumnType = "int",
                IsNullable = false,
                [XGAnnotationNames.ValueGenerationStrategy] = XGValueGenerationStrategy.IdentityColumn
            });

            Assert.Equal(
                "ALTER TABLE `People` ADD `foo` int IDENTITY NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_int_defaultValue_isnt_serial()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "foo",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    DefaultValue = 8
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `foo` int DEFAULT 8 NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddColumnOperation_with_dbgenerated_uuid()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "foo",
                    ClrType = typeof(Guid),
                    ColumnType = "varchar(38)",
                    [XGAnnotationNames.ValueGenerationStrategy] = XGValueGenerationStrategy.IdentityColumn
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `foo` varchar(38) NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddDefaultDatetimeOperation_with_valueOnUpdate()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Birthday",
                    ClrType = typeof(DateTime),
                    ColumnType = "timestamp(6)",
                    IsNullable = true,
                    [XGAnnotationNames.ValueGenerationStrategy] = XGValueGenerationStrategy.ComputedColumn
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `Birthday` timestamp(6) DEFAULT CURRENT_TIMESTAMP NULL;" +
                EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddDefaultBooleanOperation()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "IsLeader",
                    ClrType = typeof(bool),
                    ColumnType = "bit",
                    IsNullable = true,
                    DefaultValue = true
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `IsLeader` bit DEFAULT 1 NULL;" + EOL,
                Sql);
        }


        [ConditionalTheory]
        [InlineData("tinyblob")]
        [InlineData("blob")]
        [InlineData("mediumblob")]
        [InlineData("longblob")]
        [InlineData("tinytext")]
        [InlineData("text")]
        [InlineData("mediumtext")]
        [InlineData("longtext")]
        [InlineData("geometry")]
        [InlineData("point")]
        [InlineData("linestring")]
        [InlineData("polygon")]
        [InlineData("multipoint")]
        [InlineData("multilinestring")]
        [InlineData("multipolygon")]
        [InlineData("geometrycollection")]
        [InlineData("json")]
        public void AlterColumnOperation_with_no_default_value_column_types(string type)
        {
            Generate(
                builder =>
                {
                    ((Model)builder.Model).SetProductVersion("2.1.0");
                },
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Blob",
                    ClrType = typeof(string),
                    ColumnType = type,
                    OldColumn = new AddColumnOperation
                    {
                        ColumnType = type,
                    },
                    IsNullable = true,
                });

            Assert.Equal(
                $"ALTER TABLE `People` MODIFY COLUMN `Blob` {type} NULL;" + EOL,
                Sql);

            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Blob",
                    ClrType = typeof(string),
                    ColumnType = type,
                    OldColumn = new AddColumnOperation
                    {
                        ColumnType = "varchar(127)",
                    },
                    IsNullable = true,
                });

            Assert.Equal(
                $"ALTER TABLE `People` MODIFY COLUMN `Blob` {type} NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public void AlterColumnOperation_type_with_index()
        {
            Generate(
                builder =>
                {
                    ((Model)builder.Model).SetProductVersion("2.1.0");
                    builder.Entity("People", eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<string>("Blob");
                        eb.HasIndex("Blob");
                    });
                },
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Blob",
                    ClrType = typeof(string),
                    ColumnType = "char(127)",
                    OldColumn = new AddColumnOperation
                    {
                        ColumnType = "varchar(127)",
                    },
                    IsNullable = true
                });

            Assert.Equal(
                "ALTER TABLE `People` MODIFY COLUMN `Blob` char(127) NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public void AlterColumnOperation_ComputedColumnSql_with_index()
        {
            Generate(
                builder =>
                {
                    ((Model)builder.Model).SetProductVersion("2.1.0");
                    builder.Entity("People", eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<string>("Blob");
                        eb.HasIndex("Blob");
                    });
                },
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Blob",
                    ClrType = typeof(string),
                    ComputedColumnSql = "'TEST'",
                    ColumnType = "varchar(95)"
                });

            Assert.Equal(
                "ALTER TABLE `People` MODIFY COLUMN `Blob` varchar(95) NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public void AlterColumnOperation_ComputedColumnSql_stored()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "Universes",
                    Name = "AnswerToEverything",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    ComputedColumnSql = "6 * 9",
                    IsStored = true,
                });

            Assert.Equal(
                "ALTER TABLE `Universes` ADD `AnswerToEverything` int NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void AddForeignKeyOperation_with_long_name()
        {
            Generate(
                new AddForeignKeyOperation
                {
                    Table = "People",
                    Name = "FK_ASuperLongForeignKeyNameThatIsDefinetelyNotGoingToFitInThe64CharactersLimit",
                    Columns = new[] { "EmployerId1", "EmployerId2" },
                    PrincipalTable = "Companies",
                    PrincipalColumns = new[] { "Id1", "Id2" },
                    OnDelete = ReferentialAction.Cascade
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD CONSTRAINT `FK_ASuperLongForeignKeyNameThatIsDefinetelyNotGoingToFitInThe64C` FOREIGN KEY (`EmployerId1`, `EmployerId2`) REFERENCES `Companies` (`Id1`, `Id2`) ON DELETE CASCADE;" +
                EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_fulltext()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "FirstName", "LastName" },
                    [XGAnnotationNames.FullTextIndex] = true
                });

            Assert.Equal(
                "CREATE INDEX `IX_People_Name` ON `People` (`FirstName`, `LastName`);" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_fulltext_with_parser()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "FirstName", "LastName" },
                    [XGAnnotationNames.FullTextIndex] = true,
                    [XGAnnotationNames.FullTextParser] = "ngram",
                });

            Assert.Equal(
                "CREATE INDEX `IX_People_Name` ON `People` (`FirstName`, `LastName`) /*!50700 WITH PARSER `ngram` */;" + EOL,
                Sql);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.SpatialIndexes))]
        public virtual void CreateIndexOperation_spatial()
        {
            // TODO: Use meaningful column names.
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "FirstName", "LastName" },
                    [XGAnnotationNames.SpatialIndex] = true
                });

            Assert.Equal(
                "CREATE SPATIAL INDEX `IX_People_Name` ON `People` (`FirstName`, `LastName`);" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_long_name()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_ASuperLongForeignKeyNameThatIsDefinetelyNotGoingToFitInThe64CharactersLimit",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = false
                });

            Assert.Equal(
                "CREATE INDEX `IX_ASuperLongForeignKeyNameThatIsDefinetelyNotGoingToFitInThe64C` ON `People` (`Name`);" + EOL,
                Sql);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.RenameIndex))]
        public virtual void RenameIndexOperation()
        {
            var migrationBuilder = new MigrationBuilder("XG");

            migrationBuilder.RenameIndex(
                table: "Person",
                name: "IX_Person_Name",
                newName: "IX_Person_FullName");

            Generate(migrationBuilder.Operations.ToArray());

            Assert.Equal(
                @"ALTER INDEX `Person`.`IX_Person_Name` RENAME TO `IX_Person_FullName`;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void RenameIndexOperations_throws_when_no_table()
        {
            var migrationBuilder = new MigrationBuilder("XG");

            migrationBuilder.RenameIndex(
                name: "IX_OldIndex",
                newName: "IX_NewIndex");

            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(migrationBuilder.Operations.ToArray()));

            Assert.Equal(XGStrings.IndexTableRequired, ex.Message);
        }

        [ConditionalFact]
        public virtual void DropIndexOperations_throws_when_no_table()
        {
            var migrationBuilder = new MigrationBuilder("XG");

            migrationBuilder.DropIndex(
                name: "IX_Name");

            var ex = Assert.Throws<InvalidOperationException>(
                () => Generate(migrationBuilder.Operations.ToArray()));

            Assert.Equal(XGStrings.IndexTableRequired, ex.Message);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.RenameColumn))]
        public virtual void RenameColumnOperation()
        {
            var migrationBuilder = new MigrationBuilder("XG");

            migrationBuilder.RenameColumn(
                table: "Person",
                name: "Name",
                newName: "FullName");

            Generate(migrationBuilder.Operations.ToArray());

            Assert.Equal(
                "ALTER TABLE `Person` RENAME COLUMN `Name` TO `FullName`;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void RenameColumnOperation_with_model()
        {
            var migrationBuilder = new MigrationBuilder("XG");

            migrationBuilder.RenameColumn(
                table: "Person",
                name: "Name",
                newName: "FullName");

            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("FullName");
                    }),
                migrationBuilder.Operations.ToArray());

            Assert.Equal(
                AppConfig.ServerVersion.Supports.RenameColumn
                    ? "ALTER TABLE `Person` RENAME COLUMN `Name` TO `FullName`;" + EOL
                    : "ALTER TABLE `Person` CHANGE `Name` `FullName` varchar NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void RenameColumnOperation_with_model_required_without_default_value()
        {
            var migrationBuilder = new MigrationBuilder("XG");

            migrationBuilder.RenameColumn(
                table: "Person",
                name: "Name",
                newName: "FullName");

            Generate(
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<string>("FullName")
                            .HasMaxLength(64)
                            .IsRequired();
                    }),
                migrationBuilder.Operations.ToArray());

            Assert.Equal(
                AppConfig.ServerVersion.Supports.RenameColumn
                    ? "ALTER TABLE `Person` RENAME COLUMN `Name` TO `FullName`;" + EOL
                    : "ALTER TABLE `Person` CHANGE `Name` `FullName` varchar(64) NOT NULL;" + EOL,
                Sql);
        }

        [ConditionalFact]
        public override void SqlOperation()
        {
            base.SqlOperation();

            Assert.Equal(
                "-- I <3 DDL" + EOL,
                Sql);
        }

        protected override string GetGeometryCollectionStoreType()
            => "geometrycollection";

        [ConditionalFact]
        public virtual void AddColumnOperation_with_charset_annotation()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Name",
                    ClrType = typeof(string),
                    ColumnType = "varchar(255)",
                    IsNullable = true,
                    [XGAnnotationNames.CharSet] = CharSet.SJis,
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `Name` varchar(255) NULL;" +
                EOL,
                Sql);
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_with_prefix_lengths()
        {
            Generate(
                builder => builder.Entity(
                    "IceCreams",
                    entity =>
                    {
                        entity.Property<int>("IceCreamId");
                        entity.Property<string>("Name")
                            .HasMaxLength(255);
                        entity.Property<string>("Brand");

                        entity.HasKey("IceCreamId");
                    }),
                new CreateIndexOperation
                {
                    Name = "IX_IceCreams_Brand_Name",
                    Table = "IceCreams",
                    Columns = new[] { "Name", "Brand" },
                    [XGAnnotationNames.IndexPrefixLength] = new [] { 0, 20 }
                });

            Assert.Equal(
                @"CREATE INDEX `IX_IceCreams_Brand_Name` ON `IceCreams` (`Name`, `Brand`);" + EOL,
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_with_collation()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreams",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Brand",
                            ColumnType = "varchar",
                            ClrType = typeof(string),
                            Collation = "latin1_swedish_ci"
                        },
                        new AddColumnOperation
                        {
                            Name = "Name",
                            ColumnType = "varchar(255)",
                            ClrType = typeof(string),
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Name", "Brand" },
                        [XGAnnotationNames.IndexPrefixLength] = new [] { 0, 20 }
                    },
                    [RelationalAnnotationNames.Collation] = "latin1_general_ci",
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreams` (
    `Brand` varchar NOT NULL,
    `Name` varchar(255) NOT NULL,
    PRIMARY KEY (`Name`, `Brand`(20))
);" + EOL,
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void AlterTableOperation_with_collation()
        {
            Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "IceCreams",
                        entity =>
                        {
                            entity.Property<int>("Id");

                            entity.Property<string>("Brand")
                                .HasColumnType("longtext")
                                .UseCollation("latin1_swedish_ci");

                            entity.Property<string>("Name")
                                .HasColumnType("varchar(255)");

                            entity.UseCollation("latin1_general_ci");
                        });
                },
                migrationBuilder =>
                {
                    migrationBuilder.AlterTable("IceCreams")
                        .OldAnnotation(RelationalAnnotationNames.Collation, "latin1_general_ci")
                        .Annotation(RelationalAnnotationNames.Collation, "latin1_general_cs");
                });

            Assert.Equal("",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void AlterTableOperation_with_collation_reset()
        {
            Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "IceCreams",
                        entity =>
                        {
                            entity.Property<int>("Id");

                            entity.Property<string>("Brand")
                                .HasColumnType("longtext")
                                .UseCollation("latin1_swedish_ci");

                            entity.Property<string>("Name")
                                .HasColumnType("varchar(255)");

                            entity.UseCollation("latin1_general_ci");
                        });
                },
                migrationBuilder =>
                {
                    migrationBuilder.AlterTable("IceCreams")
                        .OldAnnotation(RelationalAnnotationNames.Collation, "latin1_general_ci");
                });

            Assert.Equal("",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_with_charset()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreams",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Brand",
                            ColumnType = "varchar",
                            ClrType = typeof(string),
                            [XGAnnotationNames.CharSet] = "utf8mb4"
                        },
                        new AddColumnOperation
                        {
                            Name = "Name",
                            ColumnType = "varchar(255)",
                            ClrType = typeof(string),
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Name", "Brand" },
                        [XGAnnotationNames.IndexPrefixLength] = new [] { 0, 20 }
                    },
                    [XGAnnotationNames.CharSet] = "latin1",
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreams` (
    `Brand` varchar NOT NULL,
    `Name` varchar(255) NOT NULL,
    PRIMARY KEY (`Name`, `Brand`(20))
);" + EOL,
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void AlterTableOperation_with_charset()
        {
            Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "IceCreams",
                        entity =>
                        {
                            entity.Property<int>("Id");

                            entity.Property<string>("Brand")
                                .HasColumnType("longtext")
                                .HasCharSet("utf8mb4");

                            entity.Property<string>("Name")
                                .HasColumnType("varchar(255)");

                            entity.HasCharSet("latin1");
                        });
                },
                migrationBuilder =>
                {
                    migrationBuilder.AlterTable("IceCreams")
                        .OldAnnotation(XGAnnotationNames.CharSet, "latin1")
                        .Annotation(XGAnnotationNames.CharSet, "utf8mb4");
                });

            Assert.Equal("",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void AlterTableOperation_with_charset_reset()
        {
            Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "IceCreams",
                        entity =>
                        {
                            entity.Property<int>("Id");

                            entity.Property<string>("Brand")
                                .HasColumnType("longtext")
                                .HasCharSet("utf8mb4");

                            entity.Property<string>("Name")
                                .HasColumnType("varchar(255)");

                            entity.HasCharSet("latin1");
                        });
                },
                migrationBuilder =>
                {
                    migrationBuilder.AlterTable("IceCreams")
                        .OldAnnotation(XGAnnotationNames.CharSet, "latin1");
                });

            Assert.Equal("",
                Sql,
                ignoreLineEndingDifferences: true);
        }


        [ConditionalFact]
        public virtual void CreateTableOperation_with_table_options()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreams",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Name",
                            ColumnType = "varchar(128)",
                            ClrType = typeof(string),
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Name" }
                    },
                    [XGAnnotationNames.StoreOptions] = "CHECKSUM=1,MAX_ROWS=100",
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreams` (
    `Name` varchar(128) NOT NULL,
    PRIMARY KEY (`Name`)
);" + EOL,
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void AlterTableOperation_with_table_options()
        {
            Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "IceCreams",
                        entity =>
                        {
                            entity.Property<string>("Name")
                                .HasColumnType("varchar(128)");

                            entity.HasKey("Name");

                            entity.HasTableOption("CHECKSUM", "1");
                            entity.HasTableOption("MAX_ROWS", "100");
                        });
                },
                migrationBuilder =>
                {
                    migrationBuilder.AlterTable("IceCreams")
                        .OldAnnotation(XGAnnotationNames.StoreOptions, "CHECKSUM=1,MAX_ROWS=100")
                        .Annotation(XGAnnotationNames.StoreOptions, "CHECKSUM=1,MIN_ROWS=20,MAX_ROWS=200");
                });

            Assert.Equal("",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void CreateTableOperation_primary_key_with_prefix_lengths()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreams",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Brand",
                            ColumnType = "varchar",
                            ClrType = typeof(string),
                        },
                        new AddColumnOperation
                        {
                            Name = "Name",
                            ColumnType = "varchar(255)",
                            ClrType = typeof(string),
                        },
                    },
                    PrimaryKey = new AddPrimaryKeyOperation
                    {
                        Columns = new[] { "Name", "Brand" },
                        [XGAnnotationNames.IndexPrefixLength] = new [] { 0, 20 }
                    },
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreams` (
    `Brand` varchar NOT NULL,
    `Name` varchar(255) NOT NULL,
    PRIMARY KEY (`Name`, `Brand`(20))
);" + EOL,
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override void Sequence_restart_operation(long? startsAt)
        {
            base.Sequence_restart_operation(startsAt);

            Assert.Equal(
                $@"ALTER SEQUENCE `TestRestartSequenceOperation` {(startsAt > 0 ? $"START WITH {startsAt} RESTART" : "RESTART")};" + EOL,
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public virtual void AlterSequenceOperation_with_minValue_and_maxValue()
        {
            Generate(
                new AlterSequenceOperation {
                    Name = "MySequence",
                    Schema = Schema,
                    IncrementBy=1,
                    IsCyclic=false,
                    MinValue = 10,
                    MaxValue = 20
                });

            Assert.Equal(
               @"ALTER SEQUENCE `MySequence` INCREMENT BY 1 MINVALUE 10 MAXVALUE 20 NOCYCLE;" + EOL,
               Sql,
               ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public virtual void AlterSequenceOperation_without_minValue_and_maxValue()
        {
            Generate(
                new AlterSequenceOperation
                {
                    Name = "MySequence",
                    Schema = Schema,
                    IncrementBy = 1,
                    IsCyclic = false
                });

            Assert.Equal(
               @"ALTER SEQUENCE `MySequence` INCREMENT BY 1 NO MINVALUE NO MAXVALUE NOCYCLE;" + EOL,
               Sql,
               ignoreLineEndingDifferences: true);

        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public virtual void CreateSequenceOperation_with_minValue_and_maxValue()
        {
            Generate(
              new CreateSequenceOperation
              {
                  Name = "MySequence",
                  Schema = Schema,
                  IncrementBy = 1,
                  IsCyclic = false,
                  StartValue=10,
                  MinValue = 10,
                  MaxValue = 20,

              });

            Assert.Equal(
               @"CREATE SEQUENCE `MySequence` START WITH 10 INCREMENT BY 1 MINVALUE 10 MAXVALUE 20 NOCYCLE;" + EOL,
               Sql,
               ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public virtual void CreateSequenceOperation_without_minValue_and_maxValue()
        {

            Generate(
              new CreateSequenceOperation
              {
                  Name = "MySequence",
                  Schema = Schema,
                  IncrementBy = 1,
                  IsCyclic = false
              });

            Assert.Equal(
               @"CREATE SEQUENCE `MySequence` START WITH 1 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NOCYCLE;" + EOL,
               Sql,
               ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public virtual void DropSequenceOperation()
        {
             Generate(
             new DropSequenceOperation
             {
                 Name = "MySequence",
                 Schema = Schema
             });

            Assert.Equal(
               @"DROP SEQUENCE `MySequence`;" + EOL,
               Sql,
               ignoreLineEndingDifferences: true);
        }

        protected new void AssertSql(string expected)
        {
            var testSqlLoggerFactory = new TestSqlLoggerFactory();
            var logger = testSqlLoggerFactory.CreateLogger(nameof(XGMigrationsSqlGeneratorTest));
            logger.Log(
                LogLevel.Information,
                RelationalEventId.CommandExecuted.Id,
                new List<KeyValuePair<string, object>>
                {
                    new KeyValuePair<string, object>("commandText", Sql.Trim()),
                    new KeyValuePair<string, object>("parameters", string.Empty)
                }.AsReadOnly(),
                null,
                (pairs, exception) => (string) pairs.First(kvp => kvp.Key == "commandText").Value);
            testSqlLoggerFactory.AssertBaseline(new[] {expected});
        }

        protected override void Generate(params MigrationOperation[] operation)
            => Generate(null, operation);

        protected override void Generate(
            Action<ModelBuilder> buildAction,
            MigrationOperation[] operation,
            MigrationsSqlGenerationOptions options)
            => Generate(null, buildAction, operation, options);

        protected virtual void Generate(
            Action<XGDbContextOptionsBuilder> optionsAction,
            Action<ModelBuilder> buildAction,
            MigrationOperation[] operations,
            MigrationsSqlGenerationOptions options)
        {
            // Might not be needed if we just set SchemaBehavior below.
            // ResetSchemaProperties(operations);

            var optionsBuilder = new DbContextOptionsBuilder(ContextOptions);
            var xgOptionsBuilder = new XGDbContextOptionsBuilder(optionsBuilder);

            xgOptionsBuilder.SchemaBehavior(XGSchemaBehavior.Ignore);
            optionsAction?.Invoke(xgOptionsBuilder);

            var services = TestHelpers.CreateContextServices(CustomServices, optionsBuilder.Options);

            IModel model = null;
            if (buildAction != null)
            {
                var modelBuilder = TestHelpers.CreateConventionBuilder();
                modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
                buildAction(modelBuilder);

                model = services.GetService<IModelRuntimeInitializer>().Initialize(
                    modelBuilder.FinalizeModel(), designTime: true, validationLogger: null);
            }

            var batch = services.GetRequiredService<IMigrationsSqlGenerator>().Generate(operations, model, options);

            Sql = string.Join(
                EOL,
                batch.Select(b => b.CommandText));
        }

        private static void ResetSchemaProperties(MigrationOperation[] operations)
        {
            foreach (var operation in operations)
            {
                var schemaProperties = operation.GetType().GetRuntimeProperties().Where(p => p.Name.Contains("Schema"));
                foreach (var schemaProperty in schemaProperties)
                {
                    schemaProperty.SetValue(operation, null);
                }
            }
        }
    }
}
