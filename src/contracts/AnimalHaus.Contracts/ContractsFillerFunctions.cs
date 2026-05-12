namespace AnimalHaus.Contracts;

public static class ContractsFillerFunctions
{
    public static string BuildContractTag(string entityName) => $"{NormalizeEntityName(entityName)}-{Version()}";

    public static string NormalizeEntityName(string entityName) => entityName.Trim().ToLowerInvariant();

    public static int Version() => 1;
}
