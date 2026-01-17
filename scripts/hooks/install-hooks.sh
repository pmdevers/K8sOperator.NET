#!/bin/sh
# Shell script to install git hooks

echo "Installing git hooks..."

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GIT_HOOKS_DIR="$REPO_ROOT/.git/hooks"

# Check if .git directory exists
if [ ! -d "$REPO_ROOT/.git" ]; then
    echo "Error: Not in a git repository."
    exit 1
fi

# Copy pre-commit hook
cp "$SCRIPT_DIR/pre-commit" "$GIT_HOOKS_DIR/pre-commit"
chmod +x "$GIT_HOOKS_DIR/pre-commit"

echo "✓ Installed pre-commit hook"
echo ""
echo "Git hooks installed successfully!"
echo "The pre-commit hook will now run tests before each commit."
