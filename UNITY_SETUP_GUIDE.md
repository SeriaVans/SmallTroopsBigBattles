# Unity 專案設置指南

本指南將教你如何在 Unity 中設置遊戲場景和物件。

## 目錄
1. [開啟專案](#1-開啟專案)
2. [安裝必要套件](#2-安裝必要套件)
3. [建立主場景](#3-建立主場景)
4. [建立核心管理器](#4-建立核心管理器)
5. [建立 UI 系統](#5-建立-ui-系統)
6. [建立 HUD](#6-建立-hud)
7. [建立 UI 面板預製體](#7-建立-ui-面板預製體)
8. [測試運行](#8-測試運行)

---

## 1. 開啟專案

1. 開啟 Unity Hub
2. 點擊 `Add` → `Add project from disk`
3. 選擇 `SmallTroopsBigBattles/SmallTroopsBigBattles` 資料夾
4. 等待專案載入完成

---

## 2. 安裝必要套件

### 安裝 TextMeshPro
1. 選單列 → `Window` → `Package Manager`
2. 點擊左上角 `+` → `Add package by name`
3. 輸入 `com.unity.textmeshpro` → 點擊 `Add`
4. 安裝完成後，選單列 → `Window` → `TextMeshPro` → `Import TMP Essential Resources`

---

## 3. 建立主場景

### 步驟 3.1: 建立新場景
1. 選單列 → `File` → `New Scene`
2. 選擇 `Basic (URP)` 模板 → 點擊 `Create`
3. `Ctrl + S` 儲存場景
4. 命名為 `MainGame`，存放在 `Assets/_Project/Scenes/` 資料夾

### 步驟 3.2: 設定攝影機
1. 在 Hierarchy 視窗中選擇 `Main Camera`
2. 在 Inspector 視窗中設定：
   - `Clear Flags` → `Solid Color`
   - `Background` → 選擇深色背景 (如 #1A1A2E)
   - `Projection` → `Orthographic` (2D 遊戲)
   - `Size` → `5`

---

## 4. 建立核心管理器

### 步驟 4.1: 建立 GameManager 物件
1. 在 Hierarchy 視窗空白處 → 右鍵 → `Create Empty`
2. 重命名為 `[GameManager]`
3. 選中該物件，在 Inspector 視窗：
   - 點擊 `Add Component`
   - 搜尋 `GameManager` → 點選加入

### 步驟 4.2: 建立 EventManager 物件
1. Hierarchy → 右鍵 → `Create Empty`
2. 重命名為 `[EventManager]`
3. `Add Component` → 搜尋 `EventManager` → 加入

### 步驟 4.3: 建立其他 Manager
重複上述步驟建立以下物件：

| 物件名稱 | 腳本 |
|---------|------|
| `[ResourceManager]` | ResourceManager |
| `[TerritoryManager]` | TerritoryManager |
| `[ArmyManager]` | ArmyManager |
| `[GeneralManager]` | GeneralManager |

> 💡 **提示**: 這些 Manager 使用單例模式，會在遊戲開始時自動初始化。你也可以只建立 GameManager，其他會自動生成。

---

## 5. 建立 UI 系統

### 步驟 5.1: 建立 UIManager
1. Hierarchy → 右鍵 → `Create Empty`
2. 重命名為 `[UIManager]`
3. `Add Component` → 搜尋 `UIManager` → 加入

### 步驟 5.2: 建立 UI Canvas
1. Hierarchy → 右鍵 → `UI` → `Canvas`
2. 重命名為 `MainCanvas`
3. 選中 Canvas，在 Inspector 設定：
   - `Render Mode` → `Screen Space - Overlay`
   - `Canvas Scaler`:
     - `UI Scale Mode` → `Scale With Screen Size`
     - `Reference Resolution` → `1920 x 1080`
     - `Match` → `0.5` (拖動滑桿到中間)

### 步驟 5.3: 建立 UI 層級結構
在 MainCanvas 下建立空物件作為層級容器：

1. 選中 `MainCanvas` → 右鍵 → `Create Empty`
2. 重命名為 `NormalLayer`
3. 設定 RectTransform:
   - `Anchor` → 按住 Alt + Shift，點選右下角 (Stretch)
   - 確保 Left, Top, Right, Bottom 都是 0

重複建立：
- `PopupLayer`
- `TopLayer`

最終結構：
```
MainCanvas
├── NormalLayer
├── PopupLayer
└── TopLayer
```

### 步驟 5.4: 連接 UIManager
1. 選中 `[UIManager]` 物件
2. 在 Inspector 中找到 UIManager 腳本
3. 將層級物件拖放到對應欄位：
   - `Normal Layer` ← 拖入 `NormalLayer`
   - `Popup Layer` ← 拖入 `PopupLayer`
   - `Top Layer` ← 拖入 `TopLayer`

---

## 6. 建立 HUD

### 步驟 6.1: 建立 HUD 容器
1. 在 `MainCanvas` 下 → 右鍵 → `Create Empty`
2. 重命名為 `HUD`
3. 設定 RectTransform 為 Stretch (填滿整個畫面)

### 步驟 6.2: 建立頂部資源列
1. 選中 `HUD` → 右鍵 → `UI` → `Panel`
2. 重命名為 `TopResourceBar`
3. 設定 RectTransform:
   - `Anchor` → 點選 Top-Stretch (上方拉伸)
   - `Height` → `80`
   - `Left`, `Right` → `0`

### 步驟 6.3: 建立資源顯示
在 `TopResourceBar` 下建立 4 個資源顯示：

1. 右鍵 → `UI` → `Text - TextMeshPro`
2. 重命名為 `CopperText`
3. 設定文字：`銅錢: 0`
4. 調整位置和大小

重複建立：
- `WoodText` (木材)
- `StoneText` (石材)
- `FoodText` (糧草)

### 步驟 6.4: 建立底部功能列
1. 在 `HUD` 下 → 右鍵 → `UI` → `Panel`
2. 重命名為 `BottomBar`
3. 設定 RectTransform:
   - `Anchor` → 點選 Bottom-Stretch
   - `Height` → `120`

### 步驟 6.5: 建立功能按鈕
在 `BottomBar` 下建立按鈕：

1. 右鍵 → `UI` → `Button - TextMeshPro`
2. 重命名為 `TerritoryButton`
3. 修改按鈕文字為 `領地`
4. 調整大小和位置

重複建立：
- `ArmyButton` (軍隊)
- `GeneralButton` (將領)
- `MapButton` (地圖)
- `QuestButton` (任務)
- `SettingsButton` (設定)

### 步驟 6.6: 加入 GameHUD 腳本
1. 選中 `HUD` 物件
2. `Add Component` → 搜尋 `GameHUD` → 加入
3. 在 Inspector 中連接 UI 元件：
   - 將各個 Text 拖放到對應欄位
   - 將各個 Button 拖放到對應欄位

---

## 7. 建立 UI 面板預製體

### 步驟 7.1: 建立 TerritoryPanel 預製體

1. 在 `NormalLayer` 下 → 右鍵 → `UI` → `Panel`
2. 重命名為 `TerritoryPanel`
3. 設定為填滿整個畫面
4. `Add Component` → 加入 `TerritoryPanel` 腳本
5. `Add Component` → 加入 `Canvas Group`

#### 建立面板內容：
在 `TerritoryPanel` 下建立：

```
TerritoryPanel
├── Header (Panel)
│   ├── TitleText (TextMeshPro: "領地管理")
│   └── CloseButton (Button)
├── TerritoryTabs (Horizontal Layout Group)
│   └── [領地標籤會動態生成]
├── BuildingSlotContainer (Grid Layout Group)
│   └── [建築格會動態生成]
└── BuildingDetailPopup (Panel, 預設隱藏)
    ├── BuildingNameText
    ├── BuildingLevelText
    ├── BuildingDescText
    ├── CostText
    ├── BuildButton
    └── UpgradeButton
```

#### 設定 Grid Layout:
1. 選中 `BuildingSlotContainer`
2. `Add Component` → `Grid Layout Group`
3. 設定：
   - `Cell Size` → `150 x 150`
   - `Spacing` → `10 x 10`
   - `Constraint` → `Fixed Column Count`
   - `Constraint Count` → `5`

### 步驟 7.2: 儲存為預製體
1. 將設定好的 `TerritoryPanel` 從 Hierarchy 拖到 `Assets/_Project/Prefabs/UI/`
2. 選擇 `Original Prefab`
3. 刪除 Hierarchy 中的 TerritoryPanel (預製體已保存)

### 步驟 7.3: 建立其他面板預製體
重複上述步驟建立：
- `ArmyPanel`
- `GeneralPanel`
- `MapPanel`
- `QuestPanel`
- `SettingsPanel`

### 步驟 7.4: 註冊預製體到 UIManager
1. 選中 `[UIManager]` 物件
2. 在 Inspector 找到 `Panel Prefabs` 列表
3. 點擊 `+` 按鈕新增項目
4. 將預製體從 Project 視窗拖入

---

## 8. 測試運行

### 步驟 8.1: 儲存場景
1. `Ctrl + S` 儲存場景
2. 確認所有物件和連接都正確

### 步驟 8.2: 運行測試
1. 點擊頂部 `▶ Play` 按鈕
2. 檢查 Console 視窗是否有錯誤
3. 應該看到：
   - `[GameManager] 初始化遊戲...`
   - `[GameManager] 所有 Manager 初始化完成`
   - `[GameManager] 建立測試玩家: 測試玩家`

### 步驟 8.3: 測試 UI
1. 點擊底部的功能按鈕
2. 應該能開啟對應的面板
3. 點擊關閉按鈕應該能關閉面板

---

## 常見問題

### Q: 找不到腳本？
確保：
1. 腳本已經存放在正確位置 (`Assets/_Project/Scripts/`)
2. 腳本沒有編譯錯誤 (檢查 Console 視窗)
3. 腳本的類別名稱與檔案名稱相同

### Q: UI 顯示不正確？
1. 檢查 Canvas Scaler 設定
2. 確認 RectTransform 的 Anchor 設定正確
3. 確認層級順序正確

### Q: 點擊按鈕沒反應？
1. 確認按鈕上有 `Button` 元件
2. 確認 Canvas 上有 `Graphic Raycaster` 元件
3. 確認場景中有 `EventSystem` 物件

### Q: TextMeshPro 顯示方框？
1. 確認已匯入 TMP Essential Resources
2. 選單 → `Window` → `TextMeshPro` → `Import TMP Essential Resources`

---

## 場景結構總覽

完成後，你的 Hierarchy 應該看起來像這樣：

```
MainGame (Scene)
├── Main Camera
├── Directional Light
├── [GameManager]
├── [EventManager]
├── [ResourceManager]
├── [TerritoryManager]
├── [ArmyManager]
├── [GeneralManager]
├── [UIManager]
├── MainCanvas
│   ├── NormalLayer
│   ├── PopupLayer
│   ├── TopLayer
│   └── HUD
│       ├── TopResourceBar
│       │   ├── CopperText
│       │   ├── WoodText
│       │   ├── StoneText
│       │   └── FoodText
│       └── BottomBar
│           ├── TerritoryButton
│           ├── ArmyButton
│           ├── GeneralButton
│           ├── MapButton
│           ├── QuestButton
│           └── SettingsButton
└── EventSystem
```

---

## 下一步

完成基本設置後，你可以：
1. 美化 UI 介面 (加入圖片、調整顏色)
2. 建立建築格預製體 (`BuildingSlotUI`)
3. 建立將領列表項目預製體 (`GeneralListItemUI`)
4. 開始開發地圖系統

如有問題，請查看腳本中的註解或聯繫開發團隊。
