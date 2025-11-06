# 🎯 CONFIGURACIÓN ADDRESSABLES para UpgradeUI

## 📋 Resumen

Tu proyecto ya usa **Addressables** para las UIs. Esta es la configuración correcta para mantener consistencia.

---

## ✅ Ventajas de Usar Addressables

✅ **Consistencia:** Igual que tus otras UIs  
✅ **Optimización:** Carga bajo demanda (lazy loading)  
✅ **Memoria:** Se libera cuando no se usa  
✅ **Gestión:** Centralizada en el Asset Management  
✅ **Builds:** Mejor control del tamaño de builds  
✅ **Updates:** Posibilidad de actualizar assets sin rebuild  

---

## 🔧 PASOS DE CONFIGURACIÓN

### 1. Marcar el Prefab como Addressable (2 minutos)

1. **Localiza** tu prefab: `Assets/Prefabs/UI/UpgradeUI.prefab`
2. **Selecciónalo** en el Project window
3. En el **Inspector**, busca la casilla **"Addressable"** (arriba, junto al nombre)
4. **Marca** la casilla ✅
5. Se abrirá una ventana de configuración:
   - **Address Name:** `UpgradeUI` (IMPORTANTE: debe coincidir con el código)
   - **Group:** Selecciona el grupo de UIs que ya tienes (ej: "UI_Assets" o "Default")
6. **Aplicar** los cambios

### 2. Verificar Grupos de Addressables (1 minuto)

1. Abre: **Window → Asset Management → Addressables → Groups**
2. Verifica que `UpgradeUI` aparezca en la lista
3. Debe estar en el mismo grupo que tus otras UIs (por consistencia)

### 3. Configurar el Código (Ya hecho ✅)

Ya creé el archivo `UpgradeUILoader.cs` que:
- ✅ Carga el prefab via Addressables
- ✅ Lo gestiona como Singleton
- ✅ Inyecta dependencias automáticamente
- ✅ Marca como DontDestroyOnLoad
- ✅ Libera recursos cuando no se usa

### 4. Verificar GameLifetimeScope (Ya hecho ✅)

El `GameLifetimeScope.cs` ya está configurado para usar el loader:

```csharp
builder.Register<UpgradeUILoader>(Lifetime.Singleton)
    .As<IUpgradeUI>()
    .AsSelf();
```

---

## 🎯 Diferencias con el Método Anterior

| Aspecto | Prefab Directo | Addressables (Tu proyecto) |
|---------|----------------|----------------------------|
| **Carga** | Al inicio siempre | Bajo demanda |
| **Memoria** | Siempre en RAM | Se puede liberar |
| **Consistencia** | ❌ Diferente a otras UIs | ✅ Igual que otras UIs |
| **Setup** | Campo en Inspector | Marcar como Addressable |
| **Builds** | En asset bundle principal | Asset bundle separado |
| **Actualizaciones** | Requiere rebuild | Posible actualización remota |

---

## 🔍 Cómo Funciona

### Flujo de Carga

```
1. UpgradeManager llama ShowUpgrades()
         ↓
2. UpgradeUILoader (IUpgradeUI) recibe la llamada
         ↓
3. Si NO está cargado:
   → Addressables.InstantiateAsync("UpgradeUI")
   → Espera la carga asíncrona
   → Obtiene el GameObject instanciado
   → Inyecta dependencias
   → DontDestroyOnLoad()
         ↓
4. Si YA está cargado:
   → Usa la instancia en caché
         ↓
5. _upgradeUIInstance.ShowUpgrades(up1, up2)
         ↓
6. UI se muestra con animaciones
```

### Gestión de Memoria

```
Inicio del juego:
  • UpgradeUI NO está en memoria
  • Solo el loader (ligero) está registrado

Primera victoria:
  • Se carga UpgradeUI via Addressables
  • Se cachea en memoria
  • Se muestra al jugador

Victorias siguientes:
  • Se reutiliza la instancia cacheada
  • No recarga desde disco

Fin de partida (opcional):
  • Se puede llamar loader.Release()
  • Libera la memoria del prefab
  • Próxima vez se recarga
```

---

## 🧪 TESTING

### Test 1: Verificar Addressables (2 min)

1. **Abre:** Window → Asset Management → Addressables → Groups
2. **Busca:** "UpgradeUI" en la lista
3. **Verifica:** 
   - ✅ Tiene un address asignado
   - ✅ Está en un grupo (no "Default" idealmente)
   - ✅ El address es exactamente: `UpgradeUI`

### Test 2: Build Addressables (1 min)

1. **En la ventana de Addressables Groups**
2. **Click:** Build → New Build → Default Build Script
3. **Espera** a que termine
4. **Verifica** que no haya errores en la Console

### Test 3: Probar en Play Mode (2 min)

1. **Play Mode**
2. **Presiona T** (si tienes UpgradeUITester)
3. **Observa la Console:**
   ```
   UpgradeUILoader: Loading UpgradeUI from Addressables...
   UpgradeUILoader: UpgradeUI loaded successfully via Addressables.
   ```
4. **Verifica:** La UI aparece correctamente

### Test 4: Verificar Caché (1 min)

1. **En Play Mode**, muestra la UI (presiona T)
2. **Cierra** la UI
3. **Vuelve a mostrar** la UI (presiona T otra vez)
4. **Verifica en Console:** NO debe aparecer "Loading..." de nuevo
5. **Resultado esperado:** Usa la instancia cacheada

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### ❌ Error: "Cannot find Addressable with key 'UpgradeUI'"

