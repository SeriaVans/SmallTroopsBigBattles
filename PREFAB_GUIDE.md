# 預製體建立指南

本指南詳細說明如何建立遊戲中各種 UI 預製體。

---

## 1. BuildingSlotUI (建築格) 預製體

### 建立步驟：

1. **建立基礎結構**
   - Hierarchy → 右鍵 → `UI` → `Button - TextMeshPro`
   - 重命名為 `BuildingSlot`
   - 設定大小：`150 x 150`

2. **建立子物件**
   ```
   BuildingSlot (Button)
   ├── Background (Image) - 背景圖
   ├── BuildingIcon (Image) - 建築圖示
   ├── LevelText (TextMeshPro) - 等級文字
   ├── EmptyIndicator (Image) - 空格指示 (+號圖示)
   └── ConstructingIndicator (GameObject)
       └── TimeText (TextMeshPro) - 剩餘時間
   ```

3. **設定元件**
   - 選中 `BuildingSlot`
   - `Add Component` → `BuildingSlotUI`
   - 連接各個 UI 元件到腳本欄位

4. **儲存預製體**
   - 拖動到 `Assets/_Project/Prefabs/UI/`
   - 命名為 `BuildingSlot.prefab`

### Inspector 設定範例：
```
BuildingSlotUI:
  - Button: BuildingSlot (自己)
  - Building Icon: BuildingIcon
  - Level Text: LevelText
  - Empty Indicator: EmptyIndicator
  - Constructing Indicator: ConstructingIndicator
  - Constructing Time Text: TimeText
```

---

## 2. GeneralListItemUI (將領列表項目) 預製體

### 建立步驟：

1. **建立基礎結構**
   - Hierarchy → 右鍵 → `UI` → `Button - TextMeshPro`
   - 重命名為 `GeneralListItem`
   - 設定大小：`300 x 100`

2. **建立子物件**
   ```
   GeneralListItem (Button)
   ├── Frame (Image) - 邊框 (會根據稀有度變色)
   ├── Portrait (Image) - 將領頭像
   ├── InfoPanel (GameObject)
   │   ├── NameText (TextMeshPro) - 名字
   │   ├── LevelText (TextMeshPro) - 等級
   │   └── ClassText (TextMeshPro) - 職業
   └── StarContainer (Horizontal Layout Group)
       └── [星星會動態生成]
   ```

3. **建立星星預製體**
   - 建立一個 Image，大小 `20 x 20`
   - 設定一個星星圖片 (或用 ★ 文字)
   - 存為 `Star.prefab`

4. **設定元件**
   - 選中 `GeneralListItem`
   - `Add Component` → `GeneralListItemUI`
   - 連接 UI 元件
   - 設定稀有度顏色

5. **儲存預製體**
   - 拖動到 `Assets/_Project/Prefabs/UI/`

### Inspector 設定範例：
```
GeneralListItemUI:
  - Button: GeneralListItem
  - Portrait: Portrait
  - Name Text: NameText
  - Level Text: LevelText
  - Class Text: ClassText
  - Star Container: StarContainer
  - Star Prefab: Star (預製體)
  - Frame Image: Frame

  稀有度顏色:
  - Rarity 1 Color: #808080 (灰色)
  - Rarity 2 Color: #00FF00 (綠色)
  - Rarity 3 Color: #0080FF (藍色)
  - Rarity 4 Color: #8000FF (紫色)
  - Rarity 5 Color: #FFD700 (金色)
```

---

## 3. TerritoryPanel (領地面板) 預製體

### 完整結構：
```
TerritoryPanel (Panel + CanvasGroup + TerritoryPanel腳本)
├── Header (Panel, 高度80)
│   ├── TitleText (TextMeshPro: "領地管理")
│   └── CloseButton (Button, 右上角)
│
├── TerritoryTabContainer (Horizontal Layout Group)
│   └── TabButton (Button預製體) - 動態生成
│
├── ContentArea (Panel)
│   ├── TerritoryInfo (Panel)
│   │   ├── TerritoryNameText
│   │   ├── TerritoryLevelText
│   │   └── BuildingCountText
│   │
│   └── BuildingSlotContainer (Grid Layout Group)
│       └── BuildingSlot (預製體) - 動態生成
│
└── BuildingDetailPopup (Panel, 預設隱藏)
    ├── BuildingNameText
    ├── BuildingLevelText
    ├── BuildingDescText
    ├── CostText
    ├── BuildButton
    └── UpgradeButton
```

### 重要設定：

1. **TerritoryTabContainer**
   - `Add Component` → `Horizontal Layout Group`
   - `Spacing` → `10`
   - `Child Alignment` → `Middle Left`

2. **BuildingSlotContainer**
   - `Add Component` → `Grid Layout Group`
   - `Cell Size` → `120 x 120`
   - `Spacing` → `15 x 15`
   - `Constraint` → `Fixed Column Count`
   - `Constraint Count` → `5`

3. **BuildingDetailPopup**
   - 預設設為隱藏 (取消勾選 GameObject)
   - 位置設在畫面中央

