## Tickers

There are many ways to go about it. This one uses `System.Linq`. 

___

For demonstration purposes, make a mock list of 1000 timestamped symbol price "ticks" from the past four hours with a pseudorandom distribution of 8 tickers.

```
Random _rando = new Random(1);
string[] Ticker = { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "FFFF", "GGGG", "HHHH", };

List<TickerModel> giantList =
    Enumerable.Range(1, 1000)
    .Select(_ => new TickerModel
    {
        Ticker = Ticker[_rando.Next(8)],
        Timestamp = DateTime.Now.AddMinutes(-_rando.Next(240)),
    }).ToList();

[DebuggerDisplay("{Ticker}@{Timestamp}")]
class TickerModel
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    // Presumably we are tracking something like price
    // fluctuations, but that's not important right now.
    // decimal Price {get; set; }
}
```

___

 1. Use `Linq.GroupBy` to make separate lists where all the tickers are the same.
 2. Use `Linq.ToDictionary` so that you can reference these lists by their ticker symbol.
 3. Use `Linq.OrderByDescending` to move the most recent timestamps to the top of the list.


```
Dictionary<string, List<TickerModel>> dict =
    giantList
    .GroupBy(_ => _.Ticker)
    .ToDictionary(
        _ => _.Key, 
        _ => _.OrderByDescending(_ => _.Timestamp).ToList());
```

Here, in the debugger view, is what we've got so far:

[![dictionary][1]][1]

___

 4. Use `Linq.Where` to identify the groups that have more than 15 items.

```
string[] groupsToTruncate =
    dict
    .Where(_ => _.Value.Count > 15)
    .Select(kvp => kvp.Key)
    .ToArray();
```

___

 5. Use `Linq.Take` to keep a maximum of "fifteen" items.

```
foreach (var groupKey in groupsToTruncate)
{
    dict[groupKey] = dict[groupKey].Take(15).ToList();
}
```

Here's the end result:

[![dictionary after processing][2]][2]

---

_If your list gets so gigantic that you don't want to process it in memory, you could take a similar approach to ticker items stored in a local SQLite database, and be able to query and sort using `SQL` instead._

  [1]: https://i.stack.imgur.com/TIn5N.png
  [2]: https://i.stack.imgur.com/ogrSt.png