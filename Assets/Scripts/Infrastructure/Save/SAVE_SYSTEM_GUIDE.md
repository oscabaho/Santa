// GUÍA COMPLETA: Sistema de Guardado (SaveContributor Pattern)
//
// Este archivo documenta cómo implementar persistencia de datos en tu juego usando ISaveContributor.
//
// ============================================================================
// CONCEPTOS FUNDAMENTALES
// ============================================================================
//
// 1. FLUJO DE GUARDADO:
//    SaveService.Save()
//    ↓
//    WriteContributors() - llamado por SaveService
//    ↓
//    Todos los ISaveContributor.WriteTo(ref SaveData)
//    ↓
//    Datos combinados en SaveData
//    ↓
//    Encriptación + guardado a disco
//
// 2. FLUJO DE CARGA:
//    SaveService.TryLoad(out SaveData)
//    ↓
//    Desencriptación + validación
//    ↓
//    Posicionamiento del jugador en respawn point
//    ↓
//    ReadContributors(in SaveData)
//    ↓
//    Todos los ISaveContributor.ReadFrom(in SaveData)
//    ↓
//    GameLoadedEvent publicado (notifica a otros sistemas)
//
// ============================================================================
// IMPLEMENTACIÓN BÁSICA
// ============================================================================
//
// Paso 1: Tu clase debe implementar ISaveContributor
//
//     public class MiComponente : MonoBehaviour, ISaveContributor
//     {
//         // Datos a guardar
//         private int miEstado;
//
//         // Implementar WriteTo para guardar
//         public void WriteTo(ref SaveData data)
//         {
//             // Guardar tu estado en data.extras o campos específicos de SaveData
//             // Ejemplo: usar SerializableKV
//             AppendExtra(ref data, "MiComponente_Estado", miEstado.ToString());
//         }
//
//         // Implementar ReadFrom para cargar
//         public void ReadFrom(in SaveData data)
//         {
//             // Restaurar tu estado desde data.extras
//             if (TryGetExtra(data, "MiComponente_Estado", out var value))
//             {
//                 miEstado = int.Parse(value);
//             }
//         }
//
//         // Helpers para trabajar con extras
//         private void AppendExtra(ref SaveData data, string key, string value)
//         {
//             var list = new System.Collections.Generic.List<SerializableKV>(data.extras ?? System.Array.Empty<SerializableKV>());
//             list.Add(new SerializableKV { key = key, value = value });
//             data.extras = list.ToArray();
//         }
//
//         private bool TryGetExtra(in SaveData data, string key, out string value)
//         {
//             value = null;
//             if (data.extras == null) return false;
//             foreach (var kv in data.extras)
//             {
//                 if (kv.key == key)
//                 {
//                     value = kv.value;
//                     return true;
//                 }
//             }
//             return false;
//         }
//     }
//
// ============================================================================
// IMPLEMENTACIÓN CON REGISTRO (RECOMENDADO PARA MEJOR RENDIMIENTO)
// ============================================================================
//
// Usa ISaveContributorRegistry para mejor rendimiento:
//
//     public class MiComponente : MonoBehaviour, ISaveContributor
//     {
//         private ISaveContributorRegistry _registry;
//
//         [VContainer.Inject]
//         public void Construct(ISaveContributorRegistry registry)
//         {
//             _registry = registry;
//         }
//
//         private void OnEnable()
//         {
//             _registry?.Register(this);
//         }
//
//         private void OnDisable()
//         {
//             _registry?.Unregister(this);
//         }
//
//         public void WriteTo(ref SaveData data)
//         {
//             // Guardar datos
//         }
//
//         public void ReadFrom(in SaveData data)
//         {
//             // Cargar datos
//         }
//     }
//
// ============================================================================
// CASOS DE USO EN TU PROYECTO
// ============================================================================
//
// YA IMPLEMENTADOS:
//
// 1. UpgradeManager (Presentation/Upgrades/UpgradeManager.cs)
//    ✅ Guarda: lastUpgrade, acquiredUpgrades[]
//    - WriteTo: Copia lista de upgrades a SaveData
//    - ReadFrom: Restaura upgrades y reaplica estadísticas
//
// 2. DefeatedEnemiesTracker (Infrastructure/Save/DefeatedEnemiesTracker.cs)
//    ✅ Guarda: Enemigos derrotados por ID
//    - Escucha CharacterDeathEvent
//    - Desactiva enemies cuando se cargan datos
//
// 3. EnvironmentDecorState (Infrastructure/Save/EnvironmentDecorState.cs)
//    ✅ Guarda: Cambios de decoración por ID
//    - Reaplica cambios al cargar (ej: áreas liberadas)
//
// REQUIEREN IMPLEMENTACIÓN:
//
// 4. Progreso de niveles/áreas (si no está guardado)
//    - Qué nivel está desbloqueado
//    - Qué área está completada
//
// 5. Estado del mapa/exploración
//    - Zonas visitadas
//    - Puertas abiertas
//
// ============================================================================
// INTEGRACIÓN CON GAMELOADEDEVENT
// ============================================================================
//
// Después de que SaveService.TryLoad() carga la partida, publica GameLoadedEvent.
// Cualquier sistema que necesite reaccionarmm puede suscribirse:
//
//     [Inject] private IEventBus _eventBus;
//
//     private void OnEnable()
//     {
//         _eventBus?.Subscribe<GameLoadedEvent>(OnGameLoaded);
//     }
//
//     private void OnDisable()
//     {
//         _eventBus?.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
//     }
//
//     private void OnGameLoaded(GameLoadedEvent evt)
//     {
//         var saveData = evt.SaveData;
//         // Reaccionar a la carga (ej: reproducir transiciones)
//     }
//
// ============================================================================
// VALIDACIÓN DE DATOS
// ============================================================================
//
// SaveData.Validate() verifica:
// ✅ Scene name no está vacío
// ✅ Player position dentro de límites razonables
// ✅ Timestamp no es del futuro
// ✅ Timestamp no es anterior a 2000
//
// Si alguna validación falla, SaveService rechaza la carga.
//
// ============================================================================
// FLUJO DE CARGA VISUAL (RECOMENDADO)
// ============================================================================
//
// 1. Usuario presiona "Cargar"
// 2. PauseMenuUI.OnLoadClicked() → SaveService.TryLoad()
// 3. SaveService posiciona player en respawn point (NO en posición guardada)
// 4. ReadContributors() restaura:
//    - Enemigos derrotados se desactivan
//    - Cambios de ambiente se reaplican
//    - Upgrades se restauran
// 5. GameLoadedEvent publicado
// 6. PauseMenuUI → fundido a negro → resume juego
//
// ============================================================================
// BEST PRACTICES
// ============================================================================
//
// ✅ DO:
//    - Implementa WriteTo() para guardar TODOS tus datos importantes
//    - Implementa ReadFrom() para restaurar EXACTAMENTE lo mismo
//    - Usa IDs estables (IUniqueIdProvider) para enemigos
//    - Registra en ISaveContributorRegistry si hay muchos componentes
//    - Valida datos en ReadFrom() por si acaso
//    - Publica eventos cuando estados cambian (CharacterDeathEvent, etc.)
//
// ❌ DON'T:
//    - No confíes en orden específico de lectura/escritura
//    - No guardes referencias de GameObject directamente
//    - No guardes datos que dependen de tiempo real
//    - No intentes guardar la salud actual en exploración
//    - No mezcles lógica de guardado con lógica de gameplay
//
// ============================================================================
// TESTING & DEBUGGING
// ============================================================================
//
// Para testear guardado:
// 1. Juega unos combates
// 2. Aplica upgrades
// 3. Guarda manualmente (menú pausa)
// 4. Mata algunos enemigos / libera áreas
// 5. Carga (debe restaurar exactamente cómo estaba)
//
// Logs útiles:
// - SaveService.Save() → "GameService: Game saved"
// - SaveService.TryLoad() → "SaveService: Player positioned at respawn point"
// - DefeatedEnemiesTracker.ReadFrom() → "Refreshed" con contador
// - EnvironmentDecorState.PerformChange() → cambios aplicados
//
// ============================================================================
