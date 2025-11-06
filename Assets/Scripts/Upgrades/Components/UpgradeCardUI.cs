using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Representa una tarjeta individual de upgrade en la UI.
/// Puede ser reutilizada para mostrar diferentes upgrades.
/// Incluye efectos visuales de hover y selección.
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
    private Coroutine _scaleCoroutine;

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
    /// Configura la tarjeta con los datos del upgrade.
    /// </summary>
    public void Setup(AbilityUpgrade upgrade)
    {
        _currentUpgrade = upgrade;

        if (upgradeNameText != null)
            upgradeNameText.text = upgrade.UpgradeName;

        if (upgradeDescriptionText != null)
            upgradeDescriptionText.text = upgrade.UpgradeDescription;

        // Si tienes un sistema de iconos, descoméntalo:
        // if (upgradeIcon != null && upgrade.Icon != null)
        //     upgradeIcon.sprite = upgrade.Icon;
    }

    /// <summary>
    /// Limpia la tarjeta cuando no se use.
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
    /// Habilita o deshabilita la interacción de la tarjeta.
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (selectButton != null)
            selectButton.interactable = interactable;
    }

    // Implementación de IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardBackground != null)
            _cardBackground.color = hoverColor;

        // Animación de escala al hacer hover
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        
        _scaleCoroutine = StartCoroutine(AnimateScale(_originalScale * hoverScale));
    }

    // Implementación de IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_cardBackground != null)
            _cardBackground.color = normalColor;

        // Volver a la escala original
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        
        _scaleCoroutine = StartCoroutine(AnimateScale(_originalScale));
    }

    /// <summary>
    /// Anima suavemente el cambio de escala.
    /// </summary>
    private System.Collections.IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Ease out cubic para una animación suave
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
