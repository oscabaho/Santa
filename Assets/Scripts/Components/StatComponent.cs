using System;
using UnityEngine;

[Serializable]
public abstract class StatComponent : IStatController
{
    [SerializeField, Tooltip("Valor máximo de la estadística")] 
    private int maxValue = 100;
    
    [SerializeField, HideInInspector] 
    private int currentValue;

    public int MaxValue => maxValue;
    public int CurrentValue => currentValue;

    public event Action<int, int> OnValueChanged;

    public virtual void Awake()
    {
        if (currentValue <= 0)
            currentValue = maxValue;
    }

    public void SetToMax()
    {
        currentValue = maxValue;
        OnValueChanged?.Invoke(currentValue, maxValue);
    }

    public virtual void AffectValue(int value)
    {
        SetValue(currentValue + value);
    }

    public virtual void SetValue(int newValue)
    {
        currentValue = Mathf.Clamp(newValue, 0, maxValue);
        OnValueChanged?.Invoke(currentValue, maxValue);
    }

    public virtual void ModifyMaxValue(int amount)
    {
        maxValue += amount;
        currentValue = Mathf.Clamp(currentValue, 0, maxValue);
        OnValueChanged?.Invoke(currentValue, maxValue);
    }
}