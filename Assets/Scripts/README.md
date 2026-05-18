# 千缘猫域 · Demo 启动指南

## 一键启动（场景搭建）

1. 新建空场景 `Main.unity`。
2. 在场景中创建一个空 GameObject，命名 `Bootstrap`。
3. 给它挂上唯一一个脚本：`Assets/Scripts/SceneSetup.cs`。
4. 把这个场景设为 Build Settings 的首个场景。

> 不需要任何 Prefab、不需要手动拖引用。`SceneSetup` 在 `Awake` 中会程序化生成相机、所有 Manager、Player 装配、UI Canvas 和投射物 Prefab，并完成全部跨系统绑定。

## Unity 项目必要前置

### Tags（Project Settings → Tags and Layers）

必须存在以下三个 Tag：

- `Player`
- `Enemy`
- `Material`

> `MainCamera` Tag Unity 自带，无需新建。

### 配置文件（Resources）

必须存在以下 6 个 JSON 文件，路径都在 `Assets/Resources/Configs/`：

- `characters.json` — 6 个角色
- `weapons.json` — 10 把武器
- `enemies.json` — 6 种敌人
- `waves.json` — 8 个波次
- `items.json` — 12 件道具
- `shops.json` — 商店配置

> `ConfigManager` 在 `Awake` 自动 `Resources.Load` 这些 JSON，缺失会导致角色/武器/敌人列表为空。

### Input（Project Settings → Input Manager）

使用 Unity 默认的 `Horizontal` / `Vertical` 轴（WASD/方向键），无需额外配置。

## 运行流程

```
Boot → Menu → CharacterSelect → Battle ⇄ Shop → Results → Menu
              (玩家选角)         (波次进行)  (波次间) (死亡/通关)
```

- `MainMenuUI` 显示开始按钮 → 进入 `CharacterSelect`
- `CharacterSelectUI` 选角 → 调用 `GameManager.StartGame(id)` → 进入 `Battle`
- `Battle` 进入时 `SceneSetup.InitializePlayerForRun()` 从 `ConfigManager` 拉角色数据初始化玩家，并装备 starter weapon
- 波次结束 → `WaveManager.CompleteWave()` → `GameManager.EnterShop()` → `Shop`
- `ShopManager` 与 `ShopUI` 完成购买/升级 → `ExitShop()` 回到 `Battle` 进入下一波
- 玩家死亡 → `PlayerController.HandleDeath()` → `GameManager.TriggerGameOver()` → `Results`

## 命名空间速查

| 模块 | Namespace |
| --- | --- |
| 核心/状态 | `CatBrotato.Core` |
| 数据 | `CatBrotato.Data` |
| 战斗 | `CatBrotato.Combat` |
| 玩家 | `CatBrotato.Player` |
| 敌人 | `CatBrotato.Enemy` |
| 波次 | `CatBrotato.Wave` |
| 道具 | `CatBrotato.Item` |
| 商店 | `CatBrotato.Shop` |
| UI（uGUI） | `CatBrotato.UI` |

## 排错

- **黑屏/没有玩家**：检查 `Bootstrap` 上的 `SceneSetup` 是否挂载；检查 Console 是否报 `Field 'xxx' not found`（反射注入失败说明该私有字段被改名）。
- **菜单卡住不动**：检查 `Resources/Configs/characters.json` 是否存在且包含至少 1 个角色。
- **武器不发射**：检查 `WeaponData` 配置是否正确、`tag="Enemy"` 是否打到了敌人身上。
- **敌人不掉素材**：检查 `MaterialPickup` 所在 GameObject 是否 `tag="Material"`。
