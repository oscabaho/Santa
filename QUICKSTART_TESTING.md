# ⚡ QUICKSTART - Implementación & Testing Final

**Duración estimada**: 5-10 minutos para verificar en Editor, 30 minutos para build móvil

---

## 🎯 Paso 1: Verificación en Unity Editor (5 min)

### ✅ Paso 1.1: Abrir Menu Scene
```
1. En Project, localiza: Scenes/Menu.unity
2. Double-click para abrir
3. Play (o Ctrl+P)
4. En Console, deberías ver:
   ✓ "GameLifetimeScope CONFIGURED!"
   ✓ "EventSystem using InputSystemUIInputModule"
   (Sin errores rojos)
```

**Si ves errores:**
- Revisa que InputReaderAsset esté asignado en GameLifetimeScope Inspector
- Limpia Assets → Reimport All

### ✅ Paso 1.2: Toca "Play" → Carga Gameplay
```
1. En Menu scene, toca botón "Play"
2. Espera a que Gameplay scene cargue (~2-3 seg)
3. En Console, deberías ver:
   ✓ "GameplayLifetimeScope CONFIGURED!"
   ✓ Sin errores de "Manager not found"
```

**Si falla carga:**
- Revisa que Gameplay esté en Build Settings (Scene 1)
- Limpia Assets → Reimport All

### ✅ Paso 1.3: Toca ActionButton → Inicia Combate
```
1. En Gameplay scene, busca ActionButton (generalmente en Canvas)
2. Toca el botón
3. En Console, deberías ver:
   ✓ "Combat triggered! Subscribers: X" (X > 0)
   ✓ Combate inicia (UI de combate aparece)
```

**Si no funciona:**
- Revisa que UIManager esté asignado en GameplayLifetimeScope Inspector
- Revisa que InputReader esté disponible globalmente

---

## 🚀 Paso 2: Build para Android (15-20 min)

### ✅ Paso 2.1: Configurar Build Settings
```
1. File → Build Settings
2. Scenes in Build:
   [0] Assets/Scenes/Menu.unity
   [1] Assets/Scenes/Gameplay.unity
3. Platform: Android
4. Texture Compression: ASTC
5. Graphics APIs: Remove Vulkan (quedar con OpenGL ES 3)
6. IL2CPP Scripting Backend: ✓ Enabled
7. Development Build: ✓ Enabled (para debugging)
8. Script Debugging: ✓ Enabled
9. Wait for Managed Debugger: ✗ Disabled (mientras testeas)
```

### ✅ Paso 2.2: Player Settings
```
1. File → Build Settings → Player Settings
2. Resolution and Presentation:
   - Orientation: Portrait (o tu preferencia)
   - Full Screen: ✓ Enabled
3. Other Settings:
   - Scripting Backend: IL2CPP ✓
   - API Compatibility Level: .NET 4.x
4. Yen Android:
   - Minimum Android Version: Android 8.0 (API level 26)
   - Target Android Version: Android 13+ (latest)
```

### ✅ Paso 2.3: Build APK
```
1. File → Build Settings
2. Build (o Build & Run si tienes dispositivo conectado)
3. Elige carpeta donde guardar APK
4. Espera compilación (5-15 min dependiendo de proyecto)
5. Si exitoso: "Build complete!" mensaje en console
```

**Posibles errores de build:**
```
Error: "IL2CPP not found"
→ Solución: Instala IL2CPP en Unity Hub

Error: "Gradle build failed"
→ Solución: Limpia Temp folder, reinicia build

Error: "Java not found"
→ Solución: Instala Java JDK, agrega a PATH
```

---

## 📱 Paso 3: Testing en Dispositivo Android (10 min)

### ✅ Paso 3.1: Instalar APK
```
Si tienes USB debugging enabled:
1. Connect dispositivo Android via USB
2. File → Build Settings → Build & Run
3. APK se instala automáticamente

Si instalas manual:
1. adb install -r ruta/al/apk/Santa.apk
2. O transfiere APK y abre desde Files
```

### ✅ Paso 3.2: Abrir App & Verificar Menu
```
1. Abre app en dispositivo
2. Deberías ver Menu con botón "Play"
3. Abre Logcat (Android Studio):
   adb logcat | grep -i "Santa\|GameLifetime\|Combat"
4. Verifica:
   ✓ "GameLifetimeScope CONFIGURED!"
   ✓ "EventSystem using InputSystemUIInputModule"
   ✓ Sin crashes o errores rojos
```

### ✅ Paso 3.3: Toca "Play" → Carga Gameplay
```
1. Tap "Play" button
2. Espera carga (~3-5 seg)
3. En Logcat, verifica:
   ✓ "GameplayLifetimeScope CONFIGURED!"
   ✓ Sin "Manager not found" errors
4. Gameplay scene debe aparecer con ActionButton visible
```

### ✅ Paso 3.4: Toca ActionButton → Testing Final
```
1. Localiza ActionButton (generalmente en esquina)
2. TAP el botón
3. En Logcat, verifica:
   ✓ "Combat triggered! Subscribers: X"
4. Pantalla debe cambiar a UI de combate
5. ¡ÉXITO! ✅ El botón funciona en móvil
```

---

## 🔍 Checklist de Debugging

Si algo NO funciona, usa esto:

### Debug Menu Scene
```
1. File → Build Settings → Build & Run (Development)
2. Abre Menu en dispositivo
3. Logcat debe mostrar:
   ✓ "GameLifetimeScope CONFIGURED!"
   ✓ "EventSystem exists: true"
   ✓ "InputSystemUIInputModule: true"
   ✓ Sin errores

Si falta uno:
→ Revisa GameLifetimeScope.InitializeUIEventSystem()
→ Verifica que InputReaderAsset esté asignado
```

