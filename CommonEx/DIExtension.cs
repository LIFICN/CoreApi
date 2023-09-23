using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace CommonEx;

public static class DIExtension
{
    public static void AddScoped(this IServiceCollection services, string interfaceName, string className, bool checkClassName = false)
    {
        if (string.IsNullOrWhiteSpace(interfaceName) || string.IsNullOrWhiteSpace(className))
            return;

        var interfaceArray = Assembly.Load(interfaceName).GetTypes().Where(d => d.IsInterface);
        var classArray = Assembly.Load(className).GetTypes().Where(d => d.IsClass);

        foreach (var implementationType in classArray)
        {
            var interfaceType = interfaceArray.FirstOrDefault(d => CheckAssignable(d, implementationType, checkClassName));

            if (interfaceType != null)
                services.AddScoped(interfaceType, implementationType);
        }
    }

    public static void AddScoped(this IServiceCollection services, string assemblyName, bool checkClassName = false)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            return;

        var types = Assembly.Load(assemblyName).GetTypes();
        var interfaceArray = types.Where(d => d.IsInterface);
        var classArray = types.Where(d => d.IsClass);

        foreach (var implementationType in classArray)
        {
            var interfaceType = interfaceArray.FirstOrDefault(d => CheckAssignable(d, implementationType, checkClassName));

            if (interfaceType != null)
                services.AddScoped(interfaceType, implementationType);
        }
    }

    public static bool CheckAssignable(Type interfaceType, Type classType, bool checkClassName = false)
    {
        if (!checkClassName) return interfaceType.IsAssignableFrom(classType);

        //避免多接口继承引起的脏映射
        var className = classType.Name;
        return interfaceType.IsAssignableFrom(classType) && interfaceType.Name.StartsWith($"I{className}");
    }
}
