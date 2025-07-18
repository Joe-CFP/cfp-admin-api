using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace AdminApi.Routing;

public class SlugifyParameterTransformer : IOutboundParameterTransformer
{
    public static void SlugifyControllerNames(MvcOptions options) => options.Conventions.Add(
        new RouteTokenTransformerConvention(new SlugifyParameterTransformer())
    );

    public string TransformOutbound(object? value)
    {
        return value?.ToString()?.ToLowerInvariant() ?? string.Empty;
    }
}