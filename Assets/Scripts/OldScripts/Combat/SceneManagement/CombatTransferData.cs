using UnityEngine;

namespace ProyectSecret.Combat.SceneManagement
{
    /// <summary>
    /// ScriptableObject para transferir datos de combate entre escenas.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatTransferData", menuName = "Combat/CombatTransferData")]
    public class CombatTransferData : ScriptableObject
    {
        [HideInInspector] public GameObject playerPrefab;
        [HideInInspector] public GameObject enemyPrefab;
        [HideInInspector] public string kryptoniteItemId;
        // Puedes agregar más datos según necesidad

        public void Clear()
        {
            playerPrefab = null;
            enemyPrefab = null;
            kryptoniteItemId = null;
        }
    }
}
