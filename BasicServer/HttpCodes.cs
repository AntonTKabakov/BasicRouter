using System;
using System.Collections.Generic;
using System.Text;

namespace BasicServer;

public class HttpResult
{

    public int StatusCode { get; set; }

    public string Body { get; set; } = "";

}

public static class Results
{
    public static HttpResult Ok(string body)
    {
        return new HttpResult
        {
            StatusCode = 200,
            Body = body
        };
    }

    public static HttpResult NotFound(string body = "Not Found")
    {
        return new HttpResult
        {
            StatusCode = 404,
            Body = body
        };
    }

    public static HttpResult BadRequest(string body = "Bad Request")
    {
        return new HttpResult
        {
            StatusCode = 400,
            Body = body
        };
    }
}