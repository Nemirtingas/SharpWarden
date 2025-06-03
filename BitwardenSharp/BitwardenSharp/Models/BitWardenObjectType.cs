using System.Runtime.Serialization;

namespace BitwardenSharp.Models;

public enum BitWardenObjectType
{
    [EnumMember(Value = "profile")]
    Profile,
    [EnumMember(Value = "profileOrganization")]
    ProfileOrganization,
    [EnumMember(Value = "attachment")]
    Attachment,
    [EnumMember(Value = "cipherDetails")]
    CipherDetails,
    [EnumMember(Value = "collectionDetails")]
    CollectionDetails,
    [EnumMember(Value = "policy")]
    Policy,
    [EnumMember(Value = "sync")]
    Sync,
}