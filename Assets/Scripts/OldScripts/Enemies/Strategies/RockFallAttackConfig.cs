using UnityEngine;

namespace ProyectSecret.Enemies.Strategies
{
    [CreateAssetMenu(fileName = "RockFallConfig", menuName = "ProyectSecret/Enemy Attacks/Rock Fall Config")]
    public class RockFallAttackConfig : ScriptableObject
    {
        [Header("Configuración del Ataque")]
        public int numberOfGroups = 3;
        public int rocksPerGroup = 5;
        public float intervalBetweenGroups = 2f;
        public float spawnDelayWithinGroup = 0.1f;
        public float rockSpawnHeight = 10f;
        public float rockSpawnRadius = 5f;
        public LayerMask groundLayer;
    }
}