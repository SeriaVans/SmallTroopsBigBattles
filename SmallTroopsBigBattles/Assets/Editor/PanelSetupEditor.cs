using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 面板設置編輯器 - 自動設置各種 UI 面板佈局
/// </summary>
public class PanelSetupEditor : EditorWindow
{
    [MenuItem("Tools/SLG Game/Setup All Panels")]
    public static void SetupAllPanels()
    {
        SetupTerritoryPanel();
        SetupArmyPanel();
        SetupGeneralPanel();

        Debug.Log("所有面板設置完成！");
    }

    [MenuItem("Tools/SLG Game/Setup Territory Panel")]
    public static void SetupTerritoryPanel()
    {
        var panel = GameObject.Find("MainCanvas/NormalLayer/TerritoryPanel");
        if (panel == null)
        {
            Debug.LogError("找不到 TerritoryPanel！");
            return;
        }

        // 設置面板基本屬性
        SetupPanelBase(panel, new Color(0.12f, 0.12f, 0.18f, 0.95f));

        // 設置 Header
        SetupPanelHeader(panel, "Header", "領地管理");

        // 設置 Content
        SetupPanelContent(panel, "Content");

        // 設置關閉按鈕
        SetupCloseButton(panel, "CloseButton");

        // 建立內容區域
        var content = panel.transform.Find("Content");
        if (content != null)
        {
            // 創建領地選擇列表區
            CreateTerritorySelector(content);

            // 創建建築顯示區
            CreateBuildingArea(content);
        }

        // 添加腳本組件
        AddPanelScript(panel, "TerritoryPanel");

        EditorUtility.SetDirty(panel);
        Debug.Log("TerritoryPanel 設置完成！");
    }

    [MenuItem("Tools/SLG Game/Setup Army Panel")]
    public static void SetupArmyPanel()
    {
        // 先創建面板
        var parent = GameObject.Find("MainCanvas/NormalLayer");
        if (parent == null) return;

        var panel = parent.transform.Find("ArmyPanel")?.gameObject;
        if (panel == null)
        {
            panel = new GameObject("ArmyPanel");
            panel.transform.SetParent(parent.transform, false);
        }

        EnsureComponent<RectTransform>(panel);
        EnsureComponent<Image>(panel);
        EnsureComponent<CanvasGroup>(panel);

        // 設置面板基本屬性
        SetupPanelBase(panel, new Color(0.18f, 0.1f, 0.1f, 0.95f));

        // 創建子物件
        EnsureChild(panel.transform, "Header");
        EnsureChild(panel.transform, "Content");
        EnsureChild(panel.transform, "CloseButton");

        SetupPanelHeader(panel, "Header", "軍隊管理");
        SetupPanelContent(panel, "Content");
        SetupCloseButton(panel, "CloseButton");

        // 創建軍隊內容
        var content = panel.transform.Find("Content");
        if (content != null)
        {
            CreateArmyContent(content);
        }

        AddPanelScript(panel, "ArmyPanel");

        EditorUtility.SetDirty(panel);
        Debug.Log("ArmyPanel 設置完成！");
    }

    [MenuItem("Tools/SLG Game/Setup General Panel")]
    public static void SetupGeneralPanel()
    {
        // 先創建面板
        var parent = GameObject.Find("MainCanvas/NormalLayer");
        if (parent == null) return;

        var panel = parent.transform.Find("GeneralPanel")?.gameObject;
        if (panel == null)
        {
            panel = new GameObject("GeneralPanel");
            panel.transform.SetParent(parent.transform, false);
        }

        EnsureComponent<RectTransform>(panel);
        EnsureComponent<Image>(panel);
        EnsureComponent<CanvasGroup>(panel);

        // 設置面板基本屬性
        SetupPanelBase(panel, new Color(0.12f, 0.1f, 0.18f, 0.95f));

        // 創建子物件
        EnsureChild(panel.transform, "Header");
        EnsureChild(panel.transform, "Content");
        EnsureChild(panel.transform, "CloseButton");

        SetupPanelHeader(panel, "Header", "將領管理");
        SetupPanelContent(panel, "Content");
        SetupCloseButton(panel, "CloseButton");

        // 創建將領內容
        var content = panel.transform.Find("Content");
        if (content != null)
        {
            CreateGeneralContent(content);
        }

        AddPanelScript(panel, "GeneralPanel");

        EditorUtility.SetDirty(panel);
        Debug.Log("GeneralPanel 設置完成！");
    }

    private static void SetupPanelBase(GameObject panel, Color bgColor)
    {
        var rect = panel.GetComponent<RectTransform>();
        if (rect != null)
        {
            // 設置為居中，固定大小
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800, 600);
        }

