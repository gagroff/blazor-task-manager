# Project: Task Manager

## Tech Stack
- .NET 9, Blazor SSR (no WebAssembly, no SignalR)
- Entity Framework Core with SQLite
- xUnit + bUnit for testing

## Architecture
- Pages in Components/Pages/
- Shared components in Components/Layout/
- Services in Services/
- Models in Models/
- Data access in Data/

## Conventions
- Use file-scoped namespaces
- Use primary constructors where possible
- All public methods must have XML doc comments
- Use SSR form handling with EditForm
