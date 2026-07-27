@echo off
setlocal enabledelayedexpansion

set ROOT=%~dp0
set STAGEDIR=%TEMP%\Zaya.Translator\staging

echo === Building Zaya.Translator.Impl.Yandex ===

dotnet build "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -c Release
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Building Zaya.Translator.Impl.Google ===

dotnet build "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -c Release
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Detecting versions ===

for /f "tokens=*" %%a in ('findstr /i "<Version>" "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj"') do set INF_LINE=%%a
set INF_LINE=!INF_LINE:^<Version^>=!
set INF_LINE=!INF_LINE:^</Version^>=!
set INF_MAJOR=!INF_LINE:~0,1!
if "!INF_MAJOR!"=="" set INF_MAJOR=0

for /f "tokens=*" %%a in ('findstr /i "<Version>" "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj"') do set YANDEX_VER=%%a
set YANDEX_VER=!YANDEX_VER:^<Version^>=!
set YANDEX_VER=!YANDEX_VER:^</Version^>=!
if "!YANDEX_VER!"=="" set YANDEX_VER=0.1.0

for /f "tokens=*" %%a in ('findstr /i "<Version>" "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj"') do set GOOGLE_VER=%%a
set GOOGLE_VER=!GOOGLE_VER:^<Version^>=!
set GOOGLE_VER=!GOOGLE_VER:^</Version^>=!
if "!GOOGLE_VER!"=="" set GOOGLE_VER=0.1.0

echo === Preparing output directory ===

rmdir /s /q "%ROOT%out" 2>nul
mkdir "%ROOT%out" 2>nul

echo === Creating Zaya.Translator.Impl.Yandex plugin.zip ===

rmdir /s /q "%STAGEDIR%" 2>nul
mkdir "%STAGEDIR%"

set YANDEX_TFM=%ROOT%src\Zaya.Translator.Impl.Yandex\bin\Release\net8.0

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
echo   "interfaceVersion": "!INF_MAJOR!.0.0",>>"%PLUGIN_JSON%"
echo   "pluginVersion": "!YANDEX_VER!">>"%PLUGIN_JSON%"
echo }>>"%PLUGIN_JSON%"

powershell -Command "Compress-Archive -Path '%STAGEDIR%\*' -DestinationPath '%ROOT%out\Zaya.Translator.Impl.Yandex-!YANDEX_VER!.zip' -Force"
echo   out\Zaya.Translator.Impl.Yandex-!YANDEX_VER!.zip

echo === Creating Zaya.Translator.Impl.Google plugin.zip ===

rmdir /s /q "%STAGEDIR%" 2>nul
mkdir "%STAGEDIR%"

set GOOGLE_TFM=%ROOT%src\Zaya.Translator.Impl.Google\bin\Release\net8.0

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
echo   "interfaceVersion": "!INF_MAJOR!.0.0",>>"%PLUGIN_JSON%"
echo   "pluginVersion": "!GOOGLE_VER!">>"%PLUGIN_JSON%"
echo }>>"%PLUGIN_JSON%"

powershell -Command "Compress-Archive -Path '%STAGEDIR%\*' -DestinationPath '%ROOT%out\Zaya.Translator.Impl.Google-!GOOGLE_VER!.zip' -Force"
echo   out\Zaya.Translator.Impl.Google-!GOOGLE_VER!.zip

echo === Packing NuGet packages ===

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -c Release -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -c Release -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -c Release -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Cleaning up ===

rmdir /s /q "%STAGEDIR%" 2>nul

echo === Done: Yandex !YANDEX_VER! ^| Google !GOOGLE_VER! ===
goto :eof

:CopySatellites
    for /d %%d in ("%~1\*") do (
        if exist "%%d\*.resources.dll" (
            mkdir "%~2\%%~nxd" 2>nul
            copy /y "%%d\*" "%~2\%%~nxd\"
        )
    )
    exit /b
