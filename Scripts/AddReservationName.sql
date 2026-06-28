-- Add ReservationName column to Tables for storing guest name on reserved tables
-- Run once against ChapeauDatabase
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Tables' AND COLUMN_NAME = 'ReservationName'
)
BEGIN
    ALTER TABLE dbo.Tables ADD ReservationName NVARCHAR(100) NULL;
END
