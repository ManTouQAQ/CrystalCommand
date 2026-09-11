$ErrorActionPreference = "Stop"

$source = "https://api.nuget.org/v3/index.json"
$configuration = "Release"
$packages = @(
    "CrystalCommand.Core",
    "CrystalCommand.Attr"
)

dotnet pack -c $configuration

foreach ($package in $packages) {
    Get-ChildItem "$package/bin/Release/*.nupkg" | ForEach-Object {
        Write-Host "Publishing $($_.Name)..."
        dotnet nuget push $_.FullName --source $source --skip-duplicate
    }
}

Write-Host "Done!"