using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace CoreApi.Extensions
{
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
                var serviceType = interfaceArray.FirstOrDefault(d =>
                {
                    if (checkClassName)  //避免多接口继承引起的脏映射
                    {
                        var impName = implementationType.Name;
                        return d.IsAssignableFrom(implementationType) && d.Name.StartsWith($"I{impName}");
                    }

                    return d.IsAssignableFrom(implementationType);
                });

                if (serviceType != null)
                    services.AddScoped(serviceType, implementationType);
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
                var serviceType = interfaceArray.FirstOrDefault(d =>
                {
                    if (checkClassName)  //避免多接口继承引起的脏映射
                    {
                        var impName = implementationType.Name;
                        return d.IsAssignableFrom(implementationType) && d.Name.StartsWith($"I{impName}");
                    }

                    return d.IsAssignableFrom(implementationType);
                });

                if (serviceType != null)
                    services.AddScoped(serviceType, implementationType);
            }
        }
    }
}