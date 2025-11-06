# 🔌 Integración de UpgradeUI con VContainer - Guía de Instancia Dinámica

> Nota: Este documento queda como referencia histórica. La implementación actual usa Addressables con `UpgradeUILoader` para cargar la UI dinámicamente. Revisa `UPGRADE_UI_ADDRESSABLES_SETUP.md` y `UPGRADE_UI_OPTIMIZATION_GUIDE.md` para la configuración vigente.

## 📋 Resumen

Tu proyecto usa **instancia dinámica** para la UpgradeUI porque la UI varía según el estado del juego. El sistema crea la UI automáticamente cuando es necesario y la mantiene disponible durante toda la sesión de juego.

---

## 🎯 Configuración Actual

### Archivo Modificado: `GameLifetimeScope.cs`

Se añadieron estas modificaciones:

```csharp
// 1. Nuevo campo para el prefab (en la parte superior de la clase)
[Header("UI Prefabs - Dynamic Instantiation")]
[Tooltip("Prefab de UpgradeUI que se instanciará dinámicamente cuando sea necesario")]
[SerializeField]
private UpgradeUI upgradeUIPrefab;

// 2. En el método Configure (reemplazo del RegisterComponentInHierarchy)
// Antes:
// builder.RegisterComponentInHierarchy<UpgradeUI>().As<IUpgradeUI>();

// Ahora:
if (upgradeUIPrefab != null)
{
    builder.RegisterComponentInNewPrefab(upgradeUIPrefab, Lifetime.Singleton)
        .As<IUpgradeUI>()
        .AsSelf();
}
else
{
    GameLog.LogWarning("UpgradeUI Prefab no asignado en GameLifetimeScope.");
}
```

---

## 🔧 Pasos para Completar la Configuración

### 1. Asignar el Prefab en Unity

1. **Abre** tu escena principal de juego
2. **Localiza** el GameObject que tiene el componente `GameLifetimeScope`
   - Usualmente está en la raíz de la Hierarchy
   - Busca por "GameLifetimeScope" o "DI Container" o similar
3. **Selecciona** ese GameObject
4. En el **Inspector**, busca la sección **"UI Prefabs - Dynamic Instantiation"**
5. **Arrastra** el prefab `UpgradeUI.prefab` desde `Assets/Prefabs/UI/` al campo **Upgrade UI Prefab**

### 2. Verificar que NO haya UpgradeUI en la Escena

**IMPORTANTE:** La UpgradeUI NO debe estar colocada manualmente en la escena.

- ❌ **Incorrecto:** Tener un GameObject "UpgradeUI" o "UpgradeCanvas" en la Hierarchy
- ✅ **Correcto:** Solo el prefab asignado en el GameLifetimeScope

Si encuentras una instancia en la escena:
1. Selecciónala en la Hierarchy
2. Elimínala (Delete)
3. Guarda la escena

### 3. Guardar los Cambios

1. **Guarda** la escena: `Ctrl + S`
2. **Guarda** el proyecto: `File → Save Project`

---

## 🔄 Cómo Funciona el Sistema

### Flujo de Instanciación

```
1. Unity carga la escena
         ↓
2. GameLifetimeScope.Awake() se ejecuta
   • DontDestroyOnLoad(gameObject) se aplica
         ↓
3. GameLifetimeScope.Configure() se ejecuta
   • VContainer lee el prefab upgradeUIPrefab
   • Instancia el prefab automáticamente
   • Lo registra como Singleton
         ↓
4. El UpgradeUI instanciado:
   • Permanece en memoria durante toda la sesión
   • Se inyecta automáticamente donde se necesite
   • No se destruye al cambiar de escena
         ↓
5. UpgradeManager recibe la referencia
   • Via [Inject] en el constructor
   • Puede llamar a ShowUpgrades() cuando sea necesario
```

### Ventajas de esta Configuración

✅ **Lazy Loading:** La UI se crea solo cuando VContainer la necesita (en el inicio)
✅ **Singleton:** Solo existe una instancia en toda la aplicación
✅ **Persistente:** No se destruye al cambiar de escena (hereda del LifetimeScope)
✅ **Desacoplado:** El UpgradeManager no necesita saber dónde/cómo se crea la UI
✅ **Testeable:** Fácil mockear IUpgradeUI en tests
✅ **Flexible:** Si el estado del juego cambia, la UI responde sin recrearse

