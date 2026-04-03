# P3 Performance & Stability Gate Report

Date: 2026-04-02  
Unity: 6000.2.14f1  
Project: `C:\test\Steal`

## 1. Scope (`P3-T1`)
Added automated gate coverage for:
1. Frame-time budget (average + P95).
2. GC pressure (Gen0 / Gen1 collection deltas).
3. Scene-switch latency budget (average + P95).
4. Scene-loop safety checks:
   - runtime manager accumulation
   - event subscriber leak drift
   - enemy squad registry growth
   - unexpected error/exception logs

Primary gate file:
1. `Assets/INTIFALL/Tests/PlayMode/Iteration13P3PerformanceStabilityGatePlayModeTests.cs`

## 2. Gate Thresholds
1. Frame average `<= 90ms`, frame P95 `<= 200ms`.
2. Scene switch average `<= 1200ms`, scene switch P95 `<= 2200ms`.
3. Gen0 delta `<= 32`, Gen1 delta `<= 40`.
4. Managed memory drift `<= 64 MB`.
5. No unexpected `Error/Exception/Assert` logs.
6. Subscriber counts and runtime manager counts remain bounded.

## 3. Regression Evidence
1. `codex-p3-editmode-results.xml`: `327 / 327 PASS`
2. `codex-p3-playmode-results.xml`: `21 / 21 PASS`

## 4. Verdict
`P3-T1` status: `DONE` and `PASS`.