**Causa:** El prefab no está marcado como Addressable o el nombre no coincide

**Solución:**
1. Selecciona el prefab `UpgradeUI.prefab`
2. Marca como Addressable ✅
3. Verifica que el address sea exactamente: `UpgradeUI`
4. Build Addressables: Build → New Build → Default Build Script

---

### ❌ Error: "Prefab does not have UpgradeUI component"

**Causa:** El prefab raíz no tiene el componente UpgradeUI.cs

**Solución:**
1. Doble-clic en el prefab para abrirlo
2. Selecciona el GameObject raíz (UpgradeCanvas)
3. Add Component → UpgradeUI
4. Conecta todas las referencias
5. Guarda el prefab

---

### ❌ Warning: "UpgradeUI component is missing dependencies"

**Causa:** El Construct() no se está llamando correctamente

**Solución:**
1. Verifica que UpgradeUI tenga el método:
   ```csharp
   [Inject]
   public void Construct(IUpgradeService, ILevelService, ICombatTransitionService)
   ```
2. Asegúrate de que estos servicios estén registrados en GameLifetimeScope
3. El loader llama manualmente a Construct() después de instanciar

---

### ❌ Error: "The LoadHandle is not valid"

**Causa:** El build de Addressables está desactualizado o corrupto

**Solución:**
1. Window → Asset Management → Addressables → Groups
2. Build → Clean Build → All
3. Build → New Build → Default Build Script
4. Reinicia Unity Editor
5. Play Mode de nuevo

---

### ⚠️ Warning: "Multiple instances of UpgradeUI detected"

**Causa:** El prefab se está instanciando manualmente además del loader

**Solución:**
1. Elimina cualquier instancia manual de UpgradeUI en la escena
2. NO arrastres el prefab a la Hierarchy
3. Solo debe existir via Addressables
4. El loader gestiona la única instancia

---

## 📊 Comparación: Addressables vs Prefab Directo

### Ejemplo de Memoria

**Proyecto pequeño (< 10 UIs):**
- Prefab directo: Aceptable
- Addressables: Mejor práctica

**Tu proyecto (con múltiples UIs):**
- ✅ **Addressables es la elección correcta**
- Mantiene consistencia
- Mejor gestión de recursos
- Escalabilidad

---

## 🎯 Checklist de Configuración

### Addressables Setup
- [ ] UpgradeUI.prefab marcado como Addressable
- [ ] Address name es exactamente: `UpgradeUI`
- [ ] Está en un grupo de Assets (no Default)
- [ ] Build de Addressables ejecutado sin errores

### Código
- [x] UpgradeUILoader.cs creado
- [x] GameLifetimeScope registra UpgradeUILoader
- [x] UpgradeUI tiene método Construct()
- [ ] Constante UPGRADE_UI_ADDRESS coincide con el address

### Testing
- [ ] Build Addressables completado
- [ ] Play Mode sin errores
- [ ] UI se carga correctamente
- [ ] Segunda carga usa caché (no recarga)

### Limpieza
- [ ] NO hay instancias manuales en la escena
- [ ] NO hay referencias de prefab directo en GameLifetimeScope
- [ ] Documentación anterior actualizada

---

## 🚀 Próximos Pasos

### 1. Configuración Inicial (5 min)
1. Marca el prefab como Addressable
2. Build Addressables
3. Verifica que funciona en Play Mode

### 2. Testing Completo (10 min)
1. Test de carga inicial
2. Test de caché
3. Test de flujo completo de juego

### 3. Optimización (Opcional)
- Configura el grupo de Addressables para optimize load time
- Considera preload de UI si sabes que se usará pronto
- Implementa Release() al final de una run si quieres liberar memoria

---

## 📚 Recursos Adicionales

### Archivos Creados/Modificados
- ✅ `UpgradeUILoader.cs` (nuevo)
- ✅ `GameLifetimeScope.cs` (modificado)
- 📄 Este archivo (guía de Addressables)

### Archivos Relacionados
- `UIManager.cs` (tu implementación de referencia)
- `ShowUIPanelTask.cs` (ejemplo de uso de Addressables)
- Prefab: `Assets/Prefabs/UI/UpgradeUI.prefab`

### Documentación Unity
- Unity Addressables Official Docs
- AssetReference API
- AsyncOperationHandle

---

## 💡 Tips Pro

### 1. Preload (si la UI se usa frecuentemente)

Si sabes que vas a necesitar la UI pronto:

```csharp
// En algún lugar al inicio del nivel
await _upgradeUILoader.PreloadAsync();
```

### 2. Release al final de la Run

Si quieres optimizar memoria entre runs:

```csharp
// Al volver al menú principal
_upgradeUILoader.Release();
```

### 3. Grupos de Addressables

Considera agrupar tus UIs:
- **UI_Combat:** UIs usadas en combate
- **UI_Meta:** UIs de menús/upgrades
- **UI_HUD:** UIs siempre visibles

---

## ✅ Ventajas de tu Configuración

1. ✅ **Consistente** con el resto del proyecto
2. ✅ **Optimizada** para memoria
3. ✅ **Escalable** cuando añadas más UIs
4. ✅ **Mantenible** - un patrón para todas las UIs
5. ✅ **Profesional** - industry standard

---

**¡Configuración Addressables completada! 🎉**

Tu UpgradeUI ahora se carga dinámicamente via Addressables, igual que el resto de las UIs de tu proyecto, manteniendo consistencia y optimizando el uso de recursos.
