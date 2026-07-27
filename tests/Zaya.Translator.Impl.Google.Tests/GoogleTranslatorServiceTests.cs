using Zaya.Translator.Impl.Google.Services;

namespace Zaya.Translator.Impl.Google.Tests;

public sealed class GoogleTranslatorServiceTests
{
    [Fact]
    public void EngineId_ReturnsGoogle()
    {
        using var service = new GoogleTranslatorService();
        Assert.Equal("google", service.EngineId);
    }

    [Fact]
    public void DisplayName_IsNotEmpty()
    {
        using var service = new GoogleTranslatorService();
        var name = service.DisplayName.GetValue(System.Globalization.CultureInfo.InvariantCulture);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void Settings_HasExpectedKeys()
    {
        using var service = new GoogleTranslatorService();
        var settings = service.Settings;
        Assert.Contains(settings, s => s.Key == "autoDetectLanguage");
        Assert.Contains(settings, s => s.Key == "sourceLanguage");
        Assert.Contains(settings, s => s.Key == "targetLanguage");
        Assert.Contains(settings, s => s.Key == "enableCache");
        Assert.Contains(settings, s => s.Key == "cacheTtlMinutes");
        Assert.Equal(5, settings.Count);
    }

    [Fact]
    public async Task CreateSession_DefaultSettings_Succeeds()
    {
        using var service = new GoogleTranslatorService();
        using var session = await service.CreateSessionAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(session);
    }

    [Fact]
    public async Task TranslateAsync_Null_Throws()
    {
        using var service = new GoogleTranslatorService();
        using var session = await service.CreateSessionAsync(TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<ArgumentNullException>(() => session.TranslateAsync((string)null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TranslateAsync_Empty_ReturnsEmpty()
    {
        using var service = new GoogleTranslatorService();
        using var session = await service.CreateSessionAsync(TestContext.Current.CancellationToken);
        var result = await session.TranslateAsync("", TestContext.Current.CancellationToken);
        Assert.Equal("", result);
    }
}
