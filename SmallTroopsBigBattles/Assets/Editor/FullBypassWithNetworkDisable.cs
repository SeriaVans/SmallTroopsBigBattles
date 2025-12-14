// ============================================================
// 🔴 完整繞過 + 禁用網絡驗證
// 
// 這模擬攻擊者會做的完整攻擊
// ============================================================

using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace UnityMCP.SecurityLab
{
    public static class FullBypassWithNetworkDisable
    {
        private static BindingFlags _flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        private static Type _licenseType;
        private static Type _licenseInfoType;
        
        [MenuItem("Tools/Silent Bypass Test/🔴🔴 完整繞過 + 禁用網絡驗證")]
        public static void FullBypass()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("🔴 完整繞過 + 禁用網絡驗證");
            Debug.Log("════════════════════════════════════════");
            
            if (!FindTypes())
            {
                Debug.LogError("找不到必要的類型");
                return;
            }
            
            // 步驟 1: 修改許可證狀態
            Debug.Log("");
            Debug.Log("【步驟 1】設置許可證狀態");
            SetLicenseState();
            
            // 步驟 2: 創建假的許可證對象
            Debug.Log("");
            Debug.Log("【步驟 2】創建假許可證對象");
            CreateFakeLicense();
            
            // 步驟 3: 設置離線寬限期（讓系統以為剛剛驗證過）
            Debug.Log("");
            Debug.Log("【步驟 3】偽造最後驗證時間");
            FakeLastValidationTime();
            
            // 步驟 4: 禁用定期檢查
            Debug.Log("");
            Debug.Log("【步驟 4】禁用定期驗證");
            DisablePeriodicCheck();
            
            // 驗證
            Debug.Log("");
            Debug.Log("【驗證結果】");
            VerifyBypass();
        }
        
        [MenuItem("Tools/Silent Bypass Test/單獨：偽造最後驗證時間")]
        public static void FakeValidationTimeOnly()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("偽造最後驗證時間");
            Debug.Log("════════════════════════════════════════");
            
            if (!FindTypes()) return;
            FakeLastValidationTime();
        }
        
        [MenuItem("Tools/Silent Bypass Test/單獨：禁用定期驗證")]
        public static void DisablePeriodicCheckOnly()
        {
            Debug.Log("════════════════════════════════════════");
            Debug.Log("禁用定期驗證");
            Debug.Log("════════════════════════════════════════");
            
            if (!FindTypes()) return;
            DisablePeriodicCheck();
        }
        
        private static bool FindTypes()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name != "UnityMCP") continue;
                
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "LicenseManagerV2")
                        _licenseType = type;
                    if (type.FullName == "UnityMCP.Editor.License.LicenseManagerV2+LicenseInfo")
                        _licenseInfoType = type;
                }
            }
            
            return _licenseType != null;
        }
        
        private static void SetLicenseState()
        {
            SetStaticField("_isLicensed", true);
            SetStaticField("_isInitialized", true);
            SetStaticField("_activationLimitReached", false);
        }
        
        private static void CreateFakeLicense()
        {
            if (_licenseInfoType == null)
            {
                Debug.LogWarning("  找不到 LicenseInfo 類型");
                return;
            }
            
            try
            {
                var fakeLicense = Activator.CreateInstance(_licenseInfoType);
                
                // 設置所有字段
                foreach (var field in _licenseInfoType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    try
                    {
                        if (field.FieldType == typeof(string))
                        {
                            string value = "FAKE_" + field.Name;
                            if (field.Name.ToLower().Contains("key")) value = "FAKE-KEY-12345";
                            if (field.Name.ToLower().Contains("email")) value = "fake@test.com";
                            if (field.Name.ToLower().Contains("type")) value = "Professional";
                            field.SetValue(fakeLicense, value);
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            field.SetValue(fakeLicense, true);
                        }
                        else if (field.FieldType == typeof(DateTime))
                        {
                            field.SetValue(fakeLicense, DateTime.UtcNow.AddYears(100));
                        }
                        else if (field.FieldType == typeof(int))
                        {
                            field.SetValue(fakeLicense, 999);
                        }
                    }
                    catch { }
                }
                
                // 設置屬性
                foreach (var prop in _licenseInfoType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanWrite) continue;
                    try
                    {
                        if (prop.PropertyType == typeof(bool))
                            prop.SetValue(fakeLicense, true);
                        else if (prop.PropertyType == typeof(string))
                            prop.SetValue(fakeLicense, "FAKE");
                    }
                    catch { }
                }
                
                SetStaticField("_currentLicense", fakeLicense);
                Debug.Log("  ✓ 假許可證已創建並設置");
            }
            catch (Exception ex)
            {
                Debug.LogError($"  ❌ {ex.Message}");
            }
        }
        
        private static void FakeLastValidationTime()
        {
            // 設置最後檢查時間為現在（讓系統以為剛剛驗證過）
            SetStaticField("_lastCheck", DateTime.UtcNow);
            
            // 嘗試找到 OnlineAuthManager 的緩存驗證
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name != "UnityMCP") continue;
                
                foreach (var type in asm.GetTypes())
                {
                    if (type.Name == "OnlineAuthManager")
                    {
                        Debug.Log("  找到 OnlineAuthManager");
                        
                        // 查找緩存相關字段
                        foreach (var field in type.GetFields(_flags))
                        {
                            if (field.Name.ToLower().Contains("cache") || 
                                field.Name.ToLower().Contains("validation") ||
                                field.Name.ToLower().Contains("last"))
                            {
                                Debug.Log($"    字段: {field.Name} ({field.FieldType.Name})");
                                
                                // 如果是 CachedValidation 類型
                                if (field.FieldType.Name == "CachedValidation")
                                {
                                    try
                                    {
                                        var cached = Activator.CreateInstance(field.FieldType);
                                        
                                        // 設置緩存為有效
                                        foreach (var f in field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                                        {
                                            if (f.FieldType == typeof(bool))
                                                f.SetValue(cached, true);
                                            else if (f.FieldType == typeof(DateTime))
                                                f.SetValue(cached, DateTime.UtcNow);
                                            else if (f.FieldType == typeof(string))
                                                f.SetValue(cached, "Professional");
                                        }
                                        
                                        foreach (var p in field.FieldType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                        {
                                            if (!p.CanWrite) continue;
                                            if (p.PropertyType == typeof(bool))
                                                p.SetValue(cached, true);
                                            else if (p.PropertyType == typeof(DateTime))
                                                p.SetValue(cached, DateTime.UtcNow);
                                        }
                                        
                                        field.SetValue(null, cached);
                                        Debug.Log($"    ✓ 設置假的緩存驗證結果");
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Log($"    ❌ {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private static void DisablePeriodicCheck()
        {
            // 方法 1: 設置檢查間隔為很大的值
            var intervalField = _licenseType.GetField("CHECK_INTERVAL_MINUTES", _flags);
            if (intervalField != null)
            {
                try
                {
                    intervalField.SetValue(null, int.MaxValue);
                    Debug.Log("  ✓ CHECK_INTERVAL_MINUTES = MaxValue");
                }
                catch (Exception ex)
                {
                    Debug.Log($"  ❌ 無法修改間隔: {ex.Message}");
                }
            }
            
            var fetchIntervalField = _licenseType.GetField("FETCH_LICENSE_INTERVAL_MINUTES", _flags);
            if (fetchIntervalField != null)
            {
                try
                {
                    fetchIntervalField.SetValue(null, int.MaxValue);
                    Debug.Log("  ✓ FETCH_LICENSE_INTERVAL_MINUTES = MaxValue");
                }
                catch (Exception ex)
                {
                    Debug.Log($"  ❌ 無法修改間隔: {ex.Message}");
                }
            }
            
            // 方法 2: 設置最後檢查時間為未來
            SetStaticField("_lastCheck", DateTime.UtcNow.AddYears(100));
            Debug.Log("  ✓ _lastCheck = 100年後（永遠不會觸發檢查）");
            
            Debug.Log("");
            Debug.Log("  ⚠️ 注意：這只能防止自動檢查");
            Debug.Log("  ⚠️ 如果用戶手動觸發驗證，仍會發送請求");
            Debug.Log("  ⚠️ 完整繞過需要使用 Harmony Hook ValidateLicenseAsync");
        }
        
        private static void VerifyBypass()
        {
            var isLicensed = _licenseType.GetField("_isLicensed", _flags)?.GetValue(null);
            var currentLicense = _licenseType.GetField("_currentLicense", _flags)?.GetValue(null);
            var lastCheck = _licenseType.GetField("_lastCheck", _flags)?.GetValue(null);
            
            Debug.Log($"  _isLicensed = {isLicensed}");
            Debug.Log($"  _currentLicense = {(currentLicense != null ? "已設置" : "null")}");
            Debug.Log($"  _lastCheck = {lastCheck}");
            
            if (isLicensed?.ToString() == "True" && currentLicense != null)
            {
                Debug.Log("");
                Debug.LogError("🔴🔴🔴 繞過成功！🔴🔴🔴");
                Debug.Log("");
                Debug.Log("這個繞過：");
                Debug.Log("  1. ✓ 修改了許可證狀態");
                Debug.Log("  2. ✓ 創建了假的許可證對象");
                Debug.Log("  3. ✓ 禁用了定期驗證");
                Debug.Log("");
                Debug.Log("服務器察覺：");
                Debug.Log("  ❌ 沒有請求發出（定期驗證被禁用）");
                Debug.Log("  ❌ 服務器完全不知道有人在使用");
            }
        }
        
        private static void SetStaticField(string fieldName, object value)
        {
            var field = _licenseType.GetField(fieldName, _flags);
            if (field != null)
            {
                try
                {
                    var oldVal = field.GetValue(null);
                    field.SetValue(null, value);
                    Debug.Log($"  ✓ {fieldName}: {oldVal} → {value}");
                }
                catch (Exception ex)
                {
                    Debug.Log($"  ❌ {fieldName}: {ex.Message}");
                }
            }
        }
    }
}
