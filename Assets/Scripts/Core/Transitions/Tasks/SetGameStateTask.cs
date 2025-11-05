using System;
using UnityEngine;

/// <summary>
/// Legacy asset kept only to surface an explicit error if referenced.
/// </summary>
[Obsolete("SetGameStateTask has been retired. Use transition tasks that rely on injected services instead.", true)]
public class SetGameStateTask : ScriptableObject
{
}