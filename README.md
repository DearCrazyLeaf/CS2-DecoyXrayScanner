# CS2-DecoyXrayScanner

[![中文说明](https://img.shields.io/badge/跳转到中文版-中文说明-red)](#中文版说明)
[![Release](https://img.shields.io/github/v/release/DearCrazyLeaf/CS2-DecoyXrayScanner?include_prereleases&color=blueviolet)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/releases/latest)
[![License](https://img.shields.io/badge/License-GPL%203.0-orange)](https://www.gnu.org/licenses/gpl-3.0.txt)
[![Issues](https://img.shields.io/github/issues/DearCrazyLeaf/CS2-DecoyXrayScanner?color=darkgreen)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/issues)
[![Pull Requests](https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-DecoyXrayScanner?color=blue)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/pulls)
[![Downloads](https://img.shields.io/github/downloads/DearCrazyLeaf/CS2-DecoyXrayScanner/total?color=brightgreen)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/releases)
[![GitHub Stars](https://img.shields.io/github/stars/DearCrazyLeaf/CS2-DecoyXrayScanner?color=yellow)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/stargazers)

**CS2 decoy grenade tactical scan pulse plugin.** After a player throws a decoy the plugin performs one or more “tactical awareness” style scan pulses (configurable first delay + interval) that briefly outline enemies (optionally teammates) inside a radius and (optionally) plays a sound. Scanning can be toggled live by commands.

---
## Features
- Multiple scan pulses per decoy (configurable count / first delay / interval)
- Brief glow window to create a rhythmic pulse effect
- Detects and honors `mp_teammates_are_enemies` (FFA) – teammates auto treated as enemies when enabled
- Optional manual inclusion of teammates when not in FFA
- Per‑pulse sound (leave empty to disable)
- Custom enemy & ally colors
- Localization (English + Simplified Chinese)

## How It Works
1. Listens for `weapon_decoy` fire event.
2. Waits `FirstPulseDelaySeconds` before first scan to avoid wasting a pulse while the grenade is still near the thrower.
3. Repeats scans every `PulseIntervalSeconds` until `PulseCount` reached.
4. Each scan: find decoy position → enumerate players → apply glow (timed removal) → play optional sound.

## Requirements
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

## Installation
1. Download the latest release.
2. Extract to `game/csgo/addons/counterstrikesharp/plugins/CS2-DecoyXrayScanner/`.
3. Start the server once to generate `decoy_scanner_config.json` (it is placed in the plugin root directory, **not** the global configs folder).
4. Adjust config and reload via command or restart.

## Configuration (decoy_scanner_config.json)
```json
{
  "Enabled": true,
  "PulseCount": 2,
  "PulseRadius": 800.0,
  "PulseIntervalSeconds": 3.0,
  "GlowDurationSeconds": 1.0,
  "IncludeTeamMates": false,
  "EnemyGlowColor": "#FF0000",
  "AllyGlowColor": "#FF0000",
  "PulseSound": "ui/beep07.wav",
  "FirstPulseDelaySeconds": 1.5
}
```
| Key | Description |
|-----|-------------|
| Enabled | Master on/off. |
| PulseCount | Pulses per decoy. |
| PulseRadius | Radius per scan (units). |
| PulseIntervalSeconds | Interval between pulses. |
| GlowDurationSeconds | Glow lifetime per pulse. |
| IncludeTeamMates | Include same team (auto overridden by FFA cvar). |
| EnemyGlowColor / AllyGlowColor | Outline colors (hex). |
| PulseSound | Sound path (empty = none). |
| FirstPulseDelaySeconds | Delay from throw to first pulse. |

### FFA Mode
If `mp_teammates_are_enemies` is active (“true” / non‑zero) teammates are treated as enemies regardless of `IncludeTeamMates`.

### Localization
Language files shipped:
```
Lang/en.json
Lang/zh-Hans.json
```
Add more (e.g. `Lang/de.json`) following the same keys.

## Commands
| Command | Description |
|---------|-------------|
| `css_decoyxr_reload` | Reload configuration. |
| `css_decoyxr_clear`  | Clear all active glows. |
| `css_decoyxr_info`   | Show current status. |
| `css_decoyxr_enable` | Enable pulses. |
| `css_decoyxr_disable`| Disable pulses & clear glows. |

## Balance Suggestions
| Style | Suggested Config |
|-------|------------------|
| Single (near official feel) | PulseCount=1, GlowDurationSeconds=3.0 |
| Dual pulse (default) | PulseCount=2, GlowDurationSeconds=1.0, Interval=3.0 |
| Triangulation style | PulseCount=3, GlowDurationSeconds=0.8, Interval=2.0 |

## Audio Tips
- You can distribute your own VPK / workshop sound and set its path in `PulseSound`.
- Leave `PulseSound` empty to disable audio.

## License

<a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;">
    <img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License">
</a>

---
# 中文版说明
[![Back to English](https://img.shields.io/badge/Back_to-English-blue)](#cs2-decoyxrayscanner)
[![Release](https://img.shields.io/github/v/release/DearCrazyLeaf/CS2-DecoyXrayScanner?include_prereleases&color=blueviolet&label=最新版本)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/releases/latest)
[![License](https://img.shields.io/badge/许可证-GPL%203.0-orange)](https://www.gnu.org/licenses/gpl-3.0.txt)
[![Issues](https://img.shields.io/github/issues/DearCrazyLeaf/CS2-DecoyXrayScanner?color=darkgreen&label=反馈)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/issues)
[![Pull Requests](https://img.shields.io/github/issues-pr/DearCrazyLeaf/CS2-DecoyXrayScanner?color=blue&label=请求)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/pulls)
[![Downloads](https://img.shields.io/github/downloads/DearCrazyLeaf/CS2-DecoyXrayScanner/total?color=brightgreen&label=下载)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/releases)
[![GitHub Stars](https://img.shields.io/github/stars/DearCrazyLeaf/CS2-DecoyXrayScanner?color=yellow&label=标星)](https://github.com/DearCrazyLeaf/CS2-DecoyXrayScanner/stargazers)

**CS2 诱饵弹探测雷“扫描脉冲”插件** 
玩家丢出诱饵弹后按设定的延迟与间隔执行类似探测手雷的扫描效果，将范围内敌人（可选队友）短暂描边，并播放提示音，可通过指令热开关扫描功能

## 功能
- 每枚诱饵多次扫描（可配置次数 / 首次延迟 / 间隔）
- 短暂描边显示形成“脉冲”效果
- 支持 `mp_teammates_are_enemies` 检测 ，开启时自动把队友视作敌人扫描
- 可选手动包含队友
- 每脉冲可播放音效（不填写则不播放）
- 自定义敌我颜色
- 多语言支持

## 原理
1. 监听武器开火事件检测诱饵 (`weapon_decoy`)
2. 等待 `FirstPulseDelaySeconds` 后执行第一次扫描
3. 后续按 `PulseIntervalSeconds` 重复，直到达到 `PulseCount`
4. 每次扫描计算位置 → 查找玩家 → 应用描边 → 计时取消 → 播放音效

## 需求
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

## 安装
1. 下载最新 Release
2. 解压到 `game/csgo/addons/counterstrikesharp/plugins/CS2-DecoyXrayScanner/`
3. 启动服务器生成配置 `decoy_scanner_config.json` （位于插件根目录，非configs下！）
4. 修改配置并通过命令或重启生效

## 配置示例
见上方英文部分 JSON。参数含义：
| 键 | 含义 |
|----|------|
| Enabled | 总开关|
| PulseCount | 每枚诱饵扫描次数|
| PulseRadius | 单次扫描半径|
| PulseIntervalSeconds | 扫描间隔|
| GlowDurationSeconds | 单次描边持续时间|
| IncludeTeamMates | 是否包含队友（FFA 时自动包含）|
| EnemyGlowColor / AllyGlowColor | 敌/友描边颜色|
| PulseSound | 脉冲音效路径（空=关闭）|
| FirstPulseDelaySeconds | 抛出到第一次扫描的延迟|

### FFA 模式
服务器 `mp_teammates_are_enemies` 开启时即视队友为敌人，忽略 `IncludeTeamMates` 设置

## 指令
| 指令 | 说明 |
|------|------|
| `css_decoyxr_reload` | 重载配置|
| `css_decoyxr_clear`  | 清空所有描边|
| `css_decoyxr_info`   | 查看插件状态|
| `css_decoyxr_enable` | 启用脉冲|
| `css_decoyxr_disable`| 禁用脉冲|

## 平衡建议
| 风格 | 建议配置 |
|------|----------|
| 接近官方一次探测 | PulseCount=1, GlowDurationSeconds=3.0 |
| 双脉冲（默认） | PulseCount=2, GlowDurationSeconds=1.0, Interval=3.0 |
| 三脉冲定位 | PulseCount=3, GlowDurationSeconds=0.8, Interval=2.0 |

## 音效提示
- 你可以自己上传创意工坊文件然后将VPK路径填写进 `PulseSound` 后，即可自定义扫描音效
- 留空 `PulseSound` 表示不使用音效

## 许可证

<a href="https://www.gnu.org/licenses/gpl-3.0.txt" target="_blank" style="margin-left: 10px; text-decoration: none;">
    <img src="https://img.shields.io/badge/License-GPL%203.0-orange?style=for-the-badge&logo=gnu" alt="GPL v3 License">
</a>

---