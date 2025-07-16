// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SharpWarden.BitWardenDatabaseSession.Services;

namespace SharpWarden.BitWardenDatabaseSession.Converter;

public class SessionAwareContractResolver : DefaultContractResolver
{
    private readonly IUserCryptoService _cryptoService;

    public SessionAwareContractResolver(IUserCryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    protected override JsonContract CreateContract(Type objectType)
    {
        var contract = base.CreateContract(objectType);

        if (typeof(ISessionAware).IsAssignableFrom(objectType))
        {
            var converterType = typeof(SessionAwareConverter<>).MakeGenericType(objectType);
            contract.Converter = (JsonConverter)Activator.CreateInstance(converterType, _cryptoService)!;
        }

        return contract;
    }
}