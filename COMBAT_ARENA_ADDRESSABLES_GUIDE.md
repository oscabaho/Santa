# 🎮 Guía Completa: Arenas de Combate con Addressables

## 📋 Introducción

Esta guía te llevará paso a paso para configurar arenas de combate usando Addressables en tu proyecto Unity.

**¿Por qué Addressables?**
- ✅ Carga asíncrona (sin congelar el juego)
- ✅ Pooling automático (reutiliza instancias)
- ✅ Control de memoria (ideal para mobile)
- ✅ APK más pequeño (assets fuera del build)

**Tiempo estimado:** 10-15 minutos por arena

---

## 📝 Índice

1. [Verificar tu Prefab de Arena](#paso-1-verificar-tu-prefab-de-arena)
2. [Marcar como Addressable](#paso-2-marcar-como-addressable)
3. [Configurar CombatEncounter](#paso-3-configurar-combatencounter)
4. [Build Addressables](#paso-4-build-addressables)
5. [Testing en Play Mode](#paso-5-testing-en-play-mode)
6. [Optimización (Opcional)](#paso-6-optimización-opcional)
7. [Troubleshooting](#troubleshooting)

---

## 🔧 Paso 1: Verificar tu Prefab de Arena

### 1.1 Requisito Obligatorio

Tu prefab de arena **debe tener** el componente `CombatArena` en el GameObject root.

**Estructura mínima requerida:**

```
CombatArena_Forest (Root GameObject)
├── 📦 CombatArena (Component) ← ¡OBLIGATORIO!
├── Ground
├── Props
├── Lighting
└── SpawnPoints
```

### 1.2 Verificar el Componente

1. En el **Project**, navega a tu prefab: `Assets/Prefabs/Combat/`
2. **Doble-clic** en el prefab para abrirlo en Prefab Mode
3. **Selecciona** el GameObject root (el primero de la lista)
4. En el **Inspector**, verifica que tiene el componente `CombatArena`

Si no lo tiene:
- Click en **Add Component**
- Busca `CombatArena`
- Configura los enemigos/participantes del combate

5. **Guarda** el prefab (`Ctrl + S`)
6. **Cierra** el Prefab Mode (flecha ← arriba izquierda)

---

## 🔧 Paso 2: Marcar como Addressable

### 2.1 Activar Addressables en el Prefab

1. En el **Project**, **selecciona** tu prefab (NO lo abras, solo selecciónalo)
2. En el **Inspector**, busca la casilla **"Addressable"** (parte superior)
3. **Marca** la casilla ☑️

![Inspector con Addressable checkbox marcado]

### 2.2 Configurar el Address

Después de marcar la casilla, verás más opciones. Configura:

#### Address (Campo más importante):

Escribe un nombre **descriptivo** y **único**:

✅ **Buenos ejemplos:**
- `CombatArena_Forest`
- `CombatArena_Desert_01`
- `CombatArena_Boss_Dragon`
- `CombatArena_Cave_Night`

❌ **Malos ejemplos:**
- `Arena1` (genérico)
- `Scene` (ambiguo)
- `CombatArena` (sin contexto)

**Regla:** Si tienes varias arenas similares, añade un número o variante al final.

#### Group:

- Si no existe, crea un grupo: **"Combat_Assets"** o **"Arenas"**
- Si ya tienes arenas, usa el mismo grupo

#### Labels (Opcional):

Puedes añadir etiquetas como: `combat`, `arena`, `level1`

### 2.3 Convención Recomendada

Para mantener orden en proyectos grandes:

```
CombatArena_[Bioma]_[Variante]

Ejemplos:
• CombatArena_Forest_01
• CombatArena_Forest_02  
• CombatArena_Desert_Boss
• CombatArena_Cave_Elite
```

### 2.4 Verificar en Addressables Groups

1. Abre: `Window → Asset Management → Addressables → Groups`
2. Busca tu arena en la lista
3. Verifica que aparece con el address correcto

**Si no aparece:** Repite el paso 2.1 (marcar la casilla).

---

## 🔧 Paso 3: Configurar CombatEncounter

Ahora vas a decirle al juego **qué arena** cargar cuando el jugador active un combate.

### 3.1 Localizar el CombatEncounter

En tu escena de exploración:

1. En la **Hierarchy**, busca el GameObject que tiene el trigger de combate
   - Normalmente se llama: `CombatTrigger_01`, `Encounter_01`, etc.
2. **Selecciónalo**
3. En el **Inspector**, verás el componente `CombatEncounter`

### 3.2 Configurar los Campos

#### Scene Setup - Addressables:

**Combat Scene Address:** `CombatArena_Forest`
- ⚠️ Debe ser **exactamente** el mismo nombre que configuraste en el Paso 2.2
- **Sensible a mayúsculas/minúsculas**
- Sin espacios extras

#### Combat Camera:

**Combat Camera:** Arrastra tu `MainCombatCamera` o la cámara que usas para combates
- Usualmente está en: `CombatCameras → MainCombatCamera`

#### Pooling (Configuración de Performance):

**Auto Prewarm:** ☑️ Activar
- **¿Qué hace?** Precarga la arena al iniciar el nivel
- **Cuándo activar:** En la **primera arena** que el jugador encontrará
- **Resultado:** Elimina el "hitch" o pausa al iniciar el primer combate

**Prewarm Count:** `1`
- **Mobile:** `1` instancia (ahorra memoria)
- **PC/Console:** `2-3` instancias (transiciones más rápidas)

**Release Addressables Instances:** ☐ Desactivar (por defecto)
- **¿Qué hace?** Libera la memoria después de usar la arena
- **Cuándo activar:**
  - Arenas de boss (se usan una sola vez)
  - Arenas muy pesadas (>80 MB)
  - Dispositivos con poca RAM (<2 GB)

### 3.3 Ejemplo de Configuración

```
CombatEncounter (Inspector)
══════════════════════════════════════
Scene Setup - Addressables
  Combat Scene Address: CombatArena_Forest_01

Combat Camera
  Combat Camera: MainCombatCamera

Pooling
  ☑ Auto Prewarm
  Prewarm Count: 1
  ☐ Release Addressables Instances
══════════════════════════════════════
```

### 3.4 Guardar

- **Guarda** la escena: `Ctrl + S`

---

## 🔧 Paso 4: Build Addressables

Antes de probar, necesitas "buildear" los Addressables. Esto empaqueta los assets.

### 4.1 Abrir Addressables Groups

1. `Window → Asset Management → Addressables → Groups`

### 4.2 Verificar tu Arena

En la ventana Groups:
- ✅ Verifica que tu arena aparece en la lista
- ✅ Verifica que el address es correcto
- ❌ Si no aparece, vuelve al Paso 2

### 4.3 Build

1. En la ventana Groups, click: `Build → New Build → Default Build Script`
2. **Espera** (puede tomar 1-5 minutos)
3. **No cierres** Unity mientras hace el build

### 4.4 Verificar el Build

Después del build:
- ✅ La Console debe mostrar "Build completed"
- ✅ Se debe crear la carpeta `ServerData/` en tu proyecto
- ❌ Si hay errores en rojo, anótalos y ve a [Troubleshooting](#troubleshooting)

---

## 🔧 Paso 5: Testing en Play Mode

¡Hora de probar!

### 5.1 Test Básico

1. **Play Mode** en Unity
2. En la **Console**, busca mensajes de prewarm:
   ```
   CombatScenePool: Prewarming 1 instance(s) for key 'CombatArena_Forest'
   CombatScenePool: Prewarm completed for 'CombatArena_Forest'
   ```

3. **Navega** hasta el trigger de combate
4. **Activa** el combate (entra en el trigger)

### 5.2 ¿Qué Verificar?

✅ **La arena se carga correctamente**
- No hay pantalla negra
- Los modelos/meshes aparecen

✅ **No hay hitches o freezes**
- El juego no se congela al cargar
- La transición es suave

✅ **La Hierarchy muestra la instancia**
- Busca: `CombatArena_Forest(Clone)`
- Debe estar bajo `CombatScenePool`

✅ **El combate inicia normalmente**
- Los enemigos aparecen
- El UI de combate funciona

### 5.3 Test de Pooling (Opcional)

Para verificar que el pooling funciona:

1. **Completa** el primer combate
2. **Inicia** un segundo combate (del mismo tipo)
3. En la **Console**, verifica:
   ```
   Primera vez:
   CombatScenePool: Loading from Addressables: 'CombatArena_Forest'
   
   Segunda vez:
   CombatScenePool: Reusing pooled instance for 'CombatArena_Forest'
   ```

---

## 🔧 Paso 6: Optimización (Opcional)

### 6.1 Ajustar Prewarm según Plataforma

Si desarrollas para **múltiples plataformas**, ajusta `Prewarm Count`:

| Plataforma | Prewarm Count | Razón |
|------------|---------------|-------|
| **Mobile** | 1 | Ahorra memoria |
| **PC** | 2-3 | Transiciones rápidas |
| **Console** | 2 | Balance |

### 6.2 Liberar Memoria en Arenas Pesadas

Si una arena usa mucha memoria (>80 MB):

1. Selecciona el `CombatEncounter`
2. Activa: **Release Addressables Instances** ☑️

**Trade-off:** La arena se recargará si se necesita de nuevo.

### 6.3 Organizar por Niveles

Para proyectos grandes, crea grupos por nivel:

```
Addressables Groups:
├── Level1_Combat_Assets
│   ├── CombatArena_Forest_01
│   └── CombatArena_Forest_02
├── Level2_Combat_Assets
│   ├── CombatArena_Desert_01
│   └── CombatArena_Desert_Boss
└── Boss_Combat_Assets
    └── CombatArena_FinalBoss
```

---

## 🐛 Troubleshooting

### Problema 1: "No combatSceneAddress configured"

**Síntoma:** Mensaje de error al intentar iniciar combate

**Causa:** El campo `Combat Scene Address` está vacío

**Solución:**
1. Selecciona el GameObject con `CombatEncounter`
2. En el Inspector, rellena `Combat Scene Address`
3. Usa el **mismo nombre** que en Addressables (Paso 2.2)

---

### Problema 2: "InvalidKeyException: No Location found"

**Síntoma:** Error al cargar la arena

**Causa:** El address no existe o no fue buildeado

**Solución:**
1. Abre `Window → Addressables → Groups`
2. Verifica que el prefab está marcado como Addressable
3. Verifica que el **address coincide** con `combatSceneAddress`
4. Haz un **Rebuild**: `Build → New Build → Default Build Script`
5. **Reinicia** Play Mode

---

### Problema 3: Hitch/Freeze al cargar la primera arena

**Síntoma:** Pausa de 0.5-2 segundos al iniciar el combate

**Causa:** No hay prewarm configurado

**Solución:**
1. Selecciona el `CombatEncounter` de la **primera arena**
2. Activa: **Auto Prewarm** ☑️
3. **Prewarm Count:** `1` (mobile) o `2` (PC)
4. La arena se precargará al inicio del nivel

---

### Problema 4: La arena no aparece en Addressables Groups

**Síntoma:** No ves tu prefab en la ventana Addressables Groups

**Solución:**
1. Selecciona el prefab en el Project
2. En el Inspector, **desmarca** y **vuelve a marcar** la casilla Addressable
3. Verifica que se asignó un address
4. Refresca la ventana Groups: `Tools → Addressables → Window → Groups`

---

### Problema 5: "UNITY_ADDRESSABLES is not defined"

**Síntoma:** Error de compilación

**Causa:** El paquete Addressables no está instalado

**Solución:**
1. `Window → Package Manager`
2. Busca: **"Addressables"**
3. Click: **Install**
4. Espera a que Unity recompile

---

## 📊 Configuración Recomendada por Tipo de Arena

| Tipo de Arena | Auto Prewarm | Prewarm Count | Release Instances |
|---------------|--------------|---------------|-------------------|
| **Primera Arena** (tutorial) | ✅ Sí | 1-2 | ❌ No |
| **Arenas Comunes** (se repiten) | ✅ Sí | 1 | ❌ No |
| **Arenas Pesadas** (>80 MB) | ❌ No | 0-1 | ✅ Sí |
| **Arenas de Boss** (one-time) | ❌ No | 0 | ✅ Sí |
| **Mobile Low-End** (<2 GB RAM) | ⚠️ Depende | 0-1 | ✅ Sí |

---

## ✅ Checklist Final

Antes de considerar completa la configuración:

### Por Cada Arena:
- [ ] Prefab tiene componente `CombatArena` en el root
- [ ] Prefab marcado como Addressable
- [ ] Address asignado (descriptivo y único)
- [ ] Aparece en Addressables Groups

### Por Cada CombatEncounter:
- [ ] `Combat Scene Address` configurado
- [ ] Address coincide EXACTAMENTE con el de Addressables
- [ ] `Combat Camera` asignada
- [ ] Opciones de pooling configuradas según tipo de arena

### Build y Testing:
- [ ] Addressables buildeados sin errores
- [ ] Test en Play Mode: arena carga correctamente
- [ ] No hay hitches/freezes al iniciar combates
- [ ] El pooling funciona (segunda vez reutiliza instancia)

---

## 📚 Referencias Adicionales

### Documentos Relacionados:
- `TESTSCENE_SETUP_GUIDE.md` → Configuración completa de escena de prueba
- `README_LOGGING_AND_POOLING.md` → Detalles técnicos del pool
- `UPGRADE_UI_VCONTAINER_INTEGRATION.md` → Sección de arenas de combate

### Archivos de Código:
- `Assets/Scripts/Gameplay/CombatEncounter.cs` - Configuración por encuentro
- `Assets/Scripts/Core/CombatScenePool.cs` - Pooling y Addressables
- `Assets/Scripts/Core/ICombatEncounter.cs` - Interfaz

### Unity Docs:
- [Addressables Documentation](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [Memory Management Best Practices](https://docs.unity3d.com/Manual/performance-memory-overview.html)

---

**¡Configuración completa! 🎉**

Tu sistema de arenas ahora usa Addressables para carga optimizada, pooling eficiente y control granular de memoria.
