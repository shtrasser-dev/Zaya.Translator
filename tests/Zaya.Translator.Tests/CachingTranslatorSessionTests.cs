using Zaya.Translator.Services;
using Zaya.Translator.Services.Impl;

namespace Zaya.Translator.Tests;

public sealed class CachingTranslatorSessionTests
{
    [Fact]
    public async Task TranslateAsync_CachesResult()
    {
        var inner = new CountingSession("hola");
        var cache = new MemoryTranslationCache();
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
        var cache = new MemoryTranslationCache();
        using var session = new CachingTranslatorSession(inner, cache);

        await session.TranslateAsync("one", TestContext.Current.CancellationToken);
        var batch = await session.TranslateAsync(new[] { "one", "two" }, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "A", "B" }, batch);
        Assert.Equal(2, inner.CallCount); // one single + one batch with only "two"
    }

    [Fact]
    public async Task TranslateAsync_AfterDispose_Throws()
    {
        var session = new CachingTranslatorSession(new CountingSession("x"), new MemoryTranslationCache());
        session.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            session.TranslateAsync("hello", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TranslateAsync_Null_Throws()
    {
        using var session = new CachingTranslatorSession(new CountingSession("x"), new MemoryTranslationCache());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            session.TranslateAsync((string)null!, TestContext.Current.CancellationToken));
    }

    private sealed class CountingSession : ITranslatorSession
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
        var cache = new MemoryTranslationCache(TimeSpan.FromMilliseconds(1));
        cache.Set("a", "b");
        Thread.Sleep(20);
        Assert.False(cache.TryGet("a", out _));
    }

    [Fact]
    public void TryGet_WithoutTtl_ReturnsCached()
    {
        var cache = new MemoryTranslationCache();
        cache.Set("a", "b");
        Assert.True(cache.TryGet("a", out var value));
        Assert.Equal("b", value);
    }
}
