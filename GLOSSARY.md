# 📖 Glosario de Términos

Diccionario de terminología usada en el proyecto Santa.

---

## A

**Ability**: Acción especial que puede realizar un combatiente (Atacar, Curar, Buff). Definida como ScriptableObject.

**Action Points (AP)**: Recurso renovable por turno que limita cuántas habilidades puede usar un personaje.

**Addressables**: Sistema de Unity para gestión de assets (carga asíncrona, manejo de memoria, updates remotos). Usado intensivamente en UI y escenas.

---

## B

**Baked Lighting**: Técnica de iluminación donde las sombras y luces se pre-calculan y guardan en texturas (lightmaps) para mejorar performance.

**Brain**: Componente que controla la IA de los enemigos, decidiendo qué acción tomar cada turno.

---

## C

**Combat Phase**: Estado específico dentro del flujo de combate (Selection, Targeting, Execution).

**Contributor Pattern**: Patrón de diseño usado en el Save System donde cada componente separado "contribuye" su parte de datos al archivo de guardado global.

---

## D

**Dependency Injection (DI)**: Patrón de arquitectura donde las dependencias de una clase se le entregan desde fuera en lugar de crearlas ella misma. Usamos **VContainer**.

**Domain Layer**: Capa de la Clean Architecture que contiene la lógica de negocio pura y definiciones de datos, sin depender de Unity o infraestructura externa.

---

## I

**Infrastructure Layer**: Capa que implementa interfaces del Domain y maneja detalles técnicos (archivos, red, Unity API específica).

**Instance**: En rendering, dibujar múltiples copias del mismo mesh en una sola llamada a la GPU (GPU Instancing).

---

## P

**Prefab**: Asset de Unity que funciona como plantilla para crear GameObjects.

**Presentation Layer**: Capa responsable de lo visual (UI, Vistas) y entrada de usuario.

---

## R

**Raycast**: Técnica física para detectar objetos en una línea recta. Usado para detectar clicks en enemigos.

---

## S

**ScriptableObject**: Contenedor de datos serializable de Unity, independiente de instancias de escena. Base de nuestro sistema de Abilities.

**Solid Principles**: Conjunto de 5 principios de diseño orientado a objetos (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion).

---

## T

**Targeting Strategy**: Patrón Strategy aplicado a la selección de objetivos. Define CÓMO se eligen los targets (click manual, automático, aleatorio).

---

## U

**UniTask**: Librería para manejo eficiente de asincronía (async/await) en Unity, evitando asignaciones de memoria del Task nativo de C#.

**URP (Universal Render Pipeline)**: Pipeline de renderizado de Unity optimizado para performance y flexibilidad.

---

**Última actualización:** Enero 2026
