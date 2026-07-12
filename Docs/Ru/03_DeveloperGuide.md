# Developer Guide

> Version: 1.0
> Last Updated: 12-07-2026

---

# Purpose

Данный документ описывает принятый процесс разработки внутри проекта.

Он отвечает на вопрос:

> **Как правильно добавить новую функциональность, не нарушая архитектуру проекта?**

Перед началом разработки рекомендуется ознакомиться с:

- `01_Architecture.md`
- `02_ProjectStructure.md`

---

# Development Workflow

Практически любая новая задача проходит следующие этапы.

```
Идея

↓

Определение ответственности

↓

Выбор слоя архитектуры

↓

Создание Feature / Service / UI

↓

Регистрация в DI

↓

Тестирование

↓

Code Review
```

Перед написанием кода необходимо определить:

- Что создаётся?
- Кто будет использовать систему?
- В каком слое она должна находиться?

---

# Adding a New Main Phase

Main Phase представляет собой полноценный игровой режим.

Примеры:

- Main Menu
- Exploration
- Battle
- Minigame

---

## Шаг 1

Создать новую игровую сцену.

Например:

```
SC_Snake
```

Все игровые сцены должны иметь префикс:

```
SC_
```

---

## Шаг 2

Добавить имя сцены в `SceneNames`.

Пример:

```csharp
public const string Snake = "SC_Snake";
```

---

## Шаг 3

Создать новую фазу.

```csharp
public class SnakePhase : SceneGamePhase
{
    protected override string SceneName => SceneNames.Snake;

    public SnakePhase(ISceneLoader loader)
        : base(loader)
    {
    }
}
```

---

## Шаг 4

Фаза будет автамотически зарегестрирована благодоря 

```csharp
public override void InstallBindings()
    {
        AutoBinder.BindDerivedTypes<GamePhase>(Container);
    }
```

---

## Шаг 5

Открывать новую сцену только через GameStateMachine.

Правильно:

```csharp
await gameStateMachine.ReplaceMain<SnakePhase>();
```

Неправильно:

```csharp
SceneManager.LoadScene(...);
```

---

# Adding an Overlay Phase

Overlay используется для временных игровых состояний.

Например:

- Dialogue
- Inventory
- Pause
- Settings

---

Создать новую фазу.

```csharp
public class PausePhase : OverlayPhase
{
}
```

Открытие:

```csharp
await gameStateMachine.PushOverlay<PausePhase>();
```

Закрытие:

```csharp
await gameStateMachine.PopOverlay();
```

Overlay никогда не загружает игровые сцены.

---

# Adding a New Feature

Каждая игровая механика создаётся как отдельная Feature.

Пример:

```
Fishing

Craft

Dialogue

Quest

Inventory
```

Рекомендуемая структура Feature:

```
Fishing
│
├── Controllers
├── Models
├── Views
├── Configs
├── Installers
└── Prefabs
```

Feature должна быть максимально независимой.

---

# Adding a Service

Service создаётся тогда, когда функциональность должна использоваться несколькими системами.

Примеры:

- AudioService
- SaveService
- InputService

Создать сервис.

```csharp
public class AudioService
{
}
```

Зарегистрировать в Installer.

```csharp
Container.Bind<AudioService>()
    .AsSingle();
```

Использовать через Dependency Injection.

---

# Adding UI

UI отвечает только за отображение.

UI может:

- показать данные;
- скрыть данные;
- отправить событие.

UI не должен изменять игровое состояние.

Например:

```
InventoryView

DialogueWindow

PauseWindow
```

---

# Adding Configs

Все настраиваемые параметры должны находиться в Config.

Например:

```
EnemyConfig

DialogueConfig

BalanceConfig
```

Предпочтительно использовать ScriptableObject.

---

# Adding Installers

Каждая независимая Feature должна иметь собственный Installer.

Например:

```
DialogueInstaller

BattleInstaller

QuestInstaller
```

Installer отвечает только за регистрацию зависимостей.

Любая игровая логика внутри Installer запрещена.

---

# Working with Scenes

Все игровые сцены загружаются только через SceneLoader.

Никогда не использовать SceneManager напрямую.

Правильно:

```
GameStateMachine

↓

SceneGamePhase

↓

SceneLoader
```

---

# Working with Dependency Injection

Все зависимости должны передаваться через конструктор.

Правильно:

```csharp
public BattleController(
    BattleService battleService,
    AudioService audioService)
{
}
```

Неправильно:

```csharp
var audio = new AudioService();
```

---

# Working with MonoBehaviour

MonoBehaviour должен содержать только код, связанный с Unity.

Например:

- ссылки на компоненты;
- обработку событий Unity;
- передачу управления другим системам.

Игровая логика должна находиться в обычных C# классах.

---

# Working with Features

Feature не должна обращаться напрямую к внутренним классам другой Feature.

Правильно:

```
Dialogue

↓

QuestService
```

Неправильно:

```
DialogueController

↓

QuestDatabase

↓

QuestInternalManager
```

Общение между системами должно происходить через публичные интерфейсы.

---

# Scene Transition Flow

Правильная последовательность перехода между игровыми режимами.

```
Player Action

↓

Controller

↓

GameStateMachine

↓

SceneGamePhase

↓

SceneLoader

↓

Unity SceneManager
```

Любое отклонение от этой схемы требует отдельного архитектурного обсуждения.

---

# Adding Save Support *(будет реализовано позже)*

После появления Save System каждая система, данные которой должны сохраняться, должна реализовать интерфейс сохранения.

Пример:

```
Player

Inventory

Quest

World

Settings
```

Все сохранения выполняются централизованно через SaveService.

Игровые системы не должны самостоятельно работать с файлами.

---

# Common Mistakes

## ❌ Создание сервисов через new

Всегда использовать Dependency Injection.

---

## ❌ Использование SceneManager

Всегда использовать SceneLoader.

---

## ❌ Большие MonoBehaviour

MonoBehaviour не должен содержать игровую логику.

---

## ❌ Feature зависит от Feature

Использовать сервисы, события или публичные интерфейсы.

---

## ❌ Дублирование логики

Если одинаковый код используется несколькими системами — его следует вынести в Shared или Infrastructure.

---

# Pull Request Checklist

Перед созданием Pull Request необходимо проверить:

- Архитектура не нарушена.
- Код соответствует Code Rules.
- Все зависимости зарегистрированы.
- Новые классы находятся в правильных директориях.
- Отсутствует дублирование логики.
- Не используется SceneManager напрямую.
- Не используется new для сервисов.
- Все публичные классы имеют понятные названия.
- Проект успешно собирается.
- Добавлена документация (если архитектура изменилась).

---

# Summary

При разработке новой функциональности необходимо придерживаться следующих правил:

- Каждая новая игровая механика создаётся как отдельная Feature.
- Игровые режимы реализуются через Main Phase или Overlay Phase.
- Все сцены загружаются только через SceneLoader.
- Все зависимости создаются через Dependency Injection.
- UI отвечает только за отображение.
- MonoBehaviour содержит только Unity-специфичный код.
- Любая новая архитектурная идея должна соответствовать принципам, описанным в `01_Architecture.md`.