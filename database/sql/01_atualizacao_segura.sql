/*
 FOODSMART - Atualização incremental do SQL Server
 Execute após BACKUP completo de FoodSmartDb.
 Script idempotente: verifica objetos antes de criá-los.
 Não remove tabelas nem dados.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Restrições de integridade
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Products_Price_Positive')
    ALTER TABLE dbo.Products WITH CHECK ADD CONSTRAINT CK_Products_Price_Positive CHECK (Price > 0);

IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Orders_Total_NonNegative')
    ALTER TABLE dbo.Orders WITH CHECK ADD CONSTRAINT CK_Orders_Total_NonNegative CHECK (Total >= 0);

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_OrderItems_Quantity_Positive')
    ALTER TABLE dbo.OrderItems WITH CHECK ADD CONSTRAINT CK_OrderItems_Quantity_Positive CHECK (Quantity > 0);

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_OrderItems_UnitPrice_NonNegative')
    ALTER TABLE dbo.OrderItems WITH CHECK ADD CONSTRAINT CK_OrderItems_UnitPrice_NonNegative CHECK (UnitPrice >= 0);

-- Índices para consultas frequentes
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'IX_Products_IsActive_Category')
    CREATE INDEX IX_Products_IsActive_Category ON dbo.Products(IsActive, Category);

IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'IX_Orders_CreatedAt')
    CREATE INDEX IX_Orders_CreatedAt ON dbo.Orders(CreatedAt);

IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'IX_Orders_Status_CreatedAt')
    CREATE INDEX IX_Orders_Status_CreatedAt ON dbo.Orders(Status, CreatedAt);

COMMIT TRANSACTION;
GO

/*
 Procedure de relatório administrativo.
 Retorna quantidade de pedidos, faturamento não cancelado e ticket médio.
*/
CREATE OR ALTER PROCEDURE dbo.usp_FoodSmart_OrderSummary
    @StartDate datetime2 = NULL,
    @EndDate datetime2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT_BIG(*) AS TotalOrders,
        SUM(CASE WHEN Status <> N'Cancelado' THEN Total ELSE 0 END) AS Revenue,
        AVG(CASE WHEN Status <> N'Cancelado' THEN CAST(Total AS decimal(18,2)) END) AS AverageTicket
    FROM dbo.Orders
    WHERE (@StartDate IS NULL OR CreatedAt >= @StartDate)
      AND (@EndDate IS NULL OR CreatedAt < DATEADD(DAY, 1, @EndDate));
END;
GO

/*
 Trigger de integridade: impede transições inválidas no banco, inclusive
 quando uma atualização não vier pela API.
*/
CREATE OR ALTER TRIGGER dbo.TR_Orders_ValidateStatusTransition
ON dbo.Orders
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(Status) RETURN;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        INNER JOIN deleted d ON d.Id = i.Id
        WHERE i.Status <> d.Status
          AND NOT (
              (d.Status = N'Recebido' AND i.Status IN (N'Preparando', N'Cancelado'))
              OR (d.Status = N'Preparando' AND i.Status IN (N'Saiu para entrega', N'Cancelado'))
              OR (d.Status = N'Saiu para entrega' AND i.Status IN (N'Concluído', N'Cancelado'))
          )
    )
    BEGIN
        THROW 51001, 'Transição de status de pedido não permitida.', 1;
    END
END;
GO
