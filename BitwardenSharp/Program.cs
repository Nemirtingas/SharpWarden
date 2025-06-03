using System;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BitwardenSharp;

class BitwardenDecryption
{
    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        var credentials = default(string[]);
        using (var fileStream = new FileStream("credentials", FileMode.Open))
        {
            credentials = (await new StreamReader(fileStream).ReadToEndAsync()).Trim().Split("|", 2);
        }

        var vaultwardenWebClient = new VaultwardenWebClient("https://vaultwarden.com");

        await vaultwardenWebClient.PreloginAsync(credentials[0]);
        await vaultwardenWebClient.AuthenticateAsync(credentials[1]);

        var bitwardenDatabaseSession = new BitWardenDatabaseSession();

        await bitwardenDatabaseSession.LoadBitwardenDatabaseAsync(
            null,
            Encoding.UTF8.GetBytes(credentials[1]),
            vaultwardenWebClient.UserKdfIterations,
            await vaultwardenWebClient.SyncAsync());

        var x = string.Empty;
        foreach (var item in bitwardenDatabaseSession.Items)
        {
            Console.WriteLine("================================");
            Console.WriteLine($"Item: {item.Id}");

            if (!string.IsNullOrWhiteSpace(item.Name?.CipherString))
            {
                try {
                    x = bitwardenDatabaseSession.GetClearString(item.OrganizationId, item.Name);
                } catch (Exception e) {
                    x = e.Message;
                }
                
                Console.WriteLine($"  Name: {x}");
            }
            if (!string.IsNullOrWhiteSpace(item.Login.Username?.CipherString))
            {
                try {
                    x = bitwardenDatabaseSession.GetClearString(item.OrganizationId, item.Login.Username);
                } catch (Exception e) {
                    x = e.Message;
                }

                Console.WriteLine($"  Username: {x}");
            }
            if (!string.IsNullOrWhiteSpace(item.Login.Password?.CipherString))
            {
                try
                {
                    x = bitwardenDatabaseSession.GetClearString(item.OrganizationId, item.Login.Password);
                }
                catch (Exception e)
                {
                    x = e.Message;
                }
                
                Console.WriteLine($"  Password: {x}");
            }

            if (item.Fields?.Count > 0)
            {
                Console.WriteLine();
                foreach (var customField in item.Fields)
                {
                    Console.WriteLine("  Custom field");
                    try
                    {
                        x = bitwardenDatabaseSession.GetClearString(item.OrganizationId, customField.Name);
                    }
                    catch (Exception e)
                    {
                        x = e.Message;
                    }

                    Console.WriteLine($"    Name: {x}");
                    try
                    {
                        x = bitwardenDatabaseSession.GetClearString(item.OrganizationId, customField.Value);
                    }
                    catch (Exception e)
                    {
                        x = e.Message;
                    }
                    Console.WriteLine($"    Value: {x}");

                    Console.WriteLine();
                }
            }

            if (item.Attachments?.Count > 0)
            {
                Console.WriteLine();
                foreach (var attachement in item.Attachments)
                {
                    Console.WriteLine($"  Attachement id: {attachement.Id}");
                    try
                    {
                        x = bitwardenDatabaseSession.GetClearString(item.OrganizationId, attachement.FileName);
                    }
                    catch (Exception e)
                    {
                        x = e.Message;
                    }

                    Console.WriteLine($"    Name: {x}");
                    Console.WriteLine($"    Size: {attachement.Size}");
                    Console.WriteLine($"    Url : {attachement.Url}");
                    Console.WriteLine();
                }
            }
            Console.WriteLine();
        }


    }
}