-- Read-side view: aggregates unpaid invoices across orders for reporting.
-- TODO: Define columns and JOIN logic against the Orders and Invoices read tables.

CREATE OR ALTER VIEW [read].[vw_UnpaidInvoices]
AS
SELECT
    -- TODO: Add columns
    1 AS Placeholder
WHERE
    1 = 0; -- remove when implemented
GO
