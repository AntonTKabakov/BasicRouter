using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BasicServer;

public class Request
{
    public string Method { get; init; } = "";
    public string Path { get; set; } = "";
    public string Body { get; init; } = "";

    public void readRequest()
    {
        Console.WriteLine($"Method: {Method}");
        Console.WriteLine($"Path: {Path}");
        Console.WriteLine($"Body: {Body}");
    }
}

public class Router
{
    public Router(Assembly applicationAssembly)
    {
        _controllers = applicationAssembly
            .GetTypes()
            .Where(x => x.BaseType == typeof(BaseController) && !x.IsAbstract)
            .ToDictionary(
                x => x.Name.Replace("Controller", "").ToLower(),
                x => x

            );
    }

    private readonly Dictionary<string,Type> _controllers;

    public string GetController(Request request)
    {
        string trimmed = request.Path.TrimStart('/');
        int index = trimmed.IndexOf('/');

        string firstPart = index == -1
            ? trimmed
            : trimmed[..index];

        request.Path = index == -1
            ? "/"
            : trimmed[index..];

        var controller = _controllers
            .FirstOrDefault(x => x.Key == firstPart);


        return controller.Key;
    }
}
