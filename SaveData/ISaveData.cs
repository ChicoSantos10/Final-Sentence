namespace SaveData
{
    public interface ISaveData
    {
        string Id { get; }
        object Save();
        void Load(object data);
    }
}
