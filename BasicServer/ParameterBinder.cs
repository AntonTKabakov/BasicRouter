using System.Reflection;

namespace BasicServer;

public class ParameterBinder
{
    public object?[] Bind(MethodInfo method, Request request)
    {
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            args[i] = BindParameter(parameters[i], request);
        }

        return args;
    }

    private object? BindParameter(
        ParameterInfo parameter,
        Request request)
    {
        if (parameter.GetCustomAttribute<FromQueryAttribute>() != null)
        {
            return BindFromQuery(parameter, request);
        }

        return GetDefaultValue(parameter.ParameterType);
    }

    private object? BindFromQuery(
        ParameterInfo parameter,
        Request request)
    {
        if (parameter.ParameterType == typeof(Dictionary<string, string>))
        {
            return request.Query;
        }

        if (!request.Query.TryGetValue(parameter.Name!, out var value))
        {
            throw new Exception(
                $"Missing query parameter '{parameter.Name}'");
        }

        return ConvertToType(value, parameter.ParameterType);
    }

    private object? ConvertToType(
        string value,
        Type type)
    {
        try
        {
            if (type == typeof(string))
                return value;

            if (type == typeof(int))
                return int.Parse(value);

            if (type == typeof(long))
                return long.Parse(value);

            if (type == typeof(bool))
                return bool.Parse(value);

            if (type == typeof(double))
                return double.Parse(value);

            if (type == typeof(Guid))
                return Guid.Parse(value);

            return Convert.ChangeType(value, type);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new object();
        }
       

    }

    private object? GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
            return null;

        return Activator.CreateInstance(type);
    }
}