---

## 🧪 Testing de la Integración

### Test 1: Verificar la Inyección

1. **Play Mode** en Unity
2. **Abre** la ventana Console (`Ctrl + Shift + C`)
3. **Busca** el mensaje: `"GameLifetimeScope CONFIGURED!"`
4. **Verifica** que NO haya warnings sobre "UpgradeUI Prefab no asignado"

### Test 2: Probar con UpgradeUITester

1. **Crea** un GameObject vacío en la escena
2. **Añade** el componente `UpgradeUITester`
3. **Crea** 2 ScriptableObjects de prueba (AbilityUpgrade)
4. **Asigna** los upgrades en el Inspector del tester
5. **Play Mode** → Presiona **T**
6. **Resultado esperado:** El panel de upgrades aparece con fade-in

### Test 3: Verificar Persistencia entre Escenas

1. **Play Mode** en tu escena de juego
2. **Presiona T** para mostrar la UI (con UpgradeUITester)
3. **Cambia** de escena usando tu lógica de juego
4. **Verifica** que:
   - El GameObject UpgradeUI sigue existiendo en la Hierarchy
   - Está marcado como "DontDestroyOnLoad" (texto en gris en Hierarchy)
   - Puedes volver a llamar ShowUpgrades() en la nueva escena

---

## 🐛 Solución de Problemas

### Problema: "UpgradeUI Prefab no asignado"

**Síntoma:** Warning en la consola al iniciar

**Causa:** El campo `upgradeUIPrefab` está vacío (None) en el Inspector

**Solución:**
1. Selecciona el GameObject con GameLifetimeScope
2. Arrastra el prefab al campo "Upgrade UI Prefab"
3. Guarda la escena

---

### Problema: "NullReferenceException" al llamar ShowUpgrades()

**Síntoma:** Error cuando UpgradeManager intenta mostrar la UI

**Causa:** La inyección falló o el prefab es inválido

**Solución:**
1. Verifica que el prefab tenga el componente `UpgradeUI` (doble-clic en el prefab)
2. Verifica que `UpgradeUI` implemente `IUpgradeUI`
3. Verifica que UpgradeManager tenga `[Inject]` en su constructor:
   ```csharp
   [Inject]
   public void Construct(IUpgradeUI upgradeUI, ...)
   ```
4. Reinicia Unity Editor (a veces VContainer necesita recompilar)

---

### Problema: "The object of type 'UpgradeUI' has been destroyed"

**Síntoma:** Error al cambiar de escena

**Causa:** Algo está destruyendo el objeto instanciado

**Solución:**
1. Verifica que NO tengas código que haga `Destroy(upgradeUI)`
2. Verifica que GameLifetimeScope tenga `DontDestroyOnLoad(gameObject)` en Awake
3. No uses `SceneManager.LoadScene()` sin `LoadSceneMode.Single` si quieres mantener objetos

---

### Problema: Múltiples instancias de UpgradeUI

**Síntoma:** Varios GameObjects "UpgradeUI(Clone)" en la Hierarchy

**Causa:** El prefab se está registrando varias veces o hay instancias manuales

**Solución:**
1. Elimina cualquier UpgradeUI manual de la escena
2. Verifica que `RegisterComponentInNewPrefab` solo se llame UNA vez
3. Usa `Lifetime.Singleton` (ya configurado)
4. Reinicia Play Mode

---

## 📊 Comparación: Antes vs Ahora

| Aspecto | Antes (RegisterComponentInHierarchy) | Ahora (RegisterComponentInNewPrefab) |
|---------|--------------------------------------|--------------------------------------|
| **Instanciación** | Manual, en la escena | Automática, desde prefab |
| **Ubicación** | Debe existir en la Hierarchy | Se crea dinámicamente |
| **Flexibilidad** | Una instancia por escena | Una instancia global |
| **Mantenimiento** | Hay que colocar en cada escena | Un solo prefab para todo |
| **Testing** | Requiere setup de escena | Testing independiente |
| **Estado del juego** | Limitado a la escena actual | Persiste entre escenas |

---

