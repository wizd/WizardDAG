﻿using System;
using Microsoft.Extensions.Logging;
using GrpcServerHelper;
using Communication;
using Google.Protobuf;
using Lyra.Shared;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lyra.Node2
{
    public class MessageProcessor : MessageProcessorBase<RequestMessage, ResponseMessage>
    {
        //private Func<(string type, byte[] payload), Task> OnPayload;
        public event EventHandler<(string clientId, string type, byte[] payload)> OnPayload;

        public MessageProcessor(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        //public void RegisterPayloadHandler(Func<(string type, byte[] payload), Task> onPayload)
        //{
        //    OnPayload = onPayload;
        //}

        public override string GetClientId(RequestMessage message) => message.ClientId;

        // this default process becomes heartbeat.
        public override ResponseMessage Process(RequestMessage message)
        {
            //var stopwatch = Stopwatch.StartNew();
            //Logger.LogInformation($"To be processed: {message.Type} {message.MessageId.Shorten()} from {message.ClientId}");
            //switch (message.Type)
            //{
            //    case "AuthorizerPrePrepare":
            //    case "AuthorizerPrepare":
            //    case "AuthorizerCommit":
            //        OnPayload?.Invoke(this, (message.ClientId, message.Type, message.Payload.ToByteArray()));
            //        break;
            //}

            //stopwatch.Stop();
            //Logger.LogInformation($"To be processed (after payload): {message.Type} {message.MessageId.Shorten()} from {message.ClientId} OnPlayload uses: {stopwatch.ElapsedMilliseconds} ms");

            //
            // Request message processing should be placed here
            //

            OnPayload?.Invoke(this, (message.ClientId, message.Type, message.Payload.ToByteArray()));

            if (message.Response != ResponseType.Required)
                return null;
            
            return new ResponseMessage
            {
                ClientId = message.ClientId,
                MessageId = message.MessageId,
                Type = message.Type,
                Payload = ByteString.Empty,
                Status = MessageStatus.Processed,
            };
        }
    }
}
