### Guía de Prototipado de Combate

Esta guía te mostrará cómo configurar una escena básica en Unity para probar un encuentro de combate, desde el inicio hasta el final.

#### **Paso 1: Configurar la Jerarquía de la Escena**

Tu escena necesita ciertos sistemas y managers para funcionar. Asegúrate de que los siguientes GameObjects (o prefabs) estén presentes en tu jerarquía. La mayoría deberían estar en `Assets/Prefabs/Managers/`.

1.  **\_Managers (GameObject Vacío):** Crea un objeto vacío para organizar todos los managers.
2.  **GameManager:** Esencial para el estado general del juego.
3.  **TurnBasedCombatManager:** El cerebro del combate. Se encargará de gestionar los turnos, las acciones y las condiciones de victoria.
4.  **UIManager:** Para mostrar la interfaz de usuario del combate (salud, puntos de acción, etc.).
5.  **InputManager:** Gestiona las entradas del jugador.
6.  **EventSystem:** Un `EventSystem` de Unity es necesario para que la UI funcione. Si no tienes uno, haz clic derecho en la jerarquía -> UI -> Event System.
7.  **Main Camera:** Una cámara principal para la vista fuera de combate.

#### **Paso 2: Crear el Encuentro de Combate (`CombatEncounter`)**

Este componente define los datos de un combate específico.

1.  Crea un GameObject en tu escena, por ejemplo, un cubo invisible que actúe como un *trigger*. Llámalo `EncuentroCueva`.
2.  Añádele el script `CombatEncounter`.
    *   **Combat Scene Address:** Este campo es para cargar arenas de combate usando Addressables. Para un prototipo rápido con todo en la misma escena, puedes dejarlo vacío, pero ten en cuenta las advertencias en la consola.
    *   **Combat Camera:** Crea una `CinemachineVirtualCamera` (`GameObject -> Cinemachine -> Virtual Camera`) y posiciónala donde quieras que esté la cámara durante el combate. Arrastra esta cámara virtual al campo `Combat Camera`.

#### **Paso 3: Definir el "Arena" de Combate**

El "arena" es el espacio físico donde ocurre la batalla. Puede ser parte de tu escena principal. Lo más importante es definir dónde aparecerán los personajes.

1.  Crea GameObjects vacíos para marcar las posiciones de inicio.
2.  **Posición del Jugador:** Crea un GameObject llamado `PlayerSpawnPoint`. Asígnale el **Tag** `PlayerSpawnPoint`.
3.  **Posiciones de Enemigos:** Crea uno o varios GameObjects llamados `EnemySpawnPoint_1`, `EnemySpawnPoint_2`, etc. Asígnales el **Tag** `EnemySpawnPoint`.

El `TurnBasedCombatManager` usará estos tags para encontrar dónde instanciar a los personajes al iniciar el combate.

#### **Paso 4: Configurar los Combatientes (Prefabs)**

Tanto el jugador como los enemigos necesitan tener ciertos componentes para ser compatibles con el sistema de combate.

1.  **Player Prefab:**
    *   Asegúrate de que tu prefab de jugador tenga el **Tag** `Player`.
    *   Debe tener un script que implemente `IHealthController` (para gestionar la vida), `IActionPointController` (para los puntos de acción) y `IMovementController` (para el movimiento). Probablemente ya tengas un `PlayerController` o similar que haga esto.
    *   Añade el componente `PlayerBrain`. Este script se encarga de traducir el input del jugador en acciones de combate.

2.  **Enemy Prefab:**
    *   Asegúrate de que tu prefab de enemigo tenga el **Tag** `Enemy`.
    *   Al igual que el jugador, necesita componentes que implementen `IHealthController` y `IActionPointController`.
    *   Añade el componente `EnemyBrain`. Este script contiene la lógica de IA: decide qué habilidad usar y contra quién.

#### **Paso 5: Iniciar el Combate**

El combate se inicia llamando al método `TurnBasedCombatManager.StartCombat()`. Necesitas un *trigger* que lo active.

1.  En el GameObject `EncuentroCueva` que creaste en el Paso 2, añade un componente `BoxCollider` y marca la casilla `Is Trigger`.
2.  Crea un nuevo script llamado `CombatTrigger.cs` y añádelo a `EncuentroCueva`. Este script detectará cuando el jugador entre en el collider.

    ```csharp
    using UnityEngine;
    using VContainer;

    public class CombatTrigger : MonoBehaviour
    {
        [Inject]
        private readonly TurnBasedCombatManager _combatManager;

        private bool _combatStarted = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_combatStarted || !other.CompareTag("Player"))
            {
                return;
            }

            Debug.Log("Player entered combat trigger. Starting combat...");
            _combatStarted = true;

            // Busca el CombatEncounter en este mismo GameObject
            var encounter = GetComponent<ICombatEncounter>();
            if (encounter != null)
            {
                // Busca al jugador y a los enemigos en la escena para pasarlos al combate
                // NOTA: Una implementación más robusta definiría los enemigos en el CombatEncounter
                var player = FindObjectOfType<PlayerBrain>(); // Asume que el player ya está en la escena
                var enemies = FindObjectsOfType<EnemyBrain>();

                // Prepara el estado del combate
                var combatState = new CombatState(player, enemies, encounter);
                
                // Inicia el combate
                _combatManager.StartCombat(combatState);
            }
            else
            {
                Debug.LogError("CombatEncounter component not found on this trigger object!");
            }
        }
    }
    ```

3.  Ahora, cuando el jugador (con el tag `Player`) atraviese el `BoxCollider` del `EncuentroCueva`, el combate comenzará.

#### **Paso 6: Flujo y Finalización del Combate**

Una vez iniciado, el `TurnBasedCombatManager` tomará el control total:

1.  **Inicio:** Moverá la cámara a la `CombatCamera` y colocará a los personajes en los `SpawnPoints`.
2.  **Ciclo de Turnos:** Gestionará los turnos del jugador y de la IA (`PlayerBrain` y `EnemyBrain`).
3.  **Condiciones de Victoria/Derrota:** El sistema usará un `IWinConditionChecker` (probablemente `DefaultWinConditionChecker`) para comprobar después de cada acción si todos los enemigos o si el jugador ha sido derrotado.
4.  **Final:** Al terminar el combate, el `TurnBasedCombatManager` declarará un ganador, limpiará la escena de combate y devolverá el control al jugador y a la cámara principal.
