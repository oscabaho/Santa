# 📦 Dependencias del Proyecto Santa

Lista completa de packages y dependencias externas utilizadas en el proyecto.

## 📋 Tabla de Contenidos

- [Unity Packages](#unity-packages)
- [Third-Party Packages](#third-party-packages)
- [Unity Modules](#unity-modules)
- [Versiones y Compatibilidad](#versiones-y-compatibilidad)
- [Instalación de Packages](#instalación-de-packages)

---

## Unity Packages

### Core Frameworks

#### VContainer
- **Versión**: Latest from GitHub
- **Fuente**: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer`
- **Propósito**: Dependency Injection container optimizado para Unity
- **Razón de uso**:
  - Más rápido que Zenject
  - Menor footprint de memoria
  - API limpia y fácil de usar
  - Excelente para móviles
- **Documentación**: [VContainer Docs](https://vcontainer.hadashikick.jp/)

#### UniTask
- **Versión**: Latest from GitHub
- **Fuente**: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`
- **Propósito**: Zero-allocation async/await para Unity
- **Razón de uso**:
  - Sin allocations de memoria (importante para móviles)
  - Más rápido que Coroutines y Task estándar
  - API familiar (async/await)
  - Integración con Unity lifecycle
- **Documentación**: [UniTask GitHub](https://github.com/Cysharp/UniTask)

---

### Unity Official Packages

#### Addressables
- **Package**: `com.unity.addressables`
- **Versión**: Sistema (instalado automáticamente)
- **Propósito**: Asset management dinámico
- **Razón de uso**:
  - Carga dinámica de assets
  - Reducción de build size
  - Posibilidad de content updates remotos
  - Mejor gestión de memoria

#### Addressables Android
- **Package**: `com.unity.addressables.android`
- **Versión**: 1.0.9
- **Propósito**: Soporte específico de Addressables para Android
- **Razón de uso**:
  - Optimizaciones para Android
  - Asset bundles eficientes para móvil

---

### Rendering & Graphics

#### Universal Render Pipeline (URP)
- **Package**: `com.unity.render-pipelines.universal`
- **Versión**: 17.0.4
- **Propósito**: Render pipeline optimizado
- **Razón de uso**:
  - Optimizado para móviles
  - Alto rendimiento
  - Soporte para features modernos
  - Extensible con custom render features

#### Post-Processing
- **Package**: `com.unity.postprocessing`
- **Versión**: 3.5.1
- **Propósito**: Efectos visuales post-procesamiento
- **Razón de uso**:
  - Bloom, color grading, ambient occlusion
  - Mejora estética del juego
  - Integración con URP

---

### Animation & Camera

#### Cinemachine
- **Package**: `com.unity.cinemachine`
- **Versión**: 3.1.5
- **Propósito**: Sistema de cámaras procedurales
- **Razón de uso**:
  - Cámaras dinámicas para combate
  - Transiciones suaves
  - Follow cameras para exploración
  - Target selection camera

#### Timeline
- **Package**: `com.unity.timeline`
- **Versión**: 1.8.10
- **Propósito**: Secuencias cinemáticas
- **Razón de uso**:
  - Cutscenes
  - Animaciones complejas
  - Secuencias de combate especiales

#### Animation Rigging
- **Package**: `com.unity.animation.rigging`
- **Versión**: Incluido en feature set
- **Propósito**: Rigging procedural
- **Razón de uso**:
  - IK para personajes
  - Animaciones dinámicas
  - Look-at systems

---

### Input

#### Input System
- **Package**: `com.unity.inputsystem`
- **Versión**: 1.15.0
- **Propósito**: New Input System
- **Razón de uso**:
  - Soporte multi-plataforma (PC, móvil, consolas)
  - Virtual gamepad
  - Rebinding de controles
  - Input actions

---

### AI & Navigation

#### AI Navigation
- **Package**: `com.unity.ai.navigation`
- **Versión**: 2.0.9
- **Propósito**: NavMesh y pathfinding
- **Razón de uso**:
  - Pathfinding para enemigos
  - Navegación en exploración
  - NavMesh dinámico

---

### Performance & Optimization

#### Burst Compiler
- **Package**: `com.unity.burst`
- **Versión**: 1.8.27
- **Propósito**: Compilador de alto rendimiento
- **Razón de uso**:
  - Optimización de código crítico
  - SIMD auto-vectorización
  - Compatible con Jobs System

#### Collections
- **Package**: `com.unity.collections`
- **Versión**: Sistema
- **Propósito**: Colecciones nativas de alto rendimiento
- **Razón de uso**:
  - NativeArray, NativeList
  - Compatibilidad con Jobs
  - Mejor performance que colecciones managed

#### Mathematics
- **Package**: `com.unity.mathematics`
- **Versión**: Sistema
- **Propósito**: Librería matemática optimizada
- **Razón de uso**:
  - SIMD-friendly math operations
  - Compatible con Burst
  - Mejor performance que UnityEngine.Vector3

---

### Development Tools

#### Test Framework
- **Package**: `com.unity.test-framework`
- **Versión**: 1.6.0
- **Propósito**: Unit testing
- **Razón de uso**:
  - Tests unitarios para lógica de negocio
  - Integration tests
  - Asegurar calidad del código

#### Visual Studio Editor
- **Package**: `com.unity.ide.visualstudio`
- **Versión**: 2.0.26
- **Propósito**: Integración con Visual Studio

#### Rider Editor
- **Package**: `com.unity.ide.rider`
- **Versión**: 3.0.38
- **Propósito**: Integración con JetBrains Rider

---

### Feature Sets

#### Characters & Animation
- **Package**: `com.unity.feature.characters-animation`
- **Versión**: 1.0.0
- **Propósito**: Bundle de packages para personajes

#### Cinematic
- **Package**: `com.unity.feature.cinematic`
- **Versión**: 1.0.0
- **Propósito**: Bundle de packages para cinemáticas

#### Gameplay & Storytelling
- **Package**: `com.unity.feature.gameplay-storytelling`
- **Versión**: 1.0.0
- **Propósito**: Bundle de packages para gameplay

#### Worldbuilding
- **Package**: `com.unity.feature.worldbuilding`
- **Versión**: 1.0.1
- **Propósito**: Bundle de packages para world building

---

### Tools & Utilities

#### ProBuilder
- **Package**: Incluido en feature set
- **Propósito**: Modelado in-editor
- **Razón de uso**:
  - Prototipado rápido de niveles
  - Greyboxing
  - Level design

#### Polybrush
- **Package**: Incluido en feature set
- **Propósito**: Mesh painting y decoración
- **Razón de uso**:
  - Vertex coloring
  - Textura de meshes
  - Scatter de prefabs

#### Recorder
- **Package**: `com.unity.recorder`
- **Versión**: Sistema
- **Propósito**: Grabación de gameplay
- **Razón de uso**:
  - Captura de screenshots
  - Grabación de videos para marketing
  - Debug visual

---

### Version Control

#### Collaborate Proxy
- **Package**: `com.unity.collab-proxy`
- **Versión**: 2.9.3
- **Propósito**: Integración con Unity Version Control / Plastic SCM

---

## Third-Party Packages

### Autodesk FBX Exporter
- **Packages**: 
  - `Autodesk.Fbx`
  - `Autodesk.Fbx.Editor`
  - `Autodesk.Fbx.BuildTestAssets`
- **Propósito**: Exportación de FBX
- **Razón de uso**: Workflow con herramientas 3D externas

---

## Unity Modules

El proyecto utiliza los siguientes Unity modules:

```json
{
  "com.unity.modules.accessibility": "1.0.0",
  "com.unity.modules.ai": "1.0.0",
  "com.unity.modules.androidjni": "1.0.0",
  "com.unity.modules.animation": "1.0.0",
  "com.unity.modules.assetbundle": "1.0.0",
  "com.unity.modules.audio": "1.0.0",
  "com.unity.modules.cloth": "1.0.0",
  "com.unity.modules.director": "1.0.0",
  "com.unity.modules.imageconversion": "1.0.0",
  "com.unity.modules.imgui": "1.0.0",
  "com.unity.modules.jsonserialize": "1.0.0",
  "com.unity.modules.particlesystem": "1.0.0",
  "com.unity.modules.physics": "1.0.0",
  "com.unity.modules.physics2d": "1.0.0",
  "com.unity.modules.screencapture": "1.0.0",
  "com.unity.modules.terrain": "1.0.0",
  "com.unity.modules.terrainphysics": "1.0.0",
  "com.unity.modules.tilemap": "1.0.0",
  "com.unity.modules.ui": "1.0.0",
  "com.unity.modules.uielements": "1.0.0",
  "com.unity.modules.umbra": "1.0.0",
  "com.unity.modules.unityanalytics": "1.0.0",
  "com.unity.modules.unitywebrequest": "1.0.0",
  "com.unity.modules.unitywebrequestassetbundle": "1.0.0",
  "com.unity.modules.unitywebrequestaudio": "1.0.0",
  "com.unity.modules.unitywebrequesttexture": "1.0.0",
  "com.unity.modules.unitywebrequestwww": "1.0.0",
  "com.unity.modules.vehicles": "1.0.0",
  "com.unity.modules.video": "1.0.0",
  "com.unity.modules.vr": "1.0.0",
  "com.unity.modules.wind": "1.0.0",
  "com.unity.modules.xr": "1.0.0"
}
```

---

## Versiones y Compatibilidad

### Requisitos Mínimos

- **Unity**: 6.0.x o superior
- **C#**: .NET Standard 2.1
- **.NET Framework**: 4.7.2+

### Plataformas Soportadas

- **Windows**: Standalone x64
- **Android**: API Level 24+ (Android 7.0+), ARM64
- **iOS**: iOS 13.0+, ARM64

### Versión Recomendada de Unity

**Unity 6.0.30f1** (o la versión LTS más reciente de Unity 6.0)

---

## Instalación de Packages

### Package Manager (Automático)

La mayoría de packages se instalan automáticamente al abrir el proyecto en Unity.

### Instalación Manual de VContainer

1. Abre `Window → Package Manager`
2. Click en `+` → `Add package from git URL`
3. Pega: `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer`
4. Click `Add`

### Instalación Manual de UniTask

1. Abre `Window → Package Manager`
2. Click en `+` → `Add package from git URL`
3. Pega: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`
4. Click `Add`

---

## Actualización de Packages

### Actualizar Package Oficial de Unity

1. Abre `Window → Package Manager`
2. Selecciona el package
3. Click en `Update to X.X.X`

### Actualizar Packages de Git

1. Edita `Packages/manifest.json`
2. Actualiza la URL o añade `#version` al final:
   ```json
   "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.4"
   ```
3. Unity lo actualizará automáticamente

---

## Verificación de Dependencias

### Script de Verificación

Puedes verificar que todas las dependencias estén instaladas correctamente:

1. Abre `Tools → Santa → Verify Dependencies`
2. Revisa el console para confirmar

### Troubleshooting

#### Package no se resuelve
```bash
# Limpiar cache de packages
rm -rf Library/PackageCache
# Unity re-descargará los packages al abrir el proyecto
```

#### Conflictos de versión
- Asegúrate de usar Unity 6.0.x
- Verifica que no haya packages duplicados en `Package Manager`

---

## Dependencias por Funcionalidad

### Combat System
- UniTask (async combat flow)
- VContainer (DI para managers)
- Addressables (arenas de combate)

### UI System
- Addressables (dynamic panel loading)
- UniTask (async loading)
- Input System (virtual gamepad)

### Save System
- UniTask (async save/load)
- VContainer (service injection)

### Visual Effects
- URP (rendering)
- Post-Processing (efectos visuales)
- Addressables (VFX prefabs)

### Camera System
- Cinemachine (camera management)
- Timeline (secuencias)

---

## License Information

### VContainer
- **License**: MIT License
- **Copyright**: hadashiA

### UniTask
- **License**: MIT License
- **Copyright**: Cysharp, Inc.

### Unity Packages
- **License**: Unity Companion License
- Consulta [Unity Package Licensing](https://unity.com/legal/licenses/unity-companion-license)

---

## Actualizaciones Futuras Planeadas

> [!NOTE]
> Packages considerados para futuras actualizaciones:

- **DOTween** - Animaciones de UI más avanzadas
- **TextMeshPro Upgrade** - Mejores efectos de texto
- **Odin Inspector** - Editor mejorado (opcional, paid)

---

## Manifiesto Completo

Para ver el manifiesto completo de packages:

**Ubicación**: `Packages/manifest.json`

```json
{
  "scopedRegistries": [
    {
      "name": "VContainer",
      "url": "https://registry.npmjs.com",
      "scopes": ["dev.hadashi.vcontainer"]
    }
  ],
  "dependencies": {
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unity.addressables.android": "1.0.9",
    "com.unity.ai.navigation": "2.0.9",
    "com.unity.burst": "1.8.27",
    "com.unity.cinemachine": "3.1.5",
    "com.unity.inputsystem": "1.15.0",
    "com.unity.postprocessing": "3.5.1",
    "com.unity.render-pipelines.universal": "17.0.4",
    "com.unity.test-framework": "1.6.0",
    "com.unity.timeline": "1.8.10",
    "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer"
  }
}
```

---

**Última actualización**: Enero 2026
