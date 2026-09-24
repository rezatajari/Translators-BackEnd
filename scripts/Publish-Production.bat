@echo off
cd /d "%~dp0"
echo Running Publish-Production.ps1 ...
echo.
powershell.exe -NoExit -NoProfile -ExecutionPolicy Bypass -File "%~dp0Publish-Production.ps1"
if errorlevel 1 (
  echo.
  echo Publish failed with exit code %ERRORLEVEL%.
  pause
)
