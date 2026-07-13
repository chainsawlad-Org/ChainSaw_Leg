# Placement

Shared gameplay contracts are located under `Game/Shared` according to the structure in `Docs/Ru/02_ProjectStructure.md:240-255`:

> ├── Shared

`IInteractable` is shared by multiple gameplay features, so it belongs to this module instead of `Application` or a single feature. This note also covers the assembly definition and Unity metadata that cannot contain source comments.
