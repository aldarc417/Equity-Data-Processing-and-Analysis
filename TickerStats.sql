CREATE PROCEDURE CalcStatsByTicker
    @TickerSymbol VARCHAR(10)
AS
BEGIN
    DECLARE @TickerID INT;

    -- Get the TickerID for the given TickerSymbol
    SELECT @TickerID = TickerID
    FROM TICKERS
    WHERE TickerSymbol = @TickerSymbol;

    IF @TickerID IS NOT NULL
    BEGIN
        -- Calculate 52-day moving average price
        SELECT 
            AVG(ClosePrice) AS MovingAverage52Day
        FROM (
            SELECT TOP 52 ClosePrice
            FROM PRICES
            WHERE TickerID = @TickerID
            ORDER BY TradeDate DESC
        ) AS RecentPrices;

        -- Calculate 52-week high price
        SELECT 
            MAX(HighPrice) AS HighPrice52Week
        FROM (
            SELECT TOP 52 HighPrice
            FROM PRICES
            WHERE TickerID = @TickerID
            ORDER BY TradeDate DESC
        ) AS RecentHighPrices;

        -- Calculate 52-week low price
        SELECT 
            MIN(LowPrice) AS LowPrice52Week
        FROM (
            SELECT TOP 52 LowPrice
            FROM PRICES
            WHERE TickerID = @TickerID
            ORDER BY TradeDate DESC
        ) AS RecentLowPrices;
    END
    ELSE
    BEGIN
        PRINT 'Ticker symbol not found.';
    END
END;
