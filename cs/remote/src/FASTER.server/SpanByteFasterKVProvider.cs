﻿using System.Net.Sockets;
using FASTER.common;
using FASTER.core;

namespace FASTER.server
{
    /// <summary>
    /// Session provider for FasterKV store based on
    /// [K, V, I, O, C] = [SpanByte, SpanByte, SpanByte, SpanByteAndMemory, long]
    /// </summary>
    public sealed class SpanByteFasterKVProvider : ISessionProvider
    {
        readonly StoreWrapper<SpanByte, SpanByte> storeWrapper;
        readonly SpanByteServerSerializer serializer;
        readonly SubscribeKVBroker<SpanByte, SpanByte, SpanByte, IKeyInputSerializer<SpanByte, SpanByte>> kvBroker;
        readonly SubscribeBroker<SpanByte, SpanByte, IKeySerializer<SpanByte>> broker;
        readonly MaxSizeSettings maxSizeSettings;

        /// <summary>
        /// Create SpanByte FasterKV backend
        /// </summary>
        /// <param name="store"></param>
        /// <param name="kvBroker"></param>
        /// <param name="broker"></param>
        /// <param name="tryRecover"></param>
        /// <param name="maxSizeSettings"></param>
        public SpanByteFasterKVProvider(FasterKV<SpanByte, SpanByte> store, SubscribeKVBroker<SpanByte, SpanByte, SpanByte, IKeyInputSerializer<SpanByte, SpanByte>> kvBroker = null, SubscribeBroker<SpanByte, SpanByte, IKeySerializer<SpanByte>> broker = null, bool tryRecover = true, MaxSizeSettings maxSizeSettings = default)
        {
            this.storeWrapper = new StoreWrapper<SpanByte, SpanByte>(store, tryRecover);
            this.kvBroker = kvBroker;
            this.broker = broker;
            this.serializer = new SpanByteServerSerializer();
            this.maxSizeSettings = maxSizeSettings ?? new MaxSizeSettings();
        }

        /// <inheritdoc />
        public IServerSession GetSession(WireFormat wireFormat, Socket socket)
        {
            switch (wireFormat)
            {
                case WireFormat.WebSocket:
                    return new WebsocketServerSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, SpanByteFunctionsForServer<long>, SpanByteServerSerializer>
                        (socket, storeWrapper.store, new SpanByteFunctionsForServer<long>(), serializer, maxSizeSettings, kvBroker, broker);
                default:
                    return new BinaryServerSession<SpanByte, SpanByte, SpanByte, SpanByteAndMemory, SpanByteFunctionsForServer<long>, SpanByteServerSerializer>
                        (socket, storeWrapper.store, new SpanByteFunctionsForServer<long>(), serializer, maxSizeSettings, kvBroker, broker);
            }
        }
    }
}