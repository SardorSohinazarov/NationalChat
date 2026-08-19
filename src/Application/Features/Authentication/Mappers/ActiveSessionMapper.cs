using Application.Features.Authentication.DataTransferObjects.Responses;
using Domain.Entities;

namespace Application.Features.Authentication.Mappers;

public static class ActiveSessionMapper
{
    public static ActiveSessionDto ToDto(Session session, int currentSessionId) =>
        new(session.Id, session.DeviceName, session.SystemVersion, session.AppVersion, session.IpAddress,
            session.CreatedAt, session.LastActiveAt, session.ExpiresAt, session.Id == currentSessionId);
}
