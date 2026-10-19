---
description: Always follow SYSTEM_STANDARD.md and persist architecture changes there
alwaysApply: true
---

# Translators agent rules

- Read and follow `SYSTEM_STANDARD.md` before changing this repo.
- The user expects implementation work from you, not only advice.
- After any architecture or operational change, update `SYSTEM_STANDARD.md` in the same turn so the next session still has the current design.
- Production SSH/DB/JWT secrets live in the gitignored `secrets/` folder. Read them when deploying or connecting to production. Never copy secret values into `.ps1`, source, or committed files.
