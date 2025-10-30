using System.Reflection;

namespace AdminApi.Entities;

public static class PropertyMapper
{
    public static void CopyMatchingProperties<TSource, TDest>(TSource source, TDest dest)
    {
        var srcProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destProps = typeof(TDest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var destDict = destProps.ToDictionary(p => p.Name, p => p);

        foreach (var src in srcProps)
        {
            if (destDict.TryGetValue(src.Name, out var destProp))
            {
                if (destProp.CanWrite && destProp.PropertyType.IsAssignableFrom(src.PropertyType))
                    destProp.SetValue(dest, src.GetValue(source));
            }
        }
    }
}
