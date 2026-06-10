## Code Conventions

- Do not use `var` keyword. Whenever possible, use the actual type.
- When using Linq, prefer using the name `x` for the parameter representing the item.
- When instantiating objects, prefer using the `new()` approach.
- When using the object initializer syntax, if there are more than one property to be initialized, write each property initialization on a different line.
- Do not use the curly brackets for `if`, `for` and `using` statements when the body is a single line of code.
- Do not use underscores for C# fields.

## Code Documentation

- Do not create xml documentation for the types that are used only inside the current solution.
- Only create xml documentation for public types that are exposed as a NuGet package.

## Unit Tests

- When using `Assert.Throws` method, always use a block body for the lambda expression.

- For each public method that is tested (including the constructor), create a different test file.
  - Ex: For a method called `Query()` create test file `QueryTests`
- All the test files for a single class should be placed in a directory with the name of the class.
- Use the naming pattern `Having<...>_When<...>_Then<...>` for the tests. Where:
  - `Having` describes the most important setup details.
  - `When` describes the action tested.
  - `Then` describes the expected result.

## WPF

- Avoid writing code in the code behind class of a window or user control. Instead, create attached behaviors.

## Architecture

### General structure

Follow a layered architecture with clear boundaries between layers:

- **Domain** — pure domain model objects (entities, value objects). No dependencies on frameworks, infrastructure, or UI.
- **Application** — use cases that orchestrate domain objects and port interfaces. No UI or infrastructure concerns here.
- **Ports** — interfaces that define contracts for external resources (files, APIs, databases, etc.), each paired with their concrete implementations in the same project. The application layer depends on the interfaces, not the implementations.
- **Presentation / UI** — ViewModels, Views, and Controls. Depends on the application layer only; never directly accesses port implementations.

### Use cases

- Each use case is a single class with a single public method (`Execute` or equivalent) that performs one clearly-scoped operation.
- A use case receives its dependencies (port interfaces, shared state, event bus) via constructor injection.
- Every constructor parameter must be validated against `null` and stored in a `readonly` field.
- A use case must not depend on another use case. If shared logic is needed, extract it into a domain service or a helper class.
- Each use case lives in its own subdirectory with the name of the use case. The subdirectory contains the use case class, and the request/response objects. It may also contains additional classes needed only be the use case: any events it produces, additional DTOs used in the request/response, etc.
- A use case may publish events to notify the rest of the application of state changes.
  - Ex: publish a "starting" event before the work begins and a "completed" event in a `finally` block so listeners are always notified, even on failure.

- A use case must not reference UI types or ViewModels.

### Ports (interfaces + implementations)

- For every external resource or service (file system, REST API, database, configuration, etc.) define a dedicated port: a narrow interface that expresses only what the application needs, and a concrete implementation that encapsulates all framework/library details.
- Naming conventions: The project containing the port must have the name `{ProjectName}.Ports.{PortName}Access`. (e.g., `DemoProject.Ports.LocalAccess`, `DemoProject.Ports.GitHubAccess`)
- For simplicity the implementation of the port may live in the same port project, but the application layer should references only the interface.
  - If multiple implementations of the same port are created, prefer to extract them in separate projects called adapters and leave the interface alone in the port project.

- Port interfaces may use domain types in their signatures, never framework-specific types. Adapters (the implementations) are responsible for translating between domain types and framework types.
- Register the concrete implementation against the interface in the DI container of the host project; application and domain code never instantiate port implementations directly.

### Adapters

- Naming conventions: Similar to Ports, the project containing the adapter must have the name `{ProjectName}.Adapters.{PortName}Access`. (e.g., `DemoProject.Adapters.LocalAccess`, `DemoProject.Adapters.GitHubAccess`)

### Application state

- Shared mutable state that multiple use cases need to read or write is held in a dedicated singleton state object (e.g., `AppState`).
- The state object is part of the application layer and contains only domain types.
- Use cases read from and write to the state object; ViewModels must not access or mutate it directly.

### Event bus / pub-sub

- Use cases communicate results to the rest of the application by publishing typed events through an `IEventBus` abstraction.
- ViewModels subscribe to events in their DI constructor and unsubscribe in `Dispose()`.
- Events that are specific to a single use case live in the same subdirectory as that use case. Events shared by multiple use cases live in a dedicated `Events/` directory in the application layer.
- Event classes are plain data-carrier objects with no behavior.

### UI / ViewModels

- Prefer splitting the UI into smaller, focused user controls, each backed by its own ViewModel.
- UI components must not communicate with each other directly. Any state change flows through the application layer (use cases), which publishes events that interested ViewModels react to. This keeps UI components independent from one another.
- AvaloniaUI
  - ViewModels implement `IDisposable` and clean up all event-bus subscriptions and reactive pipelines in `Dispose()`.
  - ViewModels use a two-constructor pattern: a parameterless constructor for the designer (sets up pipelines and commands with no external dependencies) and a DI constructor that chains to it (wires use cases and event-bus subscriptions).


