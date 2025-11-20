Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -UseBasicParsing -OutFile "$env:temp\dotnet-install.ps1"
& $env:temp\dotnet-install.ps1 -Architecture x64 -Version '3.1.426' -InstallDir "$env:ProgramFiles\dotnet"
