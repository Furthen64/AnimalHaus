using AnimalHaus.Tractor;

namespace AnimalHaus.Tractor.Modules;

public sealed class RoutePlanningModule
{
    private readonly Dictionary<string, string> _routes;
    private readonly string _defaultRouteTemplate;

    public RoutePlanningModule(TractorOptions options)
    {
        _routes = options.Routes;
        _defaultRouteTemplate = options.DefaultRouteTemplate;
    }

    public string Plan(string destinationSystem)
    {
        if (_routes.TryGetValue(destinationSystem, out var route))
        {
            return route;
        }

        return string.Format(_defaultRouteTemplate, destinationSystem.ToLowerInvariant());
    }
}
