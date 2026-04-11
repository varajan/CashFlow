#!/usr/bin/env bash
set -euo pipefail

PROMPT_FILE=".github/prompts/review_prompt.txt"

PROMPT=$(cat "$PROMPT_FILE")
DIFF=$(cat diff_small.txt)

jq -n \
  --arg prompt "$PROMPT" \
  --arg diff "$DIFF" \
  '{
    contents: [
      {
        parts: [
          { text: ($prompt + "\n\nDIFF:\n" + $diff) }
        ]
      }
    ]
  }' > request.json

URL="$GEMINI_URL/$GEMINI_MODEL:generateContent?key=$GEMINI_API_KEY"

attempt=0

until [ $attempt -ge $MAX_RETRIES ]; do
  attempt=$((attempt+1))
  echo "Attempt $attempt"

  HTTP_CODE=$(curl -s -o response.json -w "%{http_code}" \
    -H "Content-Type: application/json" \
    -X POST \
    --data @request.json \
    "$URL")

  if [ "$HTTP_CODE" -ge 200 ] && [ "$HTTP_CODE" -lt 300 ]; then
    exit 0
  fi

  sleep $((BACKOFF_BASE_SECONDS ** attempt))
done

echo "Failed after retries"
exit 1
