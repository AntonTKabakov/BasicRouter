using BasicServer;

public class IdRequest
{
    public int Id { get; set; }
}

namespace Test
{
    [Route("/api/[controller]/[action]")]
    public class UsersController : BaseController
    {
        [HttpGet()]
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