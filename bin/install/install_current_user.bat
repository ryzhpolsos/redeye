@echo off

:: Get parent directory
set "_dp0=%~dp0"
for %%i in ("%_dp0:~,-1%") do set "parent_dir=%%~dpi"

pushd "%parent_dir%"

if exist "%~dp0.noedge" goto register

where curl > nul 2> nul
set curlEl=%ERRORLEVEL%

:: Use alternative server if Russian IP detected
set ru=0
if %curlEl%==0 ( call :findru_curl ) else ( call :findru_vbs )
if %ru%==1 (
	echo.[!] Russian IP detected, downloading from ryzhpolsos.ru instead of microsoft.com
	set "URL=https://ryzhpolsos.ru/files/edgeRuntime.cab"
) else (
	set "URL=https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/f0a77d10-2cde-4371-bc38-4c1dff1036d5/Microsoft.WebView2.FixedVersionRuntime.138.0.3351.109.x64.cab"
)

:: Download edgeRuntime.cab
echo.Downloading Edge WebView2 runtime...
if %curlEl%==0 ( call :download_curl ) else ( call :download_vbs ) 

:: Extract downloaded cab and delete it
echo.Extracting archive...
expand edgeRuntime.cab -r -f:* . > nul
move Microsoft.WebView2.* edgeRuntime > nul
del /f edgeRuntime.cab

:register

:: Register redeye.exe as default shell for current user
reg add "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon" /v Shell /d "%parent_dir%\redeye.exe" /t REG_SZ /f
popd
exit /b

:: *_curl procedures are used when curl was found, otherwise the *_vbs ones are used
:: findru_curl and findru_vbs set "ru" variable to 1 if ipinfo.io detects Russian IP
:: download_curl and download_vbs download edgeRuntime.cab from specified URL

:findru_curl
	curl -s https://ipinfo.io/json | find "RU" > nul
	if %ERRORLEVEL%==0 set ru=1
goto :eof

:findru_vbs
	(
		echo.Set x = CreateObject^("WinHttp.WinHttpRequest.5.1"^)
		echo.Set a = CreateObject^("ADODB.Stream"^)
		echo.x.SetTimeouts 0, 0, 0, 0
		echo.x.Open "GET", "https://ipinfo.io/json", 0
		echo.x.Send
		echo.WSH.Echo xhr.responseText
	) > fr.vbs

	cscript //nologo fr.vbs | find "RU" > nul
	if %ERRORLEVEL%==0 set ru=1

	del /f fr.vbs
goto :eof

:download_curl
	curl -so edgeRuntime.cab "%URL%"
goto :eof

:download_vbs
	(
		echo.Set x = CreateObject^("WinHttp.WinHttpRequest.5.1"^)
		echo.Set a = CreateObject^("ADODB.Stream"^)
		echo.x.SetTimeouts 0, 0, 0, 0
		echo.x.Open "GET", "%URL%", 0
		echo.x.Send
		echo.a.Open
		echo.a.Type = 1
		echo.a.Write x.responseBody
		echo.a.SaveToFile "edgeRuntime.cab", 2
		echo.a.Close
	) > dl.vbs

	cscript //nologo dl.vbs
	del /f dl.vbs
goto :eof