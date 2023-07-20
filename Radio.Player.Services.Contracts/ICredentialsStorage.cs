using Radio.Player.Services.Contracts.Models;

namespace Radio.Player.Services.Contracts;

public interface ICredentialsStorage
{
    Task StoreCredentialsAsync(CredentialsType credentialsType, Credentials credentials, CancellationToken cancellationToken = default);

    Task<Credentials> GetCredentialsAsync(CredentialsType credentialsType, CancellationToken cancellationToken = default);

    Task RemoveCredentialsAsync(CredentialsType credentialsType, CancellationToken cancellationToken = default);
}