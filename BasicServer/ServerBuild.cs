using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BasicServer;

public class ServerBuild
{

    public ServerBuild(int? port,Router router)
    {
        _router = router;
        if (port != null)
        {
            _port = (int)port;
        }

        _listener.Prefixes.Add($"http://localhost:{_port}/");
    }

    private readonly HttpListener _listener = new HttpListener();
    private readonly int _port = 8080;
    private readonly ParameterBinder _binder = new();
    private readonly Router _router;

    public async Task StartServer()
    {
            _listener.Start();

        Console.WriteLine($"Listening on {_port}");


        while (true)
        {
            var context = await _listener.GetContextAsync();

            var requestRouter = await GetRequest(context);

            requestRouter.readRequest();
            var route = _router.Resolve(requestRouter);


            if (route == null)
            {
                await SendResponse(context, new HttpResult
                {
                    Body = "Not Found",
                    StatusCode = 404
                });

                continue;
            }

            route.Print();

            var controller = Activator.CreateInstance(route.ControllerType);

            var args = _binder.Bind(route.Method, requestRouter);

            if (!args.Success)
            {
                await SendResponse(context, new HttpResult
                {
                    Body = "Unprocessable Entity",
                    StatusCode = 422
                });

                continue;
            }

            var result = route.Method.Invoke(controller, args.Arguments);

            if (result == null)
            {
                await SendResponse(context, new HttpResult
                {
                    Body = "No Content",
                    StatusCode = 204
                });

                continue;
            }

            if (result is HttpResult httpResult)
            {
                await SendResponse(context, httpResult);
            }
            else if (result is string text)
            {
                await SendResponse(context, new HttpResult
                {
                    Body = text,
                    StatusCode = 200
                });
            }
            else
            {
                await SendResponse(context, new HttpResult
                {
                    Body = "Unsupported return type",
                    StatusCode = 500
                });
            }
        }
    }

    private async Task<Request> GetRequest(HttpListenerContext context)
    {
        var path = context.Request.Url!.AbsolutePath;

        var request = context.Request;

        using var reader = new StreamReader(
            request.InputStream,
            request.ContentEncoding
        );

        string body = await reader.ReadToEndAsync();

        var query = context.Request.QueryString
            .AllKeys
            .Where(x => x != null)
            .ToDictionary(
                x => x!,
                x => context.Request.QueryString[x]!,
                StringComparer.OrdinalIgnoreCase
            );

        return new Request
        {
            Body = body,
            Method = request.HttpMethod,
            Path = path,
            Query = query
        };
    }

    private async Task SendResponse(
        HttpListenerContext context,
        HttpResult result
        )
    {

        context.Response.StatusCode = result.StatusCode;

        var buffer = Encoding.UTF8.GetBytes(result.Body);

        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer);
        context.Response.Close();
    }

}
