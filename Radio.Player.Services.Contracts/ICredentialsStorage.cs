using Radio.Player.Services.Contracts.Models;

namespace Radio.Player.Services.Contracts;

public interface ICredentialsStorage
{
    Task StoreCredentialsAsync(Credentials credentials, CancellationToken cancellationToken = default);

    Task<Credentials> GetCredentialsAsync(string serviceName, CancellationToken cancellationToken = default);

    Task RemoveCredentialsAsync(string serviceName, CancellationToken cancellationToken = default);
}