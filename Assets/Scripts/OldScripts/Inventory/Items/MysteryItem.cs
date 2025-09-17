using UnityEngine;

/// <summary>
/// Representa un ítem misterioso en el inventario. Su tipo real se revela solo al usarlo.
/// </summary>
[CreateAssetMenu(fileName = "MysteryItem", menuName = "Inventory/MysteryItem")]
public class MysteryItem : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private string description; // Descripción para tooltip
    [SerializeField] private Sprite icon;
    // Puedes agregar más campos visuales si lo deseas

    public string Id => id;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
}