### 腳本連接：
```
TerritoryPanel:
  Panel Layer: Normal
  Show Animation: ✓
  Close Button: CloseButton
  Canvas Group: (自動)

  Territory Tab Container: TerritoryTabContainer
  Territory Tab Prefab: TabButton (預製體)
  Building Slot Container: BuildingSlotContainer
  Building Slot Prefab: BuildingSlot (預製體)

  Territory Name Text: TerritoryNameText
  Territory Level Text: TerritoryLevelText
  Building Count Text: BuildingCountText

  Building Detail Popup: BuildingDetailPopup
  Building Name Text: (Popup內的)
  Building Level Text: (Popup內的)
  ...
```

---

## 4. ArmyPanel (軍隊面板) 預製體

### 完整結構：
```
ArmyPanel (Panel + CanvasGroup + ArmyPanel腳本)
├── Header
│   ├── TitleText ("軍隊管理")
│   └── CloseButton
│
├── SoldierCountPanel (左側)
│   ├── SpearmanRow
│   │   ├── IconImage
│   │   ├── NameText ("槍兵")
│   │   └── CountText
│   ├── ShieldmanRow (同上結構)
│   ├── CavalryRow
│   ├── ArcherRow
│   └── TotalCountText
│
├── TrainingPanel (右側)
│   ├── SoldierTypeDropdown (TMP_Dropdown)
│   ├── CountSlider (Slider)
│   ├── CountInput (TMP_InputField)
│   ├── CostText
│   └── TrainButton
│
└── TrainingQueuePanel (底部)
    ├── ProgressBar (Image, Filled)
    ├── ProgressText
    └── QueueCountText
```

---

## 5. GeneralPanel (將領面板) 預製體

### 完整結構：
```
GeneralPanel (Panel + CanvasGroup + GeneralPanel腳本)
├── Header
│   ├── TitleText ("將領")
│   ├── GeneralCountText
│   └── CloseButton
│
├── ListPanel (左側, 佔 40% 寬度)
│   └── GeneralListContainer (Vertical Layout Group)
│       └── GeneralListItem (預製體) - 動態生成
│
├── DetailPanel (右側, 佔 60% 寬度, 預設隱藏)
│   ├── PortraitArea
│   │   ├── Portrait (Image)
│   │   └── StarContainer
│   │
│   ├── BasicInfo
│   │   ├── GeneralNameText
│   │   ├── GeneralClassText
│   │   ├── GeneralLevelText
│   │   └── ExpBar + ExpText
│   │
│   ├── StatsPanel
│   │   ├── StrengthText
│   │   ├── IntelligenceText
│   │   ├── CommandText
│   │   ├── SpeedText
│   │   ├── MaxTroopsText
│   │   └── PowerText
│   │
│   └── ActionButtons
│       └── DismissButton
│
└── BottomButtons
    └── RecruitButton
```

### GeneralListContainer 設定：
- `Add Component` → `Vertical Layout Group`
- `Spacing` → `5`
- `Add Component` → `Content Size Fitter`
- `Vertical Fit` → `Preferred Size`

如果列表會很長，加入 Scroll View：
1. 將 GeneralListContainer 放入 Scroll View 的 Content
2. Scroll View 設定為只垂直滾動

---

## 6. 通用面板模板

所有面板都應該遵循這個基本結構：

```
[PanelName] (Panel)
├── Background (Image, 半透明黑色遮罩) - 可選
├── ContentPanel (Panel, 實際內容區域)
│   ├── Header
│   │   ├── TitleText
│   │   └── CloseButton
│   └── Body
│       └── [具體內容]
└── [其他彈窗/子面板]
```

### 必要元件：
1. `CanvasGroup` - 用於淡入淡出動畫
2. 對應的 Panel 腳本
3. `Close Button` 連接到腳本

### 建議設定：
- 面板大小：`800 x 600` 或 `1200 x 800`
- 標題字體大小：`36`
- 內容字體大小：`24`
- 按鈕大小：`160 x 50`

---

## 快速建立檢查清單

建立每個預製體時，確認以下項目：

- [ ] 正確命名 GameObject
- [ ] 加入對應腳本元件
- [ ] 加入 CanvasGroup (面板需要)
- [ ] 連接所有 UI 元件到腳本
- [ ] 設定正確的 RectTransform Anchor
- [ ] 設定 Panel Layer (Normal/Popup/Top)
- [ ] 儲存到正確的 Prefabs 資料夾
- [ ] 註冊到 UIManager 的 Panel Prefabs 列表

---

## 預製體存放位置

```
Assets/_Project/Prefabs/
├── UI/
│   ├── Panels/
│   │   ├── TerritoryPanel.prefab
│   │   ├── ArmyPanel.prefab
│   │   ├── GeneralPanel.prefab
│   │   ├── MapPanel.prefab
│   │   ├── QuestPanel.prefab
│   │   └── SettingsPanel.prefab
│   │
│   └── Components/
│       ├── BuildingSlot.prefab
│       ├── GeneralListItem.prefab
│       ├── Star.prefab
│       └── TabButton.prefab
│
├── Battle/
│   └── [戰鬥相關預製體]
│
└── Map/
    └── [地圖相關預製體]
```
