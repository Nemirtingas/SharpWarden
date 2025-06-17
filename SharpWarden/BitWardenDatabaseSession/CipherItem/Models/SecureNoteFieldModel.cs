namespace SharpWarden.BitWardenDatabaseSession.CipherItem.Models;


public class SecureNoteFieldModel
{
    public SecureNoteFieldModel(DatabaseSession databaseSession, Guid? organizationId, BitWardenDatabase.CipherItem.Models.SecureNoteFieldModel databaseModel)
    {
        Type = databaseModel.Type;
    }

    public int Type { get; }

    public BitWardenDatabase.CipherItem.Models.SecureNoteFieldModel ToDatabaseModel()
    {
        return new BitWardenDatabase.CipherItem.Models.SecureNoteFieldModel
        {
            Type = Type,
        };
    }
}