using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace BasicServer;

public class Request
{
    public string Method { get; init; } = "";
    public string Path { get; set; } = "";
    public string Body { get; init; } = "";

    public Dictionary<string, string> Query { get; init; } = new();

    public void readRequest()
    {
        Console.WriteLine($"Method: {Method}");
        Console.WriteLine($"Path: {Path}");
        Console.WriteLine($"Body: {Body}");
        Console.WriteLine($"Query: ");
        foreach (var item in Query)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
    }
}

public class RouteDescriptor
{
    public string HttpMethod { get; init; } = "";
    public string Path { get; init; } = "";

    public Type ControllerType { get; init; } = null!;

    public MethodInfo Method { get; init; } = null!;

    public void Print()
    {
        Console.WriteLine("=== Route Descriptor ===");
        Console.WriteLine($"HTTP Method:     {HttpMethod}");
        Console.WriteLine($"Path:            {Path}");
        Console.WriteLine($"Controller:      {ControllerType.FullName}");
        Console.WriteLine($"Method:          {Method.Name}");
        Console.WriteLine("========================");
    }
}

public class Router
{
    public Router(Assembly applicationAssembly)
    {
        var controllers = applicationAssembly
            .GetTypes()
            .Where(x =>
                x.IsSubclassOf(typeof(BaseController)) &&
                !x.IsAbstract);

        foreach (var controllerType in controllers)
        {
            RegisterController(controllerType);
        }
    }

    private readonly List<RouteDescriptor> _routes = new();

    private void RegisterController(Type controllerType)
    {
        var controllerRoute =
            controllerType.GetCustomAttribute<RouteAttribute>();

        if (controllerRoute == null)
            throw new NullReferenceException();

        var methods = controllerType.GetMethods(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly
        );

        foreach (var method in methods)
        {
            RegisterMethod(controllerType, controllerRoute, method);
        }
    }

    private void RegisterMethod(
        Type controllerType,
        RouteAttribute controllerRoute,
        MethodInfo method)
    {
        var httpAttribute =
            method.GetCustomAttributes<HttpMethodAttribute>()
                .FirstOrDefault();

        if (httpAttribute == null)
            return;

        string httpMethod = httpAttribute switch
        {
            HttpGetAttribute => "GET",
            HttpPostAttribute => "POST",
            _ => throw new NotSupportedException()
        };

        string fullPath = BuildPath(controllerType.Name,
            controllerRoute.Path,
            httpAttribute.Path
        );

        Console.WriteLine(fullPath);

        _routes.Add(new RouteDescriptor
        {
            HttpMethod = httpMethod,
            Path = fullPath,
            ControllerType = controllerType,
            Method = method
        });
    }

    private static string BuildPath(
        string controllerName,
        string controllerRoute,
        string methodRoute)
    {
        if (!controllerRoute.Contains("[controller]") ||
            !controllerRoute.Contains("[action]"))
        {
            throw new FormatException();
        }

        if (controllerRoute[1] != '/')
        {
            controllerRoute = $"/{controllerRoute}";
        }

        controllerName = controllerName.Replace("Controller", "").ToLower();

        return controllerRoute.Replace("[controller]", controllerName)
            .Replace("[action]", methodRoute);
    }


    public RouteDescriptor? Resolve(Request request)
    {
        return _routes.FirstOrDefault(x =>
            x.HttpMethod.Equals(
                request.Method,
                StringComparison.OrdinalIgnoreCase)
            &&
            x.Path.Equals(
                request.Path,
                StringComparison.OrdinalIgnoreCase));
    }
}
