# Features

> Version: 1.0
> Last Updated: 12-07-2026

---

# Purpose

Feature — это независимая игровая подсистема, реализующая одну законченную игровую механику.

Архитектура проекта построена по принципу **Feature First**.

Каждая игровая механика разрабатывается как самостоятельная Feature с минимальными зависимостями от остальных частей проекта.

---

# Responsibilities

Feature отвечает за:

- реализацию одной игровой механики;
- управление собственным состоянием;
- взаимодействие с другими системами через публичные интерфейсы.

Feature не отвечает за:

- запуск приложения;
- управление сценами;
- управление игровыми фазами;
- создание глобальных сервисов.

---

# Current Features

На текущем этапе проекта список Feature находится в процессе формирования.

Планируемые игровые подсистемы:

```
Battle

Dialogue

Inventory

Quest

NPC

Craft

Fishing

Minigames
```

По мере разработки каждая Feature получит собственную техническую документацию в ArchitectureAtlas.

---

# High-Level Overview

```mermaid
flowchart TD

Game

Battle

Dialogue

Inventory

Quest

Game --> Battle
Game --> Dialogue
Game --> Inventory
Game --> Quest
```

Каждая Feature является самостоятельной частью игрового слоя.

---

# General Structure

Типовая структура Feature.

```text
Feature/

├── Controllers/
├── Models/
├── Views/
├── Services/
├── Configs/
├── Installers/
└── Runtime/
```

Допускается изменение структуры, если это делает Feature проще и понятнее.

---

# Design Principles

## Single Responsibility

Каждая Feature реализует одну игровую механику.

---

## Independence

Feature должна быть максимально независимой.

Связь между Feature должна осуществляться через публичные интерфейсы или общие сервисы.

---

## Encapsulation

Внутренняя реализация Feature не должна использоваться другими Feature напрямую.

---

## Composition

Feature строятся из небольших компонентов.

Предпочтительна композиция вместо сложной иерархии наследования.

---

## Scalability

Feature должна легко расширяться без изменения существующего кода.

---

# Communication

Feature не должны обращаться к внутренним классам друг друга.

Допустимые способы взаимодействия:

- публичные интерфейсы;
- сервисы;
- события (после внедрения Event System).

---

# Lifecycle

Жизненный цикл Feature определяется активной Main Phase или Overlay Phase.

Feature создаётся при входе в соответствующий игровой режим и завершает работу при его завершении.

---

# Future Documentation

По мере разработки проекта каждая Feature получит собственный документ ArchitectureAtlas.

Например:

```
BattleArchitecture.md

DialogueArchitecture.md

InventoryArchitecture.md

QuestArchitecture.md
```

Эти документы будут содержать:

- архитектуру Feature;
- диаграммы;
- жизненный цикл;
- взаимодействие компонентов;
- правила расширения.

---

# Common Mistakes

## ❌ Зависимость от внутренней реализации другой Feature

Использовать только публичные интерфейсы.

---

## ❌ Смешивание нескольких механик

Одна Feature — одна игровая механика.

---

## ❌ Зависимость от Unity API в бизнес-логике

Игровые правила должны быть максимально независимы от Unity.

---

# Related Documents

- 01_Architecture.md
- 02_ProjectStructure.md
- 03_DeveloperGuide.md
- 04_CodeRules.md
- 11_BattleArchitecture.md *(future)*
- 12_DialogueArchitecture.md *(future)*