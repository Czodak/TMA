-- Step 1: Update existing records with current timestamp - to avoid inconsistent data
UPDATE Tasks SET CreatedAt = SYSUTCDATETIME() WHERE CreatedAt IS NULL;
UPDATE Tasks SET LastUpdatedAt = SYSUTCDATETIME() WHERE LastUpdatedAt IS NULL;

-- Step 2: Add NOT NULL for timestamp columns
ALTER TABLE Tasks ALTER COLUMN CreatedAt DATETIME2 NOT NULL;
ALTER TABLE Tasks ALTER COLUMN LastUpdatedAt DATETIME2 NOT NULL;

-- Step 3: Add default constraints with default
IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Tasks_CreatedAt')
BEGIN
    ALTER TABLE Tasks ADD CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()) FOR CreatedAt;
END;

IF NOT EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Tasks_LastUpdatedAt')
BEGIN
    ALTER TABLE Tasks ADD CONSTRAINT DF_Tasks_LastUpdatedAt DEFAULT (SYSUTCDATETIME()) FOR LastUpdatedAt;
END;

-- Step 4: Create trigger to set LastUpdatedAt
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Tasks_UpdateTimestamp')
BEGIN
    DROP TRIGGER trg_Tasks_UpdateTimestamp;
END;
GO

CREATE TRIGGER trg_Tasks_UpdateTimestamp
ON Tasks
AFTER UPDATE
AS 
BEGIN
    SET NOCOUNT ON;
    
    -- only update if it wasn't explicitly modified
    IF NOT UPDATE(LastUpdatedAt)
    BEGIN
        UPDATE t
        SET LastUpdatedAt = SYSUTCDATETIME()
        FROM Tasks t
        INNER JOIN inserted i ON t.Id = i.Id;
    END;
END;
GO