using System.Drawing;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Windows.Forms;

namespace ScgServiceLibrary
{
    public enum MouseEventType
    {
        MouseDown,
        MouseUp,
        MouseMove
    }

    public enum ClientCommandType
    {
        ResetCmd,
        LeavingCmd,
        GatheringCmd,
        ExtractFirstCardCmd,
        ShuffleUpCmd,
        RequestDealingParametersCmd,
        CancelDealingParametersDialogCmd,
        DealCmd,
        CountCardsCmd,
        ContextualMenuClosedCmd
    }

    public enum StateChangeEventType
    {
        NoStateChange,
        NewSharedObject,
        DisposeSharedObject,
        SharedObjectMove,
        ShowGatheringRectangle,
        HideGatheringRectangle,
        OpenContextualMenuOnSharedObject,
        OpenDealingParametersDialog,
        ShowMessage
    }

    //[ServiceContract] //Use this attribute to test the service With WCF test client (without callback contract)
    [ServiceContract(CallbackContract = typeof (IScgBroadcastorCallBack))]
    // Use this attribute to host the service on IIS with the callback contract
    public interface IScgBroadcastorService
    {
        [OperationContract(IsOneWay = true)]
        void RegisterClient(string clientName, int clientProtocolVersion);

        [OperationContract(IsOneWay = true)]
        void NotifyServer(MouseEventDataType mouseEventData);

        [OperationContract(IsOneWay = true)]
        void NotifyServerWatchdog(WatchdogDataType watchdogData);

        [OperationContract(IsOneWay = true)]
        void NotifyServerCmd(ClientCommandDataType clientCommandData);

        [OperationContract(IsOneWay = true)]
        void NotifyServerChatMessage(ChatMessageDataType chatMessageData);
    }

    [DataContract]
    public class MouseEventDataType
    {
        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public MouseEventType MouseEventType { get; set; }

        [DataMember]
        public Point MouseLocation { get; set; }

        [DataMember]
        public MouseButtons MouseButton { get; set; }
    }

    [DataContract]
    public class WatchdogDataType
    {
        [DataMember]
        public string ClientName { get; set; }
    }

    [DataContract]
    public class ChatMessageDataType
    {
        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string ChatMessage { get; set; }
    }

    [DataContract]
    public class ClientCommandDataType
    {
        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public ClientCommandType ClientCommandType { get; set; }

        [DataMember]
        public Rectangle TargetRectangle { get; set; }

        [DataMember]
        public int SharedObjectId { get; set; }

        [DataMember]
        public bool DealingClockwise { get; set; }

        [DataMember]
        public int NumberOfCardsToDeal { get; set; }
    }


    [DataContract]
    public class StateChangeEventDataType
    {
        [DataMember]
        public StateChangeEventType StateChangeEventType { get; set; }

        [DataMember]
        public int SharedObjectId { get; set; }

        [DataMember]
        public Size SharedObjectSize { get; set; }

        [DataMember]
        public string PrivateOwnerClientName { get; set; }

        [DataMember]
        public string GrabbingClientName { get; set; }

        [DataMember]
        public string ReleasingClientName { get; set; }

        [DataMember]
        public Point NewSharedObjectLocation { get; set; }

        [DataMember]
        public string NewSharedObjectPicture { get; set; }

        [DataMember]
        public Rectangle GatheringRectangle { get; set; }

        [DataMember]
        public string MessageToDisplay { get; set; }

        [DataMember]
        public string MessageBoxTitle { get; set; }

        [DataMember]
        public string RecipientClientName { get; set; }

        [DataMember]
        public bool ToEveryone { get; set; }

        [DataMember]
        public bool DealingClockwise { get; set; }

        [DataMember]
        public int NumberOfCardsToDeal { get; set; }
    }

    public interface IScgBroadcastorCallBack
    {
        [OperationContract(IsOneWay = true)]
        void BroadcastToClient(StateChangeEventDataType stateChangeEventData);

        [OperationContract(IsOneWay = true)]
        void BroadcastChatMessageToClient(ChatMessageDataType chatMessageData);

        [OperationContract(IsOneWay = true)]
        void BroadcastBadProtocolVersion(int serverProtocolVersion);
    }
}