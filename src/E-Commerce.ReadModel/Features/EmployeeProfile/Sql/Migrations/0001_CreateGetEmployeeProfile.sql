CREATE OR ALTER PROCEDURE dbo.GetEmployeeProfile
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.Id AS EmployeeId,
        e.FullName,
        e.Department,
        (SELECT COUNT(*) FROM dbo.Shipments s WHERE s.AssignedDriverId = e.Id AND s.Status IN (1,2,3,4,5)) AS ActiveShipments,
        (SELECT COUNT(*) FROM dbo.Shipments s WHERE s.AssignedDriverId = e.Id AND s.Status = 7) AS CompletedShipments,
        (SELECT ISNULL(AVG(CAST(dr.Rating AS FLOAT)), 0.0) FROM dbo.DriverRatings dr WHERE dr.DriverId = e.Id) AS AverageRating,
        (SELECT MAX(s.UpdatedAtUtc) FROM dbo.Shipments s WHERE s.AssignedDriverId = e.Id) AS LastActiveAt
    FROM dbo.Employees e
    WHERE e.Id = @EmployeeId;
END;