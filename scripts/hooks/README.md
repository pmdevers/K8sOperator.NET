# Git Hooks

This directory contains git hooks that can be installed to improve the development workflow.

## Available Hooks

### pre-commit
Runs all unit tests before allowing a commit. If any tests fail, the commit will be aborted.

## Installation

### Windows (PowerShell)
```powershell
.\scripts\hooks\install-hooks.ps1
```

### Linux/Mac (Bash)
```bash
chmod +x scripts/hooks/install-hooks.sh
./scripts/hooks/install-hooks.sh
```

### Manual Installation
Copy the hooks to your `.git/hooks` directory:
```bash
cp scripts/hooks/pre-commit .git/hooks/pre-commit
chmod +x .git/hooks/pre-commit
```

## Bypassing Hooks

If you need to bypass the hooks for a specific commit (not recommended), use:
```bash
git commit --no-verify
```
