#Requires -Version 5.1
<#
.SYNOPSIS
  Publishes Translators.Host to the Linux production server.

.DESCRIPTION
  Reads SSH and production config from gitignored files under ./secrets
  (never hardcode credentials in this script). Builds a linux-x64 self-contained
  publish output, uploads it, writes production settings on the server, and
  restarts the systemd service.
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$secretsDir = Join-Path $repoRoot "secrets"
$projectPath = Join-Path $repoRoot "Translators.Host\Translators.Host.csproj"
$publishDir = Join-Path $repoRoot "artifacts\linux-x64"

function Write-Step {
    param([string]$Message)
    $stamp = Get-Date -Format "HH:mm:ss"
    Write-Host "[$stamp] $Message"
}

function Normalize-BashScript {
    param([string]$Script)
    return ($Script -replace "`r`n", "`n" -replace "`r", "`n").Trim()
}

function Invoke-RemoteBash {
    param(
        [Parameter(Mandatory = $true)]
        $SshSession,
        [Parameter(Mandatory = $true)]
        [string]$Script,
        [int]$TimeOut = 120
    )

    $normalized = Normalize-BashScript $Script
    $encoded = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($normalized))
    $command = "echo $encoded | base64 -d | bash"
    return Invoke-SSHCommand -SSHSession $SshSession -Command $command -TimeOut $TimeOut
}

function Send-RemoteFile {
    param(
        [Parameter(Mandatory = $true)]
        $SftpSession,
        [Parameter(Mandatory = $true)]
        [string]$LocalPath,
        [Parameter(Mandatory = $true)]
        [string]$RemoteDirectory
    )

    Write-Step "Uploading $(Split-Path -Leaf $LocalPath) to $RemoteDirectory/"
    Set-SFTPItem -SFTPSession $SftpSession -Path $LocalPath -Destination $RemoteDirectory -Force
}

function Read-SecretFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $path = Join-Path $secretsDir $Name
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing secret file: $path"
    }

    return (Get-Content -LiteralPath $path -Raw).Trim()
}

function Ensure-PoshSsh {
    if (-not (Get-Module -ListAvailable -Name Posh-SSH)) {
        Write-Host "Installing Posh-SSH for the current user..."
        Install-Module -Name Posh-SSH -Scope CurrentUser -Force -SkipPublisherCheck
    }

    Import-Module Posh-SSH -Force -ErrorAction Stop

    if (-not (Get-Command Set-SFTPItem -ErrorAction SilentlyContinue)) {
        throw "Posh-SSH is loaded but Set-SFTPItem is missing. Run: Install-Module Posh-SSH -Scope CurrentUser -Force"
    }
}

if (-not (Test-Path -LiteralPath $secretsDir)) {
    throw "The secrets folder was not found at $secretsDir"
}

$sshHost = Read-SecretFile "ssh.host"
$sshUser = Read-SecretFile "ssh.user"
$sshPassword = Read-SecretFile "ssh.password"
$remotePath = Read-SecretFile "remote.path"
$serviceName = Read-SecretFile "service.name"
$connectionString = Read-SecretFile "db.connection"
$jwtIssuer = Read-SecretFile "jwt.issuer"
$jwtAudience = Read-SecretFile "jwt.audience"
$jwtSigningKey = Read-SecretFile "jwt.signing-key"
$jwtExpirationMinutes = Read-SecretFile "jwt.expiration-minutes"

Write-Step "Publishing Translators.Host (linux-x64, self-contained)..."
Write-Step "NuGet uses repo nuget.config (nuget.org only) so restore does not scan slow machine sources like I:\nuget\packages."
if (Test-Path -LiteralPath $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

Write-Step "Restoring packages for linux-x64 (first run may download the .NET runtime; usually under a minute)..."
dotnet restore $projectPath `
    -r linux-x64 `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed with exit code $LASTEXITCODE"
}

Write-Step "Building and publishing..."
dotnet publish $projectPath `
    -c Release `
    -r linux-x64 `
    --self-contained true `
    --no-restore `
    -o $publishDir `
    --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Step "Publish output ready at $publishDir"

$productionSettings = @{
    ConnectionStrings = @{
        DefaultConnection = $connectionString
    }
    Jwt = @{
        Issuer = $jwtIssuer
        Audience = $jwtAudience
        SigningKey = $jwtSigningKey
        ExpirationMinutes = [int]$jwtExpirationMinutes
    }
} | ConvertTo-Json -Depth 5

Set-Content -LiteralPath (Join-Path $publishDir "appsettings.Production.json") -Value $productionSettings -Encoding UTF8

