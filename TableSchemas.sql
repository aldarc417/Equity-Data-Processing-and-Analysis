CREATE TABLE TICKERS (
    TickerID INT PRIMARY KEY,
    TickerSymbol VARCHAR(10) NOT NULL,
    CompanyName VARCHAR(100),
    Exchange VARCHAR(50),
    Sector VARCHAR(50),
    Industry VARCHAR(50)
);



CREATE TABLE PRICES (
    PriceID INT PRIMARY KEY IDENTITY(1,1),
    TickerID INT,
    TradeDate DATE,
    OpenPrice DECIMAL(10, 2),
    ClosePrice DECIMAL(10, 2),
    HighPrice DECIMAL(10, 2),
    LowPrice DECIMAL(10, 2),
    Volume BIGINT,
    FOREIGN KEY (TickerID) REFERENCES TICKERS(TickerID)
);