        var image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.color = bgColor;
        }
    }

    private static void SetupPanelHeader(GameObject panel, string headerName, string title)
    {
        var headerObj = panel.transform.Find(headerName)?.gameObject;
        if (headerObj == null) return;

        var rect = EnsureComponent<RectTransform>(headerObj);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0, 60);

        var image = EnsureComponent<Image>(headerObj);
        image.color = new Color(0.2f, 0.2f, 0.3f, 1f);

        // 創建標題文字
        var titleObj = headerObj.transform.Find("TitleText")?.gameObject;
        if (titleObj == null)
        {
            titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(headerObj.transform, false);
        }

        var titleRect = EnsureComponent<RectTransform>(titleObj);
        SetStretchAll(titleRect);

        var titleText = EnsureComponent<TextMeshProUGUI>(titleObj);
        titleText.text = title;
        titleText.color = Color.white;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
    }

    private static void SetupPanelContent(GameObject panel, string contentName)
    {
        var contentObj = panel.transform.Find(contentName)?.gameObject;
        if (contentObj == null) return;

        var rect = EnsureComponent<RectTransform>(contentObj);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(20, 70);  // left, bottom
        rect.offsetMax = new Vector2(-20, -70); // right, top
    }

    private static void SetupCloseButton(GameObject panel, string buttonName)
    {
        var buttonObj = panel.transform.Find(buttonName)?.gameObject;
        if (buttonObj == null) return;

        var rect = EnsureComponent<RectTransform>(buttonObj);
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-10, -10);
        rect.sizeDelta = new Vector2(40, 40);

        var image = EnsureComponent<Image>(buttonObj);
        image.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        var button = EnsureComponent<Button>(buttonObj);
        var colors = button.colors;
        colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        button.colors = colors;

        // 創建 X 文字
        var xObj = buttonObj.transform.Find("X")?.gameObject;
        if (xObj == null)
        {
            xObj = new GameObject("X");
            xObj.transform.SetParent(buttonObj.transform, false);
        }

        var xRect = EnsureComponent<RectTransform>(xObj);
        SetStretchAll(xRect);

        var xText = EnsureComponent<TextMeshProUGUI>(xObj);
        xText.text = "X";
        xText.color = Color.white;
        xText.fontSize = 24;
        xText.fontStyle = FontStyles.Bold;
        xText.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateTerritorySelector(Transform content)
    {
        // 創建領地選擇區
        var selectorObj = EnsureChild(content, "TerritorySelector");
        var selectorRect = EnsureComponent<RectTransform>(selectorObj);
        selectorRect.anchorMin = new Vector2(0, 0.7f);
        selectorRect.anchorMax = new Vector2(1, 1);
        selectorRect.offsetMin = Vector2.zero;
        selectorRect.offsetMax = Vector2.zero;

        var selectorImage = EnsureComponent<Image>(selectorObj);
        selectorImage.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

        // 創建領地列表標題
        var listTitleObj = EnsureChild(selectorObj.transform, "ListTitle");
        var listTitleRect = EnsureComponent<RectTransform>(listTitleObj);
        listTitleRect.anchorMin = new Vector2(0, 0.8f);
        listTitleRect.anchorMax = new Vector2(1, 1);
        listTitleRect.offsetMin = new Vector2(10, 0);
        listTitleRect.offsetMax = new Vector2(-10, 0);

        var listTitleText = EnsureComponent<TextMeshProUGUI>(listTitleObj);
        listTitleText.text = "我的領地";
        listTitleText.fontSize = 20;
        listTitleText.color = Color.white;

        // 創建領地列表容器
        var listContainerObj = EnsureChild(selectorObj.transform, "TerritoryList");
        var listContainerRect = EnsureComponent<RectTransform>(listContainerObj);
        listContainerRect.anchorMin = new Vector2(0, 0);
        listContainerRect.anchorMax = new Vector2(1, 0.8f);
        listContainerRect.offsetMin = new Vector2(10, 10);
        listContainerRect.offsetMax = new Vector2(-10, -5);

        var layout = EnsureComponent<HorizontalLayoutGroup>(listContainerObj);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleLeft;
    }

    private static void CreateBuildingArea(Transform content)
    {
        // 創建建築顯示區
        var buildingObj = EnsureChild(content, "BuildingArea");
        var buildingRect = EnsureComponent<RectTransform>(buildingObj);
        buildingRect.anchorMin = new Vector2(0, 0);
        buildingRect.anchorMax = new Vector2(1, 0.65f);
        buildingRect.offsetMin = Vector2.zero;
        buildingRect.offsetMax = Vector2.zero;

        var buildingImage = EnsureComponent<Image>(buildingObj);
        buildingImage.color = new Color(0.1f, 0.1f, 0.15f, 0.8f);

        // 創建建築標題
        var titleObj = EnsureChild(buildingObj.transform, "BuildingTitle");
        var titleRect = EnsureComponent<RectTransform>(titleObj);
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(10, 0);
        titleRect.offsetMax = new Vector2(-10, 0);

        var titleText = EnsureComponent<TextMeshProUGUI>(titleObj);
        titleText.text = "建築物";
        titleText.fontSize = 20;
        titleText.color = Color.white;

        // 創建建築格子容器
        var gridObj = EnsureChild(buildingObj.transform, "BuildingGrid");
        var gridRect = EnsureComponent<RectTransform>(gridObj);
        gridRect.anchorMin = new Vector2(0, 0);
        gridRect.anchorMax = new Vector2(1, 0.9f);
        gridRect.offsetMin = new Vector2(10, 10);
        gridRect.offsetMax = new Vector2(-10, -5);

        var grid = EnsureComponent<GridLayoutGroup>(gridObj);
        grid.cellSize = new Vector2(120, 120);
        grid.spacing = new Vector2(10, 10);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
    }

    private static void CreateArmyContent(Transform content)
    {
        // 創建士兵總覽
        var overviewObj = EnsureChild(content, "ArmyOverview");
        var overviewRect = EnsureComponent<RectTransform>(overviewObj);
        overviewRect.anchorMin = new Vector2(0, 0.7f);
        overviewRect.anchorMax = new Vector2(1, 1);
        overviewRect.offsetMin = Vector2.zero;
        overviewRect.offsetMax = Vector2.zero;

        var overviewImage = EnsureComponent<Image>(overviewObj);
        overviewImage.color = new Color(0.2f, 0.1f, 0.1f, 0.8f);

        // 創建兵種列表
        var troopListObj = EnsureChild(content, "TroopList");
        var troopListRect = EnsureComponent<RectTransform>(troopListObj);
        troopListRect.anchorMin = new Vector2(0, 0);
        troopListRect.anchorMax = new Vector2(1, 0.65f);
        troopListRect.offsetMin = Vector2.zero;
        troopListRect.offsetMax = Vector2.zero;

        var grid = EnsureComponent<GridLayoutGroup>(troopListObj);
        grid.cellSize = new Vector2(180, 100);
        grid.spacing = new Vector2(15, 15);
        grid.padding = new RectOffset(10, 10, 10, 10);
    }

    private static void CreateGeneralContent(Transform content)
    {
        // 創建將領列表區
        var listAreaObj = EnsureChild(content, "GeneralListArea");
        var listAreaRect = EnsureComponent<RectTransform>(listAreaObj);
        listAreaRect.anchorMin = new Vector2(0, 0);
        listAreaRect.anchorMax = new Vector2(0.5f, 1);
        listAreaRect.offsetMin = new Vector2(0, 0);
        listAreaRect.offsetMax = new Vector2(-5, 0);

        var listAreaImage = EnsureComponent<Image>(listAreaObj);
        listAreaImage.color = new Color(0.15f, 0.12f, 0.2f, 0.8f);

        // 創建將領詳情區
        var detailAreaObj = EnsureChild(content, "GeneralDetailArea");
        var detailAreaRect = EnsureComponent<RectTransform>(detailAreaObj);
        detailAreaRect.anchorMin = new Vector2(0.5f, 0);
        detailAreaRect.anchorMax = new Vector2(1, 1);
        detailAreaRect.offsetMin = new Vector2(5, 0);
        detailAreaRect.offsetMax = new Vector2(0, 0);

        var detailAreaImage = EnsureComponent<Image>(detailAreaObj);
        detailAreaImage.color = new Color(0.12f, 0.1f, 0.18f, 0.8f);

        // 創建列表滾動視圖
        var scrollObj = EnsureChild(listAreaObj.transform, "GeneralScroll");
        var scrollRect = EnsureComponent<RectTransform>(scrollObj);
        SetStretchAll(scrollRect);
        scrollRect.offsetMin = new Vector2(10, 10);
        scrollRect.offsetMax = new Vector2(-10, -10);

        var scrollView = EnsureComponent<ScrollRect>(scrollObj);

        // 創建將領列表內容
        var listContentObj = EnsureChild(scrollObj.transform, "Content");
        var listContentRect = EnsureComponent<RectTransform>(listContentObj);
        listContentRect.anchorMin = new Vector2(0, 1);
        listContentRect.anchorMax = new Vector2(1, 1);
        listContentRect.pivot = new Vector2(0.5f, 1);
        listContentRect.anchoredPosition = Vector2.zero;
        listContentRect.sizeDelta = new Vector2(0, 400);

        var layout = EnsureComponent<VerticalLayoutGroup>(listContentObj);
        layout.spacing = 5;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = EnsureComponent<ContentSizeFitter>(listContentObj);
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollView.content = listContentRect;
        scrollView.horizontal = false;
        scrollView.vertical = true;
    }

    private static void AddPanelScript(GameObject panel, string scriptName)
    {
        var typeName = $"SmallTroopsBigBattles.UI.Panels.{scriptName}";
        var type = System.Type.GetType($"{typeName}, Assembly-CSharp");

        if (type != null && panel.GetComponent(type) == null)
        {
            panel.AddComponent(type);
            Debug.Log($"已添加腳本：{scriptName}");
        }
    }

    #region Helper Methods

    private static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        var component = obj.GetComponent<T>();
        if (component == null)
            component = obj.AddComponent<T>();
        return component;
    }

    private static GameObject EnsureChild(Transform parent, string name)
    {
        var child = parent.Find(name);
        if (child == null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj;
        }
        return child.gameObject;
    }

    private static void SetStretchAll(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #endregion
}

