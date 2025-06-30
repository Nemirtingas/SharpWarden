// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

namespace SharpWarden.Models;

public enum SSHKeyType
{
    RSA = 0,
    ED25519 = 1,
}

public class SSHKeyModel
{
    public SSHKeyType KeyType { get; set; }
    public string Fingerprint { get; set; }
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }
}