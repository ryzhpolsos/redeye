@echo off
setlocal

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::                       RedEye Installer Script                      ::
::                                                                    ::
:: This script installs RedEye as default shell for the current user. ::
:: If you want to run it quietly (without any interaction with user), ::
:: you can use "/q" flag: install.bat /q                              ::
::                                                                    ::
:: P.S. I love Batch! <3                                              ::
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

:: Quiet mode (/q flag) check
if "%~1"=="/q" set _QUIET=1

:: Go to script directory
pushd "%~dp0"

:: Check if user has extracted the downloaded archive
:: Yes, there are A LOT OF users who don't do that.
if not exist "redeye.exe" (
    echo.ERROR: No "redeye.exe" file found!
    echo.Make sure you've extracted the archive before running this script.
    goto :end
)

:: Check if administrator privileges are granted
reg query "HKEY_USERS\S-1-5-20" > nul 2> nul
if not %ERRORLEVEL%==0 (
    echo.ERROR: This script requires administrator privileges!
    goto :end
)

:: Total error counter
set ErrorTotal=0

echo.Registering RedEye as the current user's shell...
reg add "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /d "%CD%\redeye.exe" /t REG_SZ /f
set /a ErrorTotal=ErrorTotal+%ERRORLEVEL%

echo.Creating scheduled task for RedEye Elevated Service...
Schtasks /Create /TN "RedEyeElevatedService" /TR "\"%CD%\redeye.exe\" --elevated-service" /RL Highest /SC Once /ST 00:00 /SD 01/01/2000 /F
set /a ErrorTotal=ErrorTotal+%ERRORLEVEL%

:: Find latest .NET Framework version available

if defined FrameworkDir goto :FoundFrameworkDir

for /f "tokens=*" %%i in ('dir /b /o:-n "%WINDIR%\Microsoft.NET\Framework64"') do (
    set "FrameworkDir=%WINDIR%\Microsoft.NET\Framework64\%%i"
    goto :FoundFrameworkDir
)

:FoundFrameworkDir

if not exist "%FrameworkDir%\RegAsm.exe" (
    echo.ERROR: RegAsm.exe not found in %FrameworkDir%.
    echo.Please specify path to .NET Framework v4 binaries by setting the FrameworkDir environment variable.
    set /a ErrorTotal=ErrorTotal+1
    goto :end
)

echo.Registering COM types...
"%FrameworkDir%\RegAsm.exe" redeye.exe

if %ErrorTotal%==0 (
    echo.RedEye is installed successfully. Log back in to see your new shell.
) else (
    echo.ERROR: installer process failed.
)

echo.Setting environment variables...
setx /m REDEYE_DIRECTORY "%CD%"

:end
if "%_QUIET%"=="" pause> nul
popd
endlocal
