﻿using System;
using System.Collections.Generic;
using Lyra.Core.Blocks;
using Lyra.Core.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Lyra.Core.Utils;
using Lyra.Core.Accounts;
using Clifton.Blockchain;
using System.Linq;

namespace Lyra.Core.Authorizers
{
    public class ConsolidationBlockAuthorizer : BaseAuthorizer
    {
        public ConsolidationBlockAuthorizer()
        {
        }

        public async override Task<(APIResultCodes, AuthorizationSignature)> AuthorizeAsync<T>(T tblock, bool WithSign = true)
        {
            var result = await AuthorizeImplAsync(tblock);
            if (APIResultCodes.Success == result)
                return (APIResultCodes.Success, Sign(tblock));
            else
                return (result, (AuthorizationSignature)null);
        }

        protected override Task<APIResultCodes> ValidateFeeAsync(TransactionBlock block)
        {
            return Task.FromResult(APIResultCodes.Success);
        }

        private async Task<APIResultCodes> AuthorizeImplAsync<T>(T tblock)
        {
            if (!(tblock is ConsolidationBlock))
                return APIResultCodes.InvalidBlockType;

            var block = tblock as ConsolidationBlock;

            //// 1. check if the block already exists
            //if (null != await BlockChain.Singleton.GetBlockByUIndexAsync(block.UIndex))
            //    return APIResultCodes.BlockWithThisUIndexAlreadyExists;

            var lastCons = await BlockChain.Singleton.GetLastConsolidationBlockAsync();
            if(block.Height > 1)
            {
                if (lastCons == null)
                    return APIResultCodes.CouldNotFindLatestBlock;
            }

            var result = await VerifyBlockAsync(block, lastCons);
            if (result != APIResultCodes.Success)
                return result;

            // recalculate merkeltree
            // use merkle tree to consolidate all previous blocks, from lastCons.UIndex to consBlock.UIndex -1
            var mt = new MerkleTree();
            foreach(var hash in block.blockHashes)
            {
                mt.AppendLeaf(MerkleHash.Create(hash));
            }

            var mkhash = mt.BuildTree().ToString();

            if (block.MerkelTreeHash == mkhash)
            {
                return APIResultCodes.Success;
            }

            return APIResultCodes.InvalidConsolidationMerkleTreeHash;
        }

    }
}