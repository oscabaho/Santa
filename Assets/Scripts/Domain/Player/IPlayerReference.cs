using UnityEngine;

namespace Santa.Core.Player
{
    public interface IPlayerReference
    {
        GameObject Player { get; }
        void Set(GameObject player);
    }
}
