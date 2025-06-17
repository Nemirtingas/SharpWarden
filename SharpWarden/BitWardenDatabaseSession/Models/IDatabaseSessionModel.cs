namespace SharpWarden.BitWardenDatabaseSession.Models;

interface IDatabaseSessionModel
{
    public bool HasSession();
    public void SetDatabaseSession(DatabaseSession databaseSession);
    public void SetDatabaseSession(DatabaseSession databaseSession, Guid? organizationId);
}
