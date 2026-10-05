using System;
using System.Collections.Generic;
using System.Text;
using BasicServer;

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
        public HttpResult Profile([FromQuery] int a)
        {
            return Results.Ok("Profile" + a);
        }

        [HttpPost]
        public string Create()
        {
            return "Created user";
        }
    }
}
