namespace Santa.Core.Save
{
    // Components can implement this to contribute custom data to the save file and restore it later.
    public interface ISaveContributor
    {
        void WriteTo(ref SaveData data);
        void ReadFrom(in SaveData data);
    }
}
