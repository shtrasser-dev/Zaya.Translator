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

echo === Detecting version ===

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -getProperty:Version -nologo -v:q`) do set VER=%%a
set VER=!VER: =!
if "!VER!"=="" set VER=0.4.0

for /f "tokens=1,2,3 delims=." %%a in ("!VER!") do (
    set VER_MAJOR=%%a
    set VER_MINOR=%%b
    set VER_PATCH=%%c
)
set CHANNEL=!VER_MAJOR!.!VER_MINOR!
echo   Version=!VER!  Channel=!CHANNEL!

echo === Preparing output directory ===

rmdir /s /q "%ROOT%out" 2>nul
mkdir "%ROOT%out" 2>nul

echo !VER!>"%ROOT%out\version.txt"
echo !CHANNEL!>"%ROOT%out\channel.txt"

echo === Creating Zaya.Translator.Impl.Yandex plugin.zip ===

rmdir /s /q "%STAGEDIR%" 2>nul
mkdir "%STAGEDIR%"

set YANDEX_TFM=%ROOT%src\Zaya.Translator.Impl.Yandex\bin\%BUILD_CONFIG%\net8.0

copy /y "%YANDEX_TFM%\Zaya.Translator.Impl.Yandex.dll" "%STAGEDIR%"
if %ERRORLEVEL% neq 0 (
    echo ERROR: Could not find Yandex DLL
    exit /b 1
)

call :CopySatellites "%YANDEX_TFM%" "%STAGEDIR%"

set PLUGIN_JSON=%STAGEDIR%\plugin.json

echo {>"%PLUGIN_JSON%"
echo   "id": "Yandex",>>"%PLUGIN_JSON%"
echo   "type": "translator",>>"%PLUGIN_JSON%"
echo   "interface": "Zaya.Translator",>>"%PLUGIN_JSON%"
echo   "interfaceVersion": "!VER!",>>"%PLUGIN_JSON%"
echo   "pluginVersion": "!VER!",>>"%PLUGIN_JSON%"
echo   "primitivesChannel": "!CHANNEL!">>"%PLUGIN_JSON%"
echo }>>"%PLUGIN_JSON%"

REM Stable asset name (no version in filename) for host updater.
powershell -Command "Compress-Archive -Path '%STAGEDIR%\*' -DestinationPath '%ROOT%out\Zaya.Translator.Impl.Yandex.zip' -Force"
echo   out\Zaya.Translator.Impl.Yandex.zip

echo === Creating Zaya.Translator.Impl.Google plugin.zip ===

rmdir /s /q "%STAGEDIR%" 2>nul
mkdir "%STAGEDIR%"

set GOOGLE_TFM=%ROOT%src\Zaya.Translator.Impl.Google\bin\%BUILD_CONFIG%\net8.0

copy /y "%GOOGLE_TFM%\Zaya.Translator.Impl.Google.dll" "%STAGEDIR%"
if %ERRORLEVEL% neq 0 (
    echo ERROR: Could not find Google DLL
    exit /b 1
)

call :CopySatellites "%GOOGLE_TFM%" "%STAGEDIR%"

set PLUGIN_JSON=%STAGEDIR%\plugin.json

echo {>"%PLUGIN_JSON%"
echo   "id": "Google",>>"%PLUGIN_JSON%"
echo   "type": "translator",>>"%PLUGIN_JSON%"
echo   "interface": "Zaya.Translator",>>"%PLUGIN_JSON%"
echo   "interfaceVersion": "!VER!",>>"%PLUGIN_JSON%"
echo   "pluginVersion": "!VER!",>>"%PLUGIN_JSON%"
echo   "primitivesChannel": "!CHANNEL!">>"%PLUGIN_JSON%"
echo }>>"%PLUGIN_JSON%"

powershell -Command "Compress-Archive -Path '%STAGEDIR%\*' -DestinationPath '%ROOT%out\Zaya.Translator.Impl.Google.zip' -Force"
echo   out\Zaya.Translator.Impl.Google.zip

echo === Packing NuGet packages ===

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Cleaning up ===

rmdir /s /q "%STAGEDIR%" 2>nul

echo === Done: version !VER! channel !CHANNEL! ===
goto :eof

:CopySatellites
    for /d %%d in ("%~1\*") do (
        if exist "%%d\*.resources.dll" (
            mkdir "%~2\%%~nxd" 2>nul
            copy /y "%%d\*" "%~2\%%~nxd\"
        )
    )
    exit /b
