using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Santa.Domain.Combat;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Santa.Presentation.Upgrades
{

/// <summary>
/// Represents an individual upgrade card in the UI.
/// Can be reused to display different upgrades.
/// Includes visual hover and selection effects.
/// </summary>
public class UpgradeCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeDescriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image upgradeIcon;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.24f, 0.24f, 0.24f);
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationDuration = 0.2f;

    private AbilityUpgrade _currentUpgrade;
    private Image _cardBackground;
    private Vector3 _originalScale;
    private CancellationTokenSource _scaleCTS;

    public event Action<AbilityUpgrade> OnUpgradeSelected;

    private void Awake()
    {
        _cardBackground = GetComponent<Image>();
        _originalScale = transform.localScale;

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnSelectButtonClicked);
        }
    }

    /// <summary>
    /// Configures the card with the upgrade's data.
    /// </summary>
    public void Setup(AbilityUpgrade upgrade)
    {
        _currentUpgrade = upgrade;

        if (upgradeNameText != null)
            upgradeNameText.text = upgrade.UpgradeName;

        if (upgradeDescriptionText != null)
            upgradeDescriptionText.text = upgrade.UpgradeDescription;

        // If you have an icon system, uncomment below:
        // if (upgradeIcon != null && upgrade.Icon != null)
        //     upgradeIcon.sprite = upgrade.Icon;
    }

    /// <summary>
    /// Clears the card when not used.
    /// </summary>
    public void Clear()
    {
        _currentUpgrade = null;
        if (upgradeNameText != null) upgradeNameText.text = "";
        if (upgradeDescriptionText != null) upgradeDescriptionText.text = "";
        if (upgradeIcon != null) upgradeIcon.sprite = null;
    }

    private void OnSelectButtonClicked()
    {
        if (_currentUpgrade != null)
        {
            OnUpgradeSelected?.Invoke(_currentUpgrade);
        }
    }

    /// <summary>
    /// Enables or disables card interaction.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (selectButton != null)
            selectButton.interactable = interactable;
    }

    // IPointerEnterHandler implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardBackground != null)
            _cardBackground.color = hoverColor;

        // Scale animation on hover
        _scaleCTS?.Cancel();
        _scaleCTS = new CancellationTokenSource();
        AnimateScale(_originalScale * hoverScale, _scaleCTS.Token).Forget();
    }

    // IPointerExitHandler implementation
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_cardBackground != null)
            _cardBackground.color = normalColor;

        // Return to original scale
        _scaleCTS?.Cancel();
        _scaleCTS = new CancellationTokenSource();
        AnimateScale(_originalScale, _scaleCTS.Token).Forget();
    }

    /// <summary>
    /// Smoothly animates scale change.
    /// </summary>
    private async UniTaskVoid AnimateScale(Vector3 targetScale, CancellationToken token)
    {
        try
        {
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                if (token.IsCancellationRequested) return;

                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / animationDuration;

                // Ease-out cubic for a smooth animation
                t = 1f - Mathf.Pow(1f - t, 3f);

                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            transform.localScale = targetScale;
        }
        catch (System.OperationCanceledException)
        {
            // Expected during scene transitions
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"UpgradeCardUI.AnimateScale: Exception: {ex.Message}");
            GameLog.LogException(ex);
            // Reset scale to target
            if (this != null && transform != null)
                transform.localScale = targetScale;
        }
    }
}
}
