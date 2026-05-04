# ChainSaw_Leg

## Установка и запуск

### 1. Установите GitHub Desktop

Скачайте и установите GitHub Desktop с официального сайта:  
[https://desktop.github.com/](https://desktop.github.com/)

### 2. Склонируйте репозиторий

- Откройте GitHub Desktop
- Нажмите `File → Clone repository`
- Выберите вкладку `URL` и введите: https://github.com/chainsawlad-Org/ChainSaw_Leg.git
- В поле `Local path` укажите папку, куда сохранить проект
- Нажмите `Clone`

### 3. Откройте проект в Unity

- Запустите Unity Hub
- Нажмите `Open` → `Add project from disk`
- Выберите папку, в которую склонировали проект
- Убедитесь, что выбран **URP 2D** шаблон (настройки уже в проекте)
- Нажмите `Open`

### 4. Запустите игру

- В Unity откройте сцену:  
`Assets/_Project/Core/Bootstrap/Scenes/SampleScene.unity`
- Нажмите `Play`

## Структура проекта

```
Assets/
├── _Project/
│   ├── Core/
│   │   ├── Bootstrap/
│   │   │   └── Scenes/
│   │   ├── Infrastructure/
│   │   │   ├── Input/
│   │   │   └── Rendering/
│   │   ├── Common/
│   │   └── Utilities/
│   ├── Features/
│   │   ├── Combat/
│   │   ├── Dialogue/
│   │   ├── Exploration/
│   │   ├── Inventory/
│   │   ├── Minigames/
│   │   ├── Progression/
│   │   └── Runner/
│   ├── Content/
│   ├── UI/
│   ├── Audio/
│   └── Tests/
├── Packages/
└── ...
```

```
Features/
└── Dialogue/
    ├── Scripts/                      # Основные скрипты диалоговой системы
    │   ├── Core/                     # Ядро системы (не зависит от UI)
    │   │   ├── DialogueManager.cs    # Главный менеджер (фасад)
    │   │   ├── DialogueState.cs      # Состояния диалога (Idle, Active, Waiting)
    │   │   ├── DialogueType.cs       # Типы диалогов (Bubble, Cutscene, RPG)
    │   │   └── IDialogueEvent.cs     # Интерфейс событий
    │   │
    │   ├── Data/                     # Данные и конфиги
    │   │   ├── DialogueData.cs       # Данные диалога (SO)
    │   │   ├── DialogueNode.cs       # Узлы диалога
    │   │   └── NpcDialogue.cs        # Связка NPC -> DialogueData
    │   │
    │   ├── Events/                   # ⚡ Реализации событий
    │   │   ├── ShowTextEvent.cs      # Показать текст
    │   │   ├── ChoiceEvent.cs        # Выбор варианта
    │   │   └── AudioEvent.cs         # (доп. пример)
    │   │
    │   ├── Input/                    # Ввод для диалогов
    │   │   └── DialogueInputRouter.cs # Маршрутизация ввода
    │   │
    │   ├── Choices/                  # Система выбора
    │   │   └── DialogueChoice.cs     # Варианты ответов
    │   │
    │   └── Utilities/                # Вспомогательные классы
    │       ├── DialogueParser.cs     # Парсинг диалогов (опционально)
    │       └── DialogueLogger.cs     # Логирование (для дебага)
    │
    └── UI/                           # UI-представления
        ├── DialogueUI_Bubble.cs      # Пузырь над головой
        ├── DialogueUI_Cutscene.cs    # Кат-сценный стиль
        ├── DialogueUI_RPG.cs         # RPG-стиль (окно внизу)
        └── EventDispatchers/         # (опционально)
            └── DialogueEventDispatcher.cs
```

## Устранение проблем

### Ошибка "Missing Render Pipeline Asset"

1. Откройте `Edit → Project Settings → Graphics`
2. В поле `Scriptable Render Pipeline Settings` укажите:  
   `Assets/_Project/Core/Infrastructure/Rendering/UniversalRP.asset`

### Сцена не найдена

Стартовая сцена находится по пути:  
`Assets/_Project/Core/Bootstrap/Scenes/SampleScene.unity`

---

## Разработка

- **Фичи** добавляйте в `Assets/_Project/Features/`
- **UI** → `Assets/_Project/UI/`
- **Аудио** → `Assets/_Project/Audio/`
- **Тесты** → `Assets/_Project/Tests/`
