using Zaya.Translator.Impl.Yandex.Constants;
using Zaya.Translator.Impl.Yandex;
using Zaya.Translator.Impl.Yandex.Services.Impl;

namespace Zaya.Translator.Impl.Yandex.Tests;

public sealed class YandexTranslatorServiceTests
{
    [Fact]
    public void AutoDetectLanguage_DefaultsToTrue_AndHidesSourceWhenUnset()
    {
        using var service = new YandexTranslatorService();
        var auto = Assert.IsType<Zaya.Primitives.BooleanSettingDescriptor>(
            service.Settings.Single(s => s.Key == "autoDetectLanguage"));
        Assert.True(auto.DefaultValue);

        var source = Assert.IsType<Zaya.Primitives.EnumSettingDescriptor>(
            service.Settings.Single(s => s.Key == "sourceLanguage"));
        var empty = new Dictionary<string, object?>();
        Assert.False(source.IsVisible(empty));
        Assert.False(source.IsRequired(empty));
        Assert.True(source.IsVisible(new Dictionary<string, object?> { ["autoDetectLanguage"] = false }));
    }

    [Fact]
    public void EngineId_ReturnsYandex()
    {
        using var service = new YandexTranslatorService();
        Assert.Equal("yandex", service.EngineId);
    }

    [Fact]
    public void DisplayName_IsNotEmpty()
    {
        using var service = new YandexTranslatorService();
        var name = service.DisplayName.GetValue(System.Globalization.CultureInfo.InvariantCulture);
        Assert.False(string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void Settings_HasExpectedKeys()
    {
        using var service = new YandexTranslatorService();
        var settings = service.Settings;
        Assert.Contains(settings, s => s.Key == "autoDetectLanguage");
        Assert.Contains(settings, s => s.Key == "sourceLanguage");
        Assert.Contains(settings, s => s.Key == "targetLanguage");
        Assert.Contains(settings, s => s.Key == "apiKey");
        Assert.Contains(settings, s => s.Key == "useApiKey");
        Assert.Contains(settings, s => s.Key == "userAgent");
        Assert.Equal(6, settings.Count);

        var userAgent = Assert.IsType<Zaya.Primitives.StringSettingDescriptor>(
            settings.Single(s => s.Key == "userAgent"));
        Assert.False(userAgent.IsVisible(new Dictionary<string, object?>()));
        Assert.False(userAgent.IsRequired(new Dictionary<string, object?>()));
        Assert.False(string.IsNullOrWhiteSpace(userAgent.DefaultValue));
    }

    [Fact]
    public async Task CreateSession_DefaultSettings_Succeeds()
    {
        using var service = new YandexTranslatorService();
        using var session = await service.CreateSessionAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(session);
    }

    [Fact]
    public async Task TranslateAsync_Null_Throws()
    {
        using var service = new YandexTranslatorService();
        using var session = await service.CreateSessionAsync(TestContext.Current.CancellationToken);
        await Assert.ThrowsAsync<ArgumentNullException>(() => session.TranslateAsync((string)null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TranslateAsync_Empty_ReturnsEmpty()
    {
        using var service = new YandexTranslatorService();
        using var session = await service.CreateSessionAsync(TestContext.Current.CancellationToken);
        var result = await session.TranslateAsync("", TestContext.Current.CancellationToken);
        Assert.Equal("", result);
    }

    [Fact]
    public async Task TranslateAsync_WithApiKey_ReturnsTranslation()
    {
        var apiKey = Environment.GetEnvironmentVariable("YANDEX_TRANSLATE_API_KEY");
        Assert.SkipWhen(string.IsNullOrWhiteSpace(apiKey), "Set YANDEX_TRANSLATE_API_KEY to run this test.");

        var settingsDict = new Dictionary<string, object>
        {
            ["autoDetectLanguage"] = false,
            ["sourceLanguage"] = LanguageCodeConstants.English,
            ["targetLanguage"] = "ru",
            ["useApiKey"] = true,
            ["apiKey"] = apiKey!,
        };

        using var service = new YandexTranslatorService();
        using var session = await service.CreateSessionAsync(settingsDict, TestContext.Current.CancellationToken);

        var result = await session.TranslateAsync("Hello", TestContext.Current.CancellationToken);
        Assert.NotEmpty(result);
        Assert.NotEqual("Hello", result);
    }

    [Fact]
    public void FormatBrowserLang_AutoDetect_UsesTargetOnly()
    {
        Assert.Equal("ru", YandexTranslatorSession.FormatBrowserLang(null, "ru"));
        Assert.Equal(LanguageCodeConstants.English, YandexTranslatorSession.FormatBrowserLang(null, LanguageCodeConstants.English));
    }

    [Fact]
    public void FormatBrowserLang_ExplicitSource_UsesPair()
    {
        Assert.Equal("en-ru", YandexTranslatorSession.FormatBrowserLang(LanguageCodeConstants.English, "ru"));
        Assert.Equal("zh-ru", YandexTranslatorSession.FormatBrowserLang(LanguageCodeConstants.ChineseSimplified, "ru"));
    }

    [Fact]
    public async Task TranslateAsync_AutoDetect_BrowserApi_ReturnsTranslation()
    {
        var settingsDict = new Dictionary<string, object>
        {
            ["autoDetectLanguage"] = true,
            ["targetLanguage"] = "ru",
            ["useApiKey"] = false,
        };

        using var service = new YandexTranslatorService();
        using var session = await service.CreateSessionAsync(settingsDict, TestContext.Current.CancellationToken);

        var result = await session.TranslateAsync("Hello", TestContext.Current.CancellationToken);
        Assert.NotEmpty(result);
        Assert.NotEqual("Hello", result);
    }

    [Fact]
    public void Settings_LanguageOptions_CollapseChinese()
    {
        using var service = new YandexTranslatorService();
        var target = Assert.IsType<Zaya.Primitives.EnumSettingDescriptor>(
            service.Settings.Single(s => s.Key == "targetLanguage"));
        Assert.Contains(target.Options, o => o.Value == LanguageCodeConstants.Chinese);
        Assert.DoesNotContain(target.Options, o => o.Value is LanguageCodeConstants.ChineseSimplified or LanguageCodeConstants.ChineseTraditional);
    }
}
