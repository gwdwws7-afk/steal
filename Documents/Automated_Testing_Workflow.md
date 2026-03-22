# INTIFALL 自动化测试流程文档

---

## 1. 概述

本文档描述 INTIFALL 项目的自动化代码质量门禁和回归测试流程。

**目标：** 确保每次代码提交都经过自动化检查，防止有问题的代码进入主分支。

---

## 2. 测试套件

INTIFALL 使用 Unity Test Framework 进行自动化测试。

### 2.1 当前测试文件

| 文件 | 测试内容 | 数量 |
|---|---|---|
| `EventBusTests.cs` | 事件总线订阅/发布/退订 | 4 |
| `GameManagerTests.cs` | 游戏状态/金币/击杀/评价 | 15 |
| `PlayerStateMachineTests.cs` | 状态转换/计时器 | 12 |
| **总计** | | **31** |

### 2.2 如何编写新测试

```csharp
using NUnit.Framework;
using INTIFALL.System;

namespace INTIFALL.Tests
{
    public class MyNewTests
    {
        [Test]
        public void SomeBehavior_ShouldDoSomething()
        {
            // Arrange
            var expected = 5;

            // Act
            var actual = CalculateSomething();

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
```

测试文件放在：`Assets/INTIFALL/Tests/` 目录下。

---

## 3. 本地回归测试

### 3.1 运行方法

```powershell
.\run-regression-tests.ps1
```

### 3.2 脚本功能

1. 启动 Unity Editor 并运行所有 PlayMode 测试
2. 解析 `test-results-regression.xml`
3. 输出测试结果摘要
4. 如果有失败，返回退出码 1（CI 中会阻止合并）

### 3.3 输出示例

```
======================================
INTIFALL 回归测试
======================================

[1/2] 运行测试...
[2/2] 分析结果...

======================================
测试结果
======================================
  总数:   31
  通过:   31
  失败:   0
  用时:   0.04s

[SUCCESS] 所有测试通过，代码质量门禁已通过
```

---

## 4. GitHub Actions CI 流程

### 4.1 配置文件

`.github/workflows/INTIFALL.yml`

### 4.2 门禁列表

| 门禁 | 检查内容 | 失败时 |
|---|---|---|
| **Build** | 代码编译是否成功 | 阻止合并 |
| **Test** | 所有单元测试是否通过 | 阻止合并 |
| **Style** | C# 代码风格检查 | 阻止合并 |
| **Coverage** | 测试覆盖率是否 ≥ 50% | 阻止合并 |

### 4.3 流程图

```
代码提交/Push/PR
       ↓
┌──────────────────────────────────────┐
│  Gate 1: Build                       │
│  Unity 编译检查                       │
│  失败 → 阻止合并                      │
└──────────────────────────────────────┘
       ↓
┌──────────────────────────────────────┐
│  Gate 2: Test                        │
│  运行 31 个 PlayMode 测试              │
│  失败 → 阻止合并（REGRESSION）         │
└──────────────────────────────────────┘
       ↓
┌──────────────────────────────────────┐
│  Gate 3: Style                       │
│  命名/格式/空行检查                   │
│  失败 → 阻止合并                      │
└──────────────────────────────────────┘
       ↓
┌──────────────────────────────────────┐
│  Gate 4: Coverage                    │
│  测试覆盖率 ≥ 50%                      │
│  失败 → 阻止合并                      │
└──────────────────────────────────────┘
       ↓
   所有门禁通过 → 允许合并
```

### 4.4 GitHub Actions 触发条件

- Push 到 `main` 或 `develop` 分支
- Pull Request 到 `main` 或 `develop` 分支

### 4.5 查看 CI 结果

在 GitHub 仓库页面 → Actions 标签页 → 选择对应的 Workflow run。

---

## 5. CI 机器要求

CI 使用自托管 GitHub Actions Runner：

- 路径：`D:\unity\6000.0.2f1\Editor\Unity.exe`
- 必须配置 `UNITY_LICENSE` Secret
- 必须能够访问 `packages.unity.com` 下载依赖

### 5.1 配置 Secrets

在 GitHub 仓库 → Settings → Secrets and variables → Actions 中添加：

| Secret 名称 | 内容 |
|---|---|
| `UNITY_LICENSE` | Unity 编辑器许可证（Base64 编码或完整内容） |

### 5.2 CI 环境要求

```
Unity 版本: 6000.0.2f1
测试框架: com.unity.test-framework 1.4.3
操作系统: Windows
```

---

## 6. 回归测试规则

### 6.1 什么情况下需要运行测试

- 每次提交代码前
- 每次提交 PR 前
- 每次合并代码后

### 6.2 测试失败的处理

```
[REGRESSION DETECTED] 测试失败，禁止合并
```

1. 修复失败的测试或代码
2. 重新运行 `.\run-regression-tests.ps1`
3. 确保全部通过后再提交

### 6.3 新增代码的要求

- 新增的每个功能必须有对应测试
- 测试覆盖率目标：≥ 50%
- 建议：核心逻辑（PlayerController, GameManager）覆盖率 80%+

---

## 7. 代码风格规范

### 7.1 C# 命名规范

| 类型 | 规范 | 例子 |
|---|---|---|
| 类名 | PascalCase | `PlayerController` |
| 方法名 | PascalCase | `TransitionTo` |
| 字段名 | _camelCase | `_currentSpeed` |
| 常量 | kPascalCase | `kMaxStamina` |
| 枚举值 | PascalCase | `EPlayerState.Idle` |

### 7.2 文件格式

- 末尾必须有空行
- 使用 4 空格缩进（不是 Tab）
- 命名空间对齐

### 7.3 自动检查项

- 字段是否使用 `_` 前缀或 camelCase
- 文件末尾是否有空行
- 是否混用 Tab 和空格

---

## 8. 故障排查

### 8.1 测试找不到

确保：
1. 测试文件在 `Assets/INTIFALL/Tests/` 目录下
2. 继承了 NUnit 的 `[Test]` 或 `[TestFixture]`
3. asmdef 引用了 `INTIFALL.Tests`

### 8.2 Unity 无法启动

确保：
1. Unity Editor 路径正确
2. 项目路径正确
3. 许可证有效

### 8.3 包解析失败

确保：
1. `Packages/manifest.json` 中的版本在缓存中可用
2. CI 机器能访问 `packages.unity.com`

---

## 9. 相关文件

| 文件 | 说明 |
|---|---|
| `.github/workflows/INTIFALL.yml` | GitHub Actions CI 配置 |
| `run-regression-tests.ps1` | 本地回归测试脚本 |
| `Assets/INTIFALL/Tests/*.cs` | 所有测试文件 |
| `Assets/INTIFALL/Scripts/Runtime/` | 核心代码 |

---

## 10. 版本历史

| 日期 | 版本 | 变更 |
|---|---|---|
| 2026-03-22 | v1.0 | 初始版本，包含 31 个测试，4 道门禁 |
