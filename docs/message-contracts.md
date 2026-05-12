# Message Contracts

Every message is wrapped in a shared envelope that includes:

- `messageId`
- `timestampUtc`
- `correlationId`
- `causationId`
- `schemaVersion`

## Commands

- `DeliverFeed`
- `TransferPig`
- `ReserveResource`
- `ReleaseResource`
- `RequestDispatch`
- `AssignTask`
- `RefuelTractor`
- `ScheduleMaintenance`

## Events

- `PigFed`
- `PigHealthChanged`
- `PigReadyForTransfer`
- `InventoryChanged`
- `ResourceLow`
- `DispatchCompleted`
- `TractorDispatched`
- `FuelLow`
- `TaskCompleted`
- `MarketPriceChanged`
