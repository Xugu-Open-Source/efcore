-- Copyright (c) .NET Foundation. All rights reserved.
-- Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

USE SYSTEM
GO
DROP DATABASE IF EXISTS `XuGuReverseEngineerTestE2E`
GO

CREATE DATABASE IF NOT EXISTS `XuGuReverseEngineerTestE2E`

GO

USE "XuGuReverseEngineerTestE2E"
GO

	DROP TABLE IF NOT EXISTS "SYSDBA"."AllDataTypes"
GO
	drop TYPE IF NOT EXISTS "SYSDBA"."TestTypeAlias"
GO
	DROP TABLE IF NOT EXISTS "SYSDBA"."PropertyConfiguration"
GO
	DROP TABLE IF NOT EXISTS "SYSDBA"."TestSpacesKeywordsTable"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToManyDependent"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToManyPrincipal"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToOneDependent"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToOnePrincipal"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToOneSeparateFKDependent"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToOneSeparateFKPrincipal"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToOneFKToUniqueKeyDependent"
GO
	drop table IF NOT EXISTS "SYSDBA"."OneToOneFKToUniqueKeyPrincipal"
GO
	drop table IF NOT EXISTS "SYSDBA"."TableWithUnmappablePrimaryKeyColumn"
GO
	drop table IF NOT EXISTS "SYSDBA"."UnmappablePKColumn"
GO

-- TODO: Remove this
	drop table IF NOT EXISTS "SYSDBA"."ReferredToByTableWithUnmappablePrimaryKeyColumn"
GO
	drop table IF NOT EXISTS "SYSDBA"."SelfReferencing"
GO
	drop table IF NOT EXISTS "SYSDBA"."MultipleFKsDependent"
GO
	drop table IF NOT EXISTS "SYSDBA"."MultipleFKsPrincipal"
GO
	drop table IF NOT EXISTS "SYSDBA"."FilteredOut"
GO
	drop table IF NOT EXISTS "SYSDBA"."PrimaryKeyWithSequence"
GO
	drop sequence IF NOT EXISTS "SYSDBA"."PrimaryKeyWithSequenceSequence"
GO

CREATE OR REPLACE TYPE TestTypeAlias IS VARRAY(1)  OF VARCHAR
GO

CREATE SEQUENCE PrimaryKeyWithSequenceSequence
START WITH 1
INCREMENT BY 1
GO

CREATE TABLE "SYSDBA"."AllDataTypes" (
	AllDataTypesID int IDENTITY(1, 1) PRIMARY KEY,
	bigintColumn bigint NOT NULL,
	bitColumn bit NOT NULL,
	decimalColumn numeric NOT NULL,
	intColumn int NOT NULL,
	moneyColumn numeric NOT NULL,
	numericColumn numeric NOT NULL,
	smallintColumn smallint NOT NULL,
	smallmoneyColumn smallint NOT NULL,
	tinyintColumn tinyint NOT NULL,
	floatColumn float NOT NULL,
	realColumn float NULL,
	dateColumn date NOT NULL,
	datetimeColumn datetime NULL,
	datetime2Column datetime NULL,
	datetime24Column datetime(4) NULL,
	datetimeoffsetColumn datetime NULL,
	datetimeoffset5Column datetime(5) NULL,
	smalldatetimeColumn smalldatetime NULL,
	timeColumn time NULL,
	time4Column time(3) NULL,
	charColumn char NULL,
	char10Column char(10) NULL,
	textColumn text NULL,
	varcharColumn varchar NULL,
	varchar66Column varchar(66) NULL,
	varcharMaxColumn varchar NULL,
	ncharColumn nchar NULL,
	nchar99Column nchar(99) NULL,
	ntextColumn ntext NULL,
	nvarcharColumn nvarchar NULL,
	nvarchar100Column nvarchar(100) NULL,
	nvarcharMaxColumn nvarchar NULL,
	binaryColumn binary NULL,
	binary111Column binary(111) NULL,
	imageColumn image NULL,
	varbinaryColumn varbinary NULL,
	varbinary123Column varbinary(123) NULL,
	varbinaryMaxColumn varbinary NULL,
	timestampColumn timestamp NULL,
	uniqueidentifierColumn uniqueidentifier NULL,
	hierarchyidColumn hierarchyid NULL,
	sql_variantColumn sql_variant NULL,
	xmlColumn xml NULL,
	geographyColumn geography NULL,
	geometryColumn geometry NULL,
	typeAliasColumn TestTypeAlias NULL,
	binaryVaryingColumn binary varying NULL,
	binaryVarying133Column binary varying(133) NULL,
	binaryVaryingMaxColumn binary varying NULL,
	charVaryingColumn char varying NULL,
	charVarying144Column char varying(144) NULL,
	charVaryingMaxColumn char varying NULL,
	characterColumn character NULL,
	character155Column character(155) NULL,
	characterVaryingColumn character varying NULL,
	characterVarying166Column character varying(166) NULL,
	characterVaryingMaxColumn character varying NULL,
	nationalCharacterColumn national character NULL,
	nationalCharacter171Column national character(171) NULL,
	nationalCharVaryingColumn national char varying NULL,
	nationalCharVarying177Column national char varying(177) NULL,
	nationalCharVaryingMaxColumn national char varying NULL,
	nationalCharacterVaryingColumn national char varying NULL,
	nationalCharacterVarying188Column national char varying(188) NULL,
	nationalCharacterVaryingMaxColumn national char varying NULL
)

