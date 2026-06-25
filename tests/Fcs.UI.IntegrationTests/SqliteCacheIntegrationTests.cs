using Fcs.UI.Cache;
using FluentAssertions;
using Microsoft.Data.Sqlite;

namespace Fcs.UI.IntegrationTests;

public class SqliteCacheIntegrationTests : IDisposable
{
    private readonly string _dbPath;
    private readonly SqliteCacheContext _ctx;
    private readonly SqliteCacheService _cache;

    public SqliteCacheIntegrationTests()
    {
        _dbPath = $"{Guid.NewGuid():N}.db";
        _ctx = new SqliteCacheContext(_dbPath);
        _cache = new SqliteCacheService(_ctx);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        SqliteConnection.ClearAllPools();
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); }
        catch { /* best-effort cleanup */ }
    }

    [Fact]
    public async Task SaveAndGet_ReturnsSameItem()
    {
        var item = new TestItem { Id = 1, Name = "Test" };

        await _cache.SaveAsync("k1", item);
        var result = await _cache.GetAsync<TestItem>("k1");

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetNonExistent_ReturnsNull()
    {
        var result = await _cache.GetAsync<TestItem>("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task OverwriteKey_UpdatesValue()
    {
        await _cache.SaveAsync("key", new TestItem { Id = 1, Name = "First" });
        await _cache.SaveAsync("key", new TestItem { Id = 2, Name = "Second" });

        var result = await _cache.GetAsync<TestItem>("key");
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("Second");
    }

    [Fact]
    public async Task MultipleKeys_AreIndependent()
    {
        await _cache.SaveAsync("a", new TestItem { Id = 1, Name = "A" });
        await _cache.SaveAsync("b", new TestItem { Id = 2, Name = "B" });

        (await _cache.GetAsync<TestItem>("a"))!.Name.Should().Be("A");
        (await _cache.GetAsync<TestItem>("b"))!.Name.Should().Be("B");
    }

    private sealed record TestItem
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
