#Requires -RunAsAdministrator
# MyTalli IIS Setup Script
# Run this in an elevated PowerShell prompt

$siteName = "MyTalli"
$port = 3000
$publishPath = "C:\Users\Robert\source\repos\MyTalli\publish"
$appPoolName = "MyTalliAppPool"

Import-Module WebAdministration

# Remove existing site/pool if they exist
if (Get-Website -Name $siteName -ErrorAction SilentlyContinue) {
    Write-Host "Removing existing IIS site '$siteName'..." -ForegroundColor Yellow
    Remove-Website -Name $siteName
}

if (Test-Path "IIS:\AppPools\$appPoolName") {
    Write-Host "Removing existing app pool '$appPoolName'..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $appPoolName
}

# Create application pool (No Managed Code for ASP.NET Core)
Write-Host "Creating application pool '$appPoolName'..." -ForegroundColor Cyan
New-WebAppPool -Name $appPoolName
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "startMode" -Value "AlwaysRunning"

# Create the IIS site
Write-Host "Creating IIS site '$siteName' on port $port..." -ForegroundColor Cyan
New-Website -Name $siteName `
    -PhysicalPath $publishPath `
    -Port $port `
    -ApplicationPool $appPoolName

# Set environment to Development
# This adds an env var so ASP.NET Core runs in Development mode under IIS
$configPath = "system.webServer/aspNetCore"
$envCollection = Get-WebConfigurationProperty -PSPath "IIS:\Sites\$siteName" -Filter "$configPath/environmentVariables" -Name "." -ErrorAction SilentlyContinue

# Add environment variable via appcmd for reliability
& "$env:SystemRoot\System32\inetsrv\appcmd.exe" set config "$siteName" `
    -section:system.webServer/aspNetCore `
    /+"environmentVariables.[name='ASPNETCORE_ENVIRONMENT',value='Development']" `
    /commit:site 2>$null

# Grant IIS_IUSRS read access to the publish folder
Write-Host "Granting IIS_IUSRS read access to publish folder..." -ForegroundColor Cyan
$acl = Get-Acl $publishPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $publishPath $acl

# Also grant the app pool identity access
$poolIdentity = "IIS AppPool\$appPoolName"
$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule($poolIdentity, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule2)
Set-Acl $publishPath $acl

# Grant the app pool identity read access to the repo root
# IIS monitors web.config changes in parent directories, so it needs access up the tree
$repoRoot = "C:\Users\Robert\source\repos\MyTalli"
Write-Host "Granting app pool identity read access to repo root (for config monitoring)..." -ForegroundColor Cyan
$repoAcl = Get-Acl $repoRoot
$repoRule = New-Object System.Security.AccessControl.FileSystemAccessRule($poolIdentity, "ReadAndExecute", "None", "None", "Allow")
$repoAcl.SetAccessRule($repoRule)
$iusrsRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "ReadAndExecute", "None", "None", "Allow")
$repoAcl.SetAccessRule($iusrsRule)
Set-Acl $repoRoot $repoAcl

# Start the site
Start-Website -Name $siteName

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " MyTalli is running on IIS!" -ForegroundColor Green
Write-Host " URL: http://localhost:$port" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Publish path: $publishPath" -ForegroundColor Gray
Write-Host "App pool:     $appPoolName (No Managed Code, InProcess)" -ForegroundColor Gray
Write-Host ""
Write-Host "To republish after code changes:" -ForegroundColor Yellow
Write-Host "  dotnet publish Source\My.Talli.Web\My.Talli.Web.csproj -c Release -o publish" -ForegroundColor White
Write-Host "  iisreset /restart" -ForegroundColor White
