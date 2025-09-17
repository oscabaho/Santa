using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Configuración de inversión")]
    [SerializeField] private float tiempoParaInvertirCamara = 1.0f;
    private float tiempoAcercandoseALaCamara = 0.0f;

    [Header("Cámaras hijas")]
    [Tooltip("Arrastra aquí el GameObject de la cámara trasera.")]
    [SerializeField] private Camera camaraTrasera;
    [Tooltip("Arrastra aquí el GameObject de la cámara frontal.")]
    [SerializeField] private Camera camaraFrontal;
    
    private Camera activeCamera;
    private PaperMarioPlayerMovement playerMovement;

    /// <summary>
    /// Evento que se dispara cuando la cámara activa cambia.
    /// </summary>
    public event System.Action<Camera> OnCameraChanged;

    void Awake()
    {
        playerMovement = GetComponent<PaperMarioPlayerMovement>();

        // Validar que las cámaras han sido asignadas en el Inspector.
        if (camaraTrasera == null || camaraFrontal == null)
        {
            #if UNITY_EDITOR
            Debug.LogError("PlayerCameraController: Faltan referencias a las cámaras. Por favor, asígnalas en el Inspector.");
            #endif
            enabled = false; // Desactivamos el componente para evitar más errores.
            return;
        }
        
        // Por defecto activa la trasera.
        SetActiveCamera(camaraTrasera);
    }

    private void Update()
    {
        if (playerMovement == null || activeCamera == null) return;

        // La lógica de 'DetectarAcercamiento' ahora vive aquí, haciendo este componente autocontenido.
        if (playerMovement.IsMovingDown)
        {
            tiempoAcercandoseALaCamara += Time.deltaTime;
            if (tiempoAcercandoseALaCamara >= tiempoParaInvertirCamara)
            {
                InvertirCamara();
                tiempoAcercandoseALaCamara = 0.0f;
            }
        }
        else
        {
            tiempoAcercandoseALaCamara = 0.0f;
        }
    }

    /// <summary>
    /// Asigna la cámara activa y dispara el evento de cambio.
    /// </summary>
    public void SetActiveCamera(Camera cam)
    {
        if (cam == null)
        {
            Debug.LogError("PlayerCameraController: Se intentó activar una cámara nula.");
            return;
        }

        // Desactivar todas las cámaras para asegurar que solo una esté activa.
        camaraTrasera.gameObject.SetActive(false);
        camaraFrontal.gameObject.SetActive(false);

        activeCamera = cam;
        activeCamera.gameObject.SetActive(true);
        OnCameraChanged?.Invoke(activeCamera);
    }

    /// <summary>
    /// Invierte entre la cámara trasera y frontal.
    /// </summary>
    public void InvertirCamara()
    {
        SetActiveCamera(activeCamera == camaraTrasera ? camaraFrontal : camaraTrasera);
        
        // Notificar al sistema que el estado de inversión ha cambiado.
        // Esto es crucial para que otros componentes (como el de equipamiento) reaccionen.
        if (playerMovement != null)
        {
            playerMovement.SetCameraInverted(activeCamera == camaraFrontal);
        }
    }

    /// <summary>
    /// Permite obtener la cámara activa actual.
    /// </summary>
    public Camera GetActiveCamera() => activeCamera;
}
