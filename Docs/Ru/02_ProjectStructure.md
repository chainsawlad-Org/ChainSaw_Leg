# Структура проекта

> Версия: **2.0**  
> Последнее обновление: **27-07-2026**

---

# Назначение

Данный документ определяет физическую структуру проекта и ответственность каждой крупной директории.

Его цель — помочь каждому разработчику сразу понять, где должен располагаться новый код, и сохранить проект поддерживаемым по мере его роста.

Главное правило:

> **Код организуется по ответственности, а не по удобству размещения.**

---

# Корневая структура

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

Каждый модуль верхнего уровня имеет единственную область ответственности.

---

# Структура Assembly

Каждый крупный модуль содержит собственную Assembly Definition.

Например:

```text
ChainSawLeg.Application.Runtime

ChainSawLeg.Infrastructure.Runtime

ChainSawLeg.Game.Shared.Runtime

ChainSawLeg.UI.Runtime
```

Код редактора всегда должен находиться в отдельной Editor Assembly.

Runtime Assembly никогда не должны зависеть от Editor Assembly.

---

# Соглашение Runtime

Каждая Assembly придерживается одинаковой физической структуры.

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

Создаются только те директории, которые действительно необходимы.

---

# Application

Application содержит жизненный цикл приложения.

Он координирует различные системы.

Он никогда не содержит игровых правил.

Типичные области ответственности:

- Bootstrap
- Startup
- State Machine
- Coordinators
- Application Services
- Installers
- Factories

---

## Bootstrap

Отвечает за запуск приложения.

Содержит:

```text
BootstrapRunner

BootstrapStartup
```

Bootstrap ничего не знает об игровой логике.

Его единственная задача — подготовить приложение к работе.

---

## Startup

Определяет начальную фазу игры.

Содержит:

```text
StartupResolver

StartupPhaseRegistry
```

Добавление нового сценария запуска не должно требовать изменения BootstrapRunner.

---

## StateMachine

Содержит жизненный цикл игровых фаз.

Например:

```text
GameStateMachine

GamePhase

SceneGamePhase

OverlayPhase
```

StateMachine не содержит игровых правил.

---

## Coordination

Содержит application use cases, объединяющие несколько систем.

Например:

```text
MainMenuCoordinator

PauseMenuCoordinator

CheckpointSaveMenuCoordinator

ExplorationGameSaveLoadService
```

Coordinator занимается оркестрацией систем.

Он не реализует игровую логику.

---

## Services

Application Services — это переиспользуемые сервисы, используемые в нескольких сценариях приложения.

Текущая организация:

```text
Services
│
├── Commands
├── Brokers
├── Registries
└── Runtime Services
```

Примеры:

```text
MainMenuStartCommandService

DialogueService

DialogueRuntimeRegistry

MainMenuSaveBrowserRequestBroker
```

---

## Installers

Содержит регистрацию зависимостей Zenject.

Структура:

```text
Installers
│
├── Core
└── Features
```

Core Installers регистрируют глобальные системы.

Feature Installers регистрируют игровые механики.

Installer никогда не должен содержать бизнес-логику.

---

## FeatureAdapters

Содержит Unity-адаптеры, связывающие объекты сцены с системами Application.

Например:

```text
FeatureAdapters
│
├── Combat
├── Dialogue
└── Exploration
```

FeatureAdapters могут использовать Unity API.

Они не должны содержать игровые правила.

---

## Factories

Отвечают за создание runtime-объектов через Dependency Injection.

Например:

```text
PhaseFactory
```

Factory никогда не содержит бизнес-логику.

---

## Signals *(Будущее)*

Содержит события уровня приложения.

Например:

```text
DialogueStartedSignal

SceneLoadedSignal

PlayerDiedSignal
```

Signals обеспечивают слабую связанность между независимыми системами.

---

## Editor

Содержит инструменты, работающие только в редакторе.

Runtime-код никогда не должен зависеть от этой директории.

---

# Infrastructure

Infrastructure содержит технические интеграции.

Он отвечает за взаимодействие с Unity API, операционной системой и сторонними библиотеками.

Типичная структура:

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

Infrastructure не содержит игровых правил.

---

## SceneManagement

Отвечает за управление сценами.

Например:

```text
SceneLoader

SceneNames

UnityActiveSceneProvider
```

Только SceneLoader имеет право напрямую взаимодействовать с Unity SceneManager.

---

## Input

Содержит глобальную систему ввода.

Например:

```text
InputService

PlayerInputActions

GameplayInputBlockService
```

---

## SaveSystem

Содержит техническую часть системы сохранений.

Например:

```text
GameSaveCoordinator

GameSaveValidationService

GameSaveMigrationService

OdinGameSaveSerializer

FileGameSaveStorageProvider
```

Игровые участники системы сохранений относятся к Feature, а не к Infrastructure.

---

## Reflection

Содержит утилиты, использующие Reflection.

Например:

```text
AutoBinder
```

---

## Rendering

Содержит инфраструктуру, связанную с рендерингом.

Например:

```text
Renderer2D

URP Settings
```

---

## Audio *(Будущее)*

