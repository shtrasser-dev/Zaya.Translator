using Zaya.Translator.Impl.Google;
using Zaya.Translator.Impl.Google.Exceptions;
using Zaya.Translator.Impl.Google.Services.Impl;

namespace Zaya.Translator.Impl.Google.Tests;

public sealed class GoogleTranslatorServiceTests
{
    [Fact]
    public void AutoDetectLanguage_DefaultsToTrue_AndHidesSourceWhenUnset()
    {
        using var service = new GoogleTranslatorService();
        var auto = Assert.IsType<Zaya.Primitives.Settings.BooleanSettingDescriptor>(
            service.Settings.Single(s => s.Key == "autoDetectLanguage"));
        Assert.True(auto.DefaultValue);

        var source = Assert.IsType<Zaya.Primitives.Settings.EnumSettingDescriptor>(
            service.Settings.Single(s => s.Key == "sourceLanguage"));
        var empty = new Dictionary<string, object?>();
        Assert.False(source.IsVisible(empty));
        Assert.False(source.IsRequired(empty));
        Assert.True(source.IsVisible(new Dictionary<string, object?> { ["autoDetectLanguage"] = false }));
    }

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
        Assert.Contains(settings, s => s.Key == "userAgent");
        Assert.Equal(4, settings.Count);

        var userAgent = Assert.IsType<Zaya.Primitives.Settings.StringSettingDescriptor>(
            settings.Single(s => s.Key == "userAgent"));
        Assert.False(userAgent.IsVisible(new Dictionary<string, object?>()));
        Assert.False(userAgent.IsRequired(new Dictionary<string, object?>()));
        Assert.False(string.IsNullOrWhiteSpace(userAgent.DefaultValue));
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

    [Fact]
    public void WrapBatch_JoinsSegmentsWithMarkers()
    {
        var wrapped = GoogleTranslatorSession.WrapBatch(["Hello", "World"]);
        Assert.Equal("([Hello])([World])", wrapped);
    }

    [Fact]
    public void UnwrapBatch_ParsesMarkedSegments()
    {
        var parts = GoogleTranslatorSession.UnwrapBatch(
            "([Утреннее солнце]) ([Пассажиры двигались])",
            expectedCount: 2);
        Assert.Equal(["Утреннее солнце", "Пассажиры двигались"], parts);
    }

    [Fact]
    public void UnwrapBatch_WrongCount_Throws()
    {
        Assert.Throws<GoogleTranslateParseException>(() =>
            GoogleTranslatorSession.UnwrapBatch("([only-one])", expectedCount: 2));
    }
}
