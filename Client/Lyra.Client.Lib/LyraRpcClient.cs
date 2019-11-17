﻿using Grpc.Net.Client;
using Lyra.Core.API;
using Lyra.Core.Blocks;
using Lyra.Core.Blocks.Transactions;
using Lyra.Core.Protos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Lyra.Client.Lib
{
    public class LyraRpcClient : Core.Protos.LyraApi.LyraApiClient, INodeAPI
    {
        public LyraRpcClient(GrpcChannel channel) : base(channel)
        {

        }

        public static LyraRpcClient Create(string networkId, string rpcUrl = null)
        {
            var httpClientHandler = new HttpClientHandler();
            // Return `true` to allow certificates that are untrusted/invalid
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var url = rpcUrl == null ? SelectNode(networkId) : rpcUrl;
            var channel = GrpcChannel.ForAddress(url,
                new GrpcChannelOptions { HttpClient = httpClient });
            var rpcClient = new LyraRpcClient(channel);
            return rpcClient;
        }

        private static string SelectNode(string networkID)
        {
            switch (networkID)
            {
#if DEBUG
                case "lexdev":
                    return "https://34.80.72.244:5492/";
#endif
                case "lexnet":
                    return "https://34.80.72.244:5392/";
                case "testnet":
                    return "https://testnet.lyratokens.com:5392/";
                case "mainnet":
                    return "https://mainnet.lyratokens.com:5392/";
                default:
                    throw new Exception("Unsupported network ID");
            }
        }

        // util 
        private T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        private string Json(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        Task<AuthorizationAPIResult> INodeAPI.CancelTradeOrder(CancelTradeOrderBlock block)
        {
            throw new NotImplementedException();
        }

        async Task<AuthorizationAPIResult> INodeAPI.CreateToken(TokenGenesisBlock block)
        {
            var request = new CreateTokenRequest()
            {
                CreateTokenJson = Json(block)
            };
            var result = await CreateTokenAsync(request);
            return ToAAR(result);
        }

        Task<AuthorizationAPIResult> INodeAPI.ExecuteTradeOrder(ExecuteTradeOrderBlock block)
        {
            throw new NotImplementedException();
        }

        async Task<AccountHeightAPIResult> INodeAPI.GetAccountHeight(string AccountId, string Signature)
        {
            var request = new GetAccountHeightRequest()
            {
                AccountId = AccountId,
                Signature = Signature
            };
            var result = await GetAccountHeightAsync(request);
            var ret = new AccountHeightAPIResult()
            {
                ResultCode = result.ResultCode,
                NetworkId = result.NetworkId,
                SyncHash = result.SyncHash,
                Height = result.Height,
            };
            return ret;
        }

        Task<ActiveTradeOrdersAPIResult> INodeAPI.GetActiveTradeOrders(string AccountId, string SellToken, string BuyToken, TradeOrderListTypes OrderType, string Signature)
        {
            throw new NotImplementedException();
        }

        async Task<BlockAPIResult> INodeAPI.GetBlockByHash(string AccountId, string Hash, string Signature)
        {
            var request = new GetBlockByHashRequest()
            {
                AccountId = AccountId,
                Signature = Signature,
                Hash = Hash
            };
            var result = await GetBlockByHashAsync(request);
            var ret = new BlockAPIResult()
            {
                ResultBlockType = result.ResultBlockType,
                BlockData = result.BlockData,
                ResultCode = result.ResultCode
            };
            return ret;
        }

        async Task<BlockAPIResult> INodeAPI.GetBlockByIndex(string AccountId, int Index, string Signature)
        {
            var request = new GetBlockByIndexRequest()
            {
                AccountId = AccountId,
                Signature = Signature,
                Index = Index
            };
            var result = await GetBlockByIndexAsync(request);
            var ret = new BlockAPIResult()
            {
                ResultBlockType = result.ResultBlockType,
                BlockData = result.BlockData,
                ResultCode = result.ResultCode
            };
            return ret;
        }

        async Task<BlockAPIResult> INodeAPI.GetLastServiceBlock(string AccountId, string Signature)
        {
            var reqGetLSB = new GetLastServiceBlockRequest()
            {
                AccountId = AccountId,
                Signature = Signature
            };

            var result = await GetLastServiceBlockAsync(reqGetLSB);
            var ret = new BlockAPIResult()
            {
                ResultBlockType = result.ResultBlockType,
                BlockData = result.BlockData,
                ResultCode = result.ResultCode
            };
            return ret;
        }

        Task<NonFungibleListAPIResult> INodeAPI.GetNonFungibleTokens(string AccountId, string Signature)
        {
            throw new NotImplementedException();
        }

        async Task<AccountHeightAPIResult> INodeAPI.GetSyncHeight()
        {
            var request = new SyncHeightRequest();
            var result = await GetSyncHeightAsync(request);
            var ret = new AccountHeightAPIResult()
            {
                ResultCode = result.ResultCode,
                NetworkId = result.NetworkId,
                Height = result.Height,
                SyncHash = result.SyncHash
            };
            return ret;
        }

        async Task<BlockAPIResult> INodeAPI.GetTokenGenesisBlock(string AccountId, string TokenTicker, string Signature)
        {
            var request = new GetTokenGenesisBlockRequest()
            {
                AccountId = AccountId,
                Signature = Signature,
                TokenTicker = TokenTicker
            };
            var result = await GetTokenGenesisBlockAsync(request);
            var ret = new BlockAPIResult()
            {
                ResultBlockType = result.ResultBlockType,
                BlockData = result.BlockData,
                ResultCode = result.ResultCode
            };
            return ret;
        }

        async Task<GetTokenNamesAPIResult> INodeAPI.GetTokenNames(string AccountId, string Signature, string keyword)
        {
            var request = new GetTokenNamesRequest()
            {
                AccountId = AccountId,
                Signature = Signature,
                Keyword = keyword
            };
            var result = await GetTokenNamesAsync(request);
            var ret = new GetTokenNamesAPIResult()
            {
                ResultCode = result.ResultCode,
                TokenNames = result.TokenNames.ToList()
            };
            return ret;
        }

        Task<AuthorizationAPIResult> INodeAPI.ImportAccount(ImportAccountBlock block)
        {
            throw new NotImplementedException();
        }

        Task<TradeAPIResult> INodeAPI.LookForNewTrade(string AccountId, string BuyTokenCode, string SellTokenCode, string Signature)
        {
            throw new NotImplementedException();
        }

        async Task<NewTransferAPIResult> INodeAPI.LookForNewTransfer(string AccountId, string Signature)
        {
            var request = new LookForNewTransferRequest()
            {
                AccountId = AccountId,
                Signature = Signature
            };

            var result = await LookForNewTransferAsync(request);
            var ret = new NewTransferAPIResult()
            {
                ResultCode = result.ResultCode,
                SourceHash = result.SourceHash,
                NonFungibleToken = FromJson<NonFungibleToken>(result.NonFungibleTokenJson),
                Transfer = FromJson<TransactionInfoEx>(result.TransferJson)
            };
            return ret;
        }

        async Task<AuthorizationAPIResult> INodeAPI.OpenAccountWithGenesis(LyraTokenGenesisBlock block)
        {
            var genReq = new OpenAccountWithGenesisRequest()
            {
                OpenTokenGenesisBlockJson = Json(block)
            };
            var result = await OpenAccountWithGenesisAsync(genReq);
            return ToAAR(result);
        }

        Task<AuthorizationAPIResult> INodeAPI.OpenAccountWithImport(OpenAccountWithImportBlock block)
        {
            throw new NotImplementedException();
        }

        async Task<AuthorizationAPIResult> INodeAPI.ReceiveTransfer(ReceiveTransferBlock block)
        {
            var request = new ReceiveTransferAndOpenAccountRequest()
            {
                OpenReceiveBlockJson = Json(block)
            };
            var result = await ReceiveTransferAndOpenAccountAsync(request);
            return ToAAR(result);
        }

        async Task<AuthorizationAPIResult> INodeAPI.ReceiveTransferAndOpenAccount(OpenWithReceiveTransferBlock block)
        {
            var request = new ReceiveTransferAndOpenAccountRequest()
            {
                OpenReceiveBlockJson = Json(block)
            };
            var result = await ReceiveTransferAndOpenAccountAsync(request);
            return ToAAR(result);
        }

        async Task<AuthorizationAPIResult> INodeAPI.SendTransfer(SendTransferBlock block)
        {
            var request = new SendTransferRequest()
            {
                SendBlockJson = Json(block)
            };
            var result = await SendTransferAsync(request);
            return ToAAR(result);
        }

        Task<AuthorizationAPIResult> INodeAPI.Trade(TradeBlock block)
        {
            throw new NotImplementedException();
        }

        Task<TradeOrderAuthorizationAPIResult> INodeAPI.TradeOrder(TradeOrderBlock block)
        {
            throw new NotImplementedException();
        }

        private AuthorizationAPIResult ToAAR(AuthorizationsReply result)
        {
            var ret = new AuthorizationAPIResult()
            {
                Authorizations = FromJson<List<AuthorizationSignature>>(result.AuthorizationsJson),
                ResultCode = result.ResultCode,
                ServiceHash = result.ServiceHash
            };
            return ret;
        }
    }
}