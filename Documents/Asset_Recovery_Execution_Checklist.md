# Asset Recovery Execution Checklist (Path + Import Order)

Updated: 2026-03-31
Project root: `C:\test\Steal`
Unity version: `6000.2.14f1`

## 0. Baseline Before Restore
1. Close Unity Editor.
2. Confirm backup source includes `.meta` files and always copy them with assets.
3. Run pre/post restore missing check:

```powershell
$required = @(
  'Assets/Scenes/Terminal/TerminalScene.unity',
  'Assets/Scenes/Oasis/OasisScene.unity',
  'Assets/Scenes/Garden/GardenScene.unity',
  'Assets/Scenes/Cockpit/CockpitScene.unity',
  'Assets/Scenes/Level01_Qhapaq_Passage.unity',
  'Assets/Scenes/Level02_Temple_Complex.unity',
  'Assets/Scenes/Level03_Underground_Labs.unity',
  'Assets/Scenes/Level04_Qhipu_Core.unity',
  'Assets/Scenes/Level05_General_Taki_Villa.unity'
)
$required | ForEach-Object {
  if (-not (Test-Path -LiteralPath $_)) { "[MISSING] $_" }
}
```

## 1. Batch A Import (Foundation Dependencies)
Import folders in this exact order:
1. `Assets/SharedAssets/**`
2. `Assets/Settings/**`
3. `Assets/Charactor/**`
4. `Assets/GameDesign/**`

Purpose: satisfy mesh/material/animation/render dependencies before prefab import.

## 2. Batch B Import (Runtime Prefabs)
Import in this order:
1. `Assets/INTIFALL/Prefabs/Player/**`
2. `Assets/INTIFALL/Prefabs/Enemies/**`
3. `Assets/INTIFALL/Prefabs/Environment/**`
4. `Assets/INTIFALL/Prefabs/UI/**`
5. `Assets/INTIFALL/Prefabs/Tools/**`

If fallback loading via `Resources.Load("Prefabs/Tools/{toolName}")` is used, ensure these paths exist:
- `Assets/Resources/Prefabs/Tools/SmokeBomb.prefab`
- `Assets/Resources/Prefabs/Tools/FlashBang.prefab`
- `Assets/Resources/Prefabs/Tools/SleepDart.prefab`
- `Assets/Resources/Prefabs/Tools/EMP.prefab`
- `Assets/Resources/Prefabs/Tools/TimedNoise.prefab`
- `Assets/Resources/Prefabs/Tools/SoundBait.prefab`
- `Assets/Resources/Prefabs/Tools/DroneInterference.prefab`
- `Assets/Resources/Prefabs/Tools/WallBreaker.prefab`

## 3. Batch C Import (ScriptableObject Data Assets)
Create/import in this order:
1. Global config:
   - `Assets/INTIFALL/ScriptableObjects/GameConfig.asset`
2. Enemy type assets (`EEnemyType` mapping):
   - `Assets/INTIFALL/ScriptableObjects/EnemyTypes/EnemyType_Normal.asset`
   - `Assets/INTIFALL/ScriptableObjects/EnemyTypes/EnemyType_Reinforced.asset`
   - `Assets/INTIFALL/ScriptableObjects/EnemyTypes/EnemyType_Heavy.asset`
   - `Assets/INTIFALL/ScriptableObjects/EnemyTypes/EnemyType_Quipucamayoc.asset`
   - `Assets/INTIFALL/ScriptableObjects/EnemyTypes/EnemyType_Saqueos.asset`
3. Tool data assets (`ToolData`) and set `runtimePrefab`:
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SmokeBomb.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_FlashBang.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SleepDart.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_EMP.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_TimedNoise.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_SoundBait.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_DroneInterference.asset`
   - `Assets/INTIFALL/ScriptableObjects/Tools/ToolData_WallBreaker.asset`
4. Level data assets:
   - `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_Level01_Qhapaq_Passage.asset`
   - `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_Level02_Temple_Complex.asset`
   - `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_Level03_Underground_Labs.asset`
   - `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_Level04_Qhipu_Core.asset`
   - `Assets/INTIFALL/ScriptableObjects/Levels/LevelData_Level05_General_Taki_Villa.asset`
5. Enemy spawn + intel spawn assets:
   - `Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_Level01_Qhapaq_Passage.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_Level02_Temple_Complex.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_Level03_Underground_Labs.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_Level04_Qhipu_Core.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/EnemySpawn_Level05_General_Taki_Villa.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_Level01_Qhapaq_Passage.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_Level02_Temple_Complex.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_Level03_Underground_Labs.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_Level04_Qhipu_Core.asset`
   - `Assets/INTIFALL/ScriptableObjects/Spawns/IntelSpawn_Level05_General_Taki_Villa.asset`

## 4. Batch D Import (Scenes Last)
Restore scene files (with `.meta`) in BuildSettings order:
1. `Assets/SharedAssets/Benchmark/BenchmarkScene.unity` (optional, disabled in build)
2. `Assets/Scenes/Terminal/TerminalScene.unity`
3. `Assets/Scenes/Oasis/OasisScene.unity`
4. `Assets/Scenes/Garden/GardenScene.unity`
5. `Assets/Scenes/Cockpit/CockpitScene.unity`
6. `Assets/Scenes/Level01_Qhapaq_Passage.unity`
7. `Assets/Scenes/Level02_Temple_Complex.unity`
8. `Assets/Scenes/Level03_Underground_Labs.unity`
9. `Assets/Scenes/Level04_Qhipu_Core.unity`
10. `Assets/Scenes/Level05_General_Taki_Villa.unity`

## 5. Validation Order After Restore
1. Open Unity and wait for script compilation to finish.
2. Verify `ProjectSettings/EditorBuildSettings.asset` scene order matches Section 4.
3. Compile check:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\test\Steal" -logFile "C:\test\Steal\codex-compile-after-restore.log"
```

4. EditMode regression:

```powershell
.\run-regression-tests.ps1 -ProjectPath "C:\test\Steal" -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -TestPlatform EditMode
```

5. PlayMode regression (optional second pass):

```powershell
.\run-regression-tests.ps1 -ProjectPath "C:\test\Steal" -UnityExe "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe" -TestPlatform PlayMode
```

## 6. Rollback Points
1. If Batch B causes broad prefab missing refs, rollback to snapshot after Batch A.
2. If Batch D scenes show Missing Script, verify Batch C ScriptableObject/prefab GUIDs came from the same backup.
3. If only tools fail to spawn, verify `ToolData.runtimePrefab` mapping and `Assets/Resources/Prefabs/Tools/*.prefab` names match `toolName`.
