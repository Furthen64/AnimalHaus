# AnimalHaus Architecture

AnimalHaus is organized as three independent .NET 8 processes:

- `AnimalHaus.Pigpen`
- `AnimalHaus.Barn`
- `AnimalHaus.Tractor`

Each system owns its own modules and configuration while sharing common primitives through:

- `AnimalHaus.Shared.Core` for result/config/tick abstractions
- `AnimalHaus.Shared.Utils` for deterministic randomness, retries, JSON helpers, and structured logging
- `AnimalHaus.Shared.Messaging` for ZeroMQ envelopes, topics, publish/subscribe, and request/reply wrappers
- `AnimalHaus.Contracts` for immutable command and event DTOs

## Topology

- Every system binds a PUB socket for domain events.
- Every system binds a REP socket for commands.
- Systems subscribe to peer PUB endpoints and issue commands to peer REP endpoints.
- Topic names use the convention `<system>.events.<EventName>` and `<system>.commands.<CommandName>`.

## Deterministic scenario

The default scenario models a feed request flowing through the farm:

1. Pigpen detects missing feed and issues `RequestDispatch` to Barn.
2. Barn decrements inventory, publishes `InventoryChanged` and `DispatchCompleted`, and commands Tractor with `AssignTask`.
3. Tractor publishes `TractorDispatched`, completes the hauling task on the next tick, and publishes `TaskCompleted`.
4. Pigpen consumes the delivered feed, publishes `PigFed`, updates health, and eventually publishes `PigReadyForTransfer`.

All systems support configuration through `appsettings.json` plus environment-variable overrides for ports, tick rate, max ticks, startup delay, and seed.
