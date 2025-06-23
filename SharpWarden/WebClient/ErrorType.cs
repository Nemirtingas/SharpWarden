using System.Runtime.Serialization;

namespace SharpWarden.WebClient;

public enum ErrorType
{
    Unknown,
    [EnumMember(Value = "device_error")]
    DeviceError,
}