# 🤝 Contributing to Santa

Gracias por tu interés en contribuir al proyecto Santa. Esta guía te ayudará a entender cómo participar efectivamente.

---

## 📋 Tabla de Contenidos

- [Code of Conduct](#code-of-conduct)
- [Cómo Contribuir](#cómo-contribuir)
- [Git Workflow](#git-workflow)
- [Code Style](#code-style)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Testing](#testing)
- [Documentación](#documentación)

---

## Code of Conduct

### Nuestros Valores

- **Respeto**: Trata a todos con respeto profesional
- **Colaboración**: Trabaja en equipo y ayuda a otros
- **Calidad**: Mantén altos estándares de código
- **Comunicación**: Sé claro y constructivo en feedback

---

## Cómo Contribuir

### Tipos de Contribuciones

#### 🐛 Bug Reports

Si encuentras un bug:

1. Busca en [Issues](https://github.com/osbaho/Santa/issues) si ya existe
2. Si no existe, crea un nuevo issue con:
   - **Título claro** describiendo el bug
   - **Pasos para reproducir**
   - **Comportamiento esperado** vs **comportamiento actual**
   - **Screenshots** o videos si es posible
   - **Versión de Unity**, OS, y specs del dispositivo

**Template de Bug Report:**
```markdown
**Descripción del Bug**
Una descripción clara del problema.

**Pasos para Reproducir**
1. Ir a '...'
2. Click en '...'
3. Ver error

**Comportamiento Esperado**
Lo que debería pasar.

**Comportamiento Actual**
Lo que realmente pasa.

**Screenshots**
Si aplica, agrega screenshots.

**Entorno**
- Unity Version: 6.0.30f1
- OS: Windows 11
- Device: Samsung Galaxy S21
```

#### ✨ Feature Requests

Para sugerir nuevas características:

1. Abre un issue con label `enhancement`
2. Describe:
   - **Por qué** es necesaria la feature
   - **Cómo** debería funcionar
   - **Impacto** en el proyecto
   - **Alternativas** consideradas

#### 📝 Documentación

Mejoras a documentación son siempre bienvenidas:

- Corregir typos
- Mejorar claridad
- Agregar ejemplos
- Actualizar información obsoleta

---

## Git Workflow

### Branching Strategy

```
main           # Producción, siempre estable
  └── Updates  # Desarrollo activo
       ├── feature/nombre-feature
       ├── bugfix/nombre-bug
       └── hotfix/nombre-hotfix
```

### Nombres de Branches

- **Features**: `feature/combat-abilities`
- **Bugfixes**: `bugfix/save-corruption`
- **Hotfixes**: `hotfix/crash-on-load`
- **Refactor**: `refactor/combat-system`

### Flujo de Trabajo

1. **Fork el repositorio** (para colaboradores externos)
2. **Crea una branch** desde `Updates`:
   ```bash
   git checkout Updates
   git pull origin Updates
   git checkout -b feature/mi-feature
   ```
3. **Haz tus cambios** siguiendo code style
4. **Commit** usando Conventional Commits
5. **Push** a tu branch:
   ```bash
   git push origin feature/mi-feature
   ```
6. **Abre un Pull Request** hacia `Updates`

---

## Code Style

### C# Guidelines

Seguimos las [C# Coding Conventions de Microsoft](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).

#### Naming Conventions

```csharp
// Clases, structs, enums: PascalCase
public class PlayerController { }
public struct HealthData { }
public enum GameState { }

// Interfaces: PascalCase con prefijo I
public interface ICombatService { }

// Métodos públicos: PascalCase
public void StartCombat() { }

// Propiedades: PascalCase
public int MaxHealth { get; set; }

// Campos privados: camelCase con _
private int _currentHealth;

// Constantes: PascalCase
private const int MaxLevel = 99;

// Parámetros y variables locales: camelCase
public void ApplyDamage(int damageAmount)
{
    int finalDamage = damageAmount;
}
```

#### Espaciado y Formato

```csharp
// Braces en nueva línea
public class ExampleClass
{
    public void ExampleMethod()
    {
        if (condition)
        {
            DoSomething();
        }
    }
}

// Un espacio después de keywords
if (condition) { }
for (int i = 0; i < count; i++) { }

// No espacios en paréntesis
DoSomething(param1, param2);

// Usar var cuando el tipo es obvio
var playerHealth = GetComponent<HealthComponent>();

// No usar var cuando no es obvio
HealthComponent health = FindHealthComponent();
```

#### Organización de Clase

```csharp
public class ExampleClass : MonoBehaviour
{
    // 1. Constantes
    private const int MaxValue = 100;
    
    // 2. Fields serializados
    [SerializeField] private int _startingHealth;
    
    // 3. Fields inyectados
    [Inject] private ICombatService _combatService;
    
    // 4. Fields privados
    private int _currentHealth;
    
    // 5. Properties públicas
    public int CurrentHealth => _currentHealth;
    
    // 6. Unity lifecycle methods
    void Awake() { }
    void Start() { }
    void Update() { }
    
    // 7. Public methods
    public void TakeDamage(int amount) { }
    
    // 8. Private methods
    private void Die() { }
}
```

### Comentarios

```csharp
// Comentarios XML para APIs públicas
/// <summary>
/// Applies damage to the combatant.
/// </summary>
/// <param name="amount">Amount of damage to apply</param>
/// <returns>True if combatant died, false otherwise</returns>
public bool TakeDamage(int amount)
{
    // Comentarios inline para lógica compleja
    _currentHealth = Mathf.Max(0, _currentHealth - amount);
    
    return _currentHealth == 0;
}
```

### Uso de LINQ

```csharp
// ❌ Evitar en hot paths (Update, combat calculations, etc.)
var enemies = allCombatants.Where(c => c.IsEnemy).ToList();

// ✅ Usar loops tradicionales en hot paths
var enemies = new List<Combatant>();
for (int i = 0; i < allCombatants.Count; i++)
{
    if (allCombatants[i].IsEnemy)
    {
        enemies.Add(allCombatants[i]);
    }
}

// ✅ LINQ es aceptable en setup/initialization
var enemyPrefabs = Resources.LoadAll<GameObject>("Enemies")
    .Where(p => p.CompareTag("Enemy"))
    .ToList();
```

### Async Programming

```csharp
// ✅ Usar UniTask en lugar de Task
public async UniTask LoadLevel(string levelKey)
{
    await Addressables.LoadSceneAsync(levelKey);
}

// ✅ UniTaskVoid para fire-and-forget
private async UniTaskVoid AutoSave()
{
    await _saveService.SaveGame("autosave");
}

// ❌ Evitar async void en general
// async void OnButtonClicked() { } // NO!

// ✅ Excepto para event handlers de Unity UI
public async void OnButtonClicked()
{
    await DoSomethingAsync();
}
```

---

## Commit Guidelines

Usamos [Conventional Commits](https://www.conventionalcommits.org/).

### Formato

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: Nueva feature
- **fix**: Bug fix
- **docs**: Cambios en documentación
- **style**: Formatting, missing semicolons, etc.
- **refactor**: Code refactor sin cambio de funcionalidad
- **perf**: Performance improvements
- **test**: Agregar tests
- **chore**: Cambios en build, tools, dependencies

### Ejemplos

```bash
# Feature
git commit -m "feat(combat): add poison ability"

# Bug fix
git commit -m "fix(save): prevent corruption on async save"

# Documentation
git commit -m "docs(readme): update installation instructions"

# Refactor
git commit -m "refactor(combat): extract targeting logic to service"

# Con body
git commit -m "feat(upgrade): add skill tree system

- Implemented skill tree data structure
- Created UI for skill selection
- Added save/load for unlocked skills

Closes #123"
```

---

## Pull Request Process

### Antes de Crear PR

- [ ] Código sigue code style
- [ ] Tests pasan (si aplica)
- [ ] Documentación actualizada
- [ ] Branch está actualizada con `Updates`
- [ ] No hay merge conflicts

### Crear PR

1. Ve a GitHub y crea Pull Request
2. **Base**: `Updates`, **Compare**: tu branch
3. Llena el template:

```markdown
## Descripción
Breve descripción de los cambios.

## Tipo de cambio
- [ ] Bug fix
- [ ] Nueva feature
- [ ] Breaking change
- [ ] Documentación

## Testing
Cómo se probó esto:
- [ ] Tested en editor
- [ ] Tested en build Android
- [ ] Tested en build iOS

## Checklist
- [ ] Mi código sigue el style guide
- [ ] He actualizado la documentación
- [ ] No introduce warnings
- [ ] He probado mis cambios
```

### Code Review

- Responde a comentarios de forma constructiva
- Haz los cambios solicitados
- Push updates a la misma branch (se actualizará el PR automáticamente)
- Solicita re-review cuando termines cambios

### Merge

Un maintainer hará merge cuando:
- ✅ Code review aprobado
- ✅ Tests pasan
- ✅ No hay merge conflicts
- ✅ Cambios probados

---

## Testing

### Unit Tests

```csharp
using NUnit.Framework;

public class DamageAbilityTests
{
    [Test]
    public void Execute_DealsDamageToTarget()
    {
        // Arrange
        var ability = new DamageAbility { BaseDamage = 10 };
        var target = new MockCombatant { Health = 100 };
        var context = new AbilityContext { Target = target };
        
        // Act
        var result = ability.Execute(context);
        
        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(90, target.Health);
    }
}
```

### Testing en Editor

- Testear en escena Bootstrap
- Verificar no hay errores en Console
- Probar flujos críticos (combat, save/load, etc.)

### Testing en Device

Para cambios que afectan performance:
- Build para Android/iOS
- Testear en dispositivo real
- Verificar con Profiler

---

## Documentación

### Cuándo Actualizar Docs

Actualiza documentación cuando:
- Agregas nueva feature pública
- Cambias API existente
- Fixes bug que afecta comportamiento documentado
- Mejoras arquitectura

### Qué Documentar

- **README.md**: Features overview, quick start
- **ARCHITECTURE.md**: Cambios arquitectónicos
- **SYSTEMS.md**: Nuevos sistemas o cambios mayores
- **Código**: Comentarios XML para APIs públicas

### Formato de Docs

- Usa Markdown
- Incluye ejemplos de código
- Agrega diagramas Mermaid cuando sea útil
- Mantén consistencia con docs existentes

---

## Recursos Adicionales

### Documentación del Proyecto

- [README.md](README.md) - Overview del proyecto
- [ARCHITECTURE.md](ARCHITECTURE.md) - Arquitectura y patrones
- [SYSTEMS.md](SYSTEMS.md) - Documentación de sistemas
- [SETUP.md](SETUP.md) - Guía de instalación

### Enlaces Externos

- [Unity Documentation](https://docs.unity3d.com/)
- [VContainer Docs](https://vcontainer.hadashikick.jp/)
- [UniTask GitHub](https://github.com/Cysharp/UniTask)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

## Preguntas?

Si tienes preguntas sobre cómo contribuir:

1. Revisa la documentación existente
2. Busca en [Issues](https://github.com/osbaho/Santa/issues)
3. Abre un nuevo issue con la pregunta
4. Contacta a los maintainers

---

**Gracias por contribuir a Santa!** 🎮✨

---

**Última actualización**: Enero 2026
