﻿using EosSharp.Core.Interfaces;
using ScatterSharp.Core;
using ScatterSharp.Core.Api;
using ScatterSharp.Core.Helpers;
using ScatterSharp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScatterSharp.EosProvider
{
    public class ScatterSignatureProvider : ISignProvider
    {
        private IScatter Scatter { get; set; }

        public ScatterSignatureProvider(IScatter scatter)
        {
            Scatter = scatter;
        }

        public async Task<IEnumerable<string>> GetAvailableKeys()
        {
            var identity = await Scatter.GetIdentityFromPermissions();

            if (identity == null)
                throw new ArgumentNullException("identity");

            if (identity.accounts == null)
                throw new ArgumentNullException("identity.Accounts");

            return identity.accounts.Select(acc => acc.publicKey);
        }

        public async Task<IEnumerable<string>> Sign(string chainId, IEnumerable<string> requiredKeys, byte[] signBytes, IEnumerable<string> abiNames = null)
        {
            IEnumerable<AbiRequest> abis = null;

            if (abiNames != null)
                abis = abiNames.Select(a => new AbiRequest() { account_name = a });
            else
                abis = new List<AbiRequest>();

            var result = await Scatter.RequestSignature(new SignatureRequest()
            {
                network = Scatter.GetNetwork(),
                blockchain = ScatterConstants.Blockchains.EOSIO,
                requiredFields = new List<object>(), //TODO create concrete object for requiredFields
                transaction = new Transaction()
                {
                    abis = abis,
                    serializedTransaction = UtilsHelper.ByteArrayToHexString(signBytes),
                    chainId = chainId
                },
                origin = Scatter.GetAppName()
            });

            return result.signatures;
        }
    }
}
