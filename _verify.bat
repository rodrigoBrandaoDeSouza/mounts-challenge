@echo off
cd /d "%~dp0template\backend"
echo Running build and tests... (log: _verify.log)
(
  dotnet --list-sdks
  dotnet build Ambev.DeveloperEvaluation.sln -nologo -clp:ErrorsOnly
  dotnet test Ambev.DeveloperEvaluation.sln --no-build -nologo --logger "console;verbosity=normal"
) > "%~dp0_verify.log" 2>&1
echo Done. Exit code %ERRORLEVEL%>> "%~dp0_verify.log"
echo Finished. You can close this window.
pause
