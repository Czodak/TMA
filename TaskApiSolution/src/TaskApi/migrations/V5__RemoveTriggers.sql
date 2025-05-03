--switched to EF Core config
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_Tasks_UpdateTimestamp')
BEGIN
    DROP TRIGGER trg_Tasks_UpdateTimestamp;
END;