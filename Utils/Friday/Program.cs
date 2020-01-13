﻿using Lyra.Core.Accounts;
using Lyra.Core.API;
using Lyra.Core.Cryptography;
using Lyra.Core.LiteDB;
using Neo.Wallets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Friday
{
    class Program
    {
        static string testCoin = "Friday.Coin";
        static string lyraCoin = "Lyra.Coin";

        // args: [number] the tps to simulate
        // 
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var workingFolder = @"C:\working\Friday";
            var network_id = "testnet";            
            var lyraFolder = BaseAccount.GetFullFolderName("Lyra-CLI-" + network_id);

            // create and save wallets
            //var tt = new TransactionTester();
            //var wlts = tt.CreateWallet(1000);
            //var json = JsonConvert.SerializeObject(wlts);
            //File.WriteAllText(workingFolder + @"\wallets.json", json);

            // key is account id
            var wallets = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(workingFolder + @"\\wallets.json"));

            var rpcClient = await LyraRestClient.CreateAsync(network_id, "Windows", "Lyra Client Cli", "1.0a", "https://192.168.3.62:4505/api/LyraNode/");
            var tt = new TransactionTester(rpcClient);

            //var all = await tt.RefreshBalancesAsync(wallets.Select(a => new KeyPair(Base58Encoding.DecodePrivateKey(a.Value))).ToArray());
            //File.WriteAllText(workingFolder + @"\balances.json", JsonConvert.SerializeObject(all));

            var rich10 = JsonConvert.DeserializeObject<List<WalletBalance>>(File.ReadAllText(workingFolder + @"\balances.json"));
            var realRich10 = rich10.Where(a => a.balance.ContainsKey(lyraCoin) && a.balance.ContainsKey(testCoin))
                .Where(a => a.balance[testCoin] > 10000).ToDictionary(a => a.privateKey, a => a.balance);

            var rich90 = wallets.Where(a => !realRich10.ContainsKey(a.Value)).Take(90);
            File.WriteAllText(workingFolder + @"\rich90.json", JsonConvert.SerializeObject(rich90));

            await tt.MultiThreadedSendAsync(realRich10.Keys.ToArray(), rich90.Select(a => a.Key).ToArray(), new Dictionary<string, decimal> { { testCoin, 100000 } });

            //var masterWallet = new Wallet(new LiteAccountDatabase(), network_id);
            //masterWallet.AccountName = "My Account";
            //masterWallet.OpenAccount(BaseAccount.GetFullPath(lyraFolder), masterWallet.AccountName);

            //await masterWallet.Sync(rpcClient);

            //foreach(var b in masterWallet.GetLatestBlock().Balances)
            //{
            //    Console.WriteLine($"{b.Key}: {b.Value}");
            //}
            //Console.WriteLine("Hello Lyra!");

            //var top10 = wallets.Take(10).ToDictionary(a => a.Key, a => a.Value);

            //await tt.SingleThreadedSendAsync(10, masterWallet, top10.Keys.ToArray(), new Dictionary<string, decimal> {
            //    { lyraCoin, 10000 }, {testCoin, 1000000}
            //});

            //var top100 = wallets.Skip(10).Take(100).ToDictionary(a => a.Key, a => a.Value);
            //await tt.MultiThreadedSendAsync(10, top10.Select(a => new KeyPair(Base58Encoding.DecodePrivateKey(a.Value))).ToArray(),
            //    top100.Values.ToArray(), new Dictionary<string, decimal> {
            //        { lyraCoin, 100 }, {testCoin, 10000} }
            //    );
        }
    }
}
