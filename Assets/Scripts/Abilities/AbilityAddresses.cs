/// <summary>
/// Centralized addresses for Ability ScriptableObjects loaded via Addressables.
/// Prevents hardcoded strings and provides compile-time safety.
/// </summary>
public static class AbilityAddresses
{
    // Player Abilities
    public const string Direct = "Direct";
    public const string Area = "Area";
    public const string Special = "Special";
    public const string GainAP = "GainAP";
    
    // Targeting Strategies
    public const string SingleEnemyTargeting = "SingleEnemyTargeting";
}
