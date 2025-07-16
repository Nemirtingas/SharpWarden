// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Models.CipherItem;

public class CipherItemModel : ISessionAware
{
    private ConditionalCryptoService _conditionalCryptoService;

    public CipherItemModel()
    {
    }

    public CipherItemModel(IUserCryptoService cryptoService)
    {
        SetCryptoService(cryptoService);
    }

    public bool HasSession() => _conditionalCryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _conditionalCryptoService ??= new ConditionalCryptoService(cryptoService.KeyProviderService, () => this.OrganizationId);
        _conditionalCryptoService.CryptoService = cryptoService;

        Card?.SetCryptoService(_conditionalCryptoService);
        Identity?.SetCryptoService(_conditionalCryptoService);
        Login?.SetCryptoService(_conditionalCryptoService);
        Name?.SetCryptoService(_conditionalCryptoService);
        Notes?.SetCryptoService(_conditionalCryptoService);
        SshKey?.SetCryptoService(_conditionalCryptoService);

        if (Attachments != null)
            foreach (var v in Attachments)
                v.SetCryptoService(_conditionalCryptoService);

        if (Fields != null)
            foreach (var v in Fields)
                v.SetCryptoService(_conditionalCryptoService);

        if (PasswordHistory != null)
            foreach (var v in PasswordHistory)
                v.SetCryptoService(_conditionalCryptoService);
    }

    public LoginFieldModel CreateLogin()
    {
        ItemType = CipherItemType.Login;
        Login = new LoginFieldModel(_conditionalCryptoService);
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
        Identity = new IdentityFieldModel(_conditionalCryptoService);
        return Identity;
    }

    public CardFieldModel CreateCard()
    {
        ItemType = CipherItemType.Card;
        Card = new CardFieldModel(_conditionalCryptoService);
        return Card;
    }

    public SshKeyFieldModel CreateSshKey()
    {
        ItemType = CipherItemType.SshKey;
        SshKey = new SshKeyFieldModel(_conditionalCryptoService);
        return SshKey;
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
    public bool OrganizationUseTotp { get; set; }

    [JsonProperty("passwordHistory")]
    public List<PasswordHistoryModel> PasswordHistory { get; set; }

    [JsonProperty("reprompt")]
    public CipherRepromptType Reprompt { get; set; }

    [JsonProperty("revisionDate")]
    public DateTime? RevisionDate { get; set; }

    [JsonProperty("secureNote")]
    public SecureNoteFieldModel SecureNote { get; set; }

    [JsonProperty("sshKey")]
    public SshKeyFieldModel SshKey { get; set; }

    [JsonProperty("type")]
    public CipherItemType ItemType { get; set; }

    [JsonProperty("viewPassword")]
    public bool ViewPassword { get; set; }

    [JsonProperty("permissions")]
    public PermissionFieldModel Permissions { get; set; }
}
