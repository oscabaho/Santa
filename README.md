# 🎮 Santa - Roguelike Turn-Based Combat

Un roguelike moderno con sistema de combate por turnos, arquitectura limpia y optimizado para dispositivos móviles a 60 FPS.

## ✨ Características Principales

- **Sistema de Combate Por Turnos** - Mecánicas profundas con abilities, targeting y phases
- **Arquitectura Clean** - Separación clara de capas (Core, Domain, Infrastructure, Presentation)
- **Optimizado para Móviles** - 60 FPS estable con zero-allocation async/await
- **UI Dinámica** - Panels cargados dinámicamente via Addressables
- **Sistema de Guardado** - Save system robusto con encriptación
- **Sistema de Upgrades** - Mejoras de personaje con UI modular
- **Dependency Injection** - VContainer para arquitectura desacoplada

## 🛠️ Tecnologías Utilizadas

### Core
- **Unity 6.0** (URP - Universal Render Pipeline)
- **C# 9.0+** con .NET Standard 2.1

### Frameworks & Libraries
- **[VContainer](https://github.com/hadashiA/VContainer)** - Dependency Injection rápido y ligero
- **[UniTask](https://github.com/Cysharp/UniTask)** - Zero-allocation async/await
- **Addressables** - Asset management dinámico y eficiente
- **Input System** - New Input System con soporte para múltiples dispositivos
- **Cinemachine 3.1** - Sistema de cámaras cinematográficas
- **AI Navigation** - NavMesh para pathfinding

### Render & VFX
- **Universal Render Pipeline (URP)** 17.0.4
- **Post-Processing** 3.5.1
- **Timeline** - Secuencias cinematográficas

## 📁 Estructura del Proyecto

```
Santa/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/              # Interfaces, eventos, DI, modelos
│   │   ├── Domain/            # Lógica de negocio (Combat, Dialogue, Upgrades)
│   │   ├── Infrastructure/    # Implementaciones de servicios
│   │   └── Presentation/      # UI y presentación
│   ├── Scenes/
│   ├── Prefabs/
│   └── Addressables/
├── Packages/
└── ProjectSettings/
```

## 🚀 Quick Start

### Requisitos Previos

- **Unity 6.0.x** o superior
- **Windows/macOS/Linux** para desarrollo
- **Git** para control de versiones

### Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/osbaho/Santa.git
   cd Santa
   ```

2. **Abrir en Unity**
   - Abre Unity Hub
   - Click en "Add" → Selecciona la carpeta del proyecto
   - Abre el proyecto con Unity 6.0.x

3. **Configurar Addressables**
   - Abre `Window → Asset Management → Addressables → Groups`
   - Click en `Build → New Build → Default Build Script`
   - Espera a que termine el build

4. **Ejecutar el Proyecto**
   - Abre la escena principal (primera escena en Build Settings)
   - Presiona Play ▶️

## 📚 Índice de Documentación
- [**SETUP.md**](SETUP.md) - Guía de instalación y configuración inicial.
- [**ARCHITECTURE.md**](ARCHITECTURE.md) - Clean Code, SOLID y patrones de diseño.
- [**SYSTEMS.md**](SYSTEMS.md) - Visión general de subsistemas (Combat, Save, UI, etc).
- [**COMBAT_SYSTEM.md**](COMBAT_SYSTEM.md) - Profundidad técnica del sistema de combate.
- [**SAVE_SYSTEM.md**](SAVE_SYSTEM.md) - Detalles del sistema de persistencia y encriptación.
- [**UI_SYSTEM.md**](UI_SYSTEM.md) - Gestión de UI con Addressables.
- [**DEPENDENCIES.md**](DEPENDENCIES.md) - Lista de paquetes y versiones.
- [**CONTRIBUTING.md**](CONTRIBUTING.md) - Guías de estilo y flujo de trabajo.

### Guías Nuevas (Enero 2026)
- [**🎓 Tutorial: First Ability**](TUTORIAL_FIRST_ABILITY.md) - Crea tu primera habilidad paso a paso.
- [**🔧 Troubleshooting**](TROUBLESHOOTING.md) - Soluciones a errores comunes.
- [**🧪 Testing Guide**](TESTING.md) - Unit, Integration y Play Mode testing.
- [**⚡ Performance**](PERFORMANCE.md) - Optimización para móviles.
- [**📚 API Reference**](API_REFERENCE.md) - Referencia técnica rápida.
- [**📖 Glosario**](GLOSSARY.md) - Terminología del proyecto.

## 🎯 Características Técnicas

### Performance
- **Zero-Allocation Async** - UniTask elimina GC allocations en hot paths
- **Object Pooling** - Pool service para VFX, projectiles y UI elements
- **LINQ-Free Hot Paths** - Código crítico sin LINQ para mejor performance
- **60 FPS Target** - Optimizado para dispositivos móviles

### Arquitectura
- **Clean Architecture** - Separación de concerns en capas
- **SOLID Principles** - Código mantenible y extensible
- **Dependency Injection** - Desacoplamiento mediante VContainer
- **Event-Driven** - GameEventBus para comunicación entre sistemas

### Sistemas Principales

1. **Combat System** - Turn-based combat con abilities y targeting
2. **Save System** - Persistencia con SecureStorage y contributors pattern
3. **Upgrade System** - Sistema de mejoras modular
4. **Level System** - Carga de niveles y transiciones
5. **UI System** - Panels dinámicos via Addressables
6. **Audio System** - Gestión de audio
7. **VFX System** - Efectos visuales pooled
8. **Camera System** - Combat cameras con Cinemachine
9. **Input System** - Virtual gamepad y input abstraction
10. **Pooling System** - Object pooling para performance
11. **Dialogue System** - Sistema de diálogos con NPCs

## 🎮 Controles

### Teclado & Mouse
- **WASD** - Movimiento
- **Mouse** - Apuntar y seleccionar
- **ESC** - Pausar
- **1-4** - Abilities en combate

### Virtual Gamepad (Mobile/Touch)
- **Joystick izquierdo** - Movimiento
- **Botones de acción** - Interactuar, atacar
- **Botones de abilities** - Usar habilidades en combate

## 🔧 Configuración de Build

### Android
```
Target API Level: 33 (Android 13)
Minimum API Level: 24 (Android 7.0)
Scripting Backend: IL2CPP
Target Architectures: ARM64
```

### iOS
```
Target Minimum iOS Version: 13.0
Scripting Backend: IL2CPP
Target Architectures: ARM64
```

## 🤝 Contribuir

Las contribuciones son bienvenidas. Por favor lee [CONTRIBUTING.md](CONTRIBUTING.md) para detalles sobre el código de conducta y el proceso para enviar pull requests.

## 📝 Notas de Desarrollo

- **Rama principal**: `main`
- **Rama de desarrollo**: `Updates`
- **Convención de commits**: Conventional Commits
- **Documentación**: Mayormente en español

## 📄 Licencia

Este proyecto es propietario. Todos los derechos reservados.

## 👥 Contacto

- **Repositorio**: [github.com/osbaho/Santa](https://github.com/osbaho/Santa)
- **Issues**: [github.com/osbaho/Santa/issues](https://github.com/osbaho/Santa/issues)

---

**Última actualización**: Enero 2026
