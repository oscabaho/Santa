using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// RESPONSABILIDAD ÚNICA: Validar que el GameObject de un Enemigo tiene todos los componentes base.
/// Este script asegura que cualquier prefab de enemigo tenga la estructura mínima para funcionar
/// en el sistema de combate y movimiento.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))] // Esencial para el movimiento por IA
[RequireComponent(typeof(AttackComponent))]
[RequireComponent(typeof(HealthComponentBehaviour))]
[RequireComponent(typeof(StaminaComponentBehaviour))]
// --- NOTA ---
// Probablemente querrás añadir aquí tu propio script de IA, por ejemplo:
// [RequireComponent(typeof(EnemyAIController))]
public class EnemyValidator : MonoBehaviour { }
