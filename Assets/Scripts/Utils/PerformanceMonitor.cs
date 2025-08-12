using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DiceGame.Utils
{
    /// <summary>
    /// Monitors game performance metrics for iOS optimization
    /// Tracks FPS, memory usage, and provides automatic quality adjustments
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        [Header("Monitoring Settings")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1f;
        [SerializeField] private int sampleSize = 60;
        [SerializeField] private bool logToConsole = false;
        
        [Header("Performance Targets")]
        [SerializeField] private int targetFPS = 60;
        [SerializeField] private int fallbackFPS = 30;
        [SerializeField] private long maxMemoryMB = 100;
        
        [Header("Auto Quality Adjustment")]
        [SerializeField] private bool enableAutoQuality = true;
        [SerializeField] private float qualityAdjustmentThreshold = 0.8f;
        [SerializeField] private int qualityCheckDuration = 5;
        
        [Header("Debug Display")]
        [SerializeField] private bool showDebugUI = false;
        [SerializeField] private Rect debugUIRect = new Rect(10, 10, 200, 100);
        
        // Performance metrics
        private Queue<float> fpsHistory = new Queue<float>();
        private Queue<long> memoryHistory = new Queue<long>();
        
        private float currentFPS = 0f;
        private long currentMemoryMB = 0L;
        private float averageFPS = 0f;
        private float minFPS = 0f;
        private float maxFPS = 0f;
        
        // Quality management
        private int currentQualityLevel = -1;
        private float qualityAdjustmentTimer = 0f;
        private bool hasAdjustedQuality = false;
        
        // Device information
        private string deviceModel = "";
        private int deviceMemoryMB = 0;
        private int processorCount = 0;
        
        public float CurrentFPS => currentFPS;
        public long CurrentMemoryMB => currentMemoryMB;
        public float AverageFPS => averageFPS;
        public bool IsPerformingWell => averageFPS >= (targetFPS * qualityAdjustmentThreshold);
        
        private void Awake()
        {
            GatherDeviceInfo();
            InitializeQualitySettings();
        }
        
        private void Start()
        {
            if (enableMonitoring)
            {
                InvokeRepeating(nameof(UpdateMetrics), 0f, updateInterval);
            }
        }
        
        private void Update()
        {
            if (enableAutoQuality)
            {
                qualityAdjustmentTimer += Time.deltaTime;
                
                if (qualityAdjustmentTimer >= qualityCheckDuration)
                {
                    CheckAndAdjustQuality();
                    qualityAdjustmentTimer = 0f;
                }
            }
        }
        
        private void GatherDeviceInfo()
        {
            deviceModel = SystemInfo.deviceModel;
            deviceMemoryMB = SystemInfo.systemMemorySize;
            processorCount = SystemInfo.processorCount;
            
            if (logToConsole)
            {
                Debug.Log($"Device Info - Model: {deviceModel}, RAM: {deviceMemoryMB}MB, Cores: {processorCount}");
            }
        }
        
        private void InitializeQualitySettings()
        {
            currentQualityLevel = QualitySettings.GetQualityLevel();
            
            // Set initial target frame rate based on device capabilities
            if (IsLowEndDevice())
            {
                Application.targetFrameRate = fallbackFPS;
                if (logToConsole)
                {
                    Debug.Log($"Low-end device detected. Setting target FPS to {fallbackFPS}");
                }
            }
            else
            {
                Application.targetFrameRate = targetFPS;
            }
        }
        
        private void UpdateMetrics()
        {
            if (!enableMonitoring) return;
            
            // Calculate FPS
            currentFPS = 1.0f / Time.unscaledDeltaTime;
            
            // Get memory usage
            currentMemoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
            
            // Update history
            UpdateFPSHistory(currentFPS);
            UpdateMemoryHistory(currentMemoryMB);
            
            // Calculate statistics
            CalculateStatistics();
            
            // Log if enabled
            if (logToConsole)
            {
                LogMetrics();
            }
        }
        
        private void UpdateFPSHistory(float fps)
        {
            fpsHistory.Enqueue(fps);
            
            if (fpsHistory.Count > sampleSize)
            {
                fpsHistory.Dequeue();
            }
        }
        
        private void UpdateMemoryHistory(long memory)
        {
            memoryHistory.Enqueue(memory);
            
            if (memoryHistory.Count > sampleSize)
            {
                memoryHistory.Dequeue();
            }
        }
        
        private void CalculateStatistics()
        {
            if (fpsHistory.Count == 0) return;
            
            var fpsArray = fpsHistory.ToArray();
            averageFPS = fpsArray.Average();
            minFPS = fpsArray.Min();
            maxFPS = fpsArray.Max();
        }
        
        private void CheckAndAdjustQuality()
        {
            if (!enableAutoQuality || hasAdjustedQuality) return;
            
            bool performanceIssue = averageFPS < (targetFPS * qualityAdjustmentThreshold);
            bool memoryIssue = currentMemoryMB > maxMemoryMB;
            
            if (performanceIssue || memoryIssue)
            {
                ReduceQuality();
                hasAdjustedQuality = true;
                
                if (logToConsole)
                {
                    Debug.Log($"Performance issue detected. FPS: {averageFPS:F1}, Memory: {currentMemoryMB}MB. Reducing quality.");
                }
            }
        }
        
        private void ReduceQuality()
        {
            // Try different optimization strategies
            
            // 1. Reduce physics quality
            if (Time.fixedDeltaTime < 0.02f)
            {
                Time.fixedDeltaTime = 0.02f; // 50Hz physics
            }
            
            // 2. Reduce shadow quality
            QualitySettings.shadows = ShadowQuality.Disable;
            
            // 3. Reduce render scale
            if (QualitySettings.renderPipeline != null)
            {
                // URP specific optimizations would go here
            }
            
            // 4. Lower frame rate target
            if (Application.targetFrameRate > fallbackFPS)
            {
                Application.targetFrameRate = fallbackFPS;
            }
            
            // 5. Force garbage collection
            System.GC.Collect();
        }
        
        private bool IsLowEndDevice()
        {
            // Heuristics for detecting low-end iOS devices
            bool lowMemory = deviceMemoryMB < 2048; // Less than 2GB RAM
            bool fewCores = processorCount < 4;
            bool oldDevice = deviceModel.Contains("iPhone6") || deviceModel.Contains("iPhone SE");
            
            return lowMemory || fewCores || oldDevice;
        }
        
        private void LogMetrics()
        {
            Debug.Log($"Performance - FPS: {currentFPS:F1} (Avg: {averageFPS:F1}), Memory: {currentMemoryMB}MB, Quality: {QualitySettings.GetQualityLevel()}");
        }
        
        public void SetTargetFPS(int fps)
        {
            targetFPS = Mathf.Clamp(fps, 30, 120);
            Application.targetFrameRate = targetFPS;
        }
        
        public void SetMemoryLimit(long memoryMB)
        {
            maxMemoryMB = Mathf.Max(50, memoryMB);
        }
        
        public void ForceQualityReduction()
        {
            ReduceQuality();
            hasAdjustedQuality = true;
        }
        
        public void ResetQualityAdjustment()
        {
            hasAdjustedQuality = false;
            qualityAdjustmentTimer = 0f;
        }
        
        public PerformanceData GetPerformanceData()
        {
            return new PerformanceData
            {
                currentFPS = currentFPS,
                averageFPS = averageFPS,
                minFPS = minFPS,
                maxFPS = maxFPS,
                currentMemoryMB = currentMemoryMB,
                deviceModel = deviceModel,
                deviceMemoryMB = deviceMemoryMB,
                qualityLevel = currentQualityLevel
            };
        }
        
        private void OnGUI()
        {
            if (!showDebugUI) return;
            
            GUI.Box(debugUIRect, "Performance Monitor");
            
            GUILayout.BeginArea(new Rect(debugUIRect.x + 5, debugUIRect.y + 20, debugUIRect.width - 10, debugUIRect.height - 25));
            
            GUILayout.Label($"FPS: {currentFPS:F1} (Avg: {averageFPS:F1})");
            GUILayout.Label($"Memory: {currentMemoryMB}MB");
            GUILayout.Label($"Device: {deviceModel}");
            GUILayout.Label($"Quality: {QualitySettings.GetQualityLevel()}");
            
            if (GUILayout.Button("Force GC"))
            {
                System.GC.Collect();
            }
            
            if (GUILayout.Button("Reduce Quality"))
            {
                ForceQualityReduction();
            }
            
            GUILayout.EndArea();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && enableMonitoring)
            {
                // Clear history when resuming to get fresh metrics
                fpsHistory.Clear();
                memoryHistory.Clear();
            }
        }
        
        private void OnValidate()
        {
            updateInterval = Mathf.Max(0.1f, updateInterval);
            sampleSize = Mathf.Clamp(sampleSize, 10, 300);
            targetFPS = Mathf.Clamp(targetFPS, 30, 120);
            fallbackFPS = Mathf.Clamp(fallbackFPS, 15, targetFPS);
            qualityAdjustmentThreshold = Mathf.Clamp01(qualityAdjustmentThreshold);
            qualityCheckDuration = Mathf.Max(1, qualityCheckDuration);
        }
        
        [System.Serializable]
        public struct PerformanceData
        {
            public float currentFPS;
            public float averageFPS;
            public float minFPS;
            public float maxFPS;
            public long currentMemoryMB;
            public string deviceModel;
            public int deviceMemoryMB;
            public int qualityLevel;
        }
    }
}