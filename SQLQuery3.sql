CREATE FUNCTION fn_GetHouseReservationCount (@HouseId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Sayisi INT;

    SELECT @Sayisi = COUNT(*)
    FROM Reservations
    WHERE HouseId = @HouseId;

    RETURN @Sayisi;
END;
    