# RC Known Issues (2026-04-03)

Project: `C:\test\Steal`

## KI-RC-2026-04-03-01

1. Title: Manual 5-level smoke evidence not re-run in this closure batch.
2. Severity: P2 (process risk).
3. Status: Accepted/Waived by owner decision.
4. Impact:
   1. Automation confidence is high.
   2. Final experiential regressions that require human playthrough may remain undetected.
5. Mitigation:
   1. Manual gate can be restored as blocker before external launch if policy changes.

## KI-RC-2026-04-03-02

1. Title: First I19 gate attempt failed due to Unity project lock (concurrent gate execution).
2. Severity: P3 (execution tooling).
3. Status: Resolved.
4. Impact: None on product binaries or runtime logic.
5. Mitigation:
   1. Execute Unity gate scripts serially for same project path.
   2. Use rerun evidence set:
      1. `codex-rc-gate-i19-2026-04-03-rerun-*`
