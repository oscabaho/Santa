/// <summary>
/// Interface for stat controllers (e.g., health, stamina, etc.)
/// </summary>
public interface IStatController
{
    int CurrentValue { get; }
    int MaxValue { get; }
    event System.Action<int, int> OnValueChanged;
    void AffectValue(int value);
    void SetValue(int value);
}
