-- Create Tables table for seating status
IF OBJECT_ID('dbo.Tables','U') IS NULL
BEGIN
    CREATE TABLE dbo.Tables (
        TableId INT IDENTITY(1,1) PRIMARY KEY,
        TableNumber NVARCHAR(50) NOT NULL,
        NumberOfSeats INT NOT NULL,
        Status INT NOT NULL DEFAULT(0),
        CurrentOrderId INT NULL,
        OccupiedSince DATETIME2 NULL,
        LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Area NVARCHAR(100) NULL,
        IsActive BIT NOT NULL DEFAULT(1),
        RowVersion ROWVERSION
    );
END

-- Optional sample data
IF NOT EXISTS (SELECT 1 FROM dbo.Tables)
BEGIN
    INSERT INTO dbo.Tables (TableNumber, NumberOfSeats, Status, LastUpdated, IsActive)
    VALUES
    ('T1', 4, 0, SYSUTCDATETIME(), 1),
    ('T2', 4, 0, SYSUTCDATETIME(), 1),
    ('T3', 2, 0, SYSUTCDATETIME(), 1),
    ('T4', 2, 0, SYSUTCDATETIME(), 1),
    ('T5', 6, 0, SYSUTCDATETIME(), 1);
END
