using System.Runtime.Serialization;

namespace SharpWarden.BitWardenDatabaseSession;

public enum ObjectType
{
    [EnumMember(Value = "profile")]
    Profile,
    [EnumMember(Value = "profileOrganization")]
    ProfileOrganization,
    [EnumMember(Value = "attachment")]
    Attachment,
    /// <summary>
    /// Has Login/Identity/Card/SecureNote/SSHKey, Id, Type, Name, Notes, Fields, PasswordHistory, RevisionDate, OrganizationId, Attachments, OrganizationUseTotp, CreationDate, DeletedDate, Reprompt, Key
    /// </summary>
    [EnumMember(Value = "cipherMini")]
    CipherMini,
    /// <summary>
    /// Same as <see cref="CipherMini"/> but with FolderId, Favorite, Edit, ViewPassword, Permissions
    /// </summary>
    [EnumMember(Value = "cipher")]
    Cipher,
    /// <summary>
    /// Same as <see cref="Cipher"/> but with CollectionsIds
    /// </summary>
    [EnumMember(Value = "cipherDetails")]
    CipherDetails,
    [EnumMember(Value = "collectionDetails")]
    CollectionDetails,
    [EnumMember(Value = "policy")]
    Policy,
    [EnumMember(Value = "sync")]
    Sync,
    [EnumMember(Value = "folder")]
    Folder,
    [EnumMember(Value = "list")]
    List,
    [EnumMember(Value = "attachment-fileUpload")]
    AttachmentFileUpload,
    [EnumMember(Value = "error")]
    Error,
}