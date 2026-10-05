using System;
using System.Collections.Generic;
using System.Text;

namespace BasicServer;

[AttributeUsage(AttributeTargets.Class)]
public class RouteAttribute : Attribute
{
    public string Path { get; set; }

    public RouteAttribute(string path)
    {
        Path = path;
    }
}


[AttributeUsage(AttributeTargets.Method)]
public abstract class HttpMethodAttribute : Attribute
{
    public string Path { get; }

    protected HttpMethodAttribute(string path)
    {
        Path = path;
    }
}

public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string path = "") : base(path)
    {
    }
}

public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string path = "") : base(path)
    {
    }
}

public class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute(string path = "") : base(path)
    {
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class FromQueryAttribute : Attribute
{
}