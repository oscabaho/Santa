using System.Collections.Generic;
using Santa.Core;
using Santa.Domain.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Santa.Presentation.Combat
{
    /// <summary>
    /// Displays combat log messages in a scrolling UI panel.
    /// </summary>
    public class CombatLogUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentTransform;
        [SerializeField] private GameObject logEntryPrefab;

        [Header("Settings")]
        [SerializeField] private int maxMessages = 50;
        [SerializeField] private bool autoScroll = true;

        [Header("Colors")]
        [SerializeField] private Color infoColor = Color.white;
        [SerializeField] private Color damageColor = Color.yellow;
        [SerializeField] private Color criticalColor = new Color(1f, 0.4f, 0f); // Orange
        [SerializeField] private Color missColor = Color.cyan;
        [SerializeField] private Color deathColor = new Color(0.8f, 0f, 0f); // Dark red
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color apColor = new Color(0.5f, 0.5f, 1f); // Light blue

        private ICombatLogService _combatLogService;
        private ICombatService _combatService;
        private readonly Queue<GameObject> _logEntries = new Queue<GameObject>();

        [Inject]
        public void Construct(ICombatLogService combatLogService, ICombatService combatService)
        {
            _combatLogService = combatLogService;
            _combatService = combatService;

            // If we're already active, we missed the OnEnable subscription (or it failed due to null deps),
            // so we should subscribe now.
            if (isActiveAndEnabled)
            {
                SubscribeToCombatLog();
            }
        }

        private void SubscribeToCombatLog()
        {
            if (_combatLogService != null)
            {
                // Unsubscribe first just in case to avoid duplicates
                _combatLogService.OnMessageLogged -= HandleMessageLogged;
                _combatLogService.OnMessageLogged += HandleMessageLogged;

                // Load recent logs to fail-safe against race conditions
                // (e.g., if messages were logged before we subscribed)
                foreach (var entry in _combatLogService.GetRecentLogs())
                {
                    // Optionally check duplicates or just re-add. 
                    // Since Unity destroys old UI entries, re-adding is usually fine 
                    // IF we treat this as a "fill initial state" step.
                    // However, to avoid duplicating if we are re-enabling, we might check count.
                    // For simplicity in this fix, we'll assume this runs once on valid connection.
                    // Ideally, we clear and refill or check existence.
                    // But effectively, _combatLogService buffer is truth.

                    // Simple approach: Only do this if our UI is empty or check logic.
                    // Better approach for "late join": relying on the service buffer.
                    // But let's just append. Since 'Subscribe' happens on Construct/Enable, 
                    // ensuring we don't double add if we already heard the event is tricky without ID.
                    // Given the bug is "no logs at start", we prioritize filling.
                    // We'll rely on the fact that we just subscribed.
                }

                // Actually, a safer way to avoid duplicates is to Clear local UI first if we are doing a full sync,
                // OR just blindly add if we assume we missed everything.
                // Since this fixes "startup" race conditions, the UI is likely empty.
                if (_logEntries.Count == 0)
                {
                    foreach (var entry in _combatLogService.GetRecentLogs())
                    {
                        AddLogEntry(entry.Message, entry.Type);
                    }
                }
            }
        }

        private void OnEnable()
        {
            SubscribeToCombatLog();


            if (_combatService != null)
            {
                _combatService.OnPhaseChanged += HandlePhaseChanged;
            }
        }

        private void OnDisable()
        {
            if (_combatLogService != null)
            {
                _combatLogService.OnMessageLogged -= HandleMessageLogged;
            }

            if (_combatService != null)
            {
                _combatService.OnPhaseChanged -= HandlePhaseChanged;
            }
        }

        private void HandleMessageLogged(string message, CombatLogType type)
        {
            AddLogEntry(message, type);
        }

        private void AddLogEntry(string message, CombatLogType type)
        {
            if (logEntryPrefab == null || contentTransform == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("CombatLogUI: LogEntryPrefab or ContentTransform not assigned.");
#endif
                return;
            }

            // Instantiate new log entry
            GameObject entry = Instantiate(logEntryPrefab, contentTransform);
            TextMeshProUGUI textComponent = entry.GetComponentInChildren<TextMeshProUGUI>();

            if (textComponent != null)
            {
                textComponent.text = message;
                textComponent.color = GetColorForType(type);
            }

            _logEntries.Enqueue(entry);

            // Limit message count
            while (_logEntries.Count > maxMessages)
            {
                GameObject oldEntry = _logEntries.Dequeue();
                if (oldEntry != null)
                {
                    Destroy(oldEntry);
                }
            }

            // Auto-scroll to bottom
            if (autoScroll && scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private Color GetColorForType(CombatLogType type)
        {
            return type switch
            {
                CombatLogType.Info => infoColor,
                CombatLogType.Damage => damageColor,
                CombatLogType.Critical => criticalColor,
                CombatLogType.Miss => missColor,
                CombatLogType.Death => deathColor,
                CombatLogType.Heal => healColor,
                CombatLogType.ActionPoints => apColor,
                _ => infoColor
            };
        }

        /// <summary>
        /// Clear all log entries (useful when starting new combat).
        /// </summary>
        public void ClearLog()
        {
            while (_logEntries.Count > 0)
            {
                GameObject entry = _logEntries.Dequeue();
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
        }

        private void HandlePhaseChanged(CombatPhase newPhase)
        {
            // Clear log at the start of each new turn (Selection phase)
            if (newPhase == CombatPhase.Selection)
            {
                ClearLog();
            }
        }
    }
}