Write-Step "Loading Posh-SSH module..."
Ensure-PoshSsh

$securePassword = ConvertTo-SecureString $sshPassword -AsPlainText -Force
$credential = New-Object System.Management.Automation.PSCredential($sshUser, $securePassword)

Write-Step "Connecting to $sshUser@$sshHost ..."
$session = $null
$sftpSession = $null
$session = New-SSHSession -ComputerName $sshHost -Credential $credential -AcceptKey
if (-not $session) {
    throw "Could not open an SSH session to $sshHost"
}

$sftpSession = New-SFTPSession -ComputerName $sshHost -Credential $credential -AcceptKey
if (-not $sftpSession) {
    throw "Could not open an SFTP session to $sshHost"
}

try {
    $artifactsDir = Join-Path $repoRoot "artifacts"
    if (-not (Test-Path -LiteralPath $artifactsDir)) {
        New-Item -ItemType Directory -Path $artifactsDir | Out-Null
    }

    $unitFileLocal = Join-Path $artifactsDir "$serviceName.service"
    $unitContent = Normalize-BashScript @"
[Unit]
Description=Translators API
After=network.target

[Service]
WorkingDirectory=$remotePath
ExecStart=$remotePath/Translators.Host
Restart=always
RestartSec=5
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
"@
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($unitFileLocal, $unitContent + "`n", $utf8NoBom)

    $remoteUnitPath = "/tmp/$serviceName.service"
    Write-Step "Preparing remote directory and systemd unit..."
    Send-RemoteFile -SftpSession $sftpSession -LocalPath $unitFileLocal -RemoteDirectory "/tmp"

    $setupScript = @"
set -euo pipefail
mkdir -p '$remotePath'
install -m 644 '$remoteUnitPath' '/etc/systemd/system/$serviceName.service'
systemctl daemon-reload
systemctl enable '$serviceName'
systemctl stop '$serviceName' || true
systemctl stop translators.service || true
systemctl disable translators.service || true
if command -v ss >/dev/null 2>&1; then
  old_pids=`$(ss -lptn 'sport = :5000' | sed -n 's/.*pid=\([0-9]*\).*/\1/p' | sort -u)
  for pid in `$old_pids; do
    kill -TERM `$pid || true
  done
  sleep 1
  old_pids=`$(ss -lptn 'sport = :5000' | sed -n 's/.*pid=\([0-9]*\).*/\1/p' | sort -u)
  for pid in `$old_pids; do
    kill -KILL `$pid || true
  done
fi
"@

    $setupResult = Invoke-RemoteBash -SshSession $session -Script $setupScript
    if ($setupResult.ExitStatus -ne 0) {
        throw "Remote setup failed: $($setupResult.Error)$($setupResult.Output)"
    }

    $archivePath = Join-Path $repoRoot "artifacts\translators-linux-x64.tar.gz"
    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }

    Write-Step "Packing publish output..."
    & tar -czf $archivePath -C $publishDir .
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create publish archive."
    }

    $remoteArchive = "/tmp/translators-linux-x64.tar.gz"
    Write-Step "Uploading archive to $sshHost ..."
    Send-RemoteFile -SftpSession $sftpSession -LocalPath $archivePath -RemoteDirectory "/tmp"

    $finishScript = @"
set -euo pipefail
tar -xzf '$remoteArchive' -C '$remotePath'
rm -f '$remoteArchive'
chmod +x '$remotePath/Translators.Host'
systemctl daemon-reload
systemctl reset-failed '$serviceName' || true
systemctl restart '$serviceName'
for i in 1 2 3 4 5 6 7 8 9 10; do
  if systemctl is-active --quiet '$serviceName'; then
    break
  fi
  sleep 1
done
systemctl --no-pager --full status '$serviceName'
if ! systemctl is-active --quiet '$serviceName'; then
  journalctl -u '$serviceName' -n 40 --no-pager
  exit 1
fi
"@

    Write-Step "Restarting $serviceName ..."
    $finishResult = Invoke-RemoteBash -SshSession $session -Script $finishScript
    if ($finishResult.ExitStatus -ne 0) {
        throw "Remote restart failed: $($finishResult.Error)$($finishResult.Output)"
    }

    Write-Host ($finishResult.Output -join [Environment]::NewLine)
    Write-Step "Publish completed."
}
finally {
    if ($sftpSession) {
        Remove-SFTPSession -SFTPSession $sftpSession | Out-Null
    }
    if ($session) {
        Remove-SSHSession -SSHSession $session | Out-Null
    }
}
