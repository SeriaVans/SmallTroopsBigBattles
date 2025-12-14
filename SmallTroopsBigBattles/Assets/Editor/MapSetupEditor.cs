using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 地圖設置編輯器 - 自動設置地圖相關預製體和組件
/// </summary>
public class MapSetupEditor : EditorWindow
{
    [MenuItem("Tools/SLG Game/Setup Map Prefabs")]
    public static void SetupMapPrefabs()
    {
        SetupMapNodePrefab();
        SetupRouteLinePrefab();
        SetupMapView();

        Debug.Log("地圖預製體設置完成！");
    }

    private static void SetupMapNodePrefab()
    {
        var prefab = GameObject.Find("MapNodePrefab");
        if (prefab == null)
        {
            Debug.LogError("找不到 MapNodePrefab！");
            return;
        }

        // 設置主要節點尺寸
        var rect = prefab.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(60, 60);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        // 設置節點背景圖
        var image = prefab.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.7f, 0.8f, 0.5f);
        }

        // 設置邊框
        var border = prefab.transform.Find("Border");
        if (border != null)
        {
            var borderRect = border.GetComponent<RectTransform>();
            if (borderRect != null)
            {
                borderRect.anchorMin = Vector2.zero;
                borderRect.anchorMax = Vector2.one;
                borderRect.offsetMin = new Vector2(-4, -4);
                borderRect.offsetMax = new Vector2(4, 4);
            }

            var borderImage = border.GetComponent<Image>();
            if (borderImage != null)
            {
                borderImage.color = new Color(0.5f, 0.5f, 0.5f);
            }

            // 確保邊框在底層
            border.SetAsFirstSibling();
        }

        // 設置節點名稱
        var nodeName = prefab.transform.Find("NodeName");
        if (nodeName != null)
        {
            var nameRect = nodeName.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                nameRect.anchorMin = new Vector2(0, -0.5f);
                nameRect.anchorMax = new Vector2(1, 0);
                nameRect.offsetMin = Vector2.zero;
                nameRect.offsetMax = Vector2.zero;
                nameRect.sizeDelta = new Vector2(100, 20);
                nameRect.anchoredPosition = new Vector2(0, -15);
            }

            var nameText = nodeName.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = "節點名稱";
                nameText.fontSize = 12;
                nameText.color = Color.white;
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.enableWordWrapping = false;
            }
        }

        // 添加 MapNodeUI 腳本
        var nodeUIType = System.Type.GetType("SmallTroopsBigBattles.UI.Map.MapNodeUI, Assembly-CSharp");
        if (nodeUIType != null && prefab.GetComponent(nodeUIType) == null)
        {
            var nodeUI = prefab.AddComponent(nodeUIType);

            // 使用 SerializedObject 設置引用
            var serializedObject = new SerializedObject(nodeUI);

            var nodeImageProp = serializedObject.FindProperty("nodeImage");
            if (nodeImageProp != null) nodeImageProp.objectReferenceValue = image;

            var borderImageProp = serializedObject.FindProperty("borderImage");
            if (borderImageProp != null && border != null)
                borderImageProp.objectReferenceValue = border.GetComponent<Image>();

            var nodeNameTextProp = serializedObject.FindProperty("nodeNameText");
            if (nodeNameTextProp != null && nodeName != null)
                nodeNameTextProp.objectReferenceValue = nodeName.GetComponent<TextMeshProUGUI>();

            serializedObject.ApplyModifiedProperties();
        }

        // 保存為預製體
        var prefabPath = "Assets/_Project/Prefabs/Map/MapNodePrefab.prefab";
        EnsureDirectoryExists("Assets/_Project/Prefabs/Map");
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

        EditorUtility.SetDirty(prefab);
        Debug.Log("MapNodePrefab 設置完成！");
    }

    private static void SetupRouteLinePrefab()
    {
        var prefab = GameObject.Find("RouteLinePrefab");
        if (prefab == null)
        {
            Debug.LogError("找不到 RouteLinePrefab！");
            return;
        }

        // 設置線條尺寸
        var rect = prefab.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(100, 4);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        // 設置線條顏色
        var image = prefab.GetComponent<Image>();
        if (image != null)
        {
            image.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        }

        // 保存為預製體
        var prefabPath = "Assets/_Project/Prefabs/Map/RouteLinePrefab.prefab";
        EnsureDirectoryExists("Assets/_Project/Prefabs/Map");
        PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);

        EditorUtility.SetDirty(prefab);
        Debug.Log("RouteLinePrefab 設置完成！");
    }

    private static void SetupMapView()
    {
        // 在 HUD 旁邊創建地圖視圖容器
        var mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas == null)
        {
            Debug.LogError("找不到 MainCanvas！");
            return;
        }

        // 創建或獲取 MapView
        var mapView = mainCanvas.transform.Find("MapView");
        GameObject mapViewObj;

        if (mapView == null)
        {
            mapViewObj = new GameObject("MapView");
            mapViewObj.transform.SetParent(mainCanvas.transform, false);
            mapViewObj.transform.SetSiblingIndex(0); // 放在最底層
        }
        else
        {
            mapViewObj = mapView.gameObject;
        }

        // 設置 RectTransform
        var mapRect = mapViewObj.GetComponent<RectTransform>();
        if (mapRect == null) mapRect = mapViewObj.AddComponent<RectTransform>();

        mapRect.anchorMin = Vector2.zero;
        mapRect.anchorMax = Vector2.one;
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;

        // 添加背景圖
        var bgImage = mapViewObj.GetComponent<Image>();
        if (bgImage == null) bgImage = mapViewObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.2f, 0.15f, 1f);

        // 創建路徑容器
        var routeContainer = mapViewObj.transform.Find("RouteContainer");
        if (routeContainer == null)
        {
            var routeContainerObj = new GameObject("RouteContainer");
            routeContainerObj.transform.SetParent(mapViewObj.transform, false);
            var routeRect = routeContainerObj.AddComponent<RectTransform>();
            SetStretchAll(routeRect);
        }

        // 創建節點容器
        var nodeContainer = mapViewObj.transform.Find("NodeContainer");
        if (nodeContainer == null)
        {
            var nodeContainerObj = new GameObject("NodeContainer");
            nodeContainerObj.transform.SetParent(mapViewObj.transform, false);
            var nodeRect = nodeContainerObj.AddComponent<RectTransform>();
            SetStretchAll(nodeRect);
        }

        // 添加 MapViewController
        var controllerType = System.Type.GetType("SmallTroopsBigBattles.UI.Map.MapViewController, Assembly-CSharp");
        if (controllerType != null)
        {
            var controller = mapViewObj.GetComponent(controllerType);
            if (controller == null)
            {
                controller = mapViewObj.AddComponent(controllerType);
            }

            // 設置引用
            var serializedObject = new SerializedObject(controller);

            var mapContainerProp = serializedObject.FindProperty("mapContainer");
            if (mapContainerProp != null && nodeContainer != null)
                mapContainerProp.objectReferenceValue = nodeContainer.GetComponent<RectTransform>()
                    ?? mapViewObj.transform.Find("NodeContainer")?.GetComponent<RectTransform>();

            var routeContainerProp = serializedObject.FindProperty("routeContainer");
            if (routeContainerProp != null && routeContainer != null)
                routeContainerProp.objectReferenceValue = routeContainer.GetComponent<RectTransform>()
                    ?? mapViewObj.transform.Find("RouteContainer")?.GetComponent<RectTransform>();

            // 載入預製體引用
            var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Map/MapNodePrefab.prefab");
            var nodeUIPrefabProp = serializedObject.FindProperty("nodeUIPrefab");
            if (nodeUIPrefabProp != null && nodePrefab != null)
                nodeUIPrefabProp.objectReferenceValue = nodePrefab;

            var routePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Map/RouteLinePrefab.prefab");
            var routeLinePrefabProp = serializedObject.FindProperty("routeLinePrefab");
            if (routeLinePrefabProp != null && routePrefab != null)
                routeLinePrefabProp.objectReferenceValue = routePrefab;

            serializedObject.ApplyModifiedProperties();
        }

        EditorUtility.SetDirty(mapViewObj);
        Debug.Log("MapView 設置完成！");
    }

    private static void SetStretchAll(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            var folderName = System.IO.Path.GetFileName(path);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureDirectoryExists(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}

