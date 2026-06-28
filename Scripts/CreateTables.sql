-- ============================================================
--  Chapeau Database Setup Script
--  Safe to re-run: all changes are guarded with IF NOT EXISTS
-- ============================================================

-- Add IsActive column to MenuItems if it does not exist yet
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.MenuItems') AND name = 'IsActive'
)
BEGIN
    ALTER TABLE dbo.MenuItems ADD IsActive BIT NOT NULL DEFAULT(1);
END
GO

-- Create Tables table for seating status
IF OBJECT_ID('dbo.Tables', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tables (
        TableId        INT IDENTITY(1,1) PRIMARY KEY,
        TableNumber    NVARCHAR(50)  NOT NULL,
        NumberOfSeats  INT           NOT NULL,
        Status         INT           NOT NULL DEFAULT(0),
        CurrentOrderId INT           NULL,
        OccupiedSince  DATETIME2     NULL,
        LastUpdated    DATETIME2     NOT NULL DEFAULT GETDATE(),
        Area           NVARCHAR(100) NULL,
        IsActive       BIT           NOT NULL DEFAULT(1),
        ReservationName NVARCHAR(100) NULL,
        RowVersion     ROWVERSION
    );
END
GO

-- Add ReservationName to existing Tables table if missing
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Tables' AND COLUMN_NAME = 'ReservationName'
)
BEGIN
    ALTER TABLE dbo.Tables ADD ReservationName NVARCHAR(100) NULL;
END
GO

-- Seed Tables rows if table is empty
IF NOT EXISTS (SELECT 1 FROM dbo.Tables)
BEGIN
    INSERT INTO dbo.Tables (TableNumber, NumberOfSeats, Status, LastUpdated, IsActive)
    VALUES
    ('T1',  4, 0, GETDATE(), 1),
    ('T2',  4, 0, GETDATE(), 1),
    ('T3',  4, 0, GETDATE(), 1),
    ('T4',  4, 0, GETDATE(), 1),
    ('T5',  4, 0, GETDATE(), 1),
    ('T6',  4, 0, GETDATE(), 1),
    ('T7',  4, 0, GETDATE(), 1),
    ('T8',  4, 0, GETDATE(), 1),
    ('T9',  4, 0, GETDATE(), 1),
    ('T10', 4, 0, GETDATE(), 1);
END
GO

