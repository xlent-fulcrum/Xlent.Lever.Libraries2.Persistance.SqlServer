@echo off

set nuget_url=http://fulcrum-nuget.azurewebsites.net/api/v2/package
set api_key=7b519fe3-ad97-460c-881c-ece381f5ae69 
set nuspec=Xlent.Lever.Libraries2.SqlServer.nuspec

echo.
echo READ THIS
echo.
echo 1. Build project (dll files are automatically put in lib folder)
echo 2. Change version number in %nuspec%
echo.
pause
echo.

del /q *.nupkg

NuGet.exe pack %nuspec%

nuget.exe push *.nupkg %api_key% -Source %nuget_url%

del /q *.nupkg

echo.
pause
