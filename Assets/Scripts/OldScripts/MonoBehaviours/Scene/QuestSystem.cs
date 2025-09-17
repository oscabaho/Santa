using UnityEngine;
using System;

/// <summary>
/// Ejemplo de sistema de misiones que reacciona a eventos globales del Event Bus.
/// </summary>
public class QuestSystem : MonoBehaviour
{
    private int enemiesDefeated = 0;
    private int itemsUsed = 0;

    private void OnEnable()
    {
        GameEventBus.Instance.Subscribe<CharacterDeathEvent>(OnCharacterDeath);
        GameEventBus.Instance.Subscribe<CombatVictoryEvent>(OnCombatVictory);
        GameEventBus.Instance.Subscribe<ItemUsedEvent>(OnItemUsed);
    }

    private void OnDisable()
    {
        GameEventBus.Instance.Unsubscribe<CharacterDeathEvent>(OnCharacterDeath);
        GameEventBus.Instance.Unsubscribe<CombatVictoryEvent>(OnCombatVictory);
        GameEventBus.Instance.Unsubscribe<ItemUsedEvent>(OnItemUsed);
    }

    private void OnCharacterDeath(CharacterDeathEvent evt)
    {
        if (evt.Entity != null && evt.Entity.CompareTag("Enemy"))
        {
            enemiesDefeated++;
            #if UNITY_EDITOR
            Debug.Log($"Misión: Enemigos derrotados = {enemiesDefeated}");
            #endif
            // Aquí puedes comprobar si se cumple una misión
        }
    }

    private void OnCombatVictory(CombatVictoryEvent evt)
    {
        #if UNITY_EDITOR
        Debug.Log("Misión: ¡Victoria en combate!");
        #endif
    }

    private void OnItemUsed(ItemUsedEvent evt)
    {
        itemsUsed++;
        #if UNITY_EDITOR
        Debug.Log($"Misión: Ítems usados = {itemsUsed}");
        #endif
        // Aquí puedes comprobar si se cumple una misión de uso de ítems
    }
}

