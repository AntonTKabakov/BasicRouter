using System.Net;
using System.Numerics;
using System.Text;

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

    private readonly Router _router;

    public async Task StartServer()
    {
            _listener.Start();

        Console.WriteLine($"Listening on {_port}");


        while (true)
        {
            var context = await _listener.GetContextAsync();

            var path = context.Request.Url!.AbsolutePath;

            var request = context.Request;

            using var reader = new StreamReader(
                request.InputStream,
                request.ContentEncoding
            );

            string body = await reader.ReadToEndAsync();

            var requestRouter = new Request
            {
                Body = body,
                Method = request.HttpMethod,
                Path = path,
            };

            requestRouter.readRequest();

            var responseR = _router.GetController(requestRouter);
            string response = path switch
            {
                "/" => "Home Page",
                "/about" => "About Page",
                var p when p == $"/{responseR}" => responseR,
                _ => "404"
            };

            var buffer = Encoding.UTF8.GetBytes(response);

            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer);
            context.Response.Close();
        }
    }

}
