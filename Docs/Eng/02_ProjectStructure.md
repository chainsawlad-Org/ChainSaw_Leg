# Project Structure

> Version: **2.0**  
> Last Updated: **27-07-2026**

---

# Purpose

This document defines the physical structure of the project and the responsibility of every major directory.

Its goal is to help every developer immediately understand where new code belongs and keep the project maintainable as it grows.

The primary rule is:

> **Code is organized by responsibility, never by convenience.**

---

# Root Structure

```text
Assets
│
└── _Project
    │
    ├── Application
    ├── Infrastructure
    ├── Game
    ├── UI
    ├── Configs
    ├── Content
    ├── Scenes
    └── Tests
```

Each top-level module has a single responsibility.

---

# Assembly Structure

Every major module owns its own Assembly Definition.

Example:

```text
ChainSawLeg.Application.Runtime

ChainSawLeg.Infrastructure.Runtime

ChainSawLeg.Game.Shared.Runtime

ChainSawLeg.UI.Runtime
```

Editor code must always live inside a separate Editor assembly.

Runtime assemblies must never reference Editor assemblies.

---

# Runtime Convention

Every assembly follows the same physical convention.

```text
Runtime
│
├── Bootstrap
├── Coordination
├── Factories
├── Installers
├── Services
├── Signals
├── Startup
└── StateMachine
```

Only folders that are actually needed should exist.

---

# Application

Application contains application flow.

It coordinates systems.

It never contains gameplay rules.

Typical responsibilities include:

- Bootstrap
- Startup
- State Machine
- Coordinators
- Application Services
- Installers
- Factories

---

## Bootstrap

Responsible for starting the application.

Contains:

```text
BootstrapRunner

BootstrapStartup
```

Bootstrap knows nothing about gameplay.

Its only responsibility is to prepare the application.

---

## Startup

Determines the initial game phase.

Contains:

```text
StartupResolver

StartupPhaseRegistry
```

Adding a new startup flow should never require modifying BootstrapRunner.

---

## StateMachine

Contains the lifecycle of game phases.

Example:

```text
GameStateMachine

GamePhase

SceneGamePhase

OverlayPhase
```

The StateMachine contains no gameplay rules.

---

## Coordination

Contains application use cases that connect multiple systems.

Example:

```text
MainMenuCoordinator

PauseMenuCoordinator

CheckpointSaveMenuCoordinator

ExplorationGameSaveLoadService
```

Coordinator orchestrates systems.

It does not implement gameplay logic.

---

## Services

Application Services are reusable services shared across multiple application scenarios.

Current organization:

```text
Services
│
├── Commands
├── Brokers
├── Registries
└── Runtime Services
```

Examples:

```text
MainMenuStartCommandService

DialogueService

DialogueRuntimeRegistry

MainMenuSaveBrowserRequestBroker
```

---

## Installers

Contains Zenject registrations.

Structure:

```text
Installers
│
├── Core
└── Features
```

Core installers register global systems.

Feature installers register gameplay features.

Installers must never contain business logic.

---

## FeatureAdapters

Contains Unity adapters connecting scene objects with Application systems.

Example:

```text
FeatureAdapters
│
├── Combat
├── Dialogue
└── Exploration
```

FeatureAdapters may know Unity.

They must not contain gameplay rules.

---

## Factories

Responsible for creating runtime objects through Dependency Injection.

Example:

```text
PhaseFactory
```

Factories never contain business logic.

---

## Signals *(Future)*

Contains application-wide events.

Example:

```text
DialogueStartedSignal

SceneLoadedSignal

PlayerDiedSignal
```

Signals provide loose coupling between independent systems.

---

## Editor

Contains editor-only tooling.

Runtime code must never depend on this directory.

---

# Infrastructure

Infrastructure contains technical integrations.

It is responsible for communication with Unity APIs, the operating system, and third-party libraries.

Typical structure:

```text
Infrastructure
│
├── Audio
├── Input
├── Reflection
├── Rendering
├── SaveSystem
└── SceneManagement
```

Infrastructure contains no gameplay rules.

---

## SceneManagement

Responsible for scene loading.

Example:

```text
SceneLoader

SceneNames

UnityActiveSceneProvider
```

Only SceneLoader may communicate directly with Unity SceneManager.

---

## Input

Contains the global input system.

Example:

```text
InputService

PlayerInputActions

GameplayInputBlockService
```

---

## SaveSystem

Contains the technical save pipeline.

Example:

```text
GameSaveCoordinator

GameSaveValidationService

GameSaveMigrationService

OdinGameSaveSerializer

FileGameSaveStorageProvider
```

Gameplay save contributors belong to Features, not Infrastructure.

---

## Reflection

Contains reflection utilities.

Example:

```text
AutoBinder
```

---

## Rendering

Contains rendering-related infrastructure.

Examples:

```text
Renderer2D

URP Settings
```

---

## Audio *(Future)*

Contains global audio systems.

Example:

```text
AudioService

MusicPlayer

SoundPlayer
```

---

# Game

Game contains all gameplay rules.

