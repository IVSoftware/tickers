## Tickers

Fildor's comment is spot on. To ensure that the data structure doesn't become too enormous, use a `Queue<TickerModel>` with a capacity of "fifteeen" (per your spec) and make dictionary of queues by ticker symbol to retrueve the price queue for a given symbol.

```
class ThrottledTickDict
{
    private Dictionary<string, Queue<TickerModel>> _data = new Dictionary<string, Queue<TickerModel>>();
    public TickerModel[] this[string key] => 
        _data.TryGetValue(key, out var symbolQueue) ?
        symbolQueue.ToArray() :
        new TickerModel[0];
    public void Add(TickerModel model)
    {
        if(!_data.ContainsKey(model.Ticker))
        {
            _data[model.Ticker] = new Queue<TickerModel>(capacity: 15);
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

###### Console code
```
var limitedData = new ThrottledTickDict();

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





t.png