-- ============================================================
--  MenuItems seed data
--  Only inserts if MenuItems table is empty
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM dbo.MenuItems)
BEGIN

    -- LUNCH STARTERS  (Type=1, Course=1, Card=1, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Steak tartare with truffle mayonnaise',                              7.50, 1, 1, 1, 9.00, 50),
        ('Pate of pheasant with Monegasque onions',                            8.50, 1, 1, 1, 9.00, 50),
        ('Provencal fish soup with rouille, aged cheese and croutons',         6.50, 1, 1, 1, 9.00, 50);

    -- LUNCH MAINS  (Type=1, Course=3, Card=1, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Deer stew with red cabbage',                                         12.50, 1, 3, 1, 9.00, 50),
        ('Fried cod with curry sabayon',                                       14.50, 1, 3, 1, 9.00, 50),
        ('Linguini with mushroom sauce',                                       13.50, 1, 3, 1, 9.00, 50);

    -- LUNCH DESSERTS  (Type=1, Course=4, Card=1, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('White chocolate and speculoos cake with mandarin',                    5.50, 1, 4, 1, 9.00, 50),
        ('Fresh madeleines with fig compote and creme patissier Grand Marnier', 6.50, 1, 4, 1, 9.00, 50),
        ('3 types of farmers cheeses with rye raisin bread',                    5.00, 1, 4, 1, 9.00, 50);

    -- DINNER STARTERS  (Type=1, Course=1, Card=2, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Veal tartar with tuna mayonnaise and fried mussels',                  8.50, 1, 1, 2, 9.00, 50),
        ('Pate of pheasant with Monegasque onions (dinner)',                    8.50, 1, 1, 2, 9.00, 50),
        ('Crab salmon cookies with sweet and sour chili sauce',                 9.00, 1, 1, 2, 9.00, 50);

    -- DINNER ENTREMETS  (Type=1, Course=2, Card=2, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Provencal fish soup with rouille and croutons',                        6.50, 1, 2, 2, 9.00, 50),
        ('Pheasant consomme with spring onion and green herbs',                  7.50, 1, 2, 2, 9.00, 50);

    -- DINNER MAINS  (Type=1, Course=3, Card=2, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('On the skin fried cod fillet with curry-sabayon',                    17.50, 1, 3, 2, 9.00, 50),
        ('Fried tenderloin with veal gravy with pink peppers',                 22.50, 1, 3, 2, 9.00, 50),
        ('Venison steak with own stew and red cabbage',                        25.00, 1, 3, 2, 9.00, 50);

    -- DINNER DESSERTS  (Type=1, Course=4, Card=2, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Cafe surprise - Coffee with homemade pralines',                        5.50, 1, 4, 2, 9.00, 50),
        ('Cherry Baby - Whipped ice cream with warm cherries',                   6.50, 1, 4, 2, 9.00, 50),
        ('Port e Fromage - different cheeses with a glass of port',              7.50, 1, 4, 2, 9.00, 50);

    -- SOFT DRINKS  (Type=2, Course=0, Card=3, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Spa red',          2.50, 2, 0, 3, 9.00, 50),
        ('Spa green',        2.50, 2, 0, 3, 9.00, 50),
        ('Coca Cola Light',  2.50, 2, 0, 3, 9.00, 50),
        ('Coca Cola',        2.50, 2, 0, 3, 9.00, 50),
        ('Sisi',             2.50, 2, 0, 3, 9.00, 50),
        ('Tonic',            2.50, 2, 0, 3, 9.00, 50),
        ('Bitter Lemon',     2.50, 2, 0, 3, 9.00, 50);

    -- BEERS  (Type=2, Course=0, Card=3, VatRate=21.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Hertog Jan',    3.00, 2, 0, 3, 21.00, 50),
        ('Duvel',         4.50, 2, 0, 3, 21.00, 50),
        ('Kriek',         4.00, 2, 0, 3, 21.00, 50),
        ('Leffe Triple',  4.50, 2, 0, 3, 21.00, 50);

    -- WINES  (Type=2, Course=0, Card=3, VatRate=21.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('White house wine (bottle)',  28.50, 2, 0, 3, 21.00, 50),
        ('White house wine (glass)',    6.50, 2, 0, 3, 21.00, 50),
        ('Red house wine (bottle)',    32.00, 2, 0, 3, 21.00, 50),
        ('Red house wine (glass)',      7.50, 2, 0, 3, 21.00, 50),
        ('Champagne (bottle)',         50.00, 2, 0, 3, 21.00, 50);

    -- SPIRITS  (Type=2, Course=0, Card=3, VatRate=21.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Young Jenever',  3.50, 2, 0, 3, 21.00, 50),
        ('Whisky',         5.00, 2, 0, 3, 21.00, 50),
        ('Rum',            4.50, 2, 0, 3, 21.00, 50),
        ('Vieux',          4.50, 2, 0, 3, 21.00, 50),
        ('Berenburg',      3.50, 2, 0, 3, 21.00, 50);

    -- COFFEE AND TEA  (Type=2, Course=0, Card=3, VatRate=9.00)
    INSERT INTO dbo.MenuItems (Name, Price, Type, Course, Card, VatRate, Stock) VALUES
        ('Coffee',      2.50, 2, 0, 3, 9.00, 50),
        ('Cappuccino',  3.50, 2, 0, 3, 9.00, 50),
        ('Espresso',    3.00, 2, 0, 3, 9.00, 50),
        ('Tea',         2.50, 2, 0, 3, 9.00, 50);

END
GO
