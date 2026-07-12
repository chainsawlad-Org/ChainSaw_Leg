# Dependency Injection

> Version: 1.0
> Last Updated: 12-07-2026

---

# Purpose

Dependency Injection (DI) is the mechanism for creating and providing dependencies between application components.

In this project, Dependency Injection is used to:

- reduce coupling between subsystems;
- centralize object creation;
- simplify testing;
- ensure architectural extensibility.

The project uses **Zenject** as its DI container.

---

# Responsibilities

The Dependency Injection subsystem is responsible for:

- creating global objects;
- registering services;
- registering game phases;
- registering infrastructure components;
- providing dependencies through constructors.

Dependency Injection is not responsible for:

- gameplay logic;
- the application startup sequence;
- scene management;
- game mode management.

---

# High-Level Overview

```mermaid
flowchart TD

ProjectContext

ProjectInstaller

DiContainer

GameStateMachine

SceneLoader

BootstrapRunner

ProjectContext --> ProjectInstaller

ProjectInstaller --> DiContainer

DiContainer --> GameStateMachine
DiContainer --> SceneLoader
DiContainer --> BootstrapRunner
```

Once registration is complete, all objects are created automatically by the container.

---

# Components

The subsystem consists of the following components.

```text
ProjectContext

↓

ProjectInstaller

↓

Installers

↓

DiContainer

↓

Application
```

---

# ProjectContext

ProjectContext is the Composition Root of the project.

It is created by Unity when the application starts.

After ProjectContext is created, dependency registration begins.

---

# ProjectInstaller

ProjectInstaller is the main Installer of the project.

Its responsibility is to register all global dependencies.

For example:

- GameStateMachine
- SceneLoader
- PhaseFactory
- BootstrapRunner
- StartupResolver
- StartupPhaseRegistry

ProjectInstaller contains no gameplay logic.

---

# Installers

Installers are used to group registrations by responsibility.

For example:

```text
ProjectInstaller

↓

PhaseInstaller

↓

ServiceInstaller
```

Each Installer is responsible only for its own area.

---

# Auto Registration

The project uses automatic registration for certain types.

For example:

```text
GamePhase

↓

AutoBinder

↓

Container.Bind()
```

This eliminates the need to manually register every new phase.

---

# Object Creation

All objects are created by the Zenject container.

A typical sequence looks like this.

```mermaid
sequenceDiagram

participant Class

participant DiContainer

participant Dependency

Class->>DiContainer: Resolve()

DiContainer->>Dependency: Create()

Dependency-->>DiContainer: Instance

DiContainer-->>Class: Inject Dependency
```

Developers do not create dependencies manually.

---

# Constructor Injection

The primary way to receive dependencies is through the constructor.

Example:

```csharp
public class BattlePhase : SceneGamePhase
{
    public BattlePhase(ISceneLoader sceneLoader)
        : base(sceneLoader)
    {
    }
}
```

The constructor explicitly shows which dependencies the class requires.

---

# Lifetime

Most global services are registered as singletons within the container.

For example:

```csharp
Container.Bind<ISceneLoader>()
    .To<SceneLoader>()
    .AsSingle();
```

This means that only one instance of the object exists during the application's lifetime.

---

# Dependency Graph

After dependencies have been registered, an object graph is created.

```mermaid
flowchart TD

BootstrapRunner --> GameStateMachine

BootstrapRunner --> StartupResolver

GameStateMachine --> PhaseFactory

PhaseFactory --> DiContainer

SceneGamePhase --> SceneLoader
```

Each object receives only the dependencies it actually requires.

---

# Composition Root

The Composition Root is the only place where dependencies are wired together.

In the current project, the Composition Root consists of:

```text
ProjectContext

↓

ProjectInstaller

↓

Installers
```

All registrations must be performed here.

---

# Design Principles

## Constructor Injection

All required dependencies are provided through the constructor.

This makes dependencies explicit.

---

## Explicit Dependencies

A class should receive only the dependencies it actually uses.

Injecting dependencies "just in case" is not allowed.

---

## No Manual Creation

Gameplay code must not create services manually.

All objects are provided by the container.

---

## Composition Root

Dependency registration is centralized.

This makes it easy to see which systems exist in the project.

---

## Loose Coupling

Classes should depend on interfaces rather than concrete implementations.

For example:

```text
ISceneLoader

↓

SceneLoader
```

This allows implementations to be replaced in the future without changing gameplay code.

---

# Current Installers

The project currently uses the following Installers.

```text
ProjectInstaller

PhaseInstaller

ServiceInstaller
```

This list may be expanded in the future.

For example:

```text
BattleInstaller

UIInstaller

SaveInstaller

AudioInstaller
```

---

# Current Registration Flow

```mermaid
flowchart TD

ProjectInstaller

PhaseInstaller

ServiceInstaller

GameStateMachine

SceneLoader

BootstrapRunner

ProjectInstaller --> PhaseInstaller

ProjectInstaller --> ServiceInstaller

PhaseInstaller --> GameStateMachine

ServiceInstaller --> SceneLoader

ProjectInstaller --> BootstrapRunner
```

---

# Common Mistakes

## ❌ Creating objects with `new`

Bad:

```csharp
var loader = new SceneLoader();
```

Good:

```csharp
public BattlePhase(ISceneLoader sceneLoader)
{
}
```

---

## ❌ Using `Container.Resolve()`

Gameplay code should never access the container directly.

`Resolve()` is allowed only inside infrastructure components (for example, Factories).

---

## ❌ Service Locator

The container must not be used as a global Service Locator.

Dependencies should always be explicitly provided through constructors.

---

## ❌ Hidden dependencies

If a class uses a service, it must receive it through its constructor.

Runtime dependency lookups are not allowed.

---

## ❌ Registering dependencies in arbitrary places

All registrations must be performed through Installers.

---

# Extension Points

As the project grows, the Dependency Injection subsystem can be extended.

For example:

- Feature Installers;
- Scene Installers;
- SignalBus;
- object factories;
- Memory Pools;
- Addressables Factories.

Such extensions should be integrated into the existing Installer system.

---

# Related Documents

- 01_ApplicationLifecycle.md
- 02_Bootstrap.md
- 03_Startup.md
- 04_GameStateMachine.md
- 05_SceneManagement.md
- 07_Features.md
- 04_CodeRules.md