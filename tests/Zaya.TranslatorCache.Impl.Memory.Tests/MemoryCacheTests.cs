using Zaya.Translator.Services;
using Zaya.TranslatorCache.Impl.Memory.Services;
using MemoryCache = Zaya.TranslatorCache.Impl.Memory.Services.MemoryTranslationCache;

namespace Zaya.TranslatorCache.Impl.Memory.Tests;

public sealed class CachingTranslatorSessionTests
{
    [Fact]
    public async Task TranslateAsync_CachesResult()
    {
        var inner = new CountingSession("hola");
        var cache = new MemoryCache();
        using var session = new CachingTranslatorSession(inner, cache);

        var first = await session.TranslateAsync("hello", TestContext.Current.CancellationToken);
        var second = await session.TranslateAsync("hello", TestContext.Current.CancellationToken);

        Assert.Equal("hola", first);
        Assert.Equal("hola", second);
        Assert.Equal(1, inner.CallCount);
    }

    [Fact]
    public async Task TranslateAsync_Batch_UsesCacheForHits()
    {
        var inner = new CountingSession("A", "B");
        var cache = new MemoryCache();
        using var session = new CachingTranslatorSession(inner, cache);

        await session.TranslateAsync("one", TestContext.Current.CancellationToken);
        var batch = await session.TranslateAsync(new[] { "one", "two" }, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "A", "B" }, batch);
        Assert.Equal(2, inner.CallCount);
    }

    [Fact]
    public async Task TranslateAsync_AfterDispose_Throws()
    {
        var session = new CachingTranslatorSession(new CountingSession("x"), new MemoryCache());
        session.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            session.TranslateAsync("hello", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TranslateAsync_Null_Throws()
    {
        using var session = new CachingTranslatorSession(new CountingSession("x"), new MemoryCache());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            session.TranslateAsync((string)null!, TestContext.Current.CancellationToken));
    }

    internal sealed class CountingSession : ITranslatorSession
    {
        private readonly Queue<string> _results;
        public int CallCount { get; private set; }

        public CountingSession(params string[] results) => _results = new Queue<string>(results);

        public Task<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(_results.Dequeue());
        }

        public Task<IReadOnlyList<string>> TranslateAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
        {
            CallCount++;
            var list = texts.Select(_ => _results.Dequeue()).ToList();
            return Task.FromResult<IReadOnlyList<string>>(list);
        }

        public void Dispose() { }
    }
}

public sealed class MemoryTranslationCacheTests
{
    [Fact]
    public void TryGet_AfterTtl_ReturnsFalse()
    {
        var cache = new MemoryCache(TimeSpan.FromMilliseconds(1));
        cache.Set("a", "b");
        Thread.Sleep(20);
        Assert.False(cache.TryGet("a", out _));
    }

    [Fact]
    public void TryGet_WithoutTtl_ReturnsCached()
    {
        var cache = new MemoryCache();
        cache.Set("a", "b");
        Assert.True(cache.TryGet("a", out var value));
        Assert.Equal("b", value);
    }

    [Fact]
    public void TryGet_DifferentKey_IsMiss()
    {
        var cache = new MemoryCache();
        cache.Set("Hello I am here with you!", "TRANSLATED");
        Assert.False(cache.TryGet("Hello am here with you!", out _));
    }
}

public sealed class MemoryTranslatorCacheServiceTests
{
    [Fact]
    public void EngineId_IsStable()
    {
        using var service = new MemoryTranslatorCacheService();
        Assert.Equal(MemoryTranslatorCacheService.EngineIdValue, service.EngineId);
    }

    [Fact]
    public void Settings_HasExpectedKeys()
    {
        using var service = new MemoryTranslatorCacheService();
        var keys = service.Settings.Select(s => s.Key).ToList();
        Assert.Contains("enableCache", keys);
        Assert.Contains("cacheTtlMinutes", keys);
        Assert.Equal(2, keys.Count);
    }

    [Fact]
    public async Task WrapSessionAsync_WhenDisabled_ReturnsInner()
    {
        using var service = new MemoryTranslatorCacheService();
        var inner = new CachingTranslatorSessionTests.CountingSession("x");
        var wrapped = await service.WrapSessionAsync(inner, new Dictionary<string, object>
        {
            ["enableCache"] = false,
        }, TestContext.Current.CancellationToken);

        Assert.Same(inner, wrapped);
        wrapped.Dispose();
    }

    [Fact]
    public async Task WrapSessionAsync_WhenEnabled_Caches()
    {
        using var service = new MemoryTranslatorCacheService();
        var inner = new CachingTranslatorSessionTests.CountingSession("hola");
        using var wrapped = await service.WrapSessionAsync(inner, new Dictionary<string, object>
        {
            ["enableCache"] = true,
        }, TestContext.Current.CancellationToken);

        var a = await wrapped.TranslateAsync("hello", TestContext.Current.CancellationToken);
        var b = await wrapped.TranslateAsync("hello", TestContext.Current.CancellationToken);
        Assert.Equal("hola", a);
        Assert.Equal("hola", b);
        Assert.Equal(1, inner.CallCount);
    }
}
