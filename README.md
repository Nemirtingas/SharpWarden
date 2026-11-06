## Tests

`SharpWarden.Tests/SharpWardenTest.cs` runs against a real server. The server and the ids of the test data come from
a configuration file: `SharpWarden.Tests/TestConfig.json` (Bitwarden EU test account, credentials in the
`SHARP_WARDEN_TEST_ACCOUNT_*` environment variables) or the file given by `SHARP_WARDEN_TEST_CONFIG`.

To run them against a throwaway Vaultwarden (no credentials needed):

```sh
docker run -d --rm --name vaultwarden -e SIGNUPS_ALLOWED=true -e I_REALLY_WANT_VOLATILE_STORAGE=true -e DOMAIN=http://localhost:8080 -p 8080:80 vaultwarden/server:1.37.5
dotnet run --project SharpWarden.Tests.Seed -- http://localhost:8080 vaultwarden-test-config.json argon2id
SHARP_WARDEN_TEST_CONFIG=$PWD/vaultwarden-test-config.json dotnet test SharpWarden.Tests/SharpWarden.Tests.csproj
```

`SharpWarden.Tests.Seed` creates the account (Argon2id or PBKDF2), an organization and the data expected by the tests,
then writes the configuration file.

## License

This project is licensed under the [Mozilla Public License 2.0](https://www.mozilla.org/MPL/2.0/).

You are free to use, modify, and distribute this code under the terms of the MPL.  
Only files you modify from this project must be shared under the same license.