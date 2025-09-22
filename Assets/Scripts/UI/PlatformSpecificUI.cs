using UnityEngine;

public class PlatformSpecificUI : MonoBehaviour
{
    private void Awake()
    {
#if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
#endif
    }
}
