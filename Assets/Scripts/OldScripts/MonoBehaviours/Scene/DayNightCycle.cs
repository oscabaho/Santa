using UnityEngine;
using ProyectSecret.Events;

namespace ProyectSecret.Core
{
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private Light directionalLight;

        [Header("Cycle Timing")]
        [SerializeField] private float dayDuration = 60f;
        [SerializeField] private float nightDuration = 120f;

        [Header("Lighting")]
        [SerializeField] private Color dayLightColor = Color.white;
        [SerializeField] private Color nightLightColor = new Color(0.2f, 0.2f, 0.5f);
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float nightIntensity = 0.2f;

        [Header("Skybox")]
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material nightSkybox;
        [SerializeField] private bool useProceduralSkybox = false;
        [SerializeField] private Color daySkyTint = new Color(0.5f, 0.7f, 1f);
        [SerializeField] private Color nightSkyTint = new Color(0.05f, 0.05f, 0.15f);

        [Header("Transition")]
        [SerializeField] private float transitionSpeed = 0.5f;

        private float cycleTimer = 0f;
        private float transitionProgress = 0f; // 0 = full day, 1 = full night
        private bool isDay = true;

        void Start()
        {
            if (directionalLight == null)
            {
                directionalLight = FindFirstObjectByType<Light>();
            }
            // Set initial state without transition
            SetDay(true);
        }

        void Update()
        {
            cycleTimer += Time.deltaTime;
            float currentCycleDuration = isDay ? dayDuration : nightDuration;

            if (cycleTimer >= currentCycleDuration)
            {
                if (isDay)
                    SetNight();
                else
                    SetDay();
            }

            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            // Smoothly move the transition progress towards its target (0 for day, 1 for night)
            float targetProgress = isDay ? 0f : 1f;
            transitionProgress = Mathf.MoveTowards(transitionProgress, targetProgress, transitionSpeed * Time.deltaTime);

            // Rotate the light based on the actual cycle timer
            float cycleProgress = cycleTimer / (isDay ? dayDuration : nightDuration);
            float sunAngle = isDay ? Mathf.Lerp(0f, 180f, cycleProgress) : Mathf.Lerp(180f, 360f, cycleProgress);
            if (directionalLight != null)
            {
                directionalLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f); // Added a bit of Y rotation for a more natural arc
            }

            // Lerp visual properties based on the smooth transition progress
            if (directionalLight != null)
            {
                directionalLight.intensity = Mathf.Lerp(dayIntensity, nightIntensity, transitionProgress);
                directionalLight.color = Color.Lerp(dayLightColor, nightLightColor, transitionProgress);
            }

            if (useProceduralSkybox && RenderSettings.skybox != null && RenderSettings.skybox.HasProperty("_SkyTint"))
            {
                RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(daySkyTint, nightSkyTint, transitionProgress));
            }
        }

        void SetDay(bool instant = false)
        {
            isDay = true;
            cycleTimer = 0f;
            if (daySkybox != null) RenderSettings.skybox = daySkybox;
            
            if (instant)
            {
                transitionProgress = 0f;
                UpdateVisuals();
            }
            
            GameEventBus.Instance.Publish(new DayStartedEvent());
        }

        void SetNight(bool instant = false)
        {
            isDay = false;
            cycleTimer = 0f;
            if (nightSkybox != null) RenderSettings.skybox = nightSkybox;

            if (instant)
            {
                transitionProgress = 1f;
                UpdateVisuals();
            }

            GameEventBus.Instance.Publish(new NightStartedEvent());
        }
    }
}
