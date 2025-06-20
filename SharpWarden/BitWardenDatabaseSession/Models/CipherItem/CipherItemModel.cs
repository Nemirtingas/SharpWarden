using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class CipherItemModel : ISessionAware
{
    private ConditionalCryptoService _ConditionalCryptoService;

    public CipherItemModel()
    {
    }

    public CipherItemModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _ConditionalCryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        if (_ConditionalCryptoService == null)
        {
            _ConditionalCryptoService = new ConditionalCryptoService(cryptoService.KeyProviderService, () => this.OrganizationId);
        }
        _ConditionalCryptoService.CryptoService = cryptoService;

        Card?.SetCryptoService(_ConditionalCryptoService);
        Identity?.SetCryptoService(_ConditionalCryptoService);
        Login?.SetCryptoService(_ConditionalCryptoService);
        Name?.SetCryptoService(_ConditionalCryptoService);
        Notes?.SetCryptoService(_ConditionalCryptoService);
        SSHKey?.SetCryptoService(_ConditionalCryptoService);

        if (Attachments != null)
            foreach (var v in Attachments)
                v.SetCryptoService(_ConditionalCryptoService);

        if (Fields != null)
            foreach (var v in Fields)
                v.SetCryptoService(_ConditionalCryptoService);

        if (PasswordHistory != null)
            foreach (var v in PasswordHistory)
                v.SetCryptoService(_ConditionalCryptoService);
    }

    public LoginFieldModel CreateLogin()
    {
        ItemType = CipherItemType.Login;
        Login = new LoginFieldModel(_ConditionalCryptoService);
        return Login;
    }

    public SecureNoteFieldModel CreateSecureNote()
    {
        ItemType = CipherItemType.SecureNote;
        SecureNote = new SecureNoteFieldModel();
        return SecureNote;
    }

    public IdentityFieldModel CreateIdentity()
    {
        ItemType = CipherItemType.Identity;
        Identity = new IdentityFieldModel(_ConditionalCryptoService);
        return Identity;
    }

    public CardFieldModel CreateCard()
    {
        ItemType = CipherItemType.Card;
        Card = new CardFieldModel(_ConditionalCryptoService);
        return Card;
    }

    public SSHKeyFieldModel CreateSSHKey()
    {
        ItemType = CipherItemType.SSHKey;
        SSHKey = new SSHKeyFieldModel(_ConditionalCryptoService);
        return SSHKey;
    }

    [JsonProperty("attachments")]
    public List<AttachmentModel> Attachments { get; set; }

    [JsonProperty("card")]
    public CardFieldModel Card { get; set; }

    [JsonProperty("collectionIds")]
    public List<Guid> CollectionsIds { get; set; }

    [JsonProperty("creationDate")]
    public DateTime? CreationDate { get; set; }

    // Ignore data item, its dynamic and contains one of the Card/Identity/Login/SecureNote object + some other fields like Fields.
    //[JsonProperty("data")]
    //public object Data { get; set; }

    [JsonProperty("deletedDate")]
    public DateTime? DeletedDate { get; set; }

    [JsonProperty("edit")]
    public bool Edit { get; set; }

    [JsonProperty("favorite")]
    public bool Favorite { get; set; }

    [JsonProperty("fields")]
    public List<CustomFieldModel> Fields { get; set; }

    [JsonProperty("folderId")]
    public Guid? FolderId { get; set; }

    [JsonProperty("id")]
    public Guid? Id { get; set; }

    [JsonProperty("identity")]
    public IdentityFieldModel Identity { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("login")]
    public LoginFieldModel Login { get; set; }

    [JsonProperty("name")]
    public EncryptedString Name { get; set; }

    [JsonProperty("notes")]
    public EncryptedString Notes { get; set; }

    [JsonProperty("object")]
    public ObjectType ObjectType { get; set; } = ObjectType.CipherDetails;

    [JsonProperty("organizationId")]
    public Guid? OrganizationId { get; set; }

    [JsonProperty("organizationUseTotp")]
    public bool OrganizationUseTOTP { get; set; }

    [JsonProperty("passwordHistory")]
    public List<PasswordHistoryModel> PasswordHistory { get; set; }

    [JsonProperty("reprompt")]
    public CipherRepromptType Reprompt { get; set; }

    [JsonProperty("revisionDate")]
    public DateTime? RevisionDate { get; set; }

    [JsonProperty("secureNote")]
    public SecureNoteFieldModel SecureNote { get; set; }

    [JsonProperty("sshKey")]
    public SSHKeyFieldModel SSHKey { get; set; }

    [JsonProperty("type")]
    public CipherItemType ItemType { get; set; }

    [JsonProperty("viewPassword")]
    public bool ViewPassword { get; set; }
}
