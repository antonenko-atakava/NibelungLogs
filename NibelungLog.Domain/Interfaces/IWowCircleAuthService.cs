using NibelungLog.Domain.Types.Dto;
using NibelungLog.Domain.Types.Dto.Response;

namespace NibelungLog.Domain.Interfaces;

public interface IWowCircleAuthService
{
    Task<LoginResult> LoginAsync(string accountName, string password, int serverId, CancellationToken cancellationToken = default);
}
