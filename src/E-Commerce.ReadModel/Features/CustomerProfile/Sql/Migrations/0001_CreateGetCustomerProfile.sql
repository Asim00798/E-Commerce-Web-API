CREATE OR ALTER PROCEDURE dbo.GetCustomerProfile
    @CustomerId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id AS CustomerId,
        p.FullName,
        p.Email,
        p.PhoneNumber,
        (SELECT COUNT(*) FROM dbo.Orders o WHERE o.CustomerId = p.Id) AS TotalOrders,
        (SELECT ISNULL(SUM(o.TotalAmount), 0) FROM dbo.Orders o WHERE o.CustomerId = p.Id) AS TotalSpent,
        (SELECT ISNULL(AVG(CAST(r.StarRatingValue AS FLOAT)), 0.0) FROM dbo.Ratings r WHERE r.CustomerId = p.Id) AS AverageRating,
        (SELECT COUNT(*) FROM dbo.WishlistItems wi
         INNER JOIN dbo.Wishlists w ON wi.WishlistId = w.Id
         WHERE w.CustomerId = p.Id) AS WishlistItemCount,
        (SELECT MAX(o.PlacedAtUtc) FROM dbo.Orders o WHERE o.CustomerId = p.Id) AS LastOrderDate
    FROM dbo.People p
    WHERE p.Id = @CustomerId;
END;