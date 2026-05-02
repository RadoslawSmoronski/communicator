namespace Application.Common.Exceptions;

public class ForbiddenException(string message) : Exception(message)
{
}

public class NotFoundException(string resource, object key)
    : Exception($"{resource} '{key}' was not found.")
{
}