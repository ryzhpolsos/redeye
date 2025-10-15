@echo off
setlocal

if not defined CSC ( set "CSC=csc.exe" )
if not defined GCC64 ( set "GCC64=gcc.exe" )
if not defined GCC32 ( set "GCC32=C:\MinGW\bin\gcc.exe" )
if not defined WINDRES ( set "WINDRES=windres.exe" )

set noedge=0
set nowmx=0

:argloop
if "%~1"=="/noedge" set noedge=1
if "%~1"=="/nowmx" set nowmx=1
shift /1
if not "%~1"=="" goto argloop

if exist bin\redeye.exe bin\redeye.exe --killwmx

:: Compile main exe
if "%noedge%"=="1" (
    if exist src\webwrapper.cs.noedge (
        move /y src\webwrapper.cs src\webwrapper.cs.edge > nul
        move /y src\webwrapper.cs.noedge src\webwrapper.cs > nul
    )
    "%CSC%" /nologo /platform:x64 /target:winexe /optimize+ /debug- /win32manifest:src\app.manifest /out:bin\redeye.exe src\*.cs
) else (
    if exist src\webwrapper.cs.edge (
        move /y src\webwrapper.cs src\webwrapper.cs.noedge > nul
        move /y src\webwrapper.cs.edge src\webwrapper.cs > nul
    )
    "%CSC%" /nologo /platform:x64 /target:winexe /optimize+ /debug- /win32manifest:src\app.manifest /reference:bin\Microsoft.Web.WebView2.Core.dll,bin\Microsoft.Web.WebView2.WinForms.dll /out:bin\redeye.exe src\*.cs
)
:: /target:winexe /optimize+ /debug- 

:: Compile WMX (Window MaXimizer) if no "/nowmx" option specified
if "%nowmx%"=="1" exit /b

if not exist bin\wmx mkdir bin\wmx

"%WINDRES%" --input src\wmx\wmx.rc --output wmx.res --output-format=coff

"%GCC64%" -m64 -mwindows -DX64 -o bin\wmx\wmx64.exe src\wmx\wmx.c wmx.res
::"%GCC64%" -m64 -mwindows -DCL_NAME=%DLL_NAME% -o bin\wmx\wmx64.exe src\wmx\wmx.c
::"%GCC32%" -m32 -mwindows -DX32 -o bin\wmx\wmx32.exe src\wmx\wmx.c

::"%GCC64%" -m64 -DCL_NAME=%DLL_NAME% -c src\wmx\wmxdll.c
"%GCC64%" -m64 -DX64 -c src\wmx\wmxdll.c
"%GCC64%" -shared -o bin\wmx\wmx64.dll wmxdll.o
::"%GCC32%" -m32 -DX32 -c src\wmx\wmxdll.c
::"%GCC32%" -shared -o bin\wmx\wmx32.dll wmxdll.o
del wmxdll.o
del wmx.res

endlocal