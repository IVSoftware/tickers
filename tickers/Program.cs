
using SQLite;
using System;
using System.Diagnostics;

var limitedData = new ThrottledTickDict();
Console.Title = "QuoteTracker";

// Simulate a subscribed ticker stream.
new MockTickerGenerator().TickerChanged += (sender, e) =>
{
    if(sender is MockTickerGenerator quoteStream)
    {
        limitedData.Add(e);
        Console.WriteLine();
        Console.WriteLine($"{e.Ticker} - {limitedData[e.Ticker].Length} quotes)");
        Console.WriteLine(string.Join(Environment.NewLine, limitedData[e.Ticker].Select(_ => _)));
    }
};
Console.ReadKey();

class TickerModel
{
    [PrimaryKey]
    public string Id { get; set; } = $"{Guid.NewGuid()}";
    public string Ticker { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Price { get; set; }
    public override string ToString() => $"{Ticker} [{Timestamp}] {Price}";
}

class MockTickerGenerator
{
    Random _rando = new Random(1);
    private Dictionary<string, double> _lastPrice = new Dictionary<string, double>();
    public MockTickerGenerator() => _ = GenerateRandomTickStream();
    List<string> Symbols { get; } =
        new List<string> { "AAA", "BBBB", "CCC", "DDDD", "EEE", "FFFF", "GGG", "HHHH", };
    private async Task GenerateRandomTickStream()
    {
        while (true)
        {
            var ticker = Symbols[_rando.Next(8)];
            if (!_lastPrice.ContainsKey(ticker))
            {
                _lastPrice[ticker] = 10 + 90 * _rando.NextDouble();
            }
            var price = Math.Max(1.0, _lastPrice[ticker] + (0.5 - _rando.NextDouble()) * 2);
            _lastPrice[ticker] = price;
            TickerChanged?.Invoke(this, new TickerModel
            {
                Ticker = ticker,
                Timestamp = DateTime.Now,
                Price = $"{price:f2}",
            });
            await Task.Delay(TimeSpan.FromSeconds(0.5 + _rando.NextDouble()));
        }
    }
    public event EventHandler<TickerModel>? TickerChanged;
}

class ThrottledTickDict
{
    const int MAX = 15;

    //  <PackageReference Include="sqlite-net-pcl" Version="1.8.116" />
    SQLiteConnection Database { get; } = new SQLiteConnection(":memory:");
    public TickerModel[] this[string key] =>
        Database.Query<TickerModel>($"select * from {nameof(TickerModel)} where {nameof(TickerModel.Ticker)}='{key}'")
        .ToArray();
    public void Add(TickerModel model)
    {
        var existing = this[model.Ticker];
        for (int i = 0; i < existing.Length - MAX; i++) 
        {
        }
        if(1 != Database.Insert(model))
        {
            Debug.Fail("The operation is expected to succeed");
        }
    }
}