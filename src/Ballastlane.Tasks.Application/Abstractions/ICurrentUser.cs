namespace Ballastlane.Tasks.Application.Abstractions;

/// <summary>
/// The authenticated caller, as seen by the current request. Task ownership is always taken
/// from here — never from request payloads — so a client cannot act on another user's data by
/// supplying a different id in the request body.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }
}
