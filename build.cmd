@echo off
if not exist build (
  md build
)
dotnet build IdentityServer4.Library.sln /p:Configuration=Release /p:Version=%1
copy .\src\Storage\src\bin\Release\*.nupkg .\build\
copy .\src\IdentityServer4\src\bin\Release\*.nupkg .\build\
copy .\src\EntityFramework.Storage\src\bin\Release\*.nupkg .\build\
copy .\src\EntityFramework\src\bin\Release\*.nupkg .\build\