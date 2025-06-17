using Newtonsoft.Json;

namespace SharpWarden.BitWardenDatabase.CipherItem.Models;

public class IdentityFieldModel
{
    [JsonProperty("address1")]
    public string Address1 { get; set; }

    [JsonProperty("address2")]
    public string Address2 { get; set; }

    [JsonProperty("address3")]
    public string Address3 { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("company")]
    public string Company { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }

    [JsonProperty("licenseNumber")]
    public string LicenseNumber { get; set; }

    [JsonProperty("middleName")]
    public string MiddleName { get; set; }

    [JsonProperty("passportNumber")]
    public string PassportNumber { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("postalCode")]
    public string PostalCode { get; set; }

    [JsonProperty("ssn")]
    public string SSN { get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }
}