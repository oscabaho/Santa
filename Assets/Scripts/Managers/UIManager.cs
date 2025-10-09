using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour, IUIManager
{
    private static UIManager Instance { get; set; }

    private Dictionary<string, UIPanel> _panels;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _panels = GetComponentsInChildren<UIPanel>(true)
            .ToDictionary(panel => panel.PanelId, panel => panel);

        ServiceLocator.Register<IUIManager>(this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            ServiceLocator.Unregister<IUIManager>();
            Instance = null;
        }
    }

    public void ShowPanel(string panelId)
    {
        if (_panels.TryGetValue(panelId, out var panel))
        {
            panel.Show();
        }
        else
        {
            GameLog.LogWarning($"UIManager: Panel with ID '{panelId}' not found.");
        }
    }

    public void HidePanel(string panelId)
    {
        if (_panels.TryGetValue(panelId, out var panel))
        {
            panel.Hide();
        }
        else
        {
            GameLog.LogWarning($"UIManager: Panel with ID '{panelId}' not found.");
        }
    }

    public void SwitchToPanel(string panelId)
    {
        if (!_panels.ContainsKey(panelId))
        {
            GameLog.LogWarning($"UIManager: Panel with ID '{panelId}' not found.");
            return;
        }

        foreach (var panel in _panels.Values)
        {
            if (panel.PanelId == panelId)
            {
                panel.Show();
            }
            else
            {
                panel.Hide();
            }
        }
    }
}