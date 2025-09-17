using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que gestiona la destreza (maestría) del jugador con cada tipo de arma.
/// Guarda el número de golpes efectivos por ID de arma.
/// </summary>
public class WeaponMasteryComponent : MonoBehaviour
{
    private Dictionary<string, int> hitsPorArma = new Dictionary<string, int>();

    public int GetHits(string weaponId)
    {
        if (hitsPorArma.TryGetValue(weaponId, out int hits))
            return hits;
        return 0;
    }

    public void AddHit(string weaponId)
    {
        if (!hitsPorArma.ContainsKey(weaponId))
            hitsPorArma[weaponId] = 0;
        hitsPorArma[weaponId]++;
    }
}
