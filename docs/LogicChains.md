# Logic Chains

## Setup

-   Read config.cfg file and parse it to settings.
-   Construct all `IComponent` inheriting classes _(/Game components)_.
-   Initialize all `IComponents`. _(Using their `Initialize()` method.)_
    -   _Environment => `Environment`_
    -   _UI => `UserInterface`_
    -   _Audio => `AudioEngine`_
    -   ...

> [!IMPORTANT]
> Audio files get loaded only when the `AudioEngine` gets loaded.
> This is unlike other textures as they get loaded with the `Environment` `Initialize()` function.

## Update

## Draw

## Exit
