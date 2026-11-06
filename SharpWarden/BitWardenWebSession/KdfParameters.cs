// Copyright (c) 2025 Nemirtingas
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// See LICENSE.txt in the project root for more information.

using System.Security.Cryptography;

namespace SharpWarden.BitWardenWebSession;

/// <summary>
/// Key derivation settings of an account, as returned by the server (prelogin / login).
/// </summary>
public sealed class KdfParameters
{
    // Same bounds as the Bitwarden clients. The lower bounds prevent a malicious server from
    // downgrading the KDF, the upper bounds prevent it from exhausting the client's memory/CPU.
    public const int MinimumPbkdf2Iterations = 5000;
    public const int MinimumArgon2Iterations = 2;
    public const int MaximumArgon2Iterations = 10;
    public const int MinimumArgon2MemoryMiB = 16;
    public const int MaximumArgon2MemoryMiB = 1024;
    public const int MinimumArgon2Parallelism = 1;
    public const int MaximumArgon2Parallelism = 16;

    public KdfType Type { get; init; }

    public int Iterations { get; init; }

    /// <summary>Argon2id memory in MiB.</summary>
    public int? Memory { get; init; }

    /// <summary>Argon2id parallelism.</summary>
    public int? Parallelism { get; init; }

    public static KdfParameters Pbkdf2(int iterations)
        => new() { Type = KdfType.PBKDF2SHA256, Iterations = iterations };

    public static KdfParameters Argon2id(int iterations, int memoryMiB, int parallelism)
        => new() { Type = KdfType.Argon2id, Iterations = iterations, Memory = memoryMiB, Parallelism = parallelism };

    /// <summary>
    /// Throws when the parameters are not supported or not safe to use.
    /// </summary>
    public void Validate()
    {
        switch (Type)
        {
            case KdfType.PBKDF2SHA256:
                if (Iterations < MinimumPbkdf2Iterations)
                    throw new CryptographicException($"KDF iteration count {Iterations} is below the minimum allowed ({MinimumPbkdf2Iterations}).");
                return;

            case KdfType.Argon2id:
                if (Iterations < MinimumArgon2Iterations || Iterations > MaximumArgon2Iterations)
                    throw new CryptographicException($"Argon2id iteration count {Iterations} is outside the allowed range [{MinimumArgon2Iterations}, {MaximumArgon2Iterations}].");
                if (Memory is not (>= MinimumArgon2MemoryMiB and <= MaximumArgon2MemoryMiB))
                    throw new CryptographicException($"Argon2id memory {Memory} MiB is outside the allowed range [{MinimumArgon2MemoryMiB}, {MaximumArgon2MemoryMiB}].");
                if (Parallelism is not (>= MinimumArgon2Parallelism and <= MaximumArgon2Parallelism))
                    throw new CryptographicException($"Argon2id parallelism {Parallelism} is outside the allowed range [{MinimumArgon2Parallelism}, {MaximumArgon2Parallelism}].");
                return;
        }

        throw new NotSupportedException($"KDF type {Type} is not supported.");
    }

    public override string ToString()
        => Type == KdfType.Argon2id ? $"Argon2id(t={Iterations}, m={Memory}MiB, p={Parallelism})" : $"PBKDF2-SHA256({Iterations})";
}
