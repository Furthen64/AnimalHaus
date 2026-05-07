# AnimalHaus

AnimalHaus is a distributed farm simulator example built on .NET 8 and ZeroMQ.

## Projects

- `src/systems/AnimalHaus.Pigpen`
- `src/systems/AnimalHaus.Barn`
- `src/systems/AnimalHaus.Tractor`
- `src/shared/AnimalHaus.Shared.Core`
- `src/shared/AnimalHaus.Shared.Utils`
- `src/shared/AnimalHaus.Shared.Messaging`
- `src/contracts/AnimalHaus.Contracts`
- `tests/AnimalHaus.Shared.Tests`
- `tests/AnimalHaus.Integration.Tests`

## Run

```bash
dotnet build AnimalHaus.sln
dotnet test AnimalHaus.sln
```

Then run each system in a separate terminal:

```bash
dotnet run --project src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj
dotnet run --project src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj
dotnet run --project src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj
```

See `docs/architecture.md` and `docs/message-contracts.md` for the system topology and message conventions.
