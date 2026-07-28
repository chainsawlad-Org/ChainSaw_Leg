using System;
using System.Linq;
using Zenject;

public static class AutoBinder
{
    public static void BindDerivedTypes<TBase>(DiContainer container, bool asSingle = true)
    {
        Type baseType = typeof(TBase);

        var types = baseType
            .Assembly
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

    public static void BindAssignableTo<TInterface>(DiContainer container, bool asSingle = true)
    {
        Type interfaceType = typeof(TInterface);
        var types = interfaceType.Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && interfaceType.IsAssignableFrom(type));

        foreach (Type type in types)
        {
            var binder = container.BindInterfacesAndSelfTo(type);

            if (asSingle)
                binder.AsSingle();
            else
                binder.AsTransient();
        }
    }
}