Every gameplay mechanic belongs here.

Structure:

```text
Game
│
├── Features
└── Shared
```

---

# Features

Every gameplay mechanic is implemented as an independent Feature.

Example:

```text
Dialogue

Combat

Exploration

Inventory

Quest

Minigames
```

A Feature should be as self-contained as possible.

---

## Internal Feature Structure

Recommended layout:

```text
Feature
│
├── Controllers
├── Models
├── Views
├── Services
├── Runtime
├── Configs
└── Prefabs
```

Small Features may omit unnecessary folders.

---

## Controllers

Contain gameplay behavior.

Controllers implement game rules.

---

## Models

Contain gameplay data.

Models should not depend on Unity.

---

## Views

Display gameplay information.

Views emit user interaction events.

Views do not make gameplay decisions.

---

## Services

Contain Feature-local services.

Example:

```text
DialogueHistoryService
```

These services are used only inside the Feature.

---

## Runtime

Contains runtime objects that do not fit into other categories.

Examples:

```text
Runtime Registries

Runtime Context

Runtime Cache
```

---

## Configs

Contains Feature-specific configuration.

Usually ScriptableObjects.

---

## Prefabs

Contains prefabs used only by the Feature.

---

## Registration

Gameplay Features do not contain Zenject Installers.

Registration happens inside:

```text
Application
    Installers
        Features
```

---

# Shared

Contains reusable gameplay code shared between multiple Features.

Example:

```text
Health

Damage

Stats

Interactable

SaveSystem
```

Shared must never become a miscellaneous folder.

---

# UI

Contains presentation only.

UI displays information and forwards user interaction.

Typical structure:

```text
UI
│
├── MainMenu
├── PauseMenu
├── CheckpointSave
├── Common
└── Services
```

Future additions may include:

```text
HUD

Widgets

Popups
```

---

# Configs

Contains global project configuration.

Example:

```text
GameConfig

LocalizationConfig

BalanceConfig
```

Feature-specific configuration belongs inside the corresponding Feature.

---

# Content

Contains game assets.

Example:

```text
Sprites

Fonts

Animations

Audio

Materials
```

No gameplay code belongs here.

---

# Scenes

Contains Unity scenes.

Recommended structure:

```text
Scenes
│
├── Core
│   └── SC_Persistent
│
├── Menu
│   └── SC_MainMenu
│
├── World
│   └── SC_World
│
├── Battle
│   └── SC_Battle
│
└── Minigames
```

All scenes must use the prefix:

```text
SC_
```

---

# Tests

Contains project tests.

Example:

```text
Editor

PlayMode
```

Tests should reference Runtime assemblies only.

---

# Assembly Definition Files

Every Runtime module owns exactly one Assembly Definition.

Every Editor module owns exactly one Editor Assembly.

Assembly dependencies should always point downward.

```text
Application
        ↓
Game
        ↓
Infrastructure
```

Circular references are prohibited.

---

# PLACEMENT.md

Every major directory should contain a `PLACEMENT.md` file.

Its purpose is to explain:

- what belongs in the folder;
- what must not be placed there;
- typical examples;
- common mistakes.

A developer should understand the folder without opening any source code.

---

# Where Should a New Class Be Created?

| If the class... | Place it in... |
|-----------------|----------------|
| Starts the application | Application |
| Coordinates multiple systems | Application/Coordination |
| Executes an application command | Application/Services |
| Integrates Unity or external libraries | Infrastructure |
| Implements gameplay rules | Game |
| Displays UI | UI |
| Adapts Unity scene objects | FeatureAdapters |
| Stores configuration | Configs |

If there is uncertainty between two folders, the responsibility of the class is probably incorrect.

---

# Feature First Rule

Every new gameplay mechanic is created as an independent Feature.

Incorrect:

```text
Controllers

Models

Views

Services
```

Correct:

```text
Features

    Dialogue

    Combat

    Exploration

    Inventory

    Quest
```

Features should remain independent whenever possible.

---

# Naming Convention

| Type | Example |
|------|---------|
| Scene | SC_MainMenu |
| Phase | ExplorationPhase |
| Service | DialogueService |
| Interface | IDialogueService |
| Installer | DialogueInstaller |
| Factory | PhaseFactory |
| Coordinator | PauseMenuCoordinator |
| Loader | SceneLoader |
| Registry | DialogueRuntimeRegistry |
| Command | MainMenuStartCommandService |
| Broker | MainMenuSaveBrowserRequestBroker |
| Controller | DialogueController |
| View | DialogueView |
| Config | DialogueConfig |

---

# Summary

The architecture follows these principles:

- Organize code by responsibility.
- Keep gameplay rules inside the Game layer.
- Keep Unity and third-party integrations inside Infrastructure.
- Use Application to orchestrate systems, never to implement gameplay.
- Keep Features independent.
- Keep UI passive.
- Every Runtime module owns its own Assembly Definition.
- Every major folder should contain a PLACEMENT.md.
- The project structure must scale to hundreds of classes without becoming difficult to navigate.