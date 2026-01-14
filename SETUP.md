# 🛠️ Guía de Instalación y Configuración - Proyecto Santa

Guía paso a paso para configurar el proyecto desde cero.

---

## 📋 Requisitos Previos

### Software Requerido

- **Unity Hub** (versión más reciente)
- **Unity 6.0.30f1** o superior (LTS recomendado)
- **Git** 2.30+
- **Visual Studio 2022** o **JetBrains Rider** (recomendado para C#)

### Especificaciones de Hardware

#### Mínimo
- **CPU**: Intel i5 / AMD Ryzen 5
- **RAM**: 8 GB
- **GPU**: DirectX 11 compatible
- **Almacenamiento**: 10 GB libres

#### Recomendado
- **CPU**: Intel i7 / AMD Ryzen 7
- **RAM**: 16 GB
- **GPU**: NVIDIA GTX 1060 / AMD RX 580 o superior
- **Almacenamiento**: SSD con 20 GB libres

---

## 🚀 Instalación Inicial

### Paso 1: Clonar el Repositorio

```bash
# Opción A: HTTPS
git clone https://github.com/osbaho/Santa.git
cd Santa

# Opción B: SSH (requiere configuración de SSH key)
git clone git@github.com:osbaho/Santa.git
cd Santa
```

### Paso 2: Abrir en Unity Hub

1. Abre **Unity Hub**
2. Click en **"Add"** → **"Add project from disk"**
3. Navega a la carpeta `Santa` clonada
4. Selecciona la carpeta y click **"Add Project"**

> [!WARNING]
> Asegúrate de tener **Unity 6.0.x** instalado. Si no, Unity Hub te pedirá instalarlo.

### Paso 3: Primera Apertura del Proyecto

1. En Unity Hub, click en el proyecto **Santa**
2. Unity comenzará a:
   - Importar assets (~10-15 minutos la primera vez)
   - Resolver dependencias de packages
   - Compilar scripts

> [!NOTE]
> La primera importación puede tardar. No interrumpas el proceso.

---

## ⚙️ Configuración de Addressables

Los Addressables son críticos para el proyecto. Deben configurarse antes de ejecutar.

### Construir Addressables Groups

1. En Unity, abre `Window → Asset Management → Addressables → Groups`
2. Si ves el mensaje "Create Addressables Settings", click en **"Create Addressables Settings"**
3. Click en `Build → New Build → Default Build Script`
4. Espera a que termine el build (~2-5 minutos)

### Verificar Grupos de Addressables

Deberías ver los siguientes grupos:

- **UI_Panels** - Panels de UI (CombatUI, UpgradeUI, etc.)
- **Combat_Arenas** - Escenas de combate
- **VFX** - Efectos visuales
- **Audio** - Música y SFX

### Rebuilding Assets

Si modificas assets addressables:

```
Window → Asset Management → Addressables → Groups
Build → Clean All → Build Content
```

---

## 🎮 Configuración de Escenas

### Escena Principal

La escena de bootstrap es: `Assets/Scenes/Bootstrap.unity`

### Build Settings

1. Abre `File → Build Settings`
2. Verifica que las siguientes escenas estén en **Scenes In Build**:
   - `Bootstrap` (índice 0)
   - Otras escenas de exploración

3. Configuración de plataforma:

#### Para PC/Standalone
```
Platform: PC, Mac & Linux Standalone
Target Platform: Windows
Architecture: x86_64
```

#### Para Android
```
Platform: Android
Texture Compression: ASTC
Minimum API Level: 24 (Android 7.0)
Target API Level: 33 (Android 13)
Scripting Backend: IL2CPP
Target Architectures: ARM64
```

---

## 🔧 Configuración del Editor

### Project Settings

#### Graphics
1. `Edit → Project Settings → Graphics`
2. Verifica que **Scriptable Render Pipeline Settings** esté asignado:
   - Debe apuntar a `UniversalRenderPipelineAsset`

#### Quality
1. `Edit → Project Settings → Quality`
2. Configurar niveles de calidad:
   - **Low**: Móviles gama baja
   - **Medium**: Móviles gama media
   - **High**: PC y móviles gama alta

#### Input System
1. `Edit → Project Settings → Player`
2. **Active Input Handling**: `Input System Package (New)`

---

## 🎨 Configuración de VContainer

El `GameLifetimeScope` debe estar en la escena bootstrap.

### Verificar GameLifetimeScope

1. Abre la escena `Bootstrap.unity`
2. Busca el GameObject **"[GameLifetimeScope]"** en la jerarquía
3. Verifica que el componente `GameLifetimeScope` tenga asignados:
   - InputReader
   - UIManager
   - TurnBasedCombatManager
   - Otros managers principales

### Si falta GameLifetimeScope

1. Crea un GameObject vacío: **"[GameLifetimeScope]"**
2. Añade el componente `GameLifetimeScope`
3. Arrastra las referencias necesarias desde la carpeta `Assets/Prefabs/Managers`

---

## 🧪 Verificar Instalación

### Test Básico

1. Abre la escena `Bootstrap.unity`
2. Presiona **Play** ▶️
3. Verifica:
   - No hay errores en Console
   - Virtual Gamepad se carga correctamente
   - Puedes moverte con WASD

### Test de Combat

1. En la escena de exploración, acércate a un enemigo
2. Presiona `E` para interactuar
3. Verifica:
   - Transición a combate funciona
   - CombatUI se carga
   - Puedes seleccionar abilities

---

## 📦 Configuración de Packages

### Verificar Packages Instalados

1. `Window → Package Manager`
2. Verifica que estén instalados:
   - ✅ VContainer
   - ✅ UniTask
   - ✅ Addressables
   - ✅ Input System
   - ✅ Cinemachine
   - ✅ URP

### Reinstalar Package (si falta)

#### VContainer
```
Window → Package Manager → + → Add package from git URL
https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer
```

#### UniTask
```
Window → Package Manager → + → Add package from git URL
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

---

## 🐛 Troubleshooting

### Error: "Assembly references not found"

**Solución**:
```
Assets → Reimport All
```

### Error: "Addressables not built"

**Solución**:
```
Window → Asset Management → Addressables → Groups
Build → New Build → Default Build Script
```

### Error: "VContainer namespace not found"

**Solución**:
1. Verifica que VContainer esté instalado en Package Manager
2. Si no está, instálalo manualmente (ver arriba)
3. Reinicia Unity

### Console lleno de warnings

**Solución**: Algunos warnings son normales durante desarrollo. Ignorar a menos que impidan ejecución.

### Performance Issues en Editor

**Solución**:
1. `Edit → Preferences → GI Cache → Clean Cache`
2. `Window → Analysis → Profiler` para identificar cuellos de botella
3. Reducir calidad gráfica en Editor si es necesario

---

## 🔐 Configuración Opcional

### Git LFS (Large File Storage)

Si trabajas con assets grandes:

```bash
git lfs install
git lfs track "*.png"
git lfs track "*.psd"
git lfs track "*.fbx"
git add .gitattributes
git commit -m "Configure Git LFS"
```

### Unity Collaborate / Plastic SCM

Si usas Unity Version Control:

1. `Window → Plastic SCM`
2. Sigue el wizard de configuración
3. Conecta al repositorio del equipo

---

## 📱 Build para Móviles

### Android Setup

#### Requisitos
- **Android Studio** con SDK instalado
- **JDK 11** o superior

#### Pasos
1. `File → Build Settings → Android → Switch Platform`
2. `Player Settings`:
   - **Package Name**: `com.studio.santa`
   - **Minimum API Level**: 24
   - **Target API Level**: 33
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARM64
3. `Build Settings → Build` o `Build And Run`

### iOS Setup

#### Requisitos
- **macOS** con Xcode instalado
- **Apple Developer Account**

#### Pasos
1. `File → Build Settings → iOS → Switch Platform`
2. `Player Settings`:
   - **Bundle Identifier**: `com.studio.santa`
   - **Target Minimum iOS Version**: 13.0
   - **Architecture**: ARM64
3. `Build Settings → Build`
4. Abre el proyecto Xcode generado
5. Configura signing y provisioning
6. Build desde Xcode

---

## 🎯 Configuración para Desarrollo

### Symbols de Compilación

Agregar símbolos para debugging:

1. `Edit → Project Settings → Player → Other Settings`
2. **Scripting Define Symbols**: `DEVELOPMENT_BUILD;ENABLE_LOGS`

### Inspector Avanzado

Habilitar debug mode en Inspector:

1. Click en el menú de 3 puntos en la esquina superior derecha del Inspector
2. Selecciona **"Debug"**
3. Ahora puedes ver campos privados

---

## ✅ Checklist de Setup Completo

- [ ] Repositorio clonado
- [ ] Proyecto abierto en Unity 6.0.x
- [ ] Addressables construidos exitosamente
- [ ] Escena Bootstrap carga sin errores
- [ ] GameLifetimeScope configurado
- [ ] Packages verificados en Package Manager
- [ ] Virtual Gamepad se carga
- [ ] Combat system funciona
- [ ] Build settings configurados para tu plataforma target
- [ ] Test básico completado exitosamente

---

## 📚 Próximos Pasos

Después de completar el setup:

1. Lee [ARCHITECTURE.md](ARCHITECTURE.md) para entender la estructura
2. Explora [SYSTEMS.md](SYSTEMS.md) para ver cómo funcionan los sistemas
3. Revisa [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) si trabajarás en combate
4. Consulta [CONTRIBUTING.md](CONTRIBUTING.md) antes de hacer cambios

---

## 🆘 Soporte

Si encuentras problemas:

1. Revisa la sección [Troubleshooting](#🐛-troubleshooting)
2. Busca en [Issues del repositorio](https://github.com/osbaho/Santa/issues)
3. Crea un nuevo issue con:
   - Descripción del problema
   - Pasos para reproducir
   - Screenshot del error
   - Versión de Unity
   - Sistema operativo

---

**Última actualización**: Enero 2026
