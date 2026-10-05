using System.Reflection;

namespace BasicServer;

public class BindingResult
{
    public bool Success { get; set; }

    public object?[] Arguments { get; set; } = [];
}

internal class ParameterResult
{
    public bool Success { get; set; }

    public object? Value { get; set; }
}

public class ParameterBinder
{
    public BindingResult Bind(MethodInfo method, Request request)
    {
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterResult = BindParameter(parameters[i], request);

            if (!parameterResult.Success)
            {
                return new BindingResult
                {
                    Success = false
                };
            }

            args[i] = parameterResult.Value;
        }

        return new BindingResult
        {
            Success = true,
            Arguments = args
        };
    }

    private ParameterResult BindParameter(
        ParameterInfo parameter,
        Request request)
    {
        if (parameter.GetCustomAttribute<FromQueryAttribute>() != null)
        {
            return BindFromQuery(parameter, request);
        }

        return new ParameterResult
        {
            Success = true,
            Value = GetDefaultValue(parameter.ParameterType)
        };
    }

    private ParameterResult BindFromQuery(
        ParameterInfo parameter,
        Request request)
    {
        if (parameter.ParameterType == typeof(Dictionary<string, string>))
        {
            return new ParameterResult
            {
                Success = true,
                Value = request.Query
            };
        }

        if (!request.Query.TryGetValue(parameter.Name!, out var value))
        {
            return new ParameterResult
            {
                Success = false
            };
        }

        return ConvertToType(value, parameter.ParameterType);
    }

    private ParameterResult ConvertToType(
        string value,
        Type type)
    {
        try
        {
            if (type == typeof(string))
                return Success(value);

            if (type == typeof(int))
                return Success(int.Parse(value));

            if (type == typeof(long))
                return Success(long.Parse(value));

            if (type == typeof(bool))
                return Success(bool.Parse(value));

            if (type == typeof(double))
                return Success(double.Parse(value));

            if (type == typeof(Guid))
                return Success(Guid.Parse(value));

            return Success(Convert.ChangeType(value, type));
        }
        catch
        {
            return new ParameterResult
            {
                Success = false
            };
        }
    }

    private static ParameterResult Success(object? value)
    {
        return new ParameterResult
        {
            Success = true,
            Value = value
        };
    }

    private object? GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
            return null;

        return Activator.CreateInstance(type);
    }
}

