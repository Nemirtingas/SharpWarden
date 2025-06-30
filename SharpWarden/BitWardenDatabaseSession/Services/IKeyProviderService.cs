// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.BitWardenDatabaseSession.Services;

public interface IKeyProviderService
{
    UserKeys GetUserKeys(Guid? ownerId);
    void LoadBitWardenUserKey(byte[] userEmail, byte[] userPassword, int kdfIterations, string key, string privateKey);
    void LoadBitWardenOrganizationKey(Guid organizationId, string organizationCipheredKey);
}