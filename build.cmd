@echo off
setlocal enabledelayedexpansion

set ROOT=%~dp0
set STAGEDIR=%TEMP%\Zaya.Translator\staging

if "%CI%"=="true" (
    set BUILD_CONFIG=Release
) else (
    set BUILD_CONFIG=Debug
)

echo === Building Zaya.Translator.Impl.Yandex (%BUILD_CONFIG%) ===

dotnet build "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -c %BUILD_CONFIG%
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Building Zaya.Translator.Impl.Google (%BUILD_CONFIG%) ===

dotnet build "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -c %BUILD_CONFIG%
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Building Zaya.TranslatorCache.Impl.Memory (%BUILD_CONFIG%) ===

dotnet build "%ROOT%src\Zaya.TranslatorCache.Impl.Memory\Zaya.TranslatorCache.Impl.Memory.csproj" -c %BUILD_CONFIG%
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Detecting versions ===

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -getProperty:Version -nologo -v:q`) do set IFACE=%%a
set IFACE=!IFACE: =!
if "!IFACE!"=="" set IFACE=1.0.0

for /f "tokens=1,2 delims=." %%a in ("!IFACE!") do set CHANNEL=%%a.%%b
if "!CHANNEL!"=="." set CHANNEL=1.0

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.TranslatorCache\Zaya.TranslatorCache.csproj" -getProperty:Version -nologo -v:q`) do set IFACE_CACHE=%%a
set IFACE_CACHE=!IFACE_CACHE: =!
if "!IFACE_CACHE!"=="" set IFACE_CACHE=1.0.0

for /f "tokens=1,2 delims=." %%a in ("!IFACE_CACHE!") do set CHANNEL_CACHE=%%a.%%b
if "!CHANNEL_CACHE!"=="." set CHANNEL_CACHE=1.0

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -getProperty:Version -nologo -v:q`) do set VER_YANDEX=%%a
set VER_YANDEX=!VER_YANDEX: =!
if "!VER_YANDEX!"=="" set VER_YANDEX=!IFACE!

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -getProperty:Version -nologo -v:q`) do set VER_GOOGLE=%%a
set VER_GOOGLE=!VER_GOOGLE: =!
if "!VER_GOOGLE!"=="" set VER_GOOGLE=!IFACE!

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.TranslatorCache.Impl.Memory\Zaya.TranslatorCache.Impl.Memory.csproj" -getProperty:Version -nologo -v:q`) do set VER_MEMORY=%%a
set VER_MEMORY=!VER_MEMORY: =!
if "!VER_MEMORY!"=="" set VER_MEMORY=!IFACE_CACHE!

set VER_TRANSLATOR=!VER_YANDEX!
if "!VER_GOOGLE!" gtr "!VER_TRANSLATOR!" set VER_TRANSLATOR=!VER_GOOGLE!

set MAXVER=!VER_TRANSLATOR!
if "!VER_MEMORY!" gtr "!MAXVER!" set MAXVER=!VER_MEMORY!

echo   TranslatorIface=!IFACE!  CacheIface=!IFACE_CACHE!  Channels=!CHANNEL! / !CHANNEL_CACHE!  MaxPlugin=!MAXVER!
echo   Yandex=!VER_YANDEX!  Google=!VER_GOOGLE!  Memory=!VER_MEMORY!

echo === Preparing output directory ===

rmdir /s /q "%ROOT%out" 2>nul
mkdir "%ROOT%out" 2>nul

echo !MAXVER!>"%ROOT%out\version.txt"
echo !CHANNEL!>"%ROOT%out\channel.txt"
del "%ROOT%out\plugins.versions.txt" 2>nul

REM One floating/immutable GitHub release per interface package.
> "%ROOT%out\interfaces.json" (
echo [
echo   {"interface":"Zaya.Translator","channel":"!CHANNEL!","version":"!VER_TRANSLATOR!","assets":["Zaya.Translator.Impl.Google.zip","Zaya.Translator.Impl.Yandex.zip"]},
echo   {"interface":"Zaya.TranslatorCache","channel":"!CHANNEL_CACHE!","version":"!VER_MEMORY!","assets":["Zaya.TranslatorCache.Impl.Memory.zip"]}
echo ]
)

