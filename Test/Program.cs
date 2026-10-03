using BasicServer;

Router router = new Router(typeof(Program).Assembly);

var server = new ServerBuild(9090,router);

await server.StartServer();