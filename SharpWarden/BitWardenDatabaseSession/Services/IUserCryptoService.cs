namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface IUserCryptoService : ICryptoService
{
    string GetClearStringWithRSAKey(string cipherString);
    string GetClearStringWithMasterKey(string cipherString);
    string GetClearStringAuto(string cipherString);
    byte[] GetClearBytesWithRSAKey(string cipherString);
    byte[] GetClearBytesWithMasterKey(string cipherString);
    byte[] GetClearBytesAuto(string cipherString);
    string CryptClearStringWithRSAKey(string clearString);
    string CryptClearStringWithMasterKey(string clearString);
    string CryptClearBytesWithRSAKey(byte[] bytes);
    string CryptClearBytesWithMasterKey(byte[] bytes);

    EncryptedString NewEncryptedString();
}