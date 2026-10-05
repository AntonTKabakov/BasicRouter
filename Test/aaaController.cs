using System;
using System.Collections.Generic;
using System.Text;
using BasicServer;

namespace Test
{
    [Route("/app/[controller]/[action]")]
    public class UsersController : BaseController
    {
        [HttpGet]
        public string GetUsers()
        {
            return "Users";
        }

        [HttpGet("profile")]
        public string Profile([FromQuery] int a)
        {
            return "Profile" + a;
        }

        [HttpPost]
        public string Create()
        {
            return "Created user";
        }
    }
}
