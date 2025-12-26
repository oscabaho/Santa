# Guía de Optimización de Sombras URP para Móviles

## Problema
Estás viendo estos warnings porque tienes **demasiadas luces con sombras activas simultáneamente**, lo que excede la capacidad del atlas de sombras (actualmente configurado en 256x256, que es muy pequeño).

## Soluciones Implementadas

### 1. **URPShadowOptimizerEditor** (Editor Tool)
Una herramienta visual en el editor de Unity para optimizar automáticamente los URP Assets.

#### Cómo usar:
1. En Unity, ve a: **Santa > Optimize URP Shadows for Mobile**
2. Configura los parámetros recomendados:
   - **Shadow Atlas Size**: 2048 (o 4096 para mejor calidad)
   - **Base Resolution**: 512
   - **Tier Low**: 128
   - **Tier Medium**: 256
   - **Tier High**: 512
   - **Shadow Distance**: 30
   - **Enable Soft Shadows**: ❌ Desactivado (mejor rendimiento)
3. Haz clic en **"Apply to All URP Assets in Project"**

#### Botones útiles:
- **Analyze Current Scene Lights**: Te muestra estadísticas de cuántas luces con sombras tienes
- **Convert Point Lights to Spot Lights**: Convierte automáticamente Point Lights a Spot (ahorra ~83% de shadow maps)
- **Disable Soft Shadows**: Cambia todas las luces de Soft a Hard Shadows

### 2. **ShadowOptimizer** (Runtime Component)
Un componente que optimiza dinámicamente las sombras durante el juego.

#### Cómo usar:
1. Agrega el componente `ShadowOptimizer` a un GameObject en tu escena (ej: GameManager)
2. Configura los parámetros:
   ```
   Shadow Atlas Size: 2048
   Max Shadow Distance: 30
   Max Shadowed Lights: 8
   Prioritize By Intensity: ✓
   Convert Point To Spot: Opcional
   Disable Soft Shadows: ✓
   Max Shadow Resolution: 512
   ```

#### Funciones:
- Desactiva automáticamente sombras de luces lejanas a la cámara
- Prioriza las luces más importantes (cercanas e intensas)
- Limita el número de luces con sombras activas simultáneamente

## Configuración Manual de URP Assets

Si prefieres hacerlo manualmente, abre cada URP Asset y ajusta:

### En la sección **Shadows**:
- **Max Distance**: `30` (en lugar de 20)
- **Cascade Count**: `1` (ya está bien para móviles)

### En la sección **Additional Lights**:
- **Additional Lights**: `Per Pixel` o `Per Vertex`
- **Per Object Limit**: `4` (reduce si es necesario)
- **Cast Shadows**: ✓ Activado
- **Shadow Atlas Resolution**: `2048` (cambiar de 256 a 2048)
- **Shadow Resolution**: `512` (cambiar de 1024 a 512)
  - **Tier Low**: `128`
  - **Tier Medium**: `256`
  - **Tier High**: `512`

### En la sección **Quality**:
- **Soft Shadows**: ❌ Desactivar para móviles

## Por qué estas soluciones funcionan

### Problema Original:
- Atlas de sombras: **256x256** (muy pequeño)
- Resolución de sombras: **1024** (muy alta para móviles)
- Muchas Point Lights con sombras (cada una usa 6 shadow maps)
- Resultado: **46-59 shadow maps** no caben en el atlas

### Solución:
1. **Atlas más grande (2048x2048)**: Caben más shadow maps
2. **Resolución menor (512)**: Cada shadow map ocupa menos espacio
3. **Convertir Point → Spot**: Reduce de 6 a 1 shadow map por luz
4. **Hard Shadows**: Mejor rendimiento que Soft Shadows
5. **Limitar luces activas**: Máximo 8-12 luces con sombras simultáneamente

## Cálculo de Shadow Maps

### Point Light:
- Usa **6 shadow maps** (uno por cada cara del cubemap)
- Con Soft Shadows requiere más resolución

### Spot Light:
- Usa **1 shadow map** (cono direccional)
- 83% más eficiente que Point Light

### Ejemplo:
- **10 Point Lights** = 60 shadow maps
- **10 Spot Lights** = 10 shadow maps
- **Ahorro**: 50 shadow maps (83%)

## Recomendaciones Específicas para Móviles

### Tier de Calidad Bajo:
```yaml
Shadow Atlas: 1024x1024
Shadow Resolution: 256
Max Shadowed Lights: 4
Soft Shadows: OFF
```

### Tier de Calidad Medio:
```yaml
Shadow Atlas: 2048x2048
Shadow Resolution: 512
Max Shadowed Lights: 8
Soft Shadows: OFF
```

### Tier de Calidad Alto:
```yaml
Shadow Atlas: 4096x4096
Shadow Resolution: 1024
Max Shadowed Lights: 16
Soft Shadows: OFF (o ON para dispositivos de gama alta)
```

## Optimizaciones Adicionales

### 1. Usar Baked Shadows cuando sea posible
- Para luces estáticas, usa **Baked** o **Mixed** lighting
- Solo usa **Realtime** shadows para luces dinámicas

### 2. Shadow Distance por cámara
```csharp
Camera.main.farClipPlane = 100f;
// Las sombras solo se renderizan hasta shadowDistance (30m)
```

### 3. Culling Masks
- Usa Culling Masks para que algunas luces no afecten ciertos objetos
- Reduce el número de shadow casters

### 4. LOD para Sombras
- Objetos lejanos no necesitan proyectar sombras
- Usa el componente LODGroup y desactiva sombras en niveles bajos

## Verificación

Después de aplicar los cambios, deberías ver:
- ✅ Sin warnings sobre "Too many shadow maps"
- ✅ Sin warnings sobre "Reduced resolution"
- ✅ Mejor rendimiento en dispositivos móviles
- ✅ Calidad visual similar o mejor

## Debugging

Si aún ves warnings:
1. Abre: **Santa > Optimize URP Shadows for Mobile**
2. Haz clic en **"Analyze Current Scene Lights"**
3. Revisa cuántos shadow maps se están usando
4. Usa **"Convert Point Lights to Spot Lights"** si es necesario

## Archivos Modificados

- `/Assets/Settings/Low_PipelineAsset.asset` - Configuración para móviles de gama baja
- `/Assets/Settings/Medium_PipelineAsset.asset` - Configuración para móviles de gama media
- `/Assets/Settings/High_PipelineAsset.asset` - Configuración para móviles de gama alta
- `/Assets/Scripts/Utils/ShadowOptimizer.cs` - Optimizador runtime
- `/Assets/Scripts/Editor/URPShadowOptimizerEditor.cs` - Herramienta de editor

## Referencias
- [URP Shadow Atlas Documentation](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Mobile Optimization Guide](https://docs.unity3d.com/Manual/MobileOptimizationPracticalGuide.html)
