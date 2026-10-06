using System.Reflection;
using System.Text.Json;

namespace BasicServer;

internal class BindingResult
{
    public bool Success { get; set; }

    public object?[] Arguments { get; set; } = [];
}

internal class ParameterResult
{
    public bool Success { get; set; }

    public object? Value { get; set; }
}

internal class ParameterBinder
{
    internal BindingResult Bind(MethodInfo method, Request request)
    {
        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var result = BindParameter(parameters[i], request);

            if (!result.Success)
            {
                return new BindingResult
                {
                    Success = false
                };
            }

            args[i] = result.Value;
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

        if (parameter.GetCustomAttribute<FromBodyAttribute>() != null)
        {
            return BindFromBody(parameter, request);
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
            return Success(request.Query);
        }

        if (IsSimpleType(parameter.ParameterType))
        {
            if (!request.Query.TryGetValue(parameter.Name!, out var value))
            {
                return Failure();
            }

            return ConvertToType(value, parameter.ParameterType);
        }

        try
        {
            var instance = Activator.CreateInstance(parameter.ParameterType);

            if (instance == null)
            {
                return Failure();
            }

            var properties = parameter.ParameterType.GetProperties();

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                    continue;

                if (!request.Query.TryGetValue(
                        property.Name,
                        out var value))
                {
                    continue;
                }

                var converted = ConvertToType(
                    value,
                    property.PropertyType);

                if (!converted.Success)
                {
                    return Failure();
                }

                property.SetValue(instance, converted.Value);
            }

            return Success(instance);
        }
        catch
        {
            return Failure();
        }
    }

    private bool IsSimpleType(Type type)
    {
        return type == typeof(string)
               || type == typeof(int)
               || type == typeof(long)
               || type == typeof(bool)
               || type == typeof(double)
               || type == typeof(Guid)
               || type.IsEnum;
    }

    private ParameterResult BindFromBody(
        ParameterInfo parameter,
        Request request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return Failure();
        }

        try
        {
            var value = JsonSerializer.Deserialize(
                request.Body,
                parameter.ParameterType,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return Success(value);
        }
        catch (JsonException)
        {
            return Failure();
        }
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
            return Failure();
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

    private static ParameterResult Failure()
    {
        return new ParameterResult
        {
            Success = false
        };
    }

    private object? GetDefaultValue(Type type)
    {
        if (!type.IsValueType)
            return null;

        return Activator.CreateInstance(type);
    }
}