#!/usr/bin/env bash
set -euo pipefail

jq -r '.candidates[0].content.parts[0].text // empty' response.json \
  | sed 's/```json//g' \
  | sed 's/```//g' \
  > ai_result.json

if [ ! -s ai_result.json ]; then
  echo '{"summary":"No valid response","comments":[]}' > ai_result.json
fi
