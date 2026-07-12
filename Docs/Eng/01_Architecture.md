# Architecture

> Version: 1.0
> Last Updated: 12-07-2026

---

# Purpose

This document provides a high-level overview of the project's architecture.

It answers the following questions:

- How is the project structured?
- Which architectural principles are used?
- What layers make up the project?
- How do the subsystems interact?

A detailed description of each subsystem is provided in the **ArchitectureAtlas**.

---

# Philosophy

The project is built around several key principles.

- Simplicity is more important than complexity.
- Every system has a single responsibility.
- Subsystems should be as independent as possible.
- Game logic is separated from the Unity API.
- All dependencies are created through Dependency Injection.
- New functionality is added by extending the existing code rather than modifying it.

The primary goal of the architecture is to make the project scalable and maintainable in the long term.

---

# Architectural Layers

The project is divided into four independent layers.

```text
Application

Infrastructure

Game

UI
```

Each layer has its own area of responsibility.

---

## Application

Manages the application lifecycle.

Responsibilities:

- starting the application;
- managing game modes;
- registering dependencies;
- coordinating core subsystems.

The Application layer does not contain game logic.

---

## Infrastructure

Isolates the project from the Unity API and external libraries.

Examples:

- Scene Management
- Save System
- Audio
- Input
- Addressables

Infrastructure provides services to other layers.

---

## Game

Contains the game rules.

All gameplay mechanics must be implemented here.

For example:

- Battle
- Dialogue
- Inventory
- Quest
- NPC
- Minigames

The Game layer knows nothing about Bootstrap or Dependency Injection.

---

## UI

Responsible exclusively for presenting information.

UI:

- displays data;
- receives user input;
- sends events to other systems.

UI does not contain game rules.

---

# Dependency Direction

Dependencies always point from top to bottom.

```text
Application
      │
      ▼
Infrastructure
      │
      ▼
Game
      │
      ▼
UI
```

Reverse dependencies are not allowed.

---

# Features

Game logic is organized using the Feature First approach.

Each gameplay mechanic is implemented as an independent Feature.

For example:

```text
Battle

Dialogue

Inventory

Quest

Craft

Fishing
```

A Feature should be as independent as possible from the rest of the project.

---

# Dependency Injection

All core objects are created by the Zenject container.

The project does not use global Singletons.

Dependencies are provided through constructors.

This reduces coupling between subsystems and simplifies testing.

---

# Scene Management

Scene management is centralized.

Game code must not use the Unity SceneManager directly.

All scene operations are performed through SceneLoader.

---

# Scalability

The project architecture should allow:

- adding new game modes;
- adding new Features;
- replacing service implementations;
- extending functionality without modifying existing code.

The core development principle is:

> **Open for Extension, Closed for Modification**

---

# Documentation

The project documentation is organized into several levels.

| Document | Purpose |
|----------|---------|
| 00_Glossary | Project terminology |
| 01_Architecture | Overall architecture |
| 02_ProjectStructure | Project structure |
| 03_DeveloperGuide | Developer guide |
| 04_CodeRules | Coding rules |
| ArchitectureAtlas | Detailed description of architectural subsystems |

---

# Architecture Atlas

Detailed information about each subsystem can be found in:

```text
Docs/
    ArchitectureAtlas/
```

Each Atlas document describes a single architectural subsystem.

For example:

```text
Application Lifecycle

Bootstrap

Startup

Game State Machine

Scene Management

Dependency Injection

Save System
```

The Atlas is the primary source of the project's technical documentation.

---

# Summary

The project architecture is built around the following principles:

- Clear separation of responsibilities.
- Independent architectural layers.
- Feature First.
- Dependency Injection.
- Centralized infrastructure management.
- Scalability through extension.
- Detailed technical documentation for every subsystem in **ArchitectureAtlas**.