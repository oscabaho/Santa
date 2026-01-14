# ⚡ Guía de Performance y Optimización

Pautas para mantener el rendimiento al máximo, especialmente en dispositivos móviles.

---

## 🎯 Objetivos

- **Frame Rate**: 60 FPS (Estable)
- **Frame Time**: < 16.6ms
- **Draw Calls**: < 150-200 (Mobile)
- **Memory**: < 1.5 GB (High safety margin)

---

## CPU Optimization

### 1. Scripting

#### ❌ Evitar en Hot Paths (Update/FixedUpdate):
- `GetComponent<T>()` → **Cachear en Awake/Start**
- `Find()`, `FindObjectOfType()` → **Nunca usar en runtime**
- `new List<T>()`, `new Object()` → **Genera Garbage Collection (GC)**
- LINQ (`.Where()`, `.Select()`, `.ToList()`) → **Lento y genera basura**
- String concatenation (`"score: " + score`) → **Usar StringBuilder o string.Format**

#### ✅ Mejores Prácticas:
```csharp
// Mal
void Update() {
    GetComponent<Transform>().position += Vector3.up;
}

// Bien
private Transform _transform;
void Awake() {
    _transform = transform;
}
void Update() {
    _transform.position += Vector3.up;
}
```

### 2. Physics

- Usar **Layers Collision Matrix** (`Edit > Project Settings > Physics`) para deshabilitar colisiones innecesarias.
- Preferir `SphereCollider` o `CapsuleCollider` sobre `MeshCollider`.
- Mover objetos con `Rigidbody.MovePosition()` en lugar de Transform directos si tienen física.

---

## GPU Optimization (Rendering)

### 1. Draw Calls y Batching

- **Static Batching**: Marcar objetos estáticos (edificios, suelo) como "Static" en el inspector.
- **GPU Instancing**: Usar shaders compatibles con Instancing para objetos repetidos (árboles, props).
- **SRP Batcher**: Asegurar que todos los materiales usen shaders compatibles con URP SRP Batcher.

### 2. UI

- Usar múltiples **Canvas** para separar elementos estáticos de dinámicos.
- Deshabilitar el componente **Graphic Raycaster** en Canvas que no requieren interacción (ej: HUD solo visual).
- Evitar **Masks** excesivas (son costosas). Preferir `RectMask2D`.

### 3. Lighting & Shadows

- **Bake Lighting**: Usar Mixed Lighting o Baked Lighting para escenarios estáticos.
- **Shadow Distance**: Reducir en mobile (ej: 40m).
- **Hard Shadows** vs Soft Shadows: Hard Shadows son más baratas.

---

## Memory Management

### 1. Addressables

- Cargar assets grandes (texturas, audio) solo cuando se necesiten.
- **Liberar** assets (`Addressables.Release()`) cuando se salga de la escena o cierre el panel UI.
- Monitorear el **Profiler** para ver "Texture Memory" y "Audio Memory".

### 2. Texture Compression

- **Android**: ASTC (4x4 o 6x6)
- **iOS**: ASTC o PVRTC
- **PC**: BC7 o DXT5
- Habilitar **Mip Maps** para texturas 3D (mejora performance de cache).
- Deshabilitar Mip Maps para UI (se ven borrosas).

### 3. Audio

- **SoundFX cortos**: `Decompress On Load`
- **Música/Ambiente largo**: `Streaming`
- **Voces medias**: `Compressed In Memory`

---

## Herramientas de Profiling

1. **Unity Profiler**: Primer paso para diagnosticar CPU/GPU usage.
2. **Frame Debugger**: Ver exactamente qué se está dibujando y en qué orden (útil para debuggear batching).
3. **Memory Profiler**: Snapshot detallado de qué ocupa RAM.
4. **Profile Analyzer**: Comparar dos capturas (ej: antes y después de una optimización).

---

## Checklist de Publicación

Antes de lanzar una build:

- [ ] **Development Build**: Desactivado
- [ ] **Logging**: Desactivar logs de debug (`Debug.Log`) o usar wrapper condicional
- [ ] **Shader Stripping**: Configurado en Graphics Settings
- [ ] **Unused Assets**: Limpiar assets no referenciados
- [ ] **Test en Dispositivo Real**: Probar en el dispositivo gama baja objetivo

---

**Última actualización:** Enero 2026