## 🎯 Mejores Prácticas Aplicadas

1. ✅ **Separation of Concerns:** La UI no necesita saber cuándo/cómo se instancia
2. ✅ **Dependency Injection:** Todo se gestiona via interfaces
3. ✅ **Singleton Pattern:** Una sola instancia de UI en toda la app
4. ✅ **Lazy Initialization:** Se crea cuando VContainer la necesita
5. ✅ **Persistence:** No se recrea innecesariamente entre escenas
6. ✅ **Testability:** Fácil mockear para unit tests

---

## 🚀 Próximos Pasos

### 1. Configuración Básica (5 min)
- [x] Modificar GameLifetimeScope.cs
- [ ] Asignar prefab en el Inspector
- [ ] Eliminar instancias manuales de la escena
- [ ] Guardar cambios

### 2. Testing (10 min)
- [ ] Test 1: Verificar inyección
- [ ] Test 2: Probar con UpgradeUITester
- [ ] Test 3: Verificar persistencia entre escenas

### 3. Integración con Gameplay (variable)
- [ ] Probar después de ganar un combate
- [ ] Verificar flujo completo (victoria → upgrades → siguiente nivel)
- [ ] Ajustar timing/animaciones según necesidad

---

## 📚 Recursos Adicionales

### Documentación Relacionada
- `UPGRADE_UI_SETUP_GUIDE.md` → Sección 6 "Integración con el Sistema"
- `UPGRADE_UI_FLOW_DIAGRAMS.md` → Sección 6 "Flujo de Inyección de Dependencias"

### Archivos Relevantes
- `Assets/Scripts/Core/GameLifetimeScope.cs` (configuración de DI)
- `Assets/Scripts/Upgrades/UpgradeUI.cs` (implementación de IUpgradeUI)
- `Assets/Scripts/Upgrades/UpgradeManager.cs` (consumidor de IUpgradeUI)
- `Assets/Prefabs/UI/UpgradeUI.prefab` (prefab a instanciar)

---

## ✅ Checklist Final

Antes de considerar la integración completa:

- [ ] Campo `upgradeUIPrefab` añadido a GameLifetimeScope
- [ ] Prefab asignado en el Inspector de Unity
- [ ] RegisterComponentInNewPrefab configurado correctamente
- [ ] NO hay instancias manuales de UpgradeUI en la escena
- [ ] Test de inyección pasa sin errores
- [ ] UpgradeUITester funciona correctamente
- [ ] UI persiste correctamente entre escenas
- [ ] Flujo completo de juego funciona

---

**¡La integración dinámica está lista! 🎉**

Tu sistema ahora instancia la UI automáticamente cuando es necesario y la mantiene disponible durante toda la sesión de juego, adaptándose perfectamente a los diferentes estados del juego.

---

## 🎮 Arena de Combate - Configuración con Addressables

### 📋 Política de Arenas de Combate

**IMPORTANTE:** Las arenas de combate (prefabs instanciados durante encuentros) **DEBEN configurarse exclusivamente mediante Addressables**.

✅ **Configuración (única opción):**
- Addressables mediante `combatSceneAddress`

**Nota:** Los campos `combatScenePrefab`, `combatSceneResourcePath` y `combatSceneParent` han sido eliminados del código.

### 🔧 Configuración de CombatEncounter

#### Origen soportado (único):

1. **Addressable** via `combatSceneAddress` (OBLIGATORIO)
   - Carga asíncrona optimizada
   - Control de memoria granular
   - Pooling y prewarm eficientes
   - Ideal para mobile y builds optimizadas

#### Ejemplo de configuración en Inspector:

```
CombatEncounter Component:
├── Combat Scene Address: "CombatArena_Forest"  ✅ REQUERIDO
├── Combat Camera: MainCombatCamera
├── Auto Prewarm: ✓ (recomendado para primera arena)
├── Prewarm Count: 1 (mobile) / 2-3 (PC)
└── Release Addressables Instances: ✓ (si la arena es pesada)
```

### 🎯 Ventajas de Usar Addressables para Arenas

