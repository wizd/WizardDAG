﻿using System.Collections.Generic;
using Lyra.Core.Blocks;
using Lyra.Core.Blocks.Transactions;

using Lyra.Core.Cryptography;
using Lyra.Core.API;
using Lyra.Core.Accounts.Node;

using System;
using Lyra.Node2.Services;
using Lyra.Core.Protos;
using Lyra.Core.Utils;
using System.Threading.Tasks;

namespace Lyra.Node2.Authorizers
{
    public delegate void AuthorizeCompleteEventHandler(object sender, AuthorizeCompletedEventArgs e);

    public class AuthorizeCompletedEventArgs : EventArgs
    {
        public Block Result { get; }
        public AuthorizeCompletedEventArgs(Block block)
        {
            Result = block;
        }
    }
    public abstract class BaseAuthorizer
    {
        protected readonly ServiceAccount _serviceAccount;
        protected readonly IAccountCollection _accountCollection;

        public static event AuthorizeCompleteEventHandler OnAuthorized;

        //public Authorizer(ServiceAccount serviceAccount, AccountCollection accountCollection)
        public BaseAuthorizer(ServiceAccount serviceAccount, IAccountCollection accountCollection)
        {
            _serviceAccount = serviceAccount;
            _accountCollection = accountCollection;
        }

        public virtual APIResultCodes Authorize<T>(ref T tblock)
        {
            OnAuthorized?.Invoke(this, new AuthorizeCompletedEventArgs(tblock as Block));

            return APIResultCodes.Success;
        }

        protected APIResultCodes VerifyBlock(TransactionBlock block, TransactionBlock previousBlock)
        {
            if (NodeGlobalParameters.Network_Id != block.NetworkId)
                return APIResultCodes.InvalidNetworkId;

            if (!block.IsBlockValid(previousBlock))
                return APIResultCodes.BlockValidationFailed;

            //if (!Signatures.VerifySignature(block.Hash, block.AccountID, block.Signature))
            //    return APIResultCodes.BlockSignatureValidationFailed;

            if (!block.VerifySignature(block.AccountID))
                return APIResultCodes.BlockSignatureValidationFailed;

            // check if this Index already exists (double-spending, kind of)
            if (_accountCollection.FindBlockByIndex(block.AccountID, block.Index) != null)
                return APIResultCodes.BlockWithThisIndexAlreadyExists;

            // This is the double-spending check for send block!
            if (!string.IsNullOrEmpty(block.PreviousHash) && _accountCollection.FindBlockByPreviousBlockHash(block.PreviousHash) != null)
                return APIResultCodes.BlockWithThisPreviousHashAlreadyExists;

            if (block.Index <= 0)
                return APIResultCodes.InvalidIndexSequence;

            if (block.Index > 1 && previousBlock == null)
                return APIResultCodes.CouldNotFindLatestBlock;

            if (block.Index == 1 && previousBlock != null)
                return APIResultCodes.InvalidIndexSequence;

            if (previousBlock != null && block.Index != previousBlock.Index + 1)
                return APIResultCodes.InvalidIndexSequence;

            if (!ValidateRenewalDate(block, previousBlock))
                return APIResultCodes.TokenExpired;

            return APIResultCodes.Success;
        }

        protected bool ValidateRenewalDate(TransactionBlock block, TransactionBlock previousBlock)
        {
            if (previousBlock == null)
                return true;

            var trs = block.GetTransaction(previousBlock);

            if (trs.Amount <= 0)
                return true;

            var token = _accountCollection.FindTokenGenesisBlock(null, trs.TokenCode);
            if (token != null)
                if (token.RenewalDate < DateTime.Now)
                    return false;

            return true;
        }