GO

CREATE INDEX "IX_UnscaffoldableIndex"
	ON "SYSDBA"."AllDataTypes" ( nvarcharColumn, sql_variantColumn, hierarchyidColumn )

GO

CREATE TABLE "SYSDBA"."PropertyConfiguration" (
	"PropertyConfigurationID" "tinyint" IDENTITY(1, 1) PRIMARY KEY, -- tests error message about tinyint identity columns
	"WithDateDefaultExpression" "datetime2" NOT NULL DEFAULT (getdate()),
	"WithDateFixedDefault" "datetime2" NOT NULL DEFAULT ('October 20, 2015 11am'),
	"WithDateNullDefault" "datetime2" NULL DEFAULT (NULL),
	"WithGuidDefaultExpression" "uniqueidentifier" NOT NULL DEFAULT (newsequentialid()),
	"WithVarcharNullDefaultValue" "varchar" NULL DEFAULT (NULL),
	"WithDefaultValue" "int" NOT NULL DEFAULT ((-1)),
	"WithNullDefaultValue" "smallint" NULL DEFAULT (NULL),
	"WithMoneyDefaultValue" "money" NOT NULL DEFAULT ((0.00)),
	"A" "int" NOT NULL,
	"B" "int" NOT NULL,
	"SumOfAAndB" AS A + B PERSISTED, -- tests StoreGeneratedPattern
	"RowversionColumn" "rowversion" NOT NULL,
	"PropertyConfiguration" "int" NULL, -- tests column with same name as its table
	"ComputedDateTimeColumn" AS GETDATE()
)

GO

CREATE INDEX Test_PropertyConfiguration_Index
	ON "SYSDBA"."PropertyConfiguration" (A, B)

GO

