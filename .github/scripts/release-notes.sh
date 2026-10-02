#!/usr/bin/env bash
# Prints the section of RELEASE_NOTES.md for a version ("## 1.0.0 ..." up to the next "## " heading).
#
#   release-notes.sh <version> [--allow-unreleased]
#
# Exit codes: 1 = no section for the version, 2 = the section is still marked "(unreleased)".
# A release must not be cut from notes that say "unreleased": edit the heading (for example to "## 1.0.0 - 2026-10-15") first.
set -euo pipefail

version="${1:?usage: release-notes.sh <version> [--allow-unreleased]}"
allow="${2:-}"
file="$(dirname "$0")/../../RELEASE_NOTES.md"

heading="$(tr -d '\r' < "$file" | grep -m1 -E "^## ${version//./\\.}( |$)" || true)"
if [[ -z "$heading" ]]; then
  echo "RELEASE_NOTES.md has no section '## $version'" >&2
  exit 1
fi

if [[ "$heading" == *unreleased* && "$allow" != "--allow-unreleased" ]]; then
  echo "RELEASE_NOTES.md section '$heading' is still marked unreleased" >&2
  exit 2
fi

tr -d '\r' < "$file" | awk -v heading="$heading" '$0 == heading { on = 1; next } on && /^## / { exit } on'
