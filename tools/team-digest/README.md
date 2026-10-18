# Team Digest

Scheduled GitHub Action that reads recent GitHub activity (commits on every branch, PRs, issues)
and Discord channel history, asks Claude to summarize who is working on what per team, and posts
the digest to `#team-digest`.

- Daily: Mon/Tue/Wed/Fri 08:00 Pacific (Monday covers the weekend)
- Weekly: Thursday 08:30 Pacific, before the exec meeting
- Manual: Actions → Team Digest → Run workflow (optional dry run / custom window)

Configuration lives in `team.yml` (roster with GitHub/Discord handles, channels to read or skip,
channel/branch → team hints). Required repo settings:

| Kind | Name |
|---|---|
| Secret | `ANTHROPIC_API_KEY`, `DISCORD_BOT_TOKEN` |
| Variable | `DISCORD_GUILD_ID`, `DISCORD_DIGEST_CHANNEL_ID`, optional `DIGEST_MODEL` |

Local run: copy `.env.example` to `.env`, then `python digest.py --context-only` (no Claude call),
`--dry-run` (print only), or no flag (post). Only channels listed/allowed in `team.yml` are read,
bot messages are ignored, and the digest never @mentions anyone.
