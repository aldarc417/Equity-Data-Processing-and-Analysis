using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

class DataDownload
{
    static async Task Main()
    {
        string[] zipFiles = { "TICKERS.zip", "PRICES.zip" };
        string extractPath = "ExtractedFiles";

        ExtractZipFiles(zipFiles, extractPath);

        string connectionString = "Server=localhost;Database=LAPTOP-021N4VB3;Integrated Security=True;";

        await ImportDataToDatabase(connectionString, extractPath);

        Console.Write("Enter a ticker symbol: ");
        string tickerSymbol = Console.ReadLine().Trim();

        // Execute stored procedure and print results
        await ExecuteTickerStatsProcedure(connectionString, tickerSymbol);

        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static void ExtractZipFiles(string[] zipFiles, string extractPath)
    {
        if (!Directory.Exists(extractPath))
        {
            Directory.CreateDirectory(extractPath);
        }

        foreach (string zipFile in zipFiles)
        {
            ZipFile.ExtractToDirectory(zipFile, extractPath, true);
        }
    }

    static async Task ImportDataToDatabase(string connectionString, string extractPath)
    {
        string tickersFile = Path.Combine(extractPath, "TICKERS.csv");
        string pricesFile = Path.Combine(extractPath, "PRICES.csv");

        // Import Tickers
        await ImportTickers(connectionString, tickersFile);

        // Import Prices
        await ImportPrices(connectionString, pricesFile);
    }

    static async Task ImportTickers(string connectionString, string filePath)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using StreamReader reader = new StreamReader(filePath);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');

            string query = "INSERT INTO TICKERS (TickerID, TickerSymbol) VALUES (@TickerID, @TickerSymbol)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TickerID", int.Parse(values[0]));
            command.Parameters.AddWithValue("@TickerSymbol", values[1]);

            await command.ExecuteNonQueryAsync();
        }
    }

    static async Task ImportPrices(string connectionString, string filePath)
    {
        using SqlConnection connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using StreamReader reader = new StreamReader(filePath);
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = line.Split(',');

            string query = "INSERT INTO PRICES (TickerID, TradeDate, OpenPrice, ClosePrice, HighPrice, LowPrice, Volume) " +
                           "VALUES (@TickerID, @TradeDate, @OpenPrice, @ClosePrice, @HighPrice, @LowPrice, @Volume)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TickerID", int.Parse(values[0]));
            command.Parameters.AddWithValue("@TradeDate", DateTime.Parse(values[1]));
            command.Parameters.AddWithValue("@OpenPrice", decimal.Parse(values[2]));
            command.Parameters.AddWithValue("@ClosePrice", decimal.Parse(values[3]));
            command.Parameters.AddWithValue("@HighPrice", decimal.Parse(values[4]));
            command.Parameters.AddWithValue("@LowPrice", decimal.Parse(values[5]));
            command.Parameters.AddWithValue("@Volume", long.Parse(values[6]));

            await command.ExecuteNonQueryAsync();
        }
    }

    static async Task ExecuteTickerStatsProcedure(string connectionString, string tickerSymbol)
    {
        try
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using SqlCommand command = new SqlCommand("CalcStatsByTicker", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@TickerSymbol", tickerSymbol);

            // Output parameters for each stat
            SqlParameter paramMovingAvg = new SqlParameter("@MovingAverage52Day", SqlDbType.Decimal);
            paramMovingAvg.Direction = ParameterDirection.Output;
            command.Parameters.Add(paramMovingAvg);

            SqlParameter paramHighPrice = new SqlParameter("@HighPrice52Week", SqlDbType.Decimal);
            paramHighPrice.Direction = ParameterDirection.Output;
            command.Parameters.Add(paramHighPrice);

            SqlParameter paramLowPrice = new SqlParameter("@LowPrice52Week", SqlDbType.Decimal);
            paramLowPrice.Direction = ParameterDirection.Output;
            command.Parameters.Add(paramLowPrice);

            await command.ExecuteNonQueryAsync();

            // Retrieve output values
            decimal movingAvg = (decimal)paramMovingAvg.Value;
            decimal highPrice = (decimal)paramHighPrice.Value;
            decimal lowPrice = (decimal)paramLowPrice.Value;

            // Print results to console
            Console.WriteLine($"Ticker: {tickerSymbol}");
            Console.WriteLine($"52-day Moving Average Price: {movingAvg:C}");
            Console.WriteLine($"52-week High Price: {highPrice:C}");
            Console.WriteLine($"52-week Low Price: {lowPrice:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing stored procedure: {ex.Message}");
        }
    }
}
