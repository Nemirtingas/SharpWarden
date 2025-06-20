using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession;

public interface ISessionAware
{
    public bool HasSession();
    public void SetCryptoService(IUserCryptoService cryptoService);
}
