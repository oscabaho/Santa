namespace Santa.Core.Save
{
    // Provides a stable unique ID for scene objects (enemies, decor, etc.)
    public interface IUniqueIdProvider
    {
        string UniqueId { get; }
    }
}
