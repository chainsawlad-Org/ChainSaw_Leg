# Dialogue System Evolution Roadmap

> Version: 1.0
> Last Updated: 15-07-2026

---

# Purpose

This document describes the planned evolution of the dialogue storage architecture.

It does not define the implementation.

Instead, it outlines the direction in which the system is expected to evolve.

---

# Stage 1 — Linear Dialogue

The minimum viable implementation.

Supports:

- linear dialogue lines;
- sequential execution;
- transition to the next node.

Example data:

```text
ID
Speaker
Text
NextID
```

---

# Stage 2 — Dialogue Choices

Introduces player dialogue choices.

Supports:

- multiple response options;
- transitions to different nodes.

Example:

```text
Choices

•

Text

TargetID
```

---

# Stage 3 — Dialogue Conditions

Introduces conditions for displaying dialogue nodes and response options.

Example data:

```text
QuestCompleted

Flag

Variable

HasItem

Relationship
```

### Gameplay Examples

**QuestCompleted**

The player has completed the quest *"Save the Blacksmith"*.

Afterward, the NPC thanks the player and unlocks a new story branch.

---

**Flag**

During a previous conversation, the player lied to the NPC.

A flag is set:

```text
PlayerLied = true
```

During the next encounter, the NPC remembers the lie and changes their attitude toward the player.

---

**Variable**

The player has earned enough reputation.

If `Reputation >= 50`, new dialogue options become available.

---

**HasItem**

The player possesses an ancient amulet.

The NPC notices it and starts a completely different conversation.

Without the amulet, this dialogue never appears.

---

**Relationship**

The player's relationship with the character has reached a high level.

Instead of formal conversation, more personal dialogue lines become available.

---

# Stage 4 — Dialogue Actions

Gameplay actions can be executed after a dialogue node is completed.

Example data:

```text
SetFlag

GiveItem

RemoveItem

StartQuest

FinishQuest

AddMoney

RemoveMoney
```

### Gameplay Examples

**SetFlag**

After confessing to a crime, the following flag is set:

```text
ConfessedToGuard = true
```

This may affect future dialogues.

---

**GiveItem**

After the conversation, the merchant gives the player an old key.

The key immediately appears in the player's inventory.

---

**RemoveItem**

The player hands medicine to the doctor.

The item is removed from the inventory.

---

**StartQuest**

During the conversation, the captain asks the player to find a missing scout.

A new quest starts automatically after the dialogue ends.

---

**FinishQuest**

The player reports that the task has been completed.

The quest finishes, rewards are granted, and new events become available.

---

**AddMoney**

The player receives a reward for completing a task.

Their balance increases.

---

**RemoveMoney**

The player pays for a night's stay at an inn.

The cost is automatically deducted.

---

# Stage 5 — Skill Checks

Introduces character attribute and skill checks.

Example data:

```text
Skill

Difficulty

Success

Failure
```

### Gameplay Examples

**Persuasion**

The player attempts to convince a guard to let them pass.

On success, the passage is opened.

On failure, the player must find another way.

---

**Intelligence**

The player examines an ancient inscription.

High Intelligence allows the text to be translated correctly.

On failure, its meaning remains unknown.

---

**Strength**

The player must lift a heavy stone slab.

A strong character succeeds without assistance.

Others must search for an alternative route.

---

**Luck**

The player searches for a hidden stash among a pile of junk.

A successful check reveals a rare item.

---

# Stage 6 — Variables

Introduces full support for gameplay variables.

Example data:

```text
bool

int

float

string
```

Supports:

- condition checks;
- value modification;
- comparisons.

### Gameplay Examples

**bool**

```text
HasSeenSecretRoom
```

After visiting the secret room for the first time, subsequent dialogue changes.

---

**int**

```text
Reputation = 82
```

The higher the player's reputation, the more NPCs trust them.

---

**float**

```text
Sanity = 34.5
```

A low Sanity value unlocks unusual dialogue lines and internal monologues.

---

**string**

```text
Guild = "Alchemists"
```

Characters react differently depending on the guild the player belongs to.

---

# Stage 7 — Advanced Presentation

Dialogue nodes begin controlling the visual and audio presentation of a scene.

Example data:

```text
Portrait

Emotion

Animation

Camera

Music

SFX

Voice

Delay
```

### Gameplay Examples

**Portrait**

During the conversation, the character portrait switches to a close-up.

This emphasizes the importance of the dialogue line.

---

**Emotion**

After receiving bad news, the NPC's expression changes from a smile to sadness.

---

**Animation**

During the dialogue, the character draws a weapon or turns away from the player.

---

**Camera**

At the moment of an important confession, the camera slowly zooms in on the character's face.

This enhances the emotional impact of the scene.

---

**Music**

A tense music track begins playing during the conversation.

The player realizes that the situation is becoming dangerous.

---

**SFX**

When a door opens, the sound of a heavy mechanism is played.

---

**Voice**

A pre-recorded voice line is played for an important piece of dialogue.

---

**Delay**

After unexpected news, a two-second pause occurs.

Only then does the next dialogue line appear, creating a stronger dramatic effect.

---

# Stage 8 — Advanced Narrative

Introduces advanced narrative mechanics.

For example:

- internal thoughts;
- hidden dialogue;
- passive checks;
- companion comments;
- automatic narrative inserts.

---

# Stage 9 — Dialogue Editor

If Excel is no longer sufficient,

the project can transition to a dedicated dialogue editor.

Possible options:

- Unity Editor;
- Graph Editor;
- a custom tool.

The runtime remains unchanged.

---

# Stage 10 — Mature Dialogue System

The final architecture represents dialogues as a graph of interconnected nodes.

Each node may contain:

```text
ID

Speaker

Text

Choices

Conditions

Actions

Variables

SkillChecks

Events

Camera

Animation

Audio

Portrait

Tags

Localization

Metadata
```

---

# Evolution Principle

The architecture evolves not by replacing the storage system,

but by gradually expanding the capabilities of each dialogue node.

```text
Node

↓

Node + Choices

↓

Node + Conditions

↓

Node + Actions

↓

Node + Skill Checks

↓

Node + Presentation

↓

Node + Narrative Systems
```

This approach preserves compatibility between early and later versions of the project.