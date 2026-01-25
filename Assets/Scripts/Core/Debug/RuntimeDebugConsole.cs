using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Santa.Core.Debug
{
    /// <summary>
    /// A simple runtime console to display logs on the screen on mobile builds.
    /// Useful for debugging input/logic chains when ADB is not available.
    /// </summary>
    public class RuntimeDebugConsole : MonoBehaviour
    {
        private TextMeshProUGUI _textMesh;
        private readonly Queue<string> _logs = new Queue<string>();
        private const int MaxLines = 15;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            CreateUI();
            Application.logMessageReceived += HandleLog;
            Log("Debug Console Initialized");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string color = "white";
            if (type == LogType.Error || type == LogType.Exception) color = "red";
            if (type == LogType.Warning) color = "yellow";

            string formatted = $"<color={color}>{logString}</color>";
            
            _logs.Enqueue(formatted);
            if (_logs.Count > MaxLines) _logs.Dequeue();

            UpdateText();
        }

        private void Log(string msg)
        {
            HandleLog(msg, "", LogType.Log);
        }

        private void UpdateText()
        {
            if (_textMesh != null)
            {
                _textMesh.text = string.Join("\n", _logs);
            }
        }

        private void CreateUI()
        {
            // Create Canvas
            var canvasGo = new GameObject("DebugCanvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Topmost
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create Text
            var textGo = new GameObject("DebugText");
            textGo.transform.SetParent(canvasGo.transform, false);
            
            _textMesh = textGo.AddComponent<TextMeshProUGUI>();
            _textMesh.fontSize = 20; // Readable on mobile
            _textMesh.alignment = TextAlignmentOptions.BottomLeft;
            _textMesh.color = Color.white;
            _textMesh.raycastTarget = false;

            // Anchor to full screen with padding
            var rect = _textMesh.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0, 0);
            rect.offsetMin = new Vector2(20, 20);
            rect.offsetMax = new Vector2(-20, -20);
        }
    }
}
