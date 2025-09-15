using UnityEngine;

namespace ProyectSecret.Events
{
    /// <summary>
    /// Evento publicado para solicitar la reproducción de un efecto visual desde el pool.
    /// </summary>
    public class PlayVFXRequest
    {
        public string Key { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public PlayVFXRequest(string key, Vector3 position, Quaternion? rotation = null)
        {
            Key = key;
            Position = position;
            Rotation = rotation ?? Quaternion.identity;
        }
    }
}

