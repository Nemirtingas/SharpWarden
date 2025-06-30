// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using SharpWarden.BitWardenDatabaseSession.Services;
using SharpWarden.BitWardenDatabaseSession.Models.CipherItem;
using SharpWarden.BitWardenDatabaseSession.Models.CollectionItem;
using SharpWarden.BitWardenDatabaseSession.Models.FolderItem;
using SharpWarden.BitWardenDatabaseSession.Models.ProfileItem;

namespace SharpWarden.BitWardenDatabaseSession.Models;

public class DatabaseModel : ISessionAware
{
    private IUserCryptoService _CryptoService;

    public DatabaseModel(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;
        Items = new();
        Folders = new();
        Collections = new();
        Profile = new(cryptoService);
    }

    public bool HasSession() => _CryptoService != null;

    public void SetCryptoService(IUserCryptoService cryptoService)
    {
        _CryptoService = cryptoService;

        foreach (var item in Items)
            item.SetCryptoService(_CryptoService);

        foreach (var item in Folders)
            item.SetCryptoService(_CryptoService);

        foreach (var item in Collections)
            item.SetCryptoService(_CryptoService);

        Profile?.SetCryptoService(_CryptoService);
    }
    
    [JsonProperty("ciphers")]
    public List<CipherItemModel> Items { get; set; }

    [JsonProperty("folders")]
    public List<FolderItemModel> Folders { get; set; }

    [JsonProperty("collections")]
    public List<CollectionItemModel> Collections { get; set; }

    [JsonProperty("profile")]
    public ProfileItemModel Profile { get; set; }
}