# Project Structure

> Version: 1.0  
> Last Updated: 12-07-2026

---

# Purpose

Данный документ описывает структуру проекта и назначение каждой директории.

Его цель — помочь разработчикам быстро понять, **где должен находиться новый код**, а также избежать хаотичного роста проекта.

Главное правило:

> **Каждый класс должен лежать там, где находится его ответственность, а не там, где его удобно положить.**

---

# Root Structure

Проект разделён на несколько крупных модулей.

```
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

Каждая директория имеет собственную область ответственности.

---

# Application

Application содержит код, управляющий жизненным циклом приложения.

Он не содержит игровой логики.

Типичная структура:

```
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

Отвечает за запуск приложения.

Содержит:

```
BootstrapStartup

BootstrapRunner
```

Bootstrap ничего не знает об игровых механиках.

Его задача — подготовить приложение к работе.

---

## Startup

Определяет стартовое состояние игры.

Например:

```
StartupResolver

StartupPhaseRegistry
```

Добавление новой стартовой сцены производится здесь.

Bootstrap при этом изменять не требуется.

---

## StateMachine

Содержит систему управления игровыми режимами.

Например:

```
GameStateMachine

GamePhase

SceneGamePhase

OverlayPhase
```

Никакой игровой логики здесь быть не должно.

StateMachine знает только о жизненном цикле фаз.

---

## Installers

Содержит все Installer'ы Zenject.

Например:

```
ProjectInstaller

PhaseInstaller

ServiceInstaller
```

Installer отвечает только за регистрацию зависимостей.

Любая бизнес-логика внутри Installer запрещена.

---

## Factories

Factory отвечает за создание объектов через Dependency Injection.

Например:

```
PhaseFactory
```

Factory не содержит логики игры.

---

## Signals *(будущее)*

Содержит события приложения.

Например:

```
PlayerDiedSignal

SceneLoadedSignal

DialogueStartedSignal
```

Signals используются для связи между независимыми системами.

---

# Infrastructure

Infrastructure является адаптером между проектом и Unity.

Любой код, работающий непосредственно с API Unity, должен находиться здесь.

Типичная структура:

```
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

Содержит управление сценами.

Например:

```
SceneLoader
```

Никакой другой код проекта не должен использовать Unity SceneManager напрямую.

---

## Audio

Будущая аудиосистема проекта.

Например:

```
AudioService

MusicPlayer

SoundPlayer
```

---

## Input

Система пользовательского ввода.

Например:

```
InputService

InputActions

InputMapper
```

---



# Game

Game содержит всю игровую логику проекта.

Любое игровое правило должно находиться здесь.

Типичная структура:

```
Game
│
├── Features
├── Common
├── Shared
└── Gameplay
```

---

# Features

Каждая игровая механика представляет отдельную Feature.

Пример:

```
Dialogue

Battle

Inventory

Quest

Exploration

Minigames
```

Feature должна быть максимально независимой.

---

## Внутренняя структура Feature

Рекомендуемая структура:

```
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

Если Feature небольшая, допускается упрощённая структура.

---

## Controllers

Содержат игровую логику Feature.

Контроллер управляет поведением системы.

Он не должен заниматься отображением.

---

## Models

Содержат игровые данные.

Model не зависит от Unity.

---

## Views

Отвечают только за отображение.

View ничего не знает о внутренней логике Feature.

---

## Services

Локальные сервисы Feature.

Например:

```
DialogueHistoryService
```

Эти сервисы используются только внутри конкретной Feature.

---

## Configs

Содержит ScriptableObject и другие настройки Feature.

---

## Installers

Installer Feature.

Регистрирует зависимости только этой Feature.

---

# Shared

Содержит общий код, используемый несколькими Feature.

Например:

```
Health

Damage

Stats

CommonInterfaces
```

Shared не должен превращаться в "свалку" общего кода.

Если код относится только к одной Feature — он должен оставаться внутри неё.

---

# UI

UI содержит исключительно отображение.

Типичная структура:

```
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

Полноэкранные окна.

Например:

```
MainMenu

Settings

Inventory
```

---

## HUD

Постоянно отображаемый интерфейс.

Например:

```
HealthBar

MiniMap

QuestTracker
```

---

## Popups

Временные окна.

Например:

```
Confirmation

Warning

MessageBox
```

---

## Widgets

Переиспользуемые элементы интерфейса.

Например:

```
Button

Slider

InventorySlot

CharacterCard
```

---

# Configs

Содержит глобальные настройки проекта.

Например:

```
GameConfig

BalanceConfig

LocalizationConfig
```

Любая конфигурация должна находиться здесь или внутри соответствующей Feature.

---

# Content

Содержит игровые ресурсы.

Например:

```
Sprites

Audio

Animations

Fonts

Materials
```

Игровой код здесь отсутствует.

---

# Scenes

Содержит все игровые сцены.

Рекомендуемая структура:

```
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

Все игровые сцены должны иметь префикс:

```
SC_
```

---

# Plugins

Содержит сторонние плагины.

Например:

```
Zenject

DOTween

UniTask
```

Код проекта изменять внутри Plugins запрещено.

---


# Где создавать новый класс?

Перед созданием класса необходимо определить его ответственность.

| Если класс... | Размещается в... |
|----------------|------------------|
| Запускает приложение | Application |
| Работает с Unity API | Infrastructure |
| Содержит игровые правила | Game |
| Отображает информацию | UI |
| Хранит настройки | Configs |

Если возникает сомнение между двумя папками, вероятнее всего ответственность класса определена неправильно.

---

# Правило Feature First

Новый игровой функционал всегда создаётся как новая Feature.

Неправильно:

```
Controllers

Models

Views

Services
```

Правильно:

```
Features

    Fishing

    Craft

    Dialogue

    Inventory
```

Каждая Feature должна быть максимально автономной.

---

# Naming Convention

Используются следующие соглашения.

| Тип | Пример |
|------|---------|
| Сцена | SC_MainMenu |
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

Основные правила структуры проекта:

- Код организуется по ответственности, а не по типам файлов.
- Игровая логика находится только в слое Game.
- Работа с Unity API изолирована в Infrastructure.
- UI отвечает только за отображение.
- Новые игровые механики создаются как отдельные Feature.
- Каждая Feature максимально независима.
- Структура проекта должна упрощать поиск кода и масштабирование проекта.