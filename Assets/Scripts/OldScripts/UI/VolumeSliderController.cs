using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla un slider de UI para ajustar un parámetro de volumen expuesto en el AudioMixer.
/// </summary>
[RequireComponent(typeof(Slider))]
public class VolumeSliderController : MonoBehaviour
{
    [Header("Mixer Configuration")]
    [Tooltip("El nombre exacto del parámetro expuesto en el AudioMixer (ej. 'MusicVolume', 'SFXVolume').")]
    [SerializeField] private string volumeParameterName = "MusicVolume";

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Start()
    {
        // El AudioManager debe existir para que esto funcione.
        if (AudioManager.Instance != null) 
        {
            // Al iniciar, ajustamos el valor del slider al valor guardado en PlayerPrefs.
            // Usamos 1.0f como valor por defecto si es la primera vez que se ejecuta.
            _slider.value = PlayerPrefs.GetFloat(volumeParameterName, 1.0f);
            // Nos suscribimos al evento de cambio de valor del slider.
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
        else
        {
            Debug.LogWarning("VolumeSliderController: AudioManager.Instance no encontrado. El slider no funcionará.", this);
            gameObject.SetActive(false);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        // Cada vez que el slider cambia, llamamos al AudioManager para que actualice el volumen.
        AudioManager.Instance?.SetVolume(volumeParameterName, value);

        // Y guardamos el nuevo valor en PlayerPrefs para que persista.
        PlayerPrefs.SetFloat(volumeParameterName, value);
    }

    private void OnDestroy() => _slider?.onValueChanged.RemoveListener(OnSliderValueChanged);
}
