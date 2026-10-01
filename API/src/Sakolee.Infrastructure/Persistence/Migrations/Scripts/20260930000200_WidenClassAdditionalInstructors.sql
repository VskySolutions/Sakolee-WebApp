-- Class.AdditionalInstructors now holds up to 2 comma-separated Staff User ids (2 x 36 + 1 = 73 chars)
-- instead of free text, so it widens from nvarchar(50) to nvarchar(100).
-- Idempotent: safe to run more than once.
IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('Class') AND name = 'AdditionalInstructors' AND max_length < 200)
    ALTER TABLE [Class] ALTER COLUMN [AdditionalInstructors] nvarchar(100) NULL;
GO
