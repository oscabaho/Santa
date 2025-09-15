using UnityEngine;
using System.Collections;

namespace ProyectSecret.VFX
{
    /// <summary>
    /// Controla el comportamiento de la sombra/indicador de ataque, incluyendo un efecto de fade-in.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class ShadowController : MonoBehaviour
    {
        [Tooltip("Duración del efecto de aparición gradual (fade-in) en segundos.")]
        [SerializeField] private float fadeInDuration = 0.3f;

        private Renderer _shadowRenderer;
        private MaterialPropertyBlock _propBlock;
        private int _colorID;
        private Color _originalColor;
        private Coroutine _fadeInCoroutine;

        private void Awake()
        {
            _shadowRenderer = GetComponent<Renderer>();
            _propBlock = new MaterialPropertyBlock();
            _colorID = Shader.PropertyToID("_Color");

            // Guardamos el color original del material para saber a qué nivel de alfa debemos llegar.
            if (_shadowRenderer.sharedMaterial.HasProperty(_colorID))
            {
                _originalColor = _shadowRenderer.sharedMaterial.color;
            }
            else
            {
                _originalColor = Color.white; // Un valor por defecto si el material no tiene color.
                Debug.LogWarning($"El material en {gameObject.name} no tiene una propiedad '_Color'. Se usará blanco por defecto.", this);
            }
        }

        private void OnEnable()
        {
            // Cada vez que la sombra se activa desde el pool, iniciamos el fade-in.
            if (_fadeInCoroutine != null)
                StopCoroutine(_fadeInCoroutine);
            _fadeInCoroutine = StartCoroutine(FadeInRoutine());
        }

        private IEnumerator FadeInRoutine()
        {
            float timer = 0f;
            float targetAlpha = _originalColor.a;

            // Empezamos con el alfa a cero para que sea invisible al inicio.
            Color currentColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0f);
            _shadowRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(_colorID, currentColor);
            _shadowRenderer.SetPropertyBlock(_propBlock);
            
            while (timer < fadeInDuration)
            {
                // Interpolamos el valor del alfa desde 0 hasta el alfa objetivo.
                float currentAlpha = Mathf.Lerp(0f, targetAlpha, timer / fadeInDuration);
                currentColor.a = currentAlpha;
                
                // Aplicamos el nuevo color usando el MaterialPropertyBlock para un rendimiento óptimo.
                _shadowRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(_colorID, currentColor);
                _shadowRenderer.SetPropertyBlock(_propBlock);

                timer += Time.deltaTime;
                yield return null;
            }

            // Al final, nos aseguramos de que el alfa sea exactamente el valor deseado.
            _propBlock.SetColor(_colorID, _originalColor);
            _shadowRenderer.SetPropertyBlock(_propBlock);
            _fadeInCoroutine = null;
        }
    }
}
