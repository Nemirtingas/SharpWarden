namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface IKeyProviderService
{
    UserKeys GetUserKeys(Guid? ownerId);
    void LoadBitWardenUserKey(byte[] userEmail, byte[] userPassword, int kdfIterations, string key, string privateKey);
    void LoadBitWardenOrganizationKey(Guid organizationId, string organizationCipheredKey);
}