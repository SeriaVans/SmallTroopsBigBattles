using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using SmallTroopsBigBattles.UI;

/// <summary>
/// GameHUD 自動連接引用編輯器
/// </summary>
public class GameHUDSetupEditor
{
    [MenuItem("Tools/SLG Game/Auto Connect GameHUD References")]
    public static void AutoConnectGameHUDReferences()
    {
        var hud = GameObject.Find("MainCanvas/HUD");
        if (hud == null)
        {
            Debug.LogError("找不到 MainCanvas/HUD！");
            return;
        }

        var gameHUD = hud.GetComponent<GameHUD>();
        if (gameHUD == null)
        {
            Debug.LogError("HUD 上沒有 GameHUD 組件！");
            return;
        }

        // 使用反射來設置私有字段
        var type = typeof(GameHUD);
        var serializedObject = new SerializedObject(gameHUD);

        // 連接玩家資訊
        ConnectReference(serializedObject, "playerNameText", "MainCanvas/HUD/PlayerInfoPanel/PlayerNameText");
        ConnectReference(serializedObject, "playerLevelText", "MainCanvas/HUD/PlayerInfoPanel/PlayerLevelText");

        // 連接資源顯示
        ConnectReference(serializedObject, "copperText", "MainCanvas/HUD/TopResourceBar/CopperText");
        ConnectReference(serializedObject, "woodText", "MainCanvas/HUD/TopResourceBar/WoodText");
        ConnectReference(serializedObject, "stoneText", "MainCanvas/HUD/TopResourceBar/StoneText");
        ConnectReference(serializedObject, "foodText", "MainCanvas/HUD/TopResourceBar/FoodText");
        ConnectReference(serializedObject, "soldierCountText", "MainCanvas/HUD/TopResourceBar/SoldierCountText");

        // 連接按鈕
        ConnectReference(serializedObject, "territoryButton", "MainCanvas/HUD/BottomButtonBar/TerritoryButton");
        ConnectReference(serializedObject, "armyButton", "MainCanvas/HUD/BottomButtonBar/ArmyButton");
        ConnectReference(serializedObject, "generalButton", "MainCanvas/HUD/BottomButtonBar/GeneralButton");
        ConnectReference(serializedObject, "mapButton", "MainCanvas/HUD/BottomButtonBar/MapButton");
        ConnectReference(serializedObject, "settingsButton", "MainCanvas/HUD/BottomButtonBar/SettingsButton");

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameHUD);

        Debug.Log("GameHUD 引用連接完成！");
    }

    private static void ConnectReference(SerializedObject serializedObject, string propertyName, string path)
    {
        var property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"找不到屬性: {propertyName}");
            return;
        }

        var obj = GameObject.Find(path);
        if (obj == null)
        {
            Debug.LogWarning($"找不到物件: {path}");
            return;
        }

        // 根據類型設置引用
        if (propertyName.Contains("Text"))
        {
            var component = obj.GetComponent<TextMeshProUGUI>();
            if (component != null)
            {
                property.objectReferenceValue = component;
                Debug.Log($"✓ 連接 {propertyName} -> {path}");
            }
        }
        else if (propertyName.Contains("Button"))
        {
            var component = obj.GetComponent<Button>();
            if (component != null)
            {
                property.objectReferenceValue = component;
                Debug.Log($"✓ 連接 {propertyName} -> {path}");
            }
        }
    }
}