echo === Creating Zaya.Translator.Impl.Yandex plugin.zip ===
call :MakeZip Yandex translator Zaya.Translator "%ROOT%src\Zaya.Translator.Impl.Yandex\bin\%BUILD_CONFIG%\net8.0" Zaya.Translator.Impl.Yandex.dll Zaya.Translator.Impl.Yandex.zip !IFACE! !VER_YANDEX! Zaya.Translator.Impl.Yandex.YandexTranslatorService
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Creating Zaya.Translator.Impl.Google plugin.zip ===
call :MakeZip Google translator Zaya.Translator "%ROOT%src\Zaya.Translator.Impl.Google\bin\%BUILD_CONFIG%\net8.0" Zaya.Translator.Impl.Google.dll Zaya.Translator.Impl.Google.zip !IFACE! !VER_GOOGLE! Zaya.Translator.Impl.Google.GoogleTranslatorService
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Creating Zaya.TranslatorCache.Impl.Memory plugin.zip ===
call :MakeZip Memory translator-cache Zaya.TranslatorCache "%ROOT%src\Zaya.TranslatorCache.Impl.Memory\bin\%BUILD_CONFIG%\net8.0" Zaya.TranslatorCache.Impl.Memory.dll Zaya.TranslatorCache.Impl.Memory.zip !IFACE_CACHE! !VER_MEMORY! Zaya.TranslatorCache.Impl.Memory.MemoryTranslatorCacheService
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Packing NuGet packages ===

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.TranslatorCache.Impl.Memory\Zaya.TranslatorCache.Impl.Memory.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.TranslatorCache\Zaya.TranslatorCache.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Cleaning up ===

rmdir /s /q "%STAGEDIR%" 2>nul

echo === Done: interface !IFACE! updateChannel !CHANNEL! release !MAXVER! ===
goto :eof

:MakeZip
    set ZIP_ID=%~1
    set ZIP_TYPE=%~2
    set ZIP_IFACE_NAME=%~3
    set ZIP_TFM=%~4
    set ZIP_DLL=%~5
    set ZIP_NAME=%~6
    set ZIP_IFACE_VER=%~7
    set ZIP_PVER=%~8
    set ZIP_ENTRY=%~9

    rmdir /s /q "%STAGEDIR%" 2>nul
    mkdir "%STAGEDIR%"

    copy /y "%ZIP_TFM%\%ZIP_DLL%" "%STAGEDIR%"
    if %ERRORLEVEL% neq 0 (
        echo ERROR: Could not find %ZIP_DLL%
        exit /b 1
    )

    call :CopySatellites "%ZIP_TFM%" "%STAGEDIR%" "%ZIP_DLL%"

    set PLUGIN_JSON=%STAGEDIR%\plugin.json
    echo {>"%PLUGIN_JSON%"
    echo   "id": "!ZIP_ID!",>>"%PLUGIN_JSON%"
    echo   "type": "!ZIP_TYPE!",>>"%PLUGIN_JSON%"
    echo   "interface": "!ZIP_IFACE_NAME!",>>"%PLUGIN_JSON%"
    echo   "interfaceVersion": "!ZIP_IFACE_VER!",>>"%PLUGIN_JSON%"
    echo   "pluginVersion": "!ZIP_PVER!",>>"%PLUGIN_JSON%"
    echo   "entryPoint": "!ZIP_ENTRY!">>"%PLUGIN_JSON%"
    echo }>>"%PLUGIN_JSON%"

    powershell -Command "Compress-Archive -Path '%STAGEDIR%\*' -DestinationPath '%ROOT%out\!ZIP_NAME!' -Force"
    echo   out\!ZIP_NAME!  pluginVersion=!ZIP_PVER!
    echo !ZIP_NAME!=!ZIP_PVER!>>"%ROOT%out\plugins.versions.txt"
    exit /b 0

REM Copy culture satellites for the plugin assembly only.
REM Do not use *.resources.dll — some TFMs ship other *.resources.dll that must not be packed.
:CopySatellites
    set "SAT_DLL=%~n3.resources.dll"
    for /d %%d in ("%~1\*") do (
        if exist "%%d\!SAT_DLL!" (
            mkdir "%~2\%%~nxd" 2>nul
            copy /y "%%d\!SAT_DLL!" "%~2\%%~nxd\"
        )
    )
    exit /b
