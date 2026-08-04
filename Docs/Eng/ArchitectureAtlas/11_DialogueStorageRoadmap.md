# Dialogue Storage Architecture

> Version: 2.0
> Last Updated: 18-07-2026

---

# Purpose

Define a unified dialogue storage architecture for the project.

The system is designed for a Story-Rich RPG / Visual Novel, where most dialogues are linear and non-linearity is implemented through localized choice points.

The architecture must support the gradual evolution of the project without requiring changes to the runtime system.

---

# Architecture Name

**Node-Based Dialogue Architecture**

Google Sheets → JSON → Dialogue Objects → Dialogue Database

A node-based dialogue pipeline designed for incremental evolution from linear narratives to complex RPG dialogue systems.

---

# General Concept

Dialogues are authored in Google Sheets.

Google Sheets serves as the project's single **Source of Truth**.

During the import process, the data is automatically exported to JSON.

After loading, the JSON is converted into an object model.

At runtime, the system works exclusively with objects rather than JSON.

```text
Google Sheets
        ↓
 Google Sheets → JSON
        ↓
     JSON Files
        ↓
 Dialogue Importer
        ↓
 Dialogue Objects
        ↓
 Dialogue Database
        ↓
 Dialogue Runtime
```

---

# Design Goals

The system should:

- be convenient for narrative designers and writers;
- support collaborative editing;
- handle large volumes of text;
- integrate well with Git;
- support linear dialogues;
- support localized branching;
- provide fast data access;
- allow future expansion without changing the overall architecture.

---

# Core Principle

A dialogue is represented as a collection of interconnected **Dialogue Nodes**.

Each node has a unique identifier (**ID**).

Connections between nodes are made exclusively through IDs.

Even a completely linear dialogue is treated as a sequence of connected nodes.

---

# Dialogue Structure

The basic dialogue structure is a linear sequence.

```text
1
↓

2
↓

3
↓

4
↓

5
```

Unless another transition is explicitly specified, execution continues to the next node.

---

# Branching Model

Branching is localized.

```text
          Choice

         /      \

       30        50

         \      /

          80
```

All transitions are performed using node IDs.

The dialogue is treated as a graph of interconnected nodes rather than a tree.

---

# Data Pipeline

The architecture is divided into several independent stages.

## Google Sheets

Used exclusively for creating and editing dialogue scripts.

Serves as the project's single source of truth.

---

## JSON

Acts as an intermediate data exchange format.

Used only during the import process.

JSON files should never be edited manually.

---

## Dialogue Importer

Responsible for loading JSON.

Converts serialized data into the game's object model.

Also performs data validation before creating runtime objects.

---

## Dialogue Objects

Represent the runtime object model of the dialogue system.

Each Dialogue Node exists as a fully instantiated object in memory.

The runtime operates exclusively on these objects.

---

## Dialogue Database

Serves as the central repository for all loaded dialogues.

Stores data in two representations:

- List — for storing the complete collection of dialogue objects and sequential iteration.
- Dictionary — for instant access to a node by its ID.

After loading is complete, all gameplay systems access dialogue data exclusively through the Dialogue Database.

---

# Source of Truth

All dialogue changes are made exclusively in Google Sheets.

JSON is an intermediate format.

Dialogue Objects are generated automatically.

The Dialogue Database is built automatically.

Manual modification of intermediate data is not allowed.

---

# Import Responsibilities

The importer must:

- load the JSON data;
- convert the data into objects;
- validate the data;
- build the Dialogue Database;
- report all detected errors.

---

# Validation

During the import process, the following checks are recommended:

- unique IDs;
- existence of all target IDs;
- absence of broken references;
- valid data structure;
- absence of corrupted entries.

If validation fails, the Dialogue Database should not be created.

---

# Runtime Principle

The Dialogue Runtime never works directly with JSON.

After loading, the JSON is fully converted into the runtime object model.

From that point onward, gameplay systems interact exclusively with Dialogue Objects through the Dialogue Database.

The pipeline therefore looks like this:

```text
Google Sheets

↓

JSON

↓

Dialogue Objects

↓

Dialogue Database

↓

Runtime
```

JSON is used solely as an intermediate storage and import format.

---

# Scalability

As the project grows, dialogue data may be split by:

- chapters;
- characters;
- locations;
- quests;
- DLC;
- episodes.

This does not affect the gameplay systems.

---

# Future Evolution

The architecture is designed to be extensible from the beginning.

As the project evolves, Dialogue Nodes may gain additional properties.

For example:

- Choices
- Conditions
- Variables
- Actions
- Skill Checks
- Events
- Camera
- Animation
- Portrait
- Voice
- Music
- Sound Effects
- Localization
- Metadata
- Custom Node Types

Extending the Dialogue Node structure should not require changes to the data storage architecture.

---

# Editor Evolution

Google Sheets is used during the early stages of development.

If necessary, the authoring tool can later be replaced.

```text
Google Sheets

↓

Google Sheets + Validation

↓

Google Sheets + Unity Tools

↓

Dedicated Dialogue Editor

↓

Graph Editor
```

The Dialogue Runtime, Dialogue Objects, and Dialogue Database remain unchanged.

Only the content creation tool is replaced.

---

# Long-Term Vision

Even after evolving into a complex RPG dialogue system with extensive use of:

- conditions;
- game variables;
- skill checks;
- internal monologues;
- complex branching;
- events;
- narrative mechanics,

the architecture remains the same.

A dialogue continues to be represented as a graph of interconnected Dialogue Nodes.

Only the amount of information contained within each node increases.

---

# Summary

Selected architecture:

**Node-Based Dialogue Architecture**

```text
Google Sheets

↓

Google Sheets → JSON

↓

JSON Files

↓

Dialogue Importer

↓

Dialogue Objects

↓

Dialogue Database

↓

Dialogue Runtime
```

---

## Main Advantages

- Google Sheets serves as the single source of truth.
- JSON is used only as an intermediate format.
- The runtime is completely independent of the storage format.
- Dialogue Objects provide the runtime data model.
- Dialogue Database enables fast access to all dialogue nodes.
- Simple workflow for narrative designers and writers.
- Efficient team collaboration.
- Excellent Git integration.
- Automatic data validation.
- High performance thanks to the object-based runtime model.
- Easy scalability.
- The system can evolve incrementally without changing the overall architecture.