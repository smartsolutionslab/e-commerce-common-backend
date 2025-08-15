# E-Commerce Common Backend

Shared libraries and common components for the E-Commerce microservices platform.

## Features

- CQRS & Event Sourcing Framework
- Domain-Driven Design Primitives
- Multi-Tenancy Support
- Minimal API Base Classes
- Value Objects & Domain Events
- Authentication & Authorization Helpers

## Packages

- `E-Commerce.Common.Domain` - Domain primitives and value objects
- `E-Commerce.Common.Application` - CQRS application layer
- `E-Commerce.Common.Infrastructure` - Infrastructure concerns
- `E-Commerce.Common.Api` - Minimal API base classes

## Usage

```csharp
// Add to your service
services.AddCommonServices();
services.AddMultiTenancy();
services.AddCqrs();
```

## Build & Test

```bash
dotnet build
dotnet test
dotnet pack
```
