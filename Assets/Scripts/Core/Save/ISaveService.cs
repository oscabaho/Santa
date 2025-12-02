namespace Santa.Core.Save
{
    public interface ISaveService
    {
        bool CanSaveNow();
        void Save();
        bool TryLoad(out SaveData data);
        void Delete();
        bool TryGetLastSaveTimeUtc(out System.DateTime utc);
    }
}
