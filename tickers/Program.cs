
using System;

var limitedData = new ThrottledTickDict();
// Simulate a subscribed ticker stream.
new MockTickerGenerator().TickerChanged += (sender, e) =>
{
    if(sender is MockTickerGenerator quoteStream)
    {
        limitedData.Add(e);
        Console.WriteLine();
        Console.WriteLine($"{e.Ticker} - Up to 15 quotes)");
        Console.WriteLine(string.Join(Environment.NewLine, limitedData[e.Ticker].Select(_ => _)));
    }
};
Console.ReadKey();

class TickerModel
{
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
        new List<string> { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "FFFF", "GGGG", "HHHH", };
    private async Task GenerateRandomTickStream()
    {
        while (true)
        {
            var ticker = Symbols[_rando.Next(8)];
            TickerChanged?.Invoke(this, new TickerModel
            {
                Ticker = ticker,
                Timestamp = DateTime.Now,
                Price = $"{(1 + Symbols.IndexOf(ticker)) + (_rando.NextDouble() -0.5):f2}",
            });
            await Task.Delay(TimeSpan.FromSeconds(0.5 + _rando.NextDouble()));
        }
    }
    public event EventHandler<TickerModel>? TickerChanged;
#if false
    using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MockTickerGenerator
{
    private Random _random = new Random();

    public MockTickerGenerator()
    {
        foreach (var symbol in Symbols)
        {
            // Initialize each symbol with a random starting price between 10 and 100
            _lastPrices[symbol] = 10 + 90 * _random.NextDouble();
        }

        _ = GenerateRandomTickStream();
    }

    public List<string> Symbols { get; } =
        new List<string> { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "FFFF", "GGGG", "HHHH", };

    private async Task GenerateRandomTickStream()
    {
        while (true)
        {
            var ticker = Symbols[_random.Next(Symbols.Count)];
            var lastPrice = _lastPrices[ticker];
            // Simulate a price change with a random walk, ensuring it doesn't go negative
            var change = (0.5 - _random.NextDouble()) * 2; // Random change between -1 and 1
            var newPrice = Math.Max(0.01, lastPrice + change); // Ensure price stays above 0.01
            _lastPrices[ticker] = newPrice;

            TickerChanged?.Invoke(this, new TickerModel
            {
                Ticker = ticker,
                Timestamp = DateTime.Now,
                Price = $"{newPrice:f2}",
            });

            await Task.Delay(TimeSpan.FromSeconds(0.5 + _random.NextDouble()));
        }
    }

    public event EventHandler<TickerModel>? TickerChanged;
}

public class TickerModel
{
    public string Ticker { get; set; }
    public DateTime Timestamp { get; set; }
    public string Price { get; set; }
}

#endif
}

class ThrottledTickDict
{
    Random _rando = new Random(1);
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