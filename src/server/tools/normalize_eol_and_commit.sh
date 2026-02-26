#!/usr/bin/env bash
# One-shot: write .gitattributes, renormalize tracked files to LF, and create commits.
# Usage: ./tools/normalize_eol_and_commit.sh [--push]
set -euo pipefail

# Ensure running from repo root
if [ ! -d .git ]; then
    echo "Error: must be run from repository root (contains .git)"
    exit 1
fi

GIT_ADD_PUSH=${1:-""}

# Write .gitattributes (overwrite)
cat > .gitattributes <<'EOF'
# Enforce LF for repository text files
* text=auto

# Project source
*.cs     text eol=lf
*.sln    text eol=lf
*.csproj text eol=lf
*.props  text eol=lf

# Mark common text formats as LF
*.xml    text eol=lf
*.xaml   text eol=lf
*.config text eol=lf
*.md     text eol=lf
*.sh     text eol=lf

# Keep CRLF for PowerShell scripts on Windows (optional)
*.ps1    text eol=crlf

# Binary files (do not normalize)
*.png    binary
*.jpg    binary
*.jpeg   binary
*.gif    binary
*.zip    binary
*.dll    binary
*.exe    binary
EOF

echo "Wrote .gitattributes"

# Stage .gitattributes and commit if changed
git add .gitattributes
if ! git diff --staged --quiet -- .gitattributes; then
    git commit -m "Add/Update .gitattributes to enforce LF for text files"
    echo "Committed .gitattributes"
else
    echo ".gitattributes unchanged"
    # Unstage .gitattributes to avoid accidental inclusion in next step
    git reset -- .gitattributes >/dev/null 2>&1 || true
fi

# List tracked files containing CR or CRLF for review
echo "Listing tracked files containing CR (CRLF):"
git ls-files -z | xargs -0 -n1 bash -c 'if LC_ALL=C grep -Iq . "$0"; then :; fi; if grep -q $'\''\r'\'' "$0"; then printf "%s\n" "$0"; fi' || true

# Renormalize tracked files according to .gitattributes
git add --renormalize .

# Commit renormalized files if any changes
if [ -n "$(git status --porcelain)" ]; then
    git commit -m "Normalize line endings to LF"
    echo "Committed renormalized files (line endings normalized to LF)"
else
    echo "No renormalization changes required"
fi

# Optional: push changes if requested
if [ "$GIT_ADD_PUSH" = "--push" ]; then
    echo "Pushing commits to origin"
    git push
else
    echo "Done. To push the commits run: git push"
fi
