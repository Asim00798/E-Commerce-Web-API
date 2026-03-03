CREATE VIEW [Read].[vw_UnpaidInvoices] AS
SELECT 
    Id,
    InvoiceNumber,
    CustomerId,
    CustomerName,
    TotalAmount,
    IssuedDate,
    DueDate,
    IsPaid
FROM [dbo].[Invoices]
WHERE IsPaid = 0;
