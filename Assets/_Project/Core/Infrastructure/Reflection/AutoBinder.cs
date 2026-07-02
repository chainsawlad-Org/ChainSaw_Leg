
using System;
using Zenject;
using System.Linq;

using System.Reflection;

public static class AutoBinder
{
    public static void BindDerivedTypes<TBase>(DiContainer container, bool asSingle = true)
    {
        Type baseType = typeof(TBase);

        var types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && baseType.IsAssignableFrom(type));

        foreach (Type type in types)
        {
            var binder = container.Bind(type);

            if (asSingle)
                binder.AsSingle();
            else
                binder.AsTransient();
        }
    }
}