CREATE TABLE "SYSDBA"."Test Spaces Keywords Table" (
	"Test Spaces Keywords TableID" "int" PRIMARY KEY,
	"abstract" "int" NOT NULL,
	"class" "int" NULL,
	"volatile" "int" NOT NULL,
	"Spaces In Column" "int" NULL,
	"Tabs	In	Column" "int" NOT NULL,
	"@AtSymbolAtStartOfColumn" "int" NULL,
	"@Multiple@At@Symbols@In@Column" "int" NOT NULL,
	"Commas,In,Column" "int" NULL,
	"$Dollar$Sign$Column" "int" NOT NULL,
	"!Exclamation!Mark!Column" "int" NULL,
	"""Double""Quotes""Column" "int" NULL,
	"\Backslashes\In\Column" "int" NULL,
)

GO

CREATE TABLE "SelfReferencing" (
	"SelfReferencingID" "int" PRIMARY KEY,
	"Name" nvarchar(20) NOT NULL,
	"Description" nvarchar(100) NOT NULL,
	"SelfReferenceFK" "int" NULL,
	CONSTRAINT "FK_SelfReferencing" FOREIGN KEY 
	(
		"SelfReferenceFK"
	) REFERENCES "SYSDBA"."SelfReferencing" (
		"SelfReferencingID"
	)
)

GO

CREATE TABLE "OneToManyPrincipal" (
	"OneToManyPrincipalID1" "int",
	"OneToManyPrincipalID2" "int",
	"Other" nvarchar(20) NOT NULL,
	CONSTRAINT "PK_OneToManyPrincipal" PRIMARY KEY CLUSTERED 
	(
		"OneToManyPrincipalID1", "OneToManyPrincipalID2"
	)
)

GO

CREATE TABLE "OneToManyDependent" (
	"OneToManyDependentID1" "int",
	"OneToManyDependentID2" "int",
	"SomeDependentEndColumn" nvarchar (20) NOT NULL,
	"OneToManyDependentFK2" "int" NULL, -- deliberately put FK columns in other order to make sure we get correct order in key
	"OneToManyDependentFK1" "int" NULL,
	CONSTRAINT "PK_OneToManyDependent" PRIMARY KEY CLUSTERED 
	(
		"OneToManyDependentID1", "OneToManyDependentID2"
	),
	CONSTRAINT "FK_OneToManyDependent" FOREIGN KEY 
	(
		"OneToManyDependentFK1", "OneToManyDependentFK2"
	) REFERENCES "SYSDBA"."OneToManyPrincipal" (
		"OneToManyPrincipalID1", "OneToManyPrincipalID2"
	)
)

GO

CREATE TABLE "OneToOnePrincipal" (
	"OneToOnePrincipalID1" "int",
	"OneToOnePrincipalID2" "int",
	"SomeOneToOnePrincipalColumn" nvarchar (20) NOT NULL,
	CONSTRAINT "PK_OneToOnePrincipal" PRIMARY KEY CLUSTERED 
	(
		"OneToOnePrincipalID1", "OneToOnePrincipalID2"
	)
)

GO

CREATE TABLE "OneToOneDependent" (
	"OneToOneDependentID1" "int",
	"OneToOneDependentID2" "int",
	"SomeDependentEndColumn" nvarchar (20) NOT NULL,
	CONSTRAINT "PK_OneToOneDependent" PRIMARY KEY CLUSTERED 
	(
		"OneToOneDependentID1", "OneToOneDependentID2"
	),
	CONSTRAINT "FK_OneToOneDependent" FOREIGN KEY 
	(
		"OneToOneDependentID1", "OneToOneDependentID2"
	) REFERENCES "SYSDBA"."OneToOnePrincipal" (
		"OneToOnePrincipalID1", "OneToOnePrincipalID2"
	),
)

GO

CREATE TABLE "OneToOneSeparateFKPrincipal" (
	"OneToOneSeparateFKPrincipalID1" "int",
	"OneToOneSeparateFKPrincipalID2" "int",
	"SomeOneToOneSeparateFKPrincipalColumn" nvarchar (20) NOT NULL,
	CONSTRAINT "PK_OneToOneSeparateFKPrincipal" PRIMARY KEY CLUSTERED 
	(
		"OneToOneSeparateFKPrincipalID1", "OneToOneSeparateFKPrincipalID2"
	)
)

GO

CREATE TABLE "OneToOneSeparateFKDependent" (
	"OneToOneSeparateFKDependentID1" "int",
	"OneToOneSeparateFKDependentID2" "int",
	"SomeDependentEndColumn" nvarchar (20) NOT NULL,
	"OneToOneSeparateFKDependentFK1" "int" NULL,
	"OneToOneSeparateFKDependentFK2" "int" NULL,
	CONSTRAINT "PK_OneToOneSeparateFKDependent" PRIMARY KEY CLUSTERED 
	(
		"OneToOneSeparateFKDependentID1", "OneToOneSeparateFKDependentID2"
	),
	CONSTRAINT "FK_OneToOneSeparateFKDependent" FOREIGN KEY 
	(
		"OneToOneSeparateFKDependentFK1", "OneToOneSeparateFKDependentFK2"
	) REFERENCES "SYSDBA"."OneToOneSeparateFKPrincipal" (
		"OneToOneSeparateFKPrincipalID1", "OneToOneSeparateFKPrincipalID2"
	),
	CONSTRAINT "UK_OneToOneSeparateFKDependent" UNIQUE
	(
		"OneToOneSeparateFKDependentFK1", "OneToOneSeparateFKDependentFK2"
	)
)

GO

CREATE TABLE "OneToOneFKToUniqueKeyPrincipal" (
	"OneToOneFKToUniqueKeyPrincipalID1" "int",
	"OneToOneFKToUniqueKeyPrincipalID2" "int",
	"SomePrincipalColumn" nvarchar (20) NOT NULL,
	"OneToOneFKToUniqueKeyPrincipalUniqueKey1" "int" NOT NULL,
	"OneToOneFKToUniqueKeyPrincipalUniqueKey2" "int" NOT NULL,
	CONSTRAINT "PK_OneToOneFKToUniqueKeyPrincipal" PRIMARY KEY CLUSTERED 
	(
		"OneToOneFKToUniqueKeyPrincipalID1", "OneToOneFKToUniqueKeyPrincipalID2"
	),
	CONSTRAINT "UK_OneToOneFKToUniqueKeyPrincipal" UNIQUE
	(
		"OneToOneFKToUniqueKeyPrincipalUniqueKey1", "OneToOneFKToUniqueKeyPrincipalUniqueKey2"
	)
)

GO

CREATE TABLE "OneToOneFKToUniqueKeyDependent" (
	"OneToOneFKToUniqueKeyDependentID1" "int",
	"OneToOneFKToUniqueKeyDependentID2" "int",
	"SomeColumn" nvarchar (20) NOT NULL,
	"OneToOneFKToUniqueKeyDependentFK1" "int" NULL,
	"OneToOneFKToUniqueKeyDependentFK2" "int" NULL,
	CONSTRAINT "PK_OneToOneFKToUniqueKeyDependent" PRIMARY KEY CLUSTERED 
	(
		"OneToOneFKToUniqueKeyDependentID1", "OneToOneFKToUniqueKeyDependentID2"
	),
	CONSTRAINT "FK_OneToOneFKToUniqueKeyDependent" FOREIGN KEY 
	(
		"OneToOneFKToUniqueKeyDependentFK1", "OneToOneFKToUniqueKeyDependentFK2"
	) REFERENCES "SYSDBA"."OneToOneFKToUniqueKeyPrincipal" (
		"OneToOneFKToUniqueKeyPrincipalUniqueKey1", "OneToOneFKToUniqueKeyPrincipalUniqueKey2"
	),
	CONSTRAINT "UK_OneToOneFKToUniqueKeyDependent" UNIQUE
	(
		"OneToOneFKToUniqueKeyDependentFK1", "OneToOneFKToUniqueKeyDependentFK2"
	)
)

GO

CREATE TABLE "UnmappablePKColumn" (
	"UnmappablePKColumnID" "int" PRIMARY KEY,
	"AColumn" nvarchar(20) NOT NULL,
	"ValueGeneratedOnAddColumn" "int" IDENTITY(1, 1) NOT NULL,
)

GO

CREATE TABLE "TableWithUnmappablePrimaryKeyColumn" (
	"TableWithUnmappablePrimaryKeyColumnID" "hierarchyid" PRIMARY KEY,
	"AnotherColumn" nvarchar(20) NOT NULL,
	"TableWithUnmappablePrimaryKeyColumnFK" "int" NULL,
	CONSTRAINT "FK_TableWithUnmappablePrimaryKeyColumn" FOREIGN KEY 
	(
		"TableWithUnmappablePrimaryKeyColumnFK"
	) REFERENCES "SYSDBA"."UnmappablePKColumn" (
		"UnmappablePKColumnID"
	),
	CONSTRAINT "UK_TableWithUnmappablePrimaryKeyColumn" UNIQUE
	(
		"AnotherColumn" -- tests that RevEng can assign an alternate key on a table with a PK which cannot be mapped
	)
)

GO

CREATE TABLE MultipleFKsPrincipal (
	MultipleFKsPrincipalId int PRIMARY KEY,
	SomePrincipalColumn nvarchar (20) NOT NULL
)

GO

CREATE TABLE MultipleFKsDependent (
	MultipleFKsDependentId int PRIMARY KEY,
	AnotherColumn nvarchar (20) NOT NULL,
	RelationAId int NOT NULL,
	RelationBId int NULL,
	RelationCId int NULL,
	CONSTRAINT FK_RelationA FOREIGN KEY 
	(
		RelationAId
	) REFERENCES SYSDBA.MultipleFKsPrincipal (
		MultipleFKsPrincipalId
	),
	CONSTRAINT FK_RelationB FOREIGN KEY 
	(
		RelationBId
	) REFERENCES SYSDBA.MultipleFKsPrincipal (
		MultipleFKsPrincipalId
	),
	CONSTRAINT FK_RelationC FOREIGN KEY 
	(
		RelationCId
	) REFERENCES SYSDBA.MultipleFKsPrincipal (
		MultipleFKsPrincipalId
	)
)

GO
CREATE TABLE "FilteredOut" (
	"FilteredOutID" "int" PRIMARY KEY,
	"Unused1" nvarchar(20) NOT NULL,
	"Unused2" "int" NOT NULL,
)

GO

CREATE TABLE PrimaryKeyWithSequence (
	PrimaryKeyWithSequenceId int DEFAULT(NEXT VALUE FOR PrimaryKeyWithSequenceSequence),
	OtherColumn nvarchar (20) NOT NULL,
	PRIMARY KEY (PrimaryKeyWithSequenceId)
)

GO
