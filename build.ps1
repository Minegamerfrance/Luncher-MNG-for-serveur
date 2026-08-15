$ErrorActionPreference = 'Stop'
dotnet restore
if ($LASTEXITCODE -ne 0) { throw 'Échec de dotnet restore' }
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
if ($LASTEXITCODE -ne 0) { throw 'Échec de dotnet publish' }
Write-Host "MNG Launcher publié dans bin\Release\net8.0-windows\win-x64\publish" -ForegroundColor Green
