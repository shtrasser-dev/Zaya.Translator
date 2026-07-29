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

echo === Detecting versions ===

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -getProperty:Version -nologo -v:q`) do set IFACE=%%a
set IFACE=!IFACE: =!
if "!IFACE!"=="" set IFACE=0.4.0

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -getProperty:ZayaPrimitivesVersion -nologo -v:q`) do set PRIM=%%a
set PRIM=!PRIM: =!
if "!PRIM!"=="" set PRIM=0.4.0

for /f "tokens=1,2 delims=." %%a in ("!PRIM!") do set CHANNEL=%%a.%%b
if "!CHANNEL!"=="." set CHANNEL=0.4

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -getProperty:Version -nologo -v:q`) do set VER_YANDEX=%%a
set VER_YANDEX=!VER_YANDEX: =!
if "!VER_YANDEX!"=="" set VER_YANDEX=!IFACE!

for /f "usebackq delims=" %%a in (`dotnet msbuild "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -getProperty:Version -nologo -v:q`) do set VER_GOOGLE=%%a
set VER_GOOGLE=!VER_GOOGLE: =!
if "!VER_GOOGLE!"=="" set VER_GOOGLE=!IFACE!

set MAXVER=!VER_YANDEX!
if "!VER_GOOGLE!" gtr "!MAXVER!" set MAXVER=!VER_GOOGLE!

echo   Interface=!IFACE!  Channel=!CHANNEL!  MaxPlugin=!MAXVER!
echo   Yandex=!VER_YANDEX!  Google=!VER_GOOGLE!

echo === Preparing output directory ===

rmdir /s /q "%ROOT%out" 2>nul
mkdir "%ROOT%out" 2>nul

echo !MAXVER!>"%ROOT%out\version.txt"
echo !CHANNEL!>"%ROOT%out\channel.txt"
del "%ROOT%out\plugins.versions.txt" 2>nul

echo === Creating Zaya.Translator.Impl.Yandex plugin.zip ===
call :MakeZip Yandex translator "%ROOT%src\Zaya.Translator.Impl.Yandex\bin\%BUILD_CONFIG%\net8.0" Zaya.Translator.Impl.Yandex.dll Zaya.Translator.Impl.Yandex.zip !VER_YANDEX!
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Creating Zaya.Translator.Impl.Google plugin.zip ===
call :MakeZip Google translator "%ROOT%src\Zaya.Translator.Impl.Google\bin\%BUILD_CONFIG%\net8.0" Zaya.Translator.Impl.Google.dll Zaya.Translator.Impl.Google.zip !VER_GOOGLE!
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Packing NuGet packages ===

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Yandex\Zaya.Translator.Impl.Yandex.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator.Impl.Google\Zaya.Translator.Impl.Google.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

dotnet pack "%ROOT%src\Zaya.Translator\Zaya.Translator.csproj" -c %BUILD_CONFIG% -o "%ROOT%out" --no-build
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo === Cleaning up ===

rmdir /s /q "%STAGEDIR%" 2>nul

echo === Done: interface !IFACE! channel !CHANNEL! release !MAXVER! ===
goto :eof

:MakeZip
    set ZIP_ID=%~1
    set ZIP_TYPE=%~2
    set ZIP_TFM=%~3
    set ZIP_DLL=%~4
    set ZIP_NAME=%~5
    set ZIP_PVER=%~6

    rmdir /s /q "%STAGEDIR%" 2>nul
    mkdir "%STAGEDIR%"

    copy /y "%ZIP_TFM%\%ZIP_DLL%" "%STAGEDIR%"
    if %ERRORLEVEL% neq 0 (
        echo ERROR: Could not find %ZIP_DLL%
        exit /b 1
    )

    call :CopySatellites "%ZIP_TFM%" "%STAGEDIR%"

    set PLUGIN_JSON=%STAGEDIR%\plugin.json
    echo {>"%PLUGIN_JSON%"
    echo   "id": "!ZIP_ID!",>>"%PLUGIN_JSON%"
    echo   "type": "!ZIP_TYPE!",>>"%PLUGIN_JSON%"
    echo   "interface": "Zaya.Translator",>>"%PLUGIN_JSON%"
    echo   "interfaceVersion": "!IFACE!",>>"%PLUGIN_JSON%"
    echo   "pluginVersion": "!ZIP_PVER!",>>"%PLUGIN_JSON%"
    echo   "primitivesChannel": "!CHANNEL!">>"%PLUGIN_JSON%"
    echo }>>"%PLUGIN_JSON%"

    powershell -Command "Compress-Archive -Path '%STAGEDIR%\*' -DestinationPath '%ROOT%out\!ZIP_NAME!' -Force"
    echo   out\!ZIP_NAME!  pluginVersion=!ZIP_PVER!
    echo !ZIP_NAME!=!ZIP_PVER!>>"%ROOT%out\plugins.versions.txt"
    exit /b 0

:CopySatellites
    for /d %%d in ("%~1\*") do (
        if exist "%%d\*.resources.dll" (
            mkdir "%~2\%%~nxd" 2>nul
            copy /y "%%d\*" "%~2\%%~nxd\"
        )
    )
    exit /b
