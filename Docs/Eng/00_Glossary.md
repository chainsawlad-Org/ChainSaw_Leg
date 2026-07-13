# Glossary

> Version: 1.0
> Last Updated: 13-07-2026

---

# Purpose

This document defines the terminology used throughout the project.

All architectural documents, comments, and discussions must use the definitions provided here.

If the same term can be interpreted in different ways, the definition in this document is considered the correct one.

---

# Architecture

## Application

The layer responsible for managing the application's lifecycle.

Responsibilities:

- starting the game;
- switching game states;
- registering dependencies;
- coordinating core systems.

The Application layer does not contain game logic.

---

## Infrastructure

The layer for global technical integrations with Unity API, the file system, and external libraries.

Examples:

- SceneLoader
- GameSaveCoordinator
- AudioService
- InputService

Infrastructure provides functionality but does not contain game rules.

---

## Game

The layer that contains all game logic.

All game rules must be located here.

---

## UI

The presentation layer.

UI displays information to the player, receives input, and emits view events.

UI does not contain game rules.

---

# Bootstrap

The application startup process.

Bootstrap begins immediately after the dependency container is created and ends when the first game phase starts.

In this project, Bootstrap consists of:

- BootstrapStartup
- BootstrapRunner

---

# BootstrapStartup

The entry point of the Bootstrap process.

Integrates with Zenject and starts BootstrapRunner.

It is no longer used after startup is complete.

---

# BootstrapRunner

The coordinator of the application startup process.

Responsibilities:

- loading the Persistent Scene;
- determining the initial Phase;
- starting the GameStateMachine.

Does not contain game logic.

---

# Startup

A subsystem that determines the application's initial state.

Used only when the game starts.

Contains:

- StartupResolver
- StartupPhaseRegistry

---

# StartupResolver

Determines which game phase should be started first.

For example:

- MainMenu
- Exploration

---

# StartupPhaseRegistry

Stores the mapping between Unity scenes and game phases.

Used only by StartupResolver.

---

# Game State Machine (FSM)

The main system for managing game states.

The FSM is responsible only for switching game phases.

The FSM does not contain game logic.

---

# Phase

A game state with a lifecycle.

Every Phase has two methods:

- Enter()
- Exit()

---

# Main Phase

The primary game mode.

Only one Main Phase can be active at any given time.

Examples:

- Main Menu
- Exploration
- Combat
- Exploration
- Minigame

A Main Phase is usually associated with a game scene.

---

# Overlay Phase

A temporary game mode that runs on top of the Main Phase.

Overlay Phases are stored in a stack.

Examples:

- Pause
- Dialogue
- Inventory
- Settings

An Overlay Phase does not replace the Main Phase.

---

# SceneGamePhase

A type of Main Phase associated with a Unity Scene.

When entered, it automatically loads the corresponding scene through SceneLoader.

---

# Feature

An independent gameplay subsystem.

A Feature is responsible for a single gameplay mechanic.

Examples:

- Dialogue
- Battle
- Inventory
- Quest
- Craft
- Fishing

A Feature should be as independent as possible.

---

# Service

A system that provides functionality to other subsystems.

A Service does not contain game rules.

Examples:

- AudioService
- InputService
- GamePauseService

---

# Coordinator

A class that coordinates multiple components within one application use case.

For example, GameSaveCoordinator manages the technical save pipeline, while PauseMenuCoordinator connects UI with phases and services.

A Coordinator contains no View logic and does not access global state through a static Instance.

---

# Scene Service

A Service whose lifecycle is tied to a game scene.

It is created when the scene is loaded and destroyed when the scene is unloaded.

---

# Installer

A class that registers dependencies in the Zenject container.

An Installer does not contain business logic.

---

# Factory

A class that creates objects through Dependency Injection.

A Factory hides the object creation details.

---

# Dependency Injection (DI)

A way of providing dependencies to an object from the outside.

This project uses Zenject.

Objects must not create their own dependencies.

---

# Dependency

An object required for another object to function.

For example:

```
GameSaveCoordinator

↓

IGameSaveSerializer

↓

IGameSaveStorageProvider
```

IGameSaveSerializer and IGameSaveStorageProvider are dependencies of GameSaveCoordinator.

---

# Scene

A Unity Scene.

Represents a collection of Unity objects.

In this project, all game scenes use the following prefix:

```
SC_
```

For example:

```
SC_MainMenu

SC_World

SC_Battle
```

---

# Persistent Scene

A special scene that exists throughout the entire lifetime of the application.

It is never unloaded when switching game scenes.

Used to host:

- global UI;
- services;
- loading screens;
- persistent objects.

---

# SceneLoader

The only system allowed to work with the Unity SceneManager.

All game scenes are loaded exclusively through SceneLoader.

---

# Scene Transition

A transition between game modes.

Standard flow:

```
GameStateMachine

↓

SceneGamePhase

↓

SceneLoader

↓

Unity SceneManager
```

---

# Controller

A class that controls the behavior of a gameplay system.

A Controller contains game logic.

It is not responsible for presentation.

---

# View

A class responsible for displaying information.

A View does not make gameplay decisions.

---

# Model

A class that contains game data.

A Model does not depend on Unity.

---

# Config

An object containing configurable system parameters.

Usually implemented as a ScriptableObject.

---

# Signal *(Future)*

A message representing an event that has occurred.

Used for communication between independent systems.

Example:

```
PlayerDied

InventoryOpened

BattleStarted
```

---

# Save System

A subsystem responsible for saving and loading the game state.

Gameplay systems do not work with files directly.

The common pipeline is coordinated by GameSaveCoordinator.

Gameplay Features participate through Save DTOs, contributors, and restorers.

---

# Addressables *(Future)*

Unity's asset loading system.

Once implemented, it will become the only way to load game assets.

---

# Composition Root

The point in the application where all dependencies are created and connected.

In the current project, the Composition Root is represented by `ProjectContext`, `SceneContext`, and installers under `Application/Installers`.

---

# Business Logic

The game's rules.

For example:

- damage calculation;
- victory validation;
- quest execution;
- inventory management.

Business Logic always belongs in the Game layer.

---

# Unity Logic

Code related exclusively to Unity.

For example:

- Awake()
- Start()
- Update()
- component references;
- Unity event handling.

Such code should be kept to a minimum and, whenever possible, only delegate control to the business logic.

---

# Summary

This glossary is the single source of terminology for the project.

When new architectural concepts are introduced, they must first be defined in this document and only then used throughout the rest of the documentation.
