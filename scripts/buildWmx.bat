@echo off
taskkill /f /im wmx64.exe > nul 2> nul
set "_CONF=%~1"
pushd wmx
make
popd