        // common validations for Send and Receive blocks
        protected APIResultCodes VerifyTransactionBlock(TransactionBlock block)
        {
            // Validate the account id
            if (!Signatures.ValidateAccountId(block.AccountID))
                return APIResultCodes.InvalidAccountId;

            if (!string.IsNullOrEmpty(block.PreviousHash)) // not for new account
            {
                // verify the entire account chain to make sure all account's blocks are valid
                TransactionBlock prevBlock, thisBlock = block;
                //while (thisBlock.BlockType != BlockTypes.OpenWithReceiveTransfer && thisBlock.BlockType != BlockTypes.OpenWithReceiveFee)
                while (!(thisBlock is IOpeningBlock))
                {
                    prevBlock = _accountCollection.FindBlockByHash(thisBlock.PreviousHash);
                    if (!thisBlock.IsBlockValid(prevBlock))
                        return APIResultCodes.AccountChainBlockValidationFailed;

                    if (!Signatures.VerifyAccountSignature(thisBlock.Hash, thisBlock.AccountID, thisBlock.Signature))
                        return APIResultCodes.AccountChainSignatureValidationFailed;

                    thisBlock = prevBlock;
                }

                // verify the spending
                TransactionBlock previousTransaction = _accountCollection.FindBlockByHash(block.PreviousHash);
                foreach (var prevbalance in previousTransaction.Balances)
                {
                    // make sure all balances from the previous block are present in a new block even if they are unchanged
                    if (!block.Balances.ContainsKey(prevbalance.Key))
                        return APIResultCodes.AccountChainBalanceValidationFailed;
                }

                // Verify fee
                if (block.BlockType == BlockTypes.SendTransfer)
                    if ((block as SendTransferBlock).Fee != _serviceAccount.GetLastServiceBlock().TransferFee)
                        return APIResultCodes.InvalidFeeAmount;

                if (block.BlockType == BlockTypes.TokenGenesis)
                    if ((block as TokenGenesisBlock).Fee != _serviceAccount.GetLastServiceBlock().TokenGenerationFee)
                        return APIResultCodes.InvalidFeeAmount;
            }

            var res = ValidateFee(block);
            if (res != APIResultCodes.Success)
                return res;

            return APIResultCodes.Success;
        }

        protected abstract APIResultCodes ValidateFee(TransactionBlock block);

        //protected virtual APIResultCodes ValidateFee(TransactionBlock block)
        //{
        //    if (block.Fee == 0 && block.FeeType != AuthorizationFeeTypes.NoFee)
        //        return APIResultCodes.InvalidFeeAmount;

        //    return APIResultCodes.Success;
        //}

        protected virtual APIResultCodes ValidateNonFungible(TransactionBlock send_or_receice_block, TransactionBlock previousBlock)
        {
            TransactionInfoEx transaction = send_or_receice_block.GetTransaction(previousBlock);

            if (transaction.TokenCode == LyraGlobal.LYRA_TICKER_CODE)
                return APIResultCodes.Success;

            var token_block = _accountCollection.FindTokenGenesisBlock(null, transaction.TokenCode);
            if (token_block == null)
                return APIResultCodes.TokenGenesisBlockNotFound;

            if (!token_block.IsNonFungible)
                return APIResultCodes.Success;

            //INonFungibleToken non_fungible_token = send_block.GetNonFungibleTransaction(previousBlock);

            if (send_or_receice_block.NonFungibleToken == null)
                return APIResultCodes.MissingNonFungibleToken;

            if (send_or_receice_block.NonFungibleToken.Denomination != transaction.Amount)
                return APIResultCodes.InvalidNonFungibleAmount;

            if (send_or_receice_block.NonFungibleToken.TokenCode != transaction.TokenCode)
                return APIResultCodes.InvalidNonFungibleTokenCode;

            if (!send_or_receice_block.NonFungibleToken.VerifySignature(token_block.NonFungibleKey))
                return APIResultCodes.NonFungibleSignatureVerificationFailed;

            return APIResultCodes.Success;
        }

        protected void Sign<T>(ref T tblock)
        {
            if (!(tblock is TransactionBlock))
                throw new System.ApplicationException("APIResultCodes.InvalidBlockType");

            var block = tblock as TransactionBlock;

            // ServiceHash is excluded when calculating the block hash,
            // but it is included when creating/validating the authorization signature
            block.ServiceHash = _serviceAccount.GetLatestBlock().Hash;

            // sign with the authorizer key
            AuthorizationSignature authSignature = new AuthorizationSignature
            {
                Key = _serviceAccount.AccountId,
                Signature = Signatures.GetSignature(_serviceAccount.PrivateKey, block.Hash + block.ServiceHash)
            };

            if (block.Authorizations == null)
                block.Authorizations = new List<AuthorizationSignature>();
            block.Authorizations.Add(authSignature);
        }

        protected bool VerifyAuthorizationSignatures(TransactionBlock block)
        {
            //block.ServiceHash = _serviceAccount.GetLatestBlock(block.ServiceHash);

            // TO DO - support multy nodes
            if (block.Authorizations == null || block.Authorizations.Count != 1)
                return false;

            if (block.Authorizations[0].Key != _serviceAccount.AccountId)
                return false;
                       
            return Signatures.VerifyAuthorizerSignature(block.Hash + block.ServiceHash, block.Authorizations[0].Key, block.Authorizations[0].Signature);

        }
    }
}
