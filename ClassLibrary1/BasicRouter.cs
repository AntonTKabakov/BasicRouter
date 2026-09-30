using System.Net;
using System.Numerics;
using System.Text;

namespace BasicRouter
{
    public class ServerBuild
    {

        public ServerBuild(int? port) 
        {
            if (port != null)
            {
                _port = (int)port;
            }

            _listener.Prefixes.Add($"http://localhost:{_port}/");
        }

        private readonly HttpListener _listener = new HttpListener();
        private readonly int _port = 8080;

        public async Task StartServer()
        {
            _listener.Start();

            Console.WriteLine($"Listening on {_port}");

            while (true)
            {
                var context = await _listener.GetContextAsync();

                var path = context.Request.Url!.AbsolutePath;

                Console.WriteLine(path);

                string response = path switch
                {
                    "/" => "Home Page",
                    "/about" => "About Page",
                    _ => "404"
                };

                var buffer = Encoding.UTF8.GetBytes(response);

                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer);
                context.Response.Close();
            }
        }
        
    }
}
