﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Lyra.Core.API
{
    public sealed class LyraGlobal
    {
        public const string APPLICATIONNAME = "Wizard DAG Permisionless Blockchain";
        public const string OFFICIALDOMAIN = "wizdag";
        public const string OFFICIALTICKERCODE = "BES";
        public const int OFFICIALTICKERPRECISION = 8;

        public const int MAXMIMUMAUTHORIZERS = 21;

        public const int ProtocolVersion = 1;
        public const int DatabaseVersion = 1;

        public static string NodeAppName = APPLICATIONNAME + " " + typeof(LyraGlobal).Assembly.GetName().Version.ToString();

        public const int MinimalAuthorizerBalance = 1000000;
        public const decimal OFFICIALGENESISAMOUNT = 12000000000;

#if DEBUG
        public static readonly IList<string> Networks = new[] { "mainnet", "testnet",
            "devnet"
        };
#else
        public static readonly IList<string> Networks = new[] { "mainnet", "testnet"
        };
#endif

        // get api for (rpcurl, resturl)
        public static string SelectNode(string networkID)
        {
            switch (networkID)
            {
#if DEBUG
                case "devnet":
                    return "https://192.168.3.93:4505/api/";
#endif
                case "testnet":
                    return "https://seed.testnet.wizdag.com:4505/api/";
                case "mainnet":
                    return "https://seed.mainnet.wizdag.com:4505/api/";
                default:
                    throw new Exception("Unsupported network ID: " + networkID);
            }
        }
    }
}
