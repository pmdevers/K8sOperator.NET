# PowerShell script to install git hooks
Write-Host "Installing git hooks..." -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$gitHooksDir = Join-Path $repoRoot ".git\hooks"

# Check if .git directory exists
if (-not (Test-Path (Join-Path $repoRoot ".git"))) {
    Write-Error "Not in a git repository. Please run this script from the repository root."
    exit 1
}

# Copy pre-commit hook
$sourceHook = Join-Path $scriptDir "pre-commit"
$targetHook = Join-Path $gitHooksDir "pre-commit"

Copy-Item -Path $sourceHook -Destination $targetHook -Force
Write-Host "✓ Installed pre-commit hook" -ForegroundColor Green

# Make executable (for Git Bash/WSL)
git update-index --chmod=+x "$targetHook"

Write-Host "`nGit hooks installed successfully!" -ForegroundColor Green
Write-Host "The pre-commit hook will now run tests before each commit." -ForegroundColor Gray
