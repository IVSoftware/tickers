## Tickers

Fildor's comment is spot on. To ensure that the data structure doesn't become too enormous, use a `Queue<TickerModel>` with a `MAX` (e.g. of 15 per your spec) and make dictionary of queues by ticker symbol to retrieve the price queue for a given symbol.

```
class ThrottledTickDict
{
    const int MAX = 15;
    private Dictionary<string, Queue<TickerModel>> _data = new Dictionary<string, Queue<TickerModel>>();
    public TickerModel[] this[string key] => 
        _data.TryGetValue(key, out var symbolQueue) ?
        symbolQueue.ToArray() :
        new TickerModel[0];
    public void Add(TickerModel model)
    {
        if(!_data.ContainsKey(model.Ticker))
        {
            _data[model.Ticker] = new Queue<TickerModel>();
        }
        if(_data[model.Ticker].Count == MAX)
        {
            _ = _data[model.Ticker].Dequeue();
        }
        _data[model.Ticker].Enqueue(model);
    }
}
```

Where:
```
class TickerModel
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Price { get; set; }
    public override string ToString() => $"{Ticker} [{Timestamp}] {Price}";
}
```
___

**Simulation**

Up to 15 quotes are listed, from oldest to newest with an eye toward some kind of rolling chart visualization where the series would plot from left to right.

[![simulation][1]][1]

###### Console code
```
var limitedData = new ThrottledTickDict();
Console.Title = "QuoteTracker";

// Simulate a subscribed ticker stream.
new MockTickerGenerator().TickerChanged += (sender, e) =>
{
    if(sender is MockTickerGenerator quoteStream)
    {
        limitedData.Add(e);
        Console.WriteLine();
        Console.WriteLine($"{e.Ticker} - {limitedData[e.Ticker].Length} quotes");
        Console.WriteLine(string.Join(Environment.NewLine, limitedData[e.Ticker].Select(_ => _)));
    }
};
Console.ReadKey();
```

Where:
 
```
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
```

___

**Database Version**

Per Dr. Null's comment, the same scheme could be implemented using a database, either in memory (as shown here) or on disk.

```csharp
class ThrottledTickDict
{
    public ThrottledTickDict() => Database.CreateTable<TickerModel>();
    const int MAX = 15;

    //  <PackageReference Include="sqlite-net-pcl" Version="1.8.116" />
    SQLiteConnection Database { get; } = new SQLiteConnection(":memory:");
    public TickerModel[] this[string key] =>
        Database
        .Query<TickerModel>($"select * from {nameof(TickerModel)} where {nameof(TickerModel.Ticker)}='{key}'")        
        .OrderByDescending(_=>_.Timestamp)
        .ToArray();
    public void Add(TickerModel model)
    {
        var existing = this[model.Ticker];
        for (int i = 0; i <= existing.Length - MAX; i++) 
        {
            Database.Delete(existing[i]);
        }
        if(1 != Database.Insert(model))
        {
            Debug.Fail("The operation is expected to succeed");
        }
    }
}
```

Where

```

class TickerModel
{
    [PrimaryKey]
    public string Id { get; set; } = $"{Guid.NewGuid()}";
    public string Ticker { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Price { get; set; }
    public override string ToString() => $"{Ticker} [{Timestamp}] {Price}";
}
```

  [1]: https://i.stack.imgur.com/UmCtp.png