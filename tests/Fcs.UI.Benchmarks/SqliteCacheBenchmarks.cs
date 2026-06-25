using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Fcs.UI.Cache;

namespace Fcs.UI.Benchmarks;

[SimpleJob(launchCount: 1, warmupCount: 2, iterationCount: 5)]
public class SqliteCacheBenchmarks
{
    private SqliteCacheContext _ctx = null!;
    private SqliteCacheService _cache = null!;
    private string _tempFile = null!;

    private static readonly CampaignCacheItem TestItem = new(
        Guid.NewGuid(), "Campaign Test", "Description", 10000m, 5000m, "Active", DateTime.UtcNow.AddDays(30), 50.0);

    [GlobalSetup]
    public void Setup()
    {
        _tempFile = Path.GetTempFileName();
        _ctx = new SqliteCacheContext(_tempFile);
        _cache = new SqliteCacheService(_ctx);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _ctx.Dispose();
        if (File.Exists(_tempFile))
            try { File.Delete(_tempFile); } catch { }
    }

    [Benchmark]
    public async Task SaveCampaign()
    {
        await _cache.SaveAsync("test-key", TestItem);
    }

    [Benchmark]
    public async Task SaveAndGetCampaign()
    {
        await _cache.SaveAsync("test-key", TestItem);
        var result = await _cache.GetAsync<CampaignCacheItem>("test-key");
    }

    [Benchmark]
    public async Task GetNonExistent()
    {
        var result = await _cache.GetAsync<CampaignCacheItem>("nonexistent");
    }
}

public sealed record CampaignCacheItem(
    Guid Id,
    string Name,
    string Description,
    decimal Goal,
    decimal Raised,
    string Status,
    DateTime EndDate,
    double ProgressPercent);

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<SqliteCacheBenchmarks>();
        Console.WriteLine(summary);
    }
}
