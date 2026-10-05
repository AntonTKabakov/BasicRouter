using BasicServer;

public class CreateUserRequest
{
    public int A { get; set; }
    public int B { get; set; }
}

namespace Test
{
    [Route("/app/[controller]/[action]")]
    public class UsersController : BaseController
    {
        [HttpGet("aa")]
        public string GetUsers()
        {
            return "Users";
        }

        [HttpGet("profile")]
        public HttpResult Profile([FromQuery] CreateUserRequest request)
        {
            return Results.Ok("Profile" + request.A + request.B);
        }

        [HttpPost]
        public HttpResult Create([FromBody] CreateUserRequest request)
        {
            return Results.Ok("Created user " + request.A);
        }
    }
}