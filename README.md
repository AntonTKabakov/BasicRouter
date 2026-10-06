# BasicServer

A small, attribute-routed HTTP server for .NET, built on top of `HttpListener`. Write controllers, decorate them with `[Route]` / `[HttpGet]` / `[HttpPost]`, and BasicServer discovers them via reflection, routes incoming requests, binds parameters from the query string or JSON body, and writes the response.

It is a learning project that mimics the feel of ASP.NET controllers without using ASP.NET.

## Features

- Controller discovery by reflection (any non-abstract class inheriting `BaseController`)
- Attribute routing with `[controller]` and `[action]` placeholders
- `GET` and `POST` handlers
- Parameter binding with `[FromQuery]` and `[FromBody]` (JSON, case-insensitive property names)
- Simple return types: `string` or `HttpResult` (via the `Results` helpers)
- Built-in `404`, `422`, `204` and `500` responses

## Project structure

```
BasicServer.slnx
├── BasicServer/            # The server library
│   ├── Attributes.cs       # [Route], [HttpGet], [HttpPost], [FromQuery], [FromBody], ...
│   ├── BaseController.cs   # Base class every controller inherits from
│   ├── HttpCodes.cs        # HttpResult + Results.Ok / NotFound / BadRequest
│   ├── ParameterBinder.cs  # Binds query string / body to method parameters
│   ├── Router.cs           # Request, RouteDescriptor, route registration and matching
│   └── ServerBuild.cs      # HttpListener loop and response writing
└── Test/                   # Example app that references BasicServer
    ├── Program.cs
    └── TestController.cs
```

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Getting started

```bash
git clone https://github.com/AntonTKabakov/BasicRouter
cd BasicRouter
dotnet run --project Test
```

The example app starts on `http://localhost:9090/`.

## Usage

### 1. Create a controller

```csharp
using BasicServer;

namespace MyApp
{
    public class IdRequest
    {
        public int Id { get; set; }
    }

    [Route("/api/[controller]/[action]")]
    public class UsersController : BaseController
    {
        [HttpGet]
        public string GetUsers()
        {
            return "Users";
        }

        [HttpGet("profile")]
        public HttpResult Profile([FromQuery] IdRequest request)
        {
            return Results.Ok("Profile" + request.Id);
        }

        [HttpPost]
        public HttpResult Create([FromBody] IdRequest request)
        {
            return Results.Ok("Created user " + request.Id);
        }
    }
}
```

### 2. Start the server

```csharp
using BasicServer;

Router router = new Router(typeof(Program).Assembly);
var server = new ServerBuild(9090, router);

await server.StartServer();
```

`Router` scans the assembly you pass in. `ServerBuild` takes a port (pass `null` to use the default, `8080`).

### 3. Call it

```bash
curl http://localhost:9090/api/users
curl "http://localhost:9090/api/users/profile?id=5"
curl -X POST http://localhost:9090/api/users \
     -H "Content-Type: application/json" \
     -d '{"id": 7}'
```

## Routing

A controller's `[Route]` template **must** contain both `[controller]` and `[action]`:

| Placeholder    | Replaced with                                                  |
| -------------- | -------------------------------------------------------------- |
| `[controller]` | Class name without `Controller`, lowercased (`UsersController` → `users`) |
| `[action]`     | The path passed to `[HttpGet]` / `[HttpPost]` (empty by default) |

Trailing slashes are trimmed, and path matching is case-insensitive. With the controller above:

| Method | Route                  | Handler      |
| ------ | ---------------------- | ------------ |
| GET    | `/api/users`           | `GetUsers()` |
| GET    | `/api/users/profile`   | `Profile()`  |
| POST   | `/api/users`           | `Create()`   |

## Parameter binding

| Attribute      | Source       | Supported targets |
| -------------- | ------------ | ----------------- |
| `[FromQuery]`  | Query string | `string`, `int`, `long`, `bool`, `double`, `Guid`, enums (matched by **parameter name**); `Dictionary<string, string>` (whole query); or a class with a public parameterless constructor (matched by **property name**) |
| `[FromBody]`   | Request body | Any type that `System.Text.Json` can deserialize; property names are case-insensitive |

Parameters without an attribute receive their default value. If a required value is missing or can't be converted, the request fails with `422`.

## Return types and status codes

| Handler returns          | Response                      |
| ------------------------ | ----------------------------- |
| `string`                 | `200` with the string as body |
| `HttpResult`             | The status code and body you set |
| `null`                   | `204`                         |
| anything else            | `500 Unsupported return type` |

The server also responds on its own with:

- `404` when no route matches the method and path
- `422` when parameter binding fails

Helpers in `Results`: `Ok(body)`, `NotFound(body)`, `BadRequest(body)`.

## Current limitations

- Only `GET` and `POST` are implemented (`PUT`, `DELETE`, `PATCH` attributes exist but registering one throws `NotSupportedException`)
- Every controller needs a `[Route]` attribute, otherwise startup fails
- Handlers are synchronous; `Task` / `async` return types aren't supported yet
- Requests are handled one at a time (sequential accept loop)
- Responses are plain text only, no automatic JSON serialization of return values
- Listens on `localhost` only
- No route parameters like `/users/{id}` (use query string or body instead)

## Ideas for next steps

- Add `PUT`, `DELETE` and `PATCH` support in `Router.RegisterMethod`
- Support `async` handlers and concurrent request handling
- Serialize returned objects to JSON
- Add path parameters
- Add automated tests

## License

Add a license of your choice here.