const fs = require("fs");

function safeParse(raw) {
  try {
    return JSON.parse(raw);
  } catch {
    return {
      summary: "AI returned invalid JSON",
      comments: []
    };
  }
}

module.exports = async ({ github, context }) => {
  const raw = fs.readFileSync("ai_result.json", "utf8");
  const data = safeParse(raw);

  const summary = data.summary ?? "No summary";
  const comments = Array.isArray(data.comments) ? data.comments : [];

  // 1. PR summary comment
  await github.rest.issues.createComment({
    issue_number: context.issue.number,
    owner: context.repo.owner,
    repo: context.repo.repo,
    body: "🤖 AI Review:\n\n" + summary
  });

  // 2. Get PR commit
  const pr = await github.rest.pulls.get({
    owner: context.repo.owner,
    repo: context.repo.repo,
    pull_number: context.issue.number
  });

  const commitId = pr.data.head.sha;

  // 3. Inline comments
  for (const c of comments) {
    if (!c?.file || !c?.line || !c?.comment) continue;

    try {
      await github.rest.pulls.createReviewComment({
        owner: context.repo.owner,
        repo: context.repo.repo,
        pull_number: context.issue.number,
        body: c.comment,
        commit_id: commitId,
        path: c.file,
        line: c.line,
        side: "RIGHT"
      });
    } catch (e) {
      console.log("Failed to post comment:", c, e.message);
    }
  }
};