✅ **Carga Asíncrona:** No congela el juego durante la instanciación  
✅ **Pooling Eficiente:** Reutiliza instancias sin recargar assets  
✅ **Gestión de Memoria:** Libera recursos cuando no se usan (mobile-friendly)  
✅ **Prewarm:** Precarga instancias para evitar hitches en el primer combate  
✅ **Menor APK:** Assets no se incluyen en el build inicial  
✅ **Actualizaciones:** Posibilidad de actualizar arenas sin rebuild  

### 📝 Checklist de Configuración

#### 1. Marcar Arena como Addressable (Unity Editor)

1. Localiza tu prefab de arena: `Assets/Prefabs/Combat/CombatArena_XXX.prefab`
2. Selecciónalo en el Project window
3. En el Inspector, marca la casilla **"Addressable"** ✅
4. Asigna un **Address Name** descriptivo: `"CombatArena_Forest"`, `"CombatArena_Desert"`, etc.
5. Selecciona el **Group**: `"Combat_Assets"` o el grupo apropiado para arenas
6. Guarda los cambios

#### 2. Configurar CombatEncounter

1. En tu escena, selecciona el GameObject con `CombatEncounter`
2. En el Inspector, componente `CombatEncounter`:
   - **Combat Scene Address**: Escribe el mismo address de Addressables (ej: `"CombatArena_Forest"`)
   - **Combat Camera**: Asigna la cámara virtual para este encuentro
   - **Auto Prewarm**: Activar ✓ (recomendado para la primera arena que el jugador encontrará)
   - **Prewarm Count**:
     - Mobile: `1` (balance memoria/performance)
     - PC: `2-3` (más instancias pre-calentadas)
   - **Release Addressables Instances**:
     - Activar ✓ si la arena es pesada (muchos assets) y no se repite frecuentemente
     - Desactivar si la arena se usa múltiples veces (se mantendrá en pool)

#### 3. Build Addressables

1. Abre: `Window → Asset Management → Addressables → Groups`
2. Verifica que tu arena aparece en la lista con el address correcto
3. Click: `Build → New Build → Default Build Script`
4. Espera a que termine y verifica que no haya errores

#### 4. Testing

1. **Play Mode** en Unity
2. Verifica en la Console:
   ```
   CombatScenePool: Prewarming 1 instance(s) for key 'CombatArena_Forest'
   CombatScenePool: Prewarm completed for 'CombatArena_Forest'
   ```
3. Inicia un combate y verifica que la arena se carga correctamente
4. En la Hierarchy, verifica que aparece: `CombatArena_Forest(Clone)`

### 🐛 Solución de Problemas

#### Problema: "CombatScenePool: No address or prefab configured"

**Síntoma:** Error al intentar iniciar combate

**Causa:** El campo `combatSceneAddress` está vacío

**Solución:**
1. Selecciona el GameObject con `CombatEncounter`
2. Verifica que `Combat Scene Address` tenga un valor (ej: `"CombatArena_Forest"`)
3. Verifica que ese address existe en Addressables Groups

---

#### Problema: "Addressables.InstantiateAsync failed"

**Síntoma:** La arena no se carga

**Causa:** El address no existe o los Addressables no están buildeados

**Solución:**
1. Abre `Window → Asset Management → Addressables → Groups`
2. Verifica que el prefab de arena está en la lista con el address correcto
3. Click `Build → New Build → Default Build Script`
4. Reinicia Play Mode

---

#### Problema: Hitch/freeze al iniciar primer combate

**Síntoma:** Pausa notable cuando inicia el primer encuentro

**Causa:** No hay prewarm configurado

**Solución:**
1. Activa `Auto Prewarm` en el `CombatEncounter`
2. Configura `Prewarm Count = 1` (mobile) o `2` (PC)
3. La arena se precargará al inicio del nivel, eliminando el hitch

---

### 🚀 Próximos Pasos para Arenas de Combate

- [ ] Marcar todos los prefabs de arena como Addressables
- [ ] Asignar addresses descriptivos a cada arena
- [ ] Configurar `combatSceneAddress` en todos los `CombatEncounter` de tu proyecto
- [ ] Asignar `Combat Camera` en cada encuentro
- [ ] Build Addressables
- [ ] Probar cada tipo de arena en Play Mode
- [ ] Optimizar `prewarmCount` según plataforma target

---

**¡Tu sistema de arenas de combate está optimizado para mobile! 🎮**
