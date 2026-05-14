using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Tadbeer.DAL.Data;

#nullable disable

namespace Tadbeer.DAL.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260514090000_FixWorkImageColumns")]
public partial class FixWorkImageColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH('WorkImages', 'Description') IS NULL
BEGIN
    EXEC(N'ALTER TABLE [WorkImages] ADD [Description] nvarchar(1000) NULL;');
END

IF COL_LENGTH('WorkImages', 'Name') IS NULL
BEGIN
    EXEC(N'ALTER TABLE [WorkImages] ADD [Name] nvarchar(255) NOT NULL CONSTRAINT [DF_WorkImages_Name] DEFAULT (''Untitled'') WITH VALUES;');
END
ELSE
BEGIN
    EXEC(N'UPDATE [WorkImages]
SET [Name] = ''Untitled''
WHERE [Name] IS NULL OR LTRIM(RTRIM([Name])) = '''';');

    IF EXISTS (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.tables t ON c.object_id = t.object_id
        WHERE t.name = 'WorkImages'
          AND c.name = 'Name'
          AND c.is_nullable = 1
    )
    BEGIN
        EXEC(N'ALTER TABLE [WorkImages] ALTER COLUMN [Name] nvarchar(255) NOT NULL;');
    END
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH('WorkImages', 'Name') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_WorkImages_Name')
    BEGIN
        EXEC(N'ALTER TABLE [WorkImages] DROP CONSTRAINT [DF_WorkImages_Name];');
    END

    EXEC(N'ALTER TABLE [WorkImages] DROP COLUMN [Name];');
END

IF COL_LENGTH('WorkImages', 'Description') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE [WorkImages] DROP COLUMN [Description];');
END
");
    }
}

