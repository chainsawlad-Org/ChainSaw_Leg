# Architecture Overview

> Version: 1.0
> Last Updated: 12-07-2026

---

# Purpose

The Architecture Overview illustrates how the project's core subsystems interact with each other.

This document does not describe the internal implementation of individual systems. Those details are covered in the corresponding **Architecture Atlas** documents.

---

# High-Level Architecture

```mermaid
flowchart TD

Bootstrap

Startup

GameStateMachine

SceneManagement

Features

UI

Bootstrap --> Startup

Startup --> GameStateMachine

GameStateMachine --> SceneManagement

GameStateMachine --> Features

Features --> UI
```

---

# System Overview

The project consists of six primary architectural subsystems.

| System | Responsibility |
|----------|----------------|
| Bootstrap | Application startup |
| Startup | Determining the initial game phase |
| Game State Machine | Managing game modes |
| Scene Management | Loading and switching game scenes |
| Features | Gameplay logic |
| UI | Displaying information to the player |

Each subsystem has a clearly defined area of responsibility.

---

# Application Flow

After the application starts, control passes through several sequential stages.

```text
Application Start

↓

Bootstrap

↓

Startup

↓

Game State Machine

↓

Main Phase

↓

Gameplay
```

Once Bootstrap is complete, control is fully transferred to the Game State Machine.

---

# Runtime Flow

During gameplay, control flows as follows.

```mermaid
flowchart TD

Player

UI

GameStateMachine

MainPhase

OverlayPhase

SceneLoader

Player --> UI

UI --> MainPhase

MainPhase --> GameStateMachine

GameStateMachine --> OverlayPhase

GameStateMachine --> SceneLoader
```

---

# Layered Architecture

The project architecture is divided into four layers.

```text
Application

↓

Infrastructure

↓

Game

↓

UI
```

Each layer depends only on the layers below it.

Reverse dependencies are prohibited.

---

# Main Phase Lifecycle

Only one Main Phase can be active at any given time.

```mermaid
flowchart LR

MainMenu

Exploration

Battle

MainMenu --> Exploration

Exploration --> Battle

Battle --> Exploration
```

Transitions between game modes are always performed through the Game State Machine.

---

# Overlay Lifecycle

Overlay Phases operate on top of the active Main Phase.

```mermaid
flowchart TD

MainPhase

Dialogue

Pause

Inventory

MainPhase --> Dialogue

Dialogue --> Pause

Pause --> Inventory
```

Overlays are organized as a stack.

The most recently opened Overlay is always closed first.

---

# Scene Lifecycle

Scene Management completely encapsulates the Unity SceneManager.

```mermaid
flowchart LR

GameStateMachine

SceneGamePhase

SceneLoader

Unity

GameStateMachine --> SceneGamePhase

SceneGamePhase --> SceneLoader

SceneLoader --> Unity
```

Gameplay systems never access the SceneManager directly.

---

# Dependency Injection

All core objects are created by the Zenject container.

```mermaid
flowchart TD

ProjectContext

ProjectInstaller

Installers

DiContainer

Application

ProjectContext --> ProjectInstaller

ProjectInstaller --> Installers

Installers --> DiContainer

DiContainer --> Application
```

All dependencies are injected through constructors.

---

# Feature Architecture

Gameplay logic follows the **Feature First** architecture.

```text
Game

├── Battle

├── Dialogue

├── Inventory

├── Quest

├── NPC

└── Minigames
```

Each Feature is an independent gameplay subsystem.

---

# UI Architecture

The UI is separated from the gameplay logic.

```mermaid
flowchart LR

Player

UI

Game

Player --> UI

UI --> Game

Game --> UI
```

The UI displays data and forwards player actions.

Gameplay decisions are made exclusively by the gameplay systems.

---

# Dependency Direction

Dependencies always flow in a single direction.

```mermaid
flowchart TD

Bootstrap

GameStateMachine

SceneManagement

Features

UI

Bootstrap --> GameStateMachine

GameStateMachine --> SceneManagement

GameStateMachine --> Features

Features --> UI
```

The architecture does not allow cyclic dependencies between subsystems.

---

# Design Principles

The project is built around the following principles:

- Single Responsibility Principle
- Separation of Concerns
- Dependency Injection
- Feature First
- Composition over Inheritance
- Explicit Lifecycle
- Explicit Dependencies
- Open/Closed Principle

All new systems should follow these principles.

---

# Architecture Atlas

Each subsystem is documented in detail in its own Architecture Atlas document.

| Document | Description |
|----------|-------------|
| 01_ApplicationLifecycle | Complete application lifecycle |
| 02_Bootstrap | Application startup |
| 03_Startup | Initial phase selection |
| 04_GameStateMachine | Game mode management |
| 05_SceneManagement | Scene management |
| 06_DependencyInjection | Dependency Injection |
| 07_Features | Gameplay Feature architecture |
| 08_UI | User Interface architecture |

---

# Summary

The project architecture is built around small, independent subsystems.

Each subsystem has a single responsibility, communicates with other systems through well-defined interfaces, and can evolve independently.

This approach provides:

- low coupling between components;
- high extensibility;
- ease of maintenance;
- a predictable application lifecycle;
- the ability to scale the project without significant changes to the existing architecture.