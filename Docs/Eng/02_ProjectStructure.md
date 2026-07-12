# Project Structure

> Version: 1.0  
> Last Updated: 12-07-2026

---

# Purpose

This document describes the project structure and the purpose of each directory.

Its goal is to help developers quickly understand **where new code should be placed** and to prevent the project from growing in a chaotic manner.

The main rule:

> **Every class should be located where its responsibility belongs, not where it is most convenient to place it.**

---

# Root Structure

The project is divided into several major modules.

```text
Assets
│
├── Application
├── Infrastructure
├── Game
├── UI
├── Configs
├── Content
├── Scenes
├── Plugins
```

Each directory has its own area of responsibility.

---

# Application

Application contains the code responsible for managing the application lifecycle.

It does not contain game logic.

Typical structure:

```text
Application
│
├── Bootstrap
├── Startup
├── StateMachine
├── Installers
├── Factories
├── Signals
```

---

## Bootstrap

Responsible for starting the application.

Contains:

```text
BootstrapStartup

BootstrapRunner
```

Bootstrap knows nothing about gameplay mechanics.

Its purpose is to prepare the application for execution.

---

## Startup

Determines the initial state of the game.

For example:

```text
StartupResolver

StartupPhaseRegistry
```

Adding a new startup scene is done here.

Bootstrap itself does not need to be modified.

---

## StateMachine

Contains the game mode management system.

For example:

```text
GameStateMachine

GamePhase

SceneGamePhase

OverlayPhase
```

There should be no game logic here.

The StateMachine is only aware of the lifecycle of phases.

---

## Installers

Contains all Zenject Installers.

For example:

```text
ProjectInstaller

PhaseInstaller

ServiceInstaller
```

An Installer is responsible only for registering dependencies.

Any business logic inside an Installer is prohibited.

---

## Factories

A Factory is responsible for creating objects through Dependency Injection.

For example:

```text
PhaseFactory
```

A Factory does not contain game logic.

---

## Signals *(Future)*

Contains application events.

For example:

```text
PlayerDiedSignal

SceneLoadedSignal

DialogueStartedSignal
```

Signals are used for communication between independent systems.

---

# Infrastructure

Infrastructure acts as the adapter between the project and Unity.

Any code that interacts directly with the Unity API must be placed here.

Typical structure:

```text
Infrastructure
│
├── SceneManagement
├── Audio
├── Input
├── Reflection
├── Rendering
```

---

## SceneManagement

Contains scene management functionality.

For example:

```text
SceneLoader
```

No other code in the project should use the Unity SceneManager directly.

---

## Audio

The project's future audio system.

For example:

```text
AudioService

MusicPlayer

SoundPlayer
```

---

## Input

The user input system.

For example:

```text
InputService

InputActions

InputMapper
```

---

# Game

Game contains all gameplay logic in the project.

Every game rule must be implemented here.

Typical structure:

```text
Game
│
├── Features
├── Common
├── Shared
└── Gameplay
```

---

# Features

Each gameplay mechanic is implemented as a separate Feature.

Example:

```text
Dialogue

Battle

Inventory

Quest

Exploration

Minigames
```

A Feature should be as independent as possible.

---

## Internal Feature Structure

Recommended structure:

```text
Dialogue
│
├── Controllers
├── Models
├── Views
├── Services
├── Configs
├── Installers
├── Prefabs
└── Runtime
```

If a Feature is small, a simplified structure is acceptable.

---

## Controllers

Contain the Feature's gameplay logic.

A Controller manages the system's behavior.

It should not be responsible for presentation.

---

## Models

Contain gameplay data.

A Model does not depend on Unity.

---

## Views

Responsible only for presentation.

A View knows nothing about the internal logic of the Feature.

---

## Services

Local services of the Feature.

For example:

```text
DialogueHistoryService
```

These services are used only within the corresponding Feature.

---

## Configs

Contains ScriptableObjects and other Feature configuration.

---

## Installers

The Feature Installer.

Registers dependencies only for that Feature.

---

# Shared

Contains common code shared between multiple Features.

For example:

```text
Health

Damage

Stats

CommonInterfaces
```

Shared must not become a dumping ground for common code.

If code belongs to only one Feature, it should remain inside that Feature.

---

# UI

UI contains presentation code only.

Typical structure:

```text
UI
│
├── Windows
├── HUD
├── Popups
├── Widgets
└── Common
```

---

## Windows

Full-screen windows.

For example:

```text
MainMenu

Settings

Inventory
```

---

## HUD

The permanently visible interface.

For example:

```text
HealthBar

MiniMap

QuestTracker
```

---

## Popups

Temporary windows.

For example:

```text
Confirmation

Warning

MessageBox
```

---

## Widgets

Reusable UI elements.

For example:

```text
Button

Slider

InventorySlot

CharacterCard
```

---

# Configs

Contains global project configuration.

For example:

```text
GameConfig

BalanceConfig

LocalizationConfig
```

Any configuration should be placed here or inside the corresponding Feature.

---

# Content

Contains game assets.

For example:

```text
Sprites

Audio

Animations

Fonts

Materials
```

There is no game code here.

---

# Scenes

Contains all game scenes.

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

All game scenes must use the following prefix:

```text
SC_
```

---

# Plugins

Contains third-party plugins.

For example:

```text
Zenject

DOTween

UniTask
```

Modifying project code inside the Plugins directory is prohibited.

---

# Where should a new class be created?

Before creating a class, its responsibility must be identified.

| If the class... | Place it in... |
|-----------------|----------------|
| Starts the application | Application |
| Works with the Unity API | Infrastructure |
| Contains game rules | Game |
| Displays information | UI |
| Stores configuration | Configs |

If there is uncertainty between two folders, the class responsibility has most likely been defined incorrectly.

---

# Feature First Rule

New gameplay functionality should always be created as a new Feature.

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

    Fishing

    Craft

    Dialogue

    Inventory
```

Each Feature should be as autonomous as possible.

---

# Naming Convention

The following naming conventions are used.

| Type | Example |
|------|---------|
| Scene | SC_MainMenu |
| Phase | MainMenuPhase |
| Service | AudioService |
| Interface | IAudioService |
| Installer | BattleInstaller |
| Factory | EnemyFactory |
| Config | BattleConfig |
| View | InventoryView |
| Controller | DialogueController |

---

# Summary

The main project structure rules are:

- Organize code by responsibility, not by file type.
- Gameplay logic belongs only in the Game layer.
- Unity API interaction is isolated in Infrastructure.
- UI is responsible only for presentation.
- New gameplay mechanics are created as separate Features.
- Each Feature should be as independent as possible.
- The project structure should make code easy to find and the project easy to scale.