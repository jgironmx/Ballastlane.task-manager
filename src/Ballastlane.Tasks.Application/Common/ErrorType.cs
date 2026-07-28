namespace Ballastlane.Tasks.Application.Common;

public enum ErrorType
{
    None = 0,
    Validation,
    NotFound,
    Unauthorized,
    Conflict,
    Failure,
}
