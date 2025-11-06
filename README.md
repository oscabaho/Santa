# Santa

Proyecto Unity para un roguelike con combate por turnos y UIs direccionadas por Addressables. Este repositorio incluye múltiples módulos (.csproj) y documentación para flujos de combate, transición, y un sistema de Upgrade UI modular.

## Documentación principal

### Sistema de Upgrade UI
- Índice maestro: `UPGRADE_UI_MASTER_INDEX.md`
- Addressables Setup: `UPGRADE_UI_ADDRESSABLES_SETUP.md`
- Optimización: `UPGRADE_UI_OPTIMIZATION_GUIDE.md`
- Integración con VContainer: `UPGRADE_UI_VCONTAINER_INTEGRATION.md`

### Sistema de Combate
- **Arenas de Combate (Addressables):** `COMBAT_ARENA_ADDRESSABLES_GUIDE.md` ⭐ NUEVO
- Configuración de TestScene: `TESTSCENE_SETUP_GUIDE.md`
- Logging y Pooling: `Assets/Scripts/README_LOGGING_AND_POOLING.md`

### Referencia Técnica
- Clases activas: `Assets/Scripts/ACTIVE_CLASSES.md`

## Cómo empezar

1) Abre el proyecto en Unity (carpeta raíz del repo).  
2) Revisa Addressables (Window → Asset Management → Addressables → Groups) y construye los grupos si es necesario.  
3) Ejecuta la escena de pruebas (TestScene) y sigue la guía si aún no está configurada.

## Notas

- La rama de trabajo activa es `Updates`; la rama por defecto es `main`.
- La documentación está mayormente en español y usa ejemplos en C#.

