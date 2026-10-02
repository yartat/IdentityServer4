#!/usr/bin/env bash
# Prints the section of a release notes file for a version ("## 4.2.0 ..." up to the next "## " heading).
#
#   release-notes.sh <version> [--file <path>] [--allow-unreleased]
#
# The default file is RELEASE_NOTES.md in the repository root; every package has its own file (src/<area>/RELEASE_NOTES.md).
# Exit codes: 1 = no section for the version, 2 = the section is still marked "(unreleased)".
# A release must not be cut from notes that say "unreleased": edit the heading (for example to "## 4.2.0 - 2026-10-15") first.
set -euo pipefail

version=""
allow=""
file="$(dirname "$0")/../../RELEASE_NOTES.md"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --allow-unreleased) allow="1" ;;
    --file) file="${2:?--file needs a path}"; shift ;;
    -*) echo "unknown option '$1'" >&2; exit 64 ;;
    *) version="$1" ;;
  esac
  shift
done

if [[ -z "$version" ]]; then
  echo "usage: release-notes.sh <version> [--file <path>] [--allow-unreleased]" >&2
  exit 64
fi

heading="$(tr -d '\r' < "$file" | grep -m1 -E "^## ${version//./\\.}( |$)" || true)"
if [[ -z "$heading" ]]; then
  echo "$file has no section '## $version'" >&2
  exit 1
fi

if [[ "$heading" == *unreleased* && -z "$allow" ]]; then
  echo "$file section '$heading' is still marked unreleased" >&2
  exit 2
fi

tr -d '\r' < "$file" | awk -v heading="$heading" '$0 == heading { on = 1; next } on && /^## / { exit } on'
