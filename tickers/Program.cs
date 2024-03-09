// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;

Random _rando = new Random(1);
string[] Ticker = { "AAAA", "BBBB", "CCCC", "DDDD", "EEEE", "FFFF", "GGGG", "HHHH", };

List<TickerModel> giantList =
    Enumerable.Range(1, 1000)
    .Select(_ => new TickerModel
    {
        Ticker = Ticker[_rando.Next(8)],
        Timestamp = DateTime.Now.AddMinutes(-_rando.Next(240)),
    }).ToList();
{ }


Dictionary<string, List<TickerModel>> dict =
    giantList
    .GroupBy(_ => _.Ticker)
    .ToDictionary(
        _ => _.Key, 
        _ => _.OrderByDescending(_ => _.Timestamp).ToList());


string[] groupsToTruncate =
    dict
    .Where(_ => _.Value.Count > 15)
    .Select(kvp => kvp.Key)
    .ToArray();

foreach (var groupKey in groupsToTruncate)
{
    dict[groupKey] = dict[groupKey].Take(15).ToList();
}

{ }

[DebuggerDisplay("{Ticker}@{Timestamp}")]
class TickerModel
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
