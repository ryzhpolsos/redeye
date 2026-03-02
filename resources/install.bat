@echo off
setlocal
if "%~1"=="/q" set _QUIET=1
pushd "%~dp0"

if not exist "redeye.exe" (
    echo.ERROR: No "redeye.exe" file found!
    echo.Make sure you've extracted the archive before running this script.
    goto :end
)

reg add "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /d "%CD%\redeye.exe" /t REG_SZ /f

if %ERRORLEVEL%==0 (
    echo.RedEye is installed successfully. Now you can log off and then log on back to see your new shell.
) else (
    echo.ERROR: installer process exited with error code %ERRORLEVEL%.
)

:end
if "%_QUIET%"=="" pause> nul
popd
endlocal
