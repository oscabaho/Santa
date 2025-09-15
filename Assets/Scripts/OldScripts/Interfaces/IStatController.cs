namespace ProyectSecret.Interfaces
{
    /// <summary>
    /// Interface for stat controllers (e.g., health, stamina, etc.)
    /// </summary>
    public interface IStatController
    {
        int CurrentValue { get; }
        int MaxValue { get; }
        void AffectValue(int value);
    }
}