### Debug Gameplay Scene (Load from Menu)
```
1. Desde Menu, tap "Play"
2. En Logcat, verifica:
   ✓ "GameplayLifetimeScope CONFIGURED!"
   ✓ "UIManager registered successfully"
   ✓ Sin "Manager not found" warnings

Si falla:
→ Revisa GameplayLifetimeScope Inspector
→ Asegura UIManager asignado
→ Verifica TurnBasedCombatManager existe
```

### Debug ActionButton
```
1. Tap ActionButton
2. En Logcat, busca:
   ✓ "Combat triggered! Subscribers: X" (X debe ser > 0)
   ✓ "PlayerInteraction: OnInteract called"
   ✓ Combat UI aparece

Si no funciona:
→ Revisa ActionButtonController en escena
→ Asegura que InputReader está disponible
→ Verifica que EventSystem está configurado
→ Comprueba que Canvas tiene GraphicRaycaster
```

---

## 📊 Logs Esperados (Orden Correcto)

### En Menu Scene (Startup)
```
[GameLifetimeScope] CONFIGURED!
[EventSystem] Found/Created EventSystem
[EventSystem] Replaced StandaloneInputModule with InputSystemUIInputModule
[InputReader] Asset loaded from Resources
[GameEventBus] Initialized as Singleton
[GameLog] Menu ready
```

### Al tapear "Play"
```
[SceneManager] Loading Gameplay...
[GameplayLifetimeScope] Awake - Ensuring UI EventSystem initialized
[EventSystem] EventSystem already exists, reusing
[GameplayLifetimeScope] CONFIGURED!
[UIManager] Registered from Gameplay scene
[TurnBasedCombatManager] Initialized
[LevelManager] Level loaded
[GameLog] Gameplay ready
```

### Al tapear ActionButton
```
[ActionButtonController] OnPointerDown - Evaluating interaction
[InputReader] RaiseInteract() called with 3 subscribers
[PlayerInteraction] OnInteract - Entered combat zone
[CombatEncounterManager] StartCombat triggered
[TurnBasedCombatManager] Combat initialized
[GameplayUIManager] Combat HUD shown
[GameLog] Combat started!
```

---

## ❌ Errores Comunes & Soluciones

| Error | Causa | Solución |
|-------|-------|----------|
| "EventSystem using StandaloneInputModule" | Init incorrecta | Asegura InitializeUIEventSystem() en GameLifetimeScope.Configure() |
| "UIManager NOT assigned!" (en Editor) | Esto es NORMAL en Editor si no lo asignas | Asigna UIManager en Gameplay scope o deja que se busque |
| "Combat button doesn't respond" | InputReader no disponible | Verifica que InputReaderAsset esté en Resources |
| "Gameplay crash on load" | Missing manager | Revisa que GameplayLifetimeScope tenga parent (GameLifetimeScope) |
| "Touch input not working" | Mobile input config | Asegura que EventSystem tiene InputSystemUIInputModule, no StandaloneInputModule |
| "Logcat shows nothing" | Release build sin logs | Habilitaalready Development Build + Script Debugging en Player Settings |

---

## 📈 Performance Checks

Mientras testeas en móvil, verifica:

```
1. Frame Rate (debe ser > 30 FPS)
   → Menu: 60 FPS esperado
   → Gameplay: 30-60 FPS dependiendo de complejidad

2. Memory Usage
   → Menu: ~200-400 MB
   → Gameplay: ~500-800 MB (normal)

3. Battery Usage
   → Debe ser razonable (no sobrecalentado)

4. Touch Responsiveness
   → ActionButton debe responder < 100ms
```

Si lento:
- Disminuye calidad gráfica
- Reduce quantity de enemigos
- Simplifica VFX

---

## 🎊 Criterios de Éxito

El proyecto está **LISTO** cuando:

- ✅ Menu scene carga sin errores en Editor
- ✅ Gameplay scene carga sin errores en Editor
- ✅ ActionButton responde al clic en Editor
- ✅ APK builds sin errores (IL2CPP compilation OK)
- ✅ App abre en dispositivo sin crash
- ✅ Gameplay carga desde Menu sin crash
- ✅ ActionButton responde al tap en dispositivo
- ✅ Combate inicia correctamente en móvil
- ✅ Logs muestran secuencia correcta de inicialización
- ✅ 0 errores rojos en Logcat/Console

---

## 🚀 Quick Command Reference

**Si necesitas ver logs rápido:**
```bash
# En terminal, con Android conectado:
adb logcat | grep -i "gamelifetime\|gameplay\|combat\|error"

# Si tienes Android Studio:
Logcat → Search: "GameLifetime" u "Combat"

# En iOS (si no tienes Xcode):
Revisa Console en dispositivo Settings → Developer
```

**Si necesitas rebuild completo:**
```bash
1. File → Build Settings → Clean Build
2. Assets → Reimport All
3. File → New Build...
```

---

## 📝 Notas Finales

- **Duración total**: 5 min (Editor) + 15-20 min (Build) + 10 min (Testing) = ~30-35 minutos
- **Requisitos**: Android SDK, Java JDK, Android device o emulator
- **Éxito**: Cuando ActionButton funciona en móvil sin errores
- **Documentación**: Si necesitas volver a esto, revisa ARQUITECTURA_FINAL_OPTIMIZADA.md

---

**¡A testear se ha dicho! 🚀**

```
═══════════════════════════════════════════════════
       BUILD → INSTALL → TEST → CELEBRATE ✅
═══════════════════════════════════════════════════
```
