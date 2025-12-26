using UnityEngine;

namespace Santa.Core.Player
{
    /// <summary>
    /// Scene-level provider for the player's exploration GameObject.
    /// Prefer assigning via Inspector. Falls back to tag/identifier lookup on Awake.
    /// </summary>
    public class PlayerReference : MonoBehaviour, IPlayerReference
    {
        [SerializeField] private GameObject player;

        public GameObject Player => player;

        private void Awake()
        {
            if (player == null)
            {
                var id = FindFirstObjectByType<ExplorationPlayerIdentifier>(FindObjectsInactive.Include);
                if (id != null)
                {
                    player = id.gameObject;
                }
                else
                {
                    // Fallback by tag
                    var byTag = GameObject.FindWithTag("Player");
                    if (byTag != null) player = byTag;
                }
            }
        }

        public void Set(GameObject newPlayer)
        {
            if (newPlayer != null)
            {
                player = newPlayer;
            }
        }
    }
}
