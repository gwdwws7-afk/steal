# RC Gate Closure Report (2026-04-03)

Project: `C:\test\Steal`  
Unity: `6000.2.14f1`  
Executor: Codex

## 1. Scope

This closure run executed release gates for current RC code state after I31-I35 integration.

## 2. Gate Execution

### 2.1 I19 Stability Gate (rerun)

Command:

```powershell
.\run-i19-stability-gate.ps1 -ProjectPath C:\test\Steal -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -OutputPrefix "codex-rc-gate-i19-2026-04-03-rerun"
```

Result:

1. EditMode: `384/384 PASS`, failed `0`, duration `0.6069s` (threshold `<=120s`)
2. PlayMode: `27/27 PASS`, failed `0`, duration `10.8598s` (threshold `<=180s`)
3. Critical markers in logs (`Aborting batchmode due to failure`, `Unhandled Exception`, `error CS`): `None`

Evidence:

1. `codex-rc-gate-i19-2026-04-03-rerun-editmode-results.xml`
2. `codex-rc-gate-i19-2026-04-03-rerun-playmode-results.xml`
3. `codex-rc-gate-i19-2026-04-03-rerun-editmode.log`
4. `codex-rc-gate-i19-2026-04-03-rerun-playmode.log`

### 2.2 RC Final Gate

Command:

```powershell
.\run-rc-final-gate.ps1 -ProjectPath C:\test\Steal -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -OutputPrefix "codex-rc-gate-final-2026-04-03"
```

Result:

1. EditMode: `384/384 PASS`, failed `0`, duration `0.6020s`
2. PlayMode: `27/27 PASS`, failed `0`, duration `10.1766s`

Evidence:

1. `codex-rc-gate-final-2026-04-03-editmode-results.xml`
2. `codex-rc-gate-final-2026-04-03-playmode-results.xml`
3. `codex-rc-gate-final-2026-04-03-editmode.log`
4. `codex-rc-gate-final-2026-04-03-playmode.log`

## 3. Run Note

One initial I19 run attempt failed because two gate commands were started concurrently and Unity project lock was hit.  
This was an execution-order issue, not a product issue, and was resolved by serialized rerun (PASS).

## 4. Manual Gate Status

Manual 5-level smoke gate is recorded as **waived for this closure** per owner instruction to skip manual steps.

## 5. Closure Verdict

1. Automated release gates: `PASS`
2. Stability thresholds: `PASS`
3. RC automation signoff: `PASS`
4. Manual smoke: `WAIVED (owner decision)`

Overall: **RC gate closure completed** for automation scope.
