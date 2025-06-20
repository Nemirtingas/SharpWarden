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
    [EnumMember(Value = "cipherDetails")]
    CipherDetails,
    [EnumMember(Value = "cipher")]
    Cipher,
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
}