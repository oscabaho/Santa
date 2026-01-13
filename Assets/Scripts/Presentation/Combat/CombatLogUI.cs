using Santa.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Santa.Domain.Combat;

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
        }

        private void OnEnable()
        {
            if (_combatLogService != null)
            {
                _combatLogService.OnMessageLogged += HandleMessageLogged;
            }

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
