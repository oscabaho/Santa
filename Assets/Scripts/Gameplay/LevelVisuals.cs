using UnityEngine;

/// <summary>
/// Este componente se usa para marcar los GameObjects que representan
/// los diferentes estados visuales de un nivel.
/// No contiene ninguna lógica, solo sirve como un "tag" para el sistema de niveles.
///
/// Una vez que agregues este script a tus GameObjects (por ejemplo, 'EstadoInicial' y 'EstadoLiberado'),
/// podrás arrastrarlos a los campos 'Gentrified Visuals' y 'Liberated Visuals' en el inspector
/// del LevelManager sin que aparezca el error de "Type Mismatch".
/// </summary>
public class LevelVisuals : MonoBehaviour
{
    // Componente marcador. No necesita código adicional.
}
