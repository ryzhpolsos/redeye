@echo off
set _CONF=Debug

taskkill /f /im wmx64.exe
make

pushd ..\bin\Debug\netframework4.8\wmx
start "" wmx64.exe 0 0 100 100 0
popd