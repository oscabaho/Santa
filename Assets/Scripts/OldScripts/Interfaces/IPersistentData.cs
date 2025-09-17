
/// <summary>
/// Interfaz para componentes que pueden guardar y cargar su estado
/// utilizando un objeto PlayerPersistentData.
/// </summary>
public interface IPersistentData
{
    /// <summary>
    /// Guarda el estado del componente en el objeto de datos.
    /// </summary>
    void SaveData(PlayerPersistentData data);

    /// <summary>
    /// Carga el estado del componente desde el objeto de datos.
    /// </summary>
    void LoadData(PlayerPersistentData data, ItemDatabase itemDatabase);
}
