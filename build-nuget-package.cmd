@ECHO off

CALL .\build.cmd Release
IF %ERRORLEVEL% GTR 0 EXIT /b

IF NOT EXIST ".\dist" MKDIR ".\dist"
.\.nuget\NuGet.exe pack ".\NDbfReader\NDbfReader.csproj" -OutputDirectory ".\dist" -Properties Configuration=Release