@echo off
setlocal

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::                      RedEye Uninstaller Script                     ::
::                                                                    ::
:: This script reverts all the changes install.bat script made.       ::
:: If you want to run it quietly (without any interaction with user), ::
:: you can use "/q" flag: uninstall.bat /q                            ::
::                                                                    ::
:: P.S. I love Batch! <3                                              ::
::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

:: Quiet mode (/q flag) check
if "%~1"=="/q" set _QUIET=1

:: Check if administrator privileges are granted
reg query "HKEY_USERS\S-1-5-20" > nul 2> nul
if not %ERRORLEVEL%==0 (
    echo.ERROR: This script requires administrator privileges!
    goto :end
)

:: Delete shell registration
reg delete "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /f

:: Delete scheduled task
Schtasks /Delete /TN "RedEyeElevatedService" /F


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

echo.Unregistering COM types...
"%FrameworkDir%\RegAsm.exe" /u redeye.exe

:end
if "%_QUIET%"=="" pause> nul
endlocal
