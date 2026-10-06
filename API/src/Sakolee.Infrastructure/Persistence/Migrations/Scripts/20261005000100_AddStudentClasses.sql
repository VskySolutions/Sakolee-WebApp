-- Adds StudentClasses (StudentClass, see StudentClassConfiguration): every class a student is enrolled
-- in, so one student can be in several (Family Quick Registration, Student edit). Students.ClassId keeps
-- the first class for older screens.
-- Backfills one row per student that already has a Students.ClassId, so existing enrollments carry over.
-- Idempotent: safe to run more than once.
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[StudentClasses]', N'U') IS NULL
BEGIN
    CREATE TABLE [StudentClasses] (
        [Id] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [ClassId] uniqueidentifier NOT NULL,
        [CreatedById] uniqueidentifier NULL,
        [CreatedOnUtc] datetime2 NOT NULL,
        [UpdatedById] uniqueidentifier NULL,
        [UpdatedOnUtc] datetime2 NOT NULL,
        [Deleted] bit NOT NULL,
        [DeletedOnUtc] datetime2 NULL,
        CONSTRAINT [PK_StudentClasses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentClasses_ClassId' AND object_id = OBJECT_ID('StudentClasses'))
BEGIN
    CREATE INDEX [IX_StudentClasses_ClassId] ON [StudentClasses] ([ClassId]);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StudentClasses_StudentId_ClassId' AND object_id = OBJECT_ID('StudentClasses'))
BEGIN
    CREATE UNIQUE INDEX [IX_StudentClasses_StudentId_ClassId] ON [StudentClasses] ([StudentId], [ClassId]) WHERE [Deleted] = 0;
END;
GO

-- Students stores its ids as nvarchar(450); TRY_CONVERT skips any value that is not a valid guid.
INSERT INTO [StudentClasses] ([Id], [StudentId], [ClassId], [CreatedOnUtc], [UpdatedOnUtc], [Deleted])
SELECT NEWID(), TRY_CONVERT(uniqueidentifier, s.[Id]), TRY_CONVERT(uniqueidentifier, s.[ClassId]),
       SYSUTCDATETIME(), SYSUTCDATETIME(), 0
FROM [Students] s
WHERE s.[Deleted] = 0
  AND TRY_CONVERT(uniqueidentifier, s.[Id]) IS NOT NULL
  AND TRY_CONVERT(uniqueidentifier, s.[ClassId]) IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM [StudentClasses] sc
      WHERE sc.[Deleted] = 0
        AND sc.[StudentId] = TRY_CONVERT(uniqueidentifier, s.[Id])
        AND sc.[ClassId] = TRY_CONVERT(uniqueidentifier, s.[ClassId])
  );
GO
