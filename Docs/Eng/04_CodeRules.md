# Code Rules

> Version: 1.0
> Last Updated: 13-07-2026

---

# Purpose

This document defines the mandatory development rules for the project.

All developers are required to follow these rules.

If a rule must be violated, the decision must be approved and documented in advance.

In case of conflicts, this document takes precedence over individual developer preferences.

---

# General Principles

When writing code, follow these principles:

- Simplicity is more important than complexity.
- Explicit code is better than implicit code.
- Composition is preferred over inheritance.
- Every system should have a single responsibility.
- Code should be understandable without additional comments.

---

# Architecture Rules

## Responsibility

Every class must have a single responsibility.

If a class performs two independent tasks, it should be split.

---

## Dependencies

Dependencies must always point from top to bottom.

```text
Application

↓

Infrastructure

↓

Game

↓

UI
```

Lower layers must not know about higher layers.

---

## Dependency Injection

### ✔ Allowed

Use Dependency Injection.

```csharp
public ExplorationSaveCatalogService(
    IGameSaveStorageProvider storageProvider,
    GameSaveCoordinator saveCoordinator)
{
}
```

---

### ❌ Forbidden

Creating services manually.

```csharp
new FileGameSaveStorageProvider(...);

new GameSaveCoordinator(...);
```

---

# Scene Management

### ✔ Allowed

Use SceneLoader.

```csharp
await sceneLoader.SwitchTo(...);
```

---

### ❌ Forbidden

Using the Unity SceneManager directly.

```csharp
SceneManager.LoadScene(...);

SceneManager.UnloadScene(...);
```

The only exception is the Infrastructure layer.

---

# Game State Machine

Game mode transitions must be performed only through the FSM.

### ✔ Allowed

```csharp
await gameStateMachine.ReplaceMain<BattlePhase>();
```

---

### ❌ Forbidden

Switching game scenes manually.

---

# Features

Every gameplay system must be independent.

### ✔ Allowed

```text
Dialogue

↓

QuestService
```

---

### ❌ Forbidden

```text
Dialogue

↓

QuestController

↓

QuestDatabase

↓

QuestInternalClass
```

Using the internal classes of another Feature is prohibited.

---

# MonoBehaviour

MonoBehaviour is used exclusively for Unity integration.

It may:

- obtain references;
- receive Unity events;
- delegate control to other systems.

---

### ❌ Forbidden

Storing gameplay business logic inside a MonoBehaviour.

---

# Update()

Update should be used only when objectively necessary.

Prefer using:

- events;
- UniTask;
- timers;
- SignalBus.

---

### ❌ Forbidden

Creating an Update method "just in case."

---

# Coroutines

The project prefers using UniTask.

Coroutines are allowed only when UniTask does not provide an equivalent solution.

---

# Async Code

All asynchronous code should use UniTask.

Using Task is allowed only when working with .NET libraries.

---

# Exceptions

Exceptions must not be used to control application logic.

An exception should represent a truly exceptional situation.

---

# Logging

Use Unity `Debug.Log` only:

- during development;
- for debugging.

Unnecessary logging should be removed before release.

---

# Comments

Comments explain **why**, not **what**.

### Good

```csharp
// A separate timer is used
// to synchronize the battle with the server.
```

---

### Bad

```csharp
// Increase HP
hp++;
```

Code should be self-documenting.

---

# Naming

The following naming conventions are used.

## Classes

```text
PlayerController

DialogueManager

QuestService
```

---

## Interfaces

```text
IAudioService

ISceneLoader

IGameSaveSerializer
```

---

## Services

Always use the suffix:

```text
Service
```

---

## Factories

Always use the suffix:

```text
Factory
```

---

## Installers

Always use the suffix:

```text
Installer
```

---

## Phases

Always use the suffix:

```text
Phase
```

---

## Configs

Always use the suffix:

```text
Config
```

---

## Scenes

All game scenes use the prefix:

```text
SC_
```

Example:

```text
SC_MainMenu

SC_World

SC_Battle
```

---

# Folder Rules

A new class must be placed according to its responsibility.

| Responsibility | Folder |
|----------------|--------|
| Application management | Application |
| Unity API integration | Infrastructure |
| Gameplay logic | Game |
| User interface | UI |

---

# Code Style

## Methods

A method should perform only one task.

If a method cannot be explained quickly in a single sentence, it should be split.

---

## Classes

Large classes should be divided into several smaller ones.

---

## Fields

Use `readonly` wherever possible.

---

## Magic Numbers

Unexplained numeric literals are prohibited.

Bad:

```csharp
speed = 7.5f;
```

Good:

```csharp
speed = movementConfig.DefaultSpeed;
```

---

# ScriptableObjects

ScriptableObjects are used only for storing data.

A ScriptableObject must not contain gameplay business logic.

---

# Resources

The Resources folder is not used.

All assets must be loaded through the project's asset loading system (for example, Addressables after they are implemented).

---

# Reflection

Reflection is allowed only inside Infrastructure.

---

# Singletons

Using the Singleton pattern is prohibited.

All global dependencies are provided through Dependency Injection.

---

# Static Classes

Static classes are allowed only for:

- constants;
- stateless utilities;
- extension methods.

---

# Events

Every event subscription must have a corresponding unsubscription.

Every subscription must have an obvious resource cleanup point.

---

# Save System

Gameplay systems must not work with files directly.

The common save pipeline goes through GameSaveCoordinator.

Only Save DTOs with no UnityEngine.Object references are serialized.

---

# Pull Requests

Before creating a Pull Request, verify the following:

- The code follows the Architecture.
- The code follows the Project Structure.
- The code follows the Developer Guide.
- The code follows this document.
- Required tests have been added (if applicable).
- The project compiles successfully.
- No temporary code remains.
- No `Debug.Log` statements remain.
- No undocumented TODOs remain.

---

# Forbidden

The following are prohibited in the project:

- using `new` to create services;
- using the Singleton pattern;
- using `SceneManager` directly;
- using `FindObjectOfType`;
- using `GameObject.Find`;
- using `Resources.Load`;
- storing gameplay logic inside a MonoBehaviour;
- directly modifying the state of another Feature;
- violating the architectural layers.

---

# Final Rule

Before writing any new class, answer these three questions:

1. Who will use this class?
2. What is its responsibility?
3. Is its purpose clear from its name?

If the answer to any of these questions is "no," the architectural decision should be reconsidered.

---

# Summary

Project code should be:

- simple;
- readable;
- extensible;
- testable;
- independent;
- consistent with the project architecture.

The primary goal of these rules is to ensure the project's long-term maintainability and a consistent development style regardless of the size of the team.
