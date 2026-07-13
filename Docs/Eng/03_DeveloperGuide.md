# Developer Guide

> Version: 1.0
> Last Updated: 13-07-2026

---

# Purpose

This document describes the development workflow adopted in the project.

It answers the following question:

> **How should new functionality be added without violating the project's architecture?**

Before starting development, it is recommended to read:

- `01_Architecture.md`
- `02_ProjectStructure.md`

---

# Development Workflow

Almost every new task follows these steps.

```text
Idea

↓

Define Responsibility

↓

Choose the Architectural Layer

↓

Create a Feature / Service / UI

↓

Register in DI

↓

Testing

↓

Code Review
```

Before writing code, determine:

- What is being created?
- Who will use the system?
- Which architectural layer should it belong to?

---

# Adding a New Main Phase

A Main Phase represents a complete game mode.

Examples:

- Main Menu
- Exploration
- Battle
- Minigame

---

## Step 1

Create a new game scene.

For example:

```text
SC_Snake
```

All game scenes must use the following prefix:

```text
SC_
```

---

## Step 2

Add the scene name to `SceneNames`.

Example:

```csharp
public const string Snake = "SC_Snake";
```

---

## Step 3

Create a new phase.

```csharp
public class SnakePhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.Snake;

    public SnakePhase(ISceneLoader loader)
        : base(loader)
    {
    }
}
```

---

## Step 4

The phase will be registered automatically thanks to:

```csharp
public override void InstallBindings()
{
    AutoBinder.BindDerivedTypes<GamePhase>(Container);
}
```

---

## Step 5

Open the new scene only through the GameStateMachine.

Correct:

```csharp
await gameStateMachine.ReplaceMain<SnakePhase>();
```

Incorrect:

```csharp
SceneManager.LoadScene(...);
```

---

# Adding an Overlay Phase

An Overlay Phase is used for temporary game states.

Examples:

- Dialogue
- Inventory
- Pause
- Settings

---

Create a new phase.

```csharp
public class PausePhase : OverlayPhase
{
}
```

Open:

```csharp
await gameStateMachine.PushOverlay<PausePhase>();
```

Close:

```csharp
await gameStateMachine.PopOverlay();
```

An Overlay Phase never loads game scenes.

---

# Adding a New Feature

Each gameplay mechanic is created as a separate Feature.

Example:

```text
Fishing

Craft

Dialogue

Quest

Inventory
```

Recommended Feature structure:

```text
Fishing
│
├── Controllers
├── Models
├── Views
├── Configs
├── Installers
└── Prefabs
```

A Feature should be as independent as possible.

---

# Adding a Service

A Service should be created when its functionality is shared by multiple systems.

Examples:

- AudioService
- GamePauseService
- InputService

Create the service.

```csharp
public class AudioService
{
}
```

Register it in an Installer.

```csharp
Container.Bind<AudioService>()
    .AsSingle();
```

Use it through Dependency Injection.

---

# Adding UI

UI is responsible only for presentation.

UI may:

- display data;
- hide data;
- send events.

UI must not modify the game state.

For example:

```text
InventoryView

DialogueWindow

PauseWindow
```

---

# Adding Configs

All configurable parameters should be stored in Configs.

For example:

```text
EnemyConfig

DialogueConfig

BalanceConfig
```

ScriptableObject is the preferred implementation.

---

# Adding Installers

Each independent Feature should have its own Installer.

For example:

```text
DialogueInstaller

BattleInstaller

QuestInstaller
```

An Installer is responsible only for dependency registration.

Any game logic inside an Installer is prohibited.

---

# Working with Scenes

All game scenes must be loaded only through SceneLoader.

Never use SceneManager directly.

Correct:

```text
GameStateMachine

↓

SceneGamePhase

↓

SceneLoader
```

---

# Working with Dependency Injection

All dependencies should be provided through constructors.

Correct:

```csharp
public BattleController(
    BattleService battleService,
    AudioService audioService)
{
}
```

Incorrect:

```csharp
var audio = new AudioService();
```

---

# Working with MonoBehaviour

A MonoBehaviour should contain only Unity-related code.

For example:

- component references;
- Unity event handling;
- delegating control to other systems.

Game logic should be implemented in regular C# classes.

---

# Working with Features

A Feature must not access the internal classes of another Feature directly.

Correct:

```text
Dialogue

↓

QuestService
```

Incorrect:

```text
DialogueController

↓

QuestDatabase

↓

QuestInternalManager
```

Communication between systems should occur through public interfaces.

---

# Scene Transition Flow

The correct sequence for switching between game modes.

```text
Player Action

↓

Controller

↓

GameStateMachine

↓

SceneGamePhase

↓

SceneLoader

↓

Unity SceneManager
```

Any deviation from this flow requires a separate architectural discussion.

---

# Adding Save Support

Every Feature whose data must be saved provides a dedicated Save DTO and contributor.

If the data must be restored, the Feature also provides a restorer with the same stable contributor ID.

Correct flow:

```text
Runtime Model

↓

IGameSaveContributor

↓

Save DTO

↓

GameSaveCoordinator
```

The DTO contains no `MonoBehaviour`, `Transform`, `GameObject`, `Component`, or other `UnityEngine.Object` reference.

Contributor and restorer are registered through the corresponding installer. Gameplay systems and UI never access the serializer, files, or full path directly.

The complete pipeline and migration rules are documented in `ArchitectureAtlas/09_SaveSystem.md`.

---

# Common Mistakes

## ❌ Creating services with `new`

Always use Dependency Injection.

---

## ❌ Using SceneManager

Always use SceneLoader.

---

## ❌ Large MonoBehaviours

A MonoBehaviour must not contain game logic.

---

## ❌ Feature depending directly on another Feature

Use services, events, or public interfaces.

---

## ❌ Duplicating logic

If the same code is used by multiple systems, it should be moved to Shared or Infrastructure.

---

# Pull Request Checklist

Before creating a Pull Request, verify the following:

- The architecture has not been violated.
- The code follows the Code Rules.
- All dependencies are registered.
- New classes are placed in the correct directories.
- There is no duplicated logic.
- SceneManager is not used directly.
- `new` is not used for services.
- All public classes have clear names.
- The project builds successfully.
- Documentation has been updated (if the architecture has changed).

---

# Summary

When developing new functionality, follow these rules:

- Every new gameplay mechanic is created as a separate Feature.
- Game modes are implemented as either a Main Phase or an Overlay Phase.
- All scenes are loaded only through SceneLoader.
- All dependencies are created through Dependency Injection.
- UI is responsible only for presentation.
- MonoBehaviour contains only Unity-specific code.
- Any new architectural idea must follow the principles described in `01_Architecture.md`.
