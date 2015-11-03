using System;
using System.ComponentModel;
using System.Threading;
using SharedCardGame.ScgServiceLibrary;

namespace SharedCardGame
{
    /// <summary>
    /// Class ScgBroadcastorCallback.
    /// This class will implement the IScgBroadcastorServiceCallback interface on the WCF service, 
    /// so the service can callback to the client when an event occurs on another client.
    /// </summary>
    class ScgBroadcastorCallback : IScgBroadcastorServiceCallback
    {
        private SynchronizationContext syncContext = AsyncOperationManager.SynchronizationContext;

        private EventHandler _mBroadcastorCallBackHandler;
        public void SetHandler(EventHandler handler)
        {
            _mBroadcastorCallBackHandler = handler;
        }

        public void BroadcastToClient(StateChangeEventDataType stateChangeEventData)
        {
            syncContext.Post(new SendOrPostCallback(OnBroadcast), stateChangeEventData);
        }

        public void BroadcastChatMessageToClient(ChatMessageDataType chatMessageData)
        {
            syncContext.Post(new SendOrPostCallback(OnBroadcast), chatMessageData);
        }

        public void BroadcastBadProtocolVersion(int serverProtocolVersion)
        {
            syncContext.Post(new SendOrPostCallback(OnBroadcast), serverProtocolVersion);
        }

        private void OnBroadcast(object eventData)
        {
            _mBroadcastorCallBackHandler.Invoke(eventData, null);
        }

    }
}