Содержит глобальную аудиосистему.

Например:

```text
AudioService

MusicPlayer

SoundPlayer
```

---

# Game

Game содержит все игровые правила.

Любая игровая механика относится именно сюда.

Структура:

```text
Game
│
├── Features
└── Shared
```

---

# Features

Каждая игровая механика реализуется как отдельная Feature.

Например:

```text
Dialogue

Combat

Exploration

Inventory

Quest

Minigames
```

Каждая Feature должна быть максимально независимой.

---

## Внутренняя структура Feature

Рекомендуемая структура:

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

Небольшие Feature могут не создавать лишние директории.

---

## Controllers

Содержат игровое поведение.

Controllers реализуют игровые правила.

---

## Models

Содержат игровые данные.

Models не должны зависеть от Unity.

---

## Views

Отображают игровую информацию.

Views генерируют события пользовательского интерфейса.

Views не принимают игровых решений.

---

## Services

Содержат локальные сервисы Feature.

Например:

```text
DialogueHistoryService
```

Эти сервисы используются только внутри соответствующей Feature.

---

## Runtime

Содержит runtime-объекты, которые не подходят ни под одну другую категорию.

Например:

```text
Runtime Registries

Runtime Context

Runtime Cache
```

---

## Configs

Содержит настройки Feature.

Обычно это ScriptableObject.

---

## Prefabs

Содержит Prefab'ы, используемые только данной Feature.

---

## Registration

Игровые Feature не содержат Zenject Installer.

Регистрация зависимостей выполняется внутри:

```text
Application
    Installers
        Features
```

---

# Shared

Содержит переиспользуемый игровой код, используемый несколькими Feature.

Например:

```text
Health

Damage

Stats

Interactable

SaveSystem
```

Shared никогда не должен превращаться в папку «разное».

---

# UI

UI содержит исключительно отображение.

UI показывает информацию и передает пользовательские события.

Типичная структура:

```text
UI
│
├── MainMenu
├── PauseMenu
├── CheckpointSave
├── Common
└── Services
```

В будущем могут быть добавлены:

```text
HUD

Widgets

Popups
```

---

# Configs

Содержит глобальные настройки проекта.

Например:

```text
GameConfig

LocalizationConfig

BalanceConfig
```

Настройки, относящиеся только к одной Feature, должны располагаться внутри соответствующей Feature.

---

# Content

Содержит игровые ресурсы.

Например:

```text
Sprites

Fonts

Animations

Audio

Materials
```

Игровой код здесь отсутствует.

---

# Scenes

Содержит Unity-сцены.

Рекомендуемая структура:

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

Все сцены должны использовать префикс:

```text
SC_
```

---

# Tests

Содержит тесты проекта.

Например:

```text
Editor

PlayMode
```

Тесты должны ссылаться только на Runtime Assembly.

---

# Assembly Definition Files

Каждый Runtime-модуль содержит ровно одну Assembly Definition.

Каждый Editor-модуль содержит ровно одну Editor Assembly.

Зависимости между Assembly всегда должны быть направлены вниз.

```text
Application
        ↓
Game
        ↓
Infrastructure
```

Циклические зависимости запрещены.

---

# PLACEMENT.md

Каждая крупная директория должна содержать файл `PLACEMENT.md`.

Его задача — объяснить:

- что должно находиться в этой папке;
- что не должно находиться в этой папке;
- типичные примеры;
- распространенные ошибки.

Разработчик должен понимать назначение директории, не открывая исходный код.

---

# Где создавать новый класс?

| Если класс... | Размещается в... |
|----------------|------------------|
| Запускает приложение | Application |
| Координирует несколько систем | Application/Coordination |
| Выполняет команду уровня приложения | Application/Services |
| Интегрирует Unity или сторонние библиотеки | Infrastructure |
| Реализует игровые правила | Game |
| Отображает пользовательский интерфейс | UI |
| Адаптирует Unity-объекты сцены | FeatureAdapters |
| Хранит настройки | Configs |

Если возникает сомнение между двумя папками, скорее всего ответственность класса определена неверно.

---

# Правило Feature First

Каждая новая игровая механика создается как отдельная Feature.

Неправильно:

```text
Controllers

Models

Views

Services
```

Правильно:

```text
Features

    Dialogue

    Combat

    Exploration

    Inventory

    Quest
```

Каждая Feature должна оставаться максимально независимой.

---

# Naming Convention

| Тип | Пример |
|------|---------|
| Сцена | SC_MainMenu |
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

Архитектура проекта основана на следующих принципах:

- Организовывать код по ответственности.
- Хранить игровые правила только в слое Game.
- Размещать интеграции с Unity и сторонними библиотеками только в Infrastructure.
- Использовать Application для координации систем, но не для реализации игровой логики.
- Делать Feature максимально независимыми.
- Сохранять UI пассивным.
- Каждый Runtime-модуль должен иметь собственную Assembly Definition.
- Каждая крупная директория должна содержать файл PLACEMENT.md.
- Структура проекта должна масштабироваться до сотен классов без потери читаемости и удобства навигации.
```