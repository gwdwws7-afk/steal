# Iteration 5 Known Issues

Date: 2026-04-01
Scope: RC0 gate (`I5-T5`)

## Active Issues

| ID | Priority | Area | Description | Impact | Owner | Target |
|---|---|---|---|---|---|---|
| KI-001 | P2 | Localization/Text | Several `toolNameCN` strings contain mojibake/garbled characters in runtime scripts (legacy encoding artifact). | Cosmetic UI text quality issue only; no gameplay impact. | UI/Localization | I6 text cleanup |
| KI-002 | P2 | Tool Config Pipeline | Tool ScriptableObject assets (`Assets/INTIFALL/ScriptableObjects/Tools/*.asset`) still have mostly zeroed combat stat fields; runtime behavior currently comes from tool scripts. | Balancing cannot yet be fully data-driven; requires script edits for many tuning changes. | Systems/Tools | I6 config normalization |

## Closed / Not Reproduced
1. No P0 compile blockers in RC0 regression.
2. No P1 mission-loop blockers in 5-scene smoke coverage.

## Gate Status
RC0 gate remains `PASS` with current known issues profile.
