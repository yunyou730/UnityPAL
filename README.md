# UnityPAL

Unity 6 原型项目，用自定义渲染与玩法服务还原经典 RPG 《仙剑奇侠传一》的核心系统。项目会加载原版游戏资产（MKF 数据、调色板、精灵、地图），并通过自定义管线进行渲染，包括地图/精灵实体管理和基础队伍移动。

## 特性
- 自定义 MKF 解码：地图、精灵、调色板、游戏数据（`Assets/StreamingAssets/*.MKF`）。
- 调色板加载（昼/夜）与索引色精灵渲染（`Palette`、`Renderer`）。
- 地图与精灵实体管理，视口 + 队伍偏移处理（`MapService`、`ViewportService`、`SpriteEntityManager`）。
- 基础玩法循环：约 10 FPS 逻辑帧，队伍朝向与行走/站立帧切换（`PALGameplayService`）。
- 简单输入层（方向键）和 Debug 菜单，可切换调色板/地图/精灵。
- 内置调试场景工具：精灵帧查看、拼 Sheet、相机控制、瓦片覆盖信息开关。

## 项目状态
早期原型 / 持续开发中。当前重心是渲染正确性与数据解码，玩法系统仍在完善。

## 环境要求
- Unity Editor **6000.0.38f1**（见 `ProjectVersion.txt`）。
- Windows（win32）测试。使用内置管线，`Packages/manifest.json` 中包含 URP 依赖。

## 快速开始
1. 克隆仓库。
2. 用 Unity 6000.0.38f1 打开项目。
3. 场景：
   - `Assets/Scenes/Game.unity` —— 运行时演示（队伍 + 地图）。
   - `Assets/Scenes/AssetViewer.unity` —— 资产查看（调色板/地图/精灵）。
   - `Assets/Scenes/Test.unity` —— 沙盒/测试。
4. 确认原版 PAL 数据已放在 `Assets/StreamingAssets/`（本项目已包含：`PAT.MKF`、`MAP.MKF`、`MGO.MKF`、`GOP.MKF`、`DATA.MKF` 等）。
5. 点击 Play。

## 操作说明
- 玩法场景：方向键移动/调整队伍朝向（逻辑帧 ~10 FPS）。
- Debug 菜单（包含该组件的场景）：
  - 切换地图：左右方向键（需开启调试输入）。
  - 切换精灵：上下方向键；`P` 轮换精灵帧；`L` 跳转视口/采样位置。
  - 相机（调试）：`W/A/S/D` 平移，鼠标滚轮缩放。
  - UI 下拉框选择调色板/地图/精灵；按钮用于加载默认游戏状态、设置位置、切换瓦片信息/控制调试。

## 代码结构
- `Assets/PAL/Scripts/Core` —— MKF 读取、调色板处理、精灵渲染、地图封装。
- `Assets/PAL/Scripts/Services` —— 服务层（调色板、地图、精灵、视口、游戏状态、玩法）。
- `Assets/PAL/Scripts/Gameplay` —— 输入、实体管理。
- `Assets/PAL/Scripts/Presenter` —— 视图/呈现层对接。
- `Assets/Scripts/DebugMenu.cs` —— 运行时调试 UI 与控制逻辑。
- `Assets/StreamingAssets` —— 原版 PAL 数据文件。

## 规划方向
- 补充移动障碍/碰撞检测。
- NPC 实体与交互。
- 战斗场景与 UI 层。
- 原声效/BGM 读取与播放。
- 存档/读档。
- 迁移至新版 Input System（当前用 `Input.GetKey`）。

## 致谢
- 致敬原作《仙剑奇侠传一》及社区的逆向工程项目（如 SDLPal）。
- 感谢 Unity 及 `Packages/manifest.json` 中列出的相关依赖。

## 许可证
待定。请添加 LICENSE 文件以明确代码与资产的使用条款。
