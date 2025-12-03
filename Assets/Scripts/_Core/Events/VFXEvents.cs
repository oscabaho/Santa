using UnityEngine;

namespace Santa.Core.Events
{
    /// <summary>
    /// Event published to request playing a visual effect from the pool.
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

