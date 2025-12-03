using UnityEngine;

public interface IVFXService
{
    GameObject PlayEffect(string key, Vector3 position, Quaternion? rotation = null);
    void PlayFadeAndDestroyEffect(GameObject targetObject, float duration);
}
