using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Windows.Forms;

namespace ScgServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ScgBroadcastorService : IScgBroadcastorService
    {
        private const string EventlogSourceName = "SCG BrodcastorService";
        private const string EventlogLogName = "Application";
        private const int MaxClientCount = 10;
        private static readonly Dictionary<string, Client> Clients = new Dictionary<string, Client>();
        private static readonly object ClientsLocker = new object();
        private static readonly object GameStateLocker = new object();
        private static ScgGameState _gameState;
        public static string GatheringClient { get; private set; }        
        public static Point GatheringFromLocation { get; private set; }        
        public static Point GatheringToLocation { get; private set; }
        public static bool DealingClockwise { get; set; }
        public static int NumberOfCardsToDeal { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScgBroadcastorService"/> class.
        /// </summary>
        public ScgBroadcastorService()
        {
            // Prepare the EventLog:
            try
            {
                if (!EventLog.SourceExists(EventlogSourceName))
                {
                    // Warning: Requires to be run as Administrator to create the Event Source (once):
                    EventLog.CreateEventSource(EventlogSourceName, EventlogLogName);
                }
                EventLogActive = true;
            }
            catch (SecurityException)
            {
                // Was not run as administrator and source does not exist:
                EventLogActive = false;
            }

            DealingClockwise = true;
            NumberOfCardsToDeal = 1;
            LogInfo("Service started.", 0);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the event logging is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if event logging is active; otherwise, <c>false</c>.
        /// </value>
        private bool EventLogActive { get; set; }

        /// <summary>
        /// Registers the new client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="clientProtocolVersion"></param>
        public void RegisterClient(string clientName, int clientProtocolVersion)
        {
            if (!string.IsNullOrEmpty(clientName))
            {
                try
                {
                    var callback = OperationContext.Current.GetCallbackChannel<IScgBroadcastorCallBack>();
                    if (clientProtocolVersion != Common.ClientServerProtocolVersion)
                    {
                        callback.BroadcastBadProtocolVersion(Common.ClientServerProtocolVersion);
                        return;
                    }
                    lock (ClientsLocker)
                    {
                        if (Clients.Count <= MaxClientCount)
                        {
                            lock (GameStateLocker)
                            {
                                LogInfo(string.Format("RegisterClient(clientName = '{0}')", clientName), 1);
                                var removedPrivateAreaId = -1;
                                //Remove the old client using the same name:
                                if (Clients.Keys.Contains(clientName))
                                {
                                    // The new client uses an already registerd name. Refuse it:
                                    return;
                                }
                                //{
                                //    Clients.Remove(clientName);
                                //    LogWarning(string.Format("_clients.Remove(clientName = '{0}')", clientName), 2);
                                //    // Remove the private area of the old client
                                //    removedPrivateAreaId = _gameState.RemovePrivateArea(clientName);
                                //    // Note: The removedSharedObjectId should be >= 0
                                //    if (removedPrivateAreaId >= 0)
                                //    {
                                //        // Inform other existing clients of the disposed PrivateArea id:
                                //        foreach (var client in Clients)
                                //        {
                                //            // Prepare StateChangeEventDataType:
                                //            var stateChangeEventDataType =
                                //                new StateChangeEventDataType();
                                //            stateChangeEventDataType.StateChangeEventType =
                                //                StateChangeEventType.DisposeSharedObject;
                                //            stateChangeEventDataType.SharedObjectId = removedPrivateAreaId;
                                //            // Send the StateChangeEventDataType to the client:
                                //            client.Value.ScgBroadcastorCallBack.BroadcastToClient(
                                //                stateChangeEventDataType);
                                //            LogInfo(
                                //                string.Format(
                                //                    "BroadcastToClient'{0}'(DisposeSharedObject, removedPrivateAreaId = '{1}')",
                                //                    client.Value.Name, removedPrivateAreaId), 3);
                                //        }
                                //    }
                                //}
                                Clients.Add(clientName, new Client(clientName, callback));
                                LogInfo(string.Format("_clients.Add(clientName = '{0}')", clientName), 4);

                                if (Clients.Count == 1)
                                {
                                    // Initialize a new game state
                                    _gameState = new ScgGameState(this);
                                }
                                bool newSharedObject;
                                var privateArea = _gameState.CreatePrivateArea(clientName, out newSharedObject);

                                // Search a possible client without private area (The registering client was previously already registered under another name we do not know)
                                // WARNING: This now shouldn't occur since the registered clients will now hide the registering pane: 
                                foreach (var client in Clients)
                                {
                                    if (_gameState.FindPrivateArea(client.Value.Name) == null)
                                    {
                                        var oldClientName = client.Value.Name;
                                        // Remove this orphan client corresponding to the old name of the registering client:
                                        Clients.Remove(oldClientName);
                                        LogWarning(
                                            string.Format("_clients.Remove(oldClientName = '{0}')", oldClientName), 5);

                                        // Inform other existing clients of the disposed PrivateArea id:
                                        foreach (var cli in Clients)
                                        {
                                            if (cli.Value.Name != clientName)
                                            {
                                                // Prepare StateChangeEventDataType:
                                                var stateChangeEventDataType =
                                                    new StateChangeEventDataType();
                                                stateChangeEventDataType.StateChangeEventType =
                                                    StateChangeEventType.DisposeSharedObject;
                                                stateChangeEventDataType.PrivateOwnerClientName = oldClientName;
                                                stateChangeEventDataType.SharedObjectId = removedPrivateAreaId;
                                                // Send the StateChangeEventDataType to the client:
                                                cli.Value.ScgBroadcastorCallBack.BroadcastToClient(
                                                    stateChangeEventDataType);
                                                LogInfo(
                                                    string.Format(
                                                        "BroadcastToClient'{0}'(DisposeSharedObject, removedPrivateAreaId = '{1}')",
                                                        cli.Value.Name, removedPrivateAreaId), 6);
                                            }
                                        }
                                        break;
                                    }
                                }

                                // Inform other existing clients of new PrivateArea:
                                foreach (var client in Clients)
                                {
                                    if (client.Value.Name != clientName)
                                    {
                                        // Prepare StateChangeEventDataType:
                                        var stateChangeEventDataType =
                                            new StateChangeEventDataType();
                                        stateChangeEventDataType.StateChangeEventType =
                                            StateChangeEventType.NewSharedObject;
                                        stateChangeEventDataType.SharedObjectId = privateArea.Id;
                                        stateChangeEventDataType.PrivateOwnerClientName = privateArea.PrivateOwnerName;
                                        stateChangeEventDataType.NewSharedObjectLocation =
                                            new Point(privateArea.Rectangle.Left, privateArea.Rectangle.Top);
                                        stateChangeEventDataType.SharedObjectSize =
                                            new Size(privateArea.Rectangle.Size.Width, privateArea.Rectangle.Size.Height);
                                        stateChangeEventDataType.NewSharedObjectPicture = privateArea.Picture;
                                        // Send the StateChangeEventDataType to the client:
                                        client.Value.ScgBroadcastorCallBack.BroadcastToClient(stateChangeEventDataType);
                                        LogInfo(
                                            string.Format(
                                                "BroadcastToClient'{0}'(NewSharedObject, privateArea.Id = '{1}')",
                                                client.Value.Name, privateArea.Id), 7);
                                    }
                                }

                                // Inform the new client (and only him) of the current state of the game:
                                Client newClient;
                                var foundNewClient = Clients.TryGetValue(clientName, out newClient);
                                if (foundNewClient && newClient != null)
                                {
                                    // Parse all shared objects in reversed order (from the back to the topmost one shared object):
                                    for (var i = _gameState.ZOrderListOfSharedObjects.Count - 1; i >= 0; i--)
                                    {
                                        var sharedObject = _gameState.ZOrderListOfSharedObjects[i];

                                        // Prepare StateChangeEventDataType:
                                        var stateChangeEventDataType = new StateChangeEventDataType();
                                        stateChangeEventDataType.StateChangeEventType =
                                            StateChangeEventType.NewSharedObject;
                                        stateChangeEventDataType.SharedObjectId = sharedObject.Id;
                                        stateChangeEventDataType.PrivateOwnerClientName = sharedObject.PrivateOwnerName;
                                        stateChangeEventDataType.NewSharedObjectLocation =
                                            new Point(sharedObject.Rectangle.Left, sharedObject.Rectangle.Top);
                                        stateChangeEventDataType.SharedObjectSize =
                                            new Size(sharedObject.Rectangle.Size.Width,
                                                sharedObject.Rectangle.Size.Height);
                                        stateChangeEventDataType.NewSharedObjectPicture = sharedObject.Picture;
                                        // Send the StateChangeEventDataType to the new (and only) client:
                                        newClient.ScgBroadcastorCallBack.BroadcastToClient(stateChangeEventDataType);
                                        LogInfo(
                                            string.Format(
                                                "BroadcastToClient'{0}'(NewSharedObject, sharedObject.Id = '{1}')",
                                                newClient, sharedObject.Id), 8);
                                    }
                                }
                            }
                        }
                        else
                        {
                            LogWarning(
                                string.Format(
                                    "Refused to RegisterClient(clientName = '{0}') because the _clients list is full. ({1} clients)",
                                    clientName, Clients.Count), 1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Unexpected exception:
                    LogWarning(string.Format("Unexpected exception ex.Message = '{0}' in RegisterClient)", ex.Message), 9);
                    // Throw it to restart the service (in case of service channel canceled)
                    throw;
                }
            }
        }

        /// <summary>
        /// Notifies the server of a MouseEvent on the client.
        /// (Will be remotely called by the clients)
        /// </summary>
        /// <param name="mouseEventData">The mouse event data.</param>
        public void NotifyServer(MouseEventDataType mouseEventData)
        {
            // Handle the received mouseEventData and update the State of the Shared Card Game:
            var stateChangeEventDataType = new StateChangeEventDataType();
            Client theClient;
            bool foundTheClient;
            SharedObject clickedSharedObject;
            string otherFaceCardPicture = null;
            SharedObject enclosingPrivateArea = null;
            lock (GameStateLocker)
            {
                switch (mouseEventData.MouseEventType)
                {
                    case MouseEventType.MouseDown:
                        if (mouseEventData.MouseButton == MouseButtons.Left)
                        {
                            clickedSharedObject = _gameState.GetSharedObjectAt(mouseEventData.MouseLocation);
                            foundTheClient = Clients.TryGetValue(mouseEventData.ClientName, out theClient);

                            if (clickedSharedObject != null && clickedSharedObject.GrabberName == null
                                &&
                                (clickedSharedObject.PrivateOwnerName == null ||
                                 clickedSharedObject.PrivateOwnerName == mouseEventData.ClientName)
                                &&
                                !_gameState.IsInOtherClientPrivateAreaAt(mouseEventData.MouseLocation,
                                    mouseEventData.ClientName)
                                &&
                                (clickedSharedObject.PrivateOwnerName == null ||
                                 !_gameState.PrivateAreaIsNotEmpty(clickedSharedObject)))
                            {
                                LogInfo("LeftClick accepted", 999);

                                if (foundTheClient && theClient != null && theClient.GrabbedSharedObject == null)
                                {
                                    //  Set client grabbing data:
                                    theClient.GrabbedSharedObject = clickedSharedObject;
                                    theClient.GrabbedRelativePosition =
                                        new Point(mouseEventData.MouseLocation.X - clickedSharedObject.Location.X,
                                            mouseEventData.MouseLocation.Y - clickedSharedObject.Location.Y);
                                    // Set the clicked object data:
                                    clickedSharedObject.GrabberName = mouseEventData.ClientName;
                                    clickedSharedObject.GrabbedLocation = clickedSharedObject.Location;
                                    clickedSharedObject.BringToFront();
                                    stateChangeEventDataType.StateChangeEventType =
                                        StateChangeEventType.SharedObjectMove;
                                    stateChangeEventDataType.SharedObjectId = clickedSharedObject.Id;
                                    stateChangeEventDataType.GrabbingClientName = mouseEventData.ClientName;
                                    stateChangeEventDataType.ReleasingClientName = null;
                                    stateChangeEventDataType.PrivateOwnerClientName =
                                        clickedSharedObject.PrivateOwnerName;
                                    stateChangeEventDataType.NewSharedObjectLocation = clickedSharedObject.Location;
                                    stateChangeEventDataType.SharedObjectSize = clickedSharedObject.Rectangle.Size;
                                    
                                    // Prepare the shown picture of cards according to the card location on Private Areas
                                    enclosingPrivateArea = _gameState.FindEnclosingPrivateArea(clickedSharedObject);
                                    var clickedSharedObjectTopElement = clickedSharedObject.GetTopElement();
                                    if (clickedSharedObjectTopElement != null)
                                    {
                                        if (enclosingPrivateArea != null && clickedSharedObject.GameElements.Count == 1 &&
                                            clickedSharedObjectTopElement is Card)
                                        {
                                            // The grabbedSharedObject is a Card and is completely inside a PrivateArea
                                            if ((clickedSharedObjectTopElement as Card).FaceUp)
                                            {
                                                stateChangeEventDataType.NewSharedObjectPicture =
                                                    (clickedSharedObjectTopElement as Card).RectoPicture;
                                                otherFaceCardPicture =
                                                    (clickedSharedObjectTopElement as Card).VersoPicture;
                                            }
                                            else
                                            {
                                                stateChangeEventDataType.NewSharedObjectPicture =
                                                    (clickedSharedObjectTopElement as Card).VersoPicture;
                                                otherFaceCardPicture =
                                                    (clickedSharedObjectTopElement as Card).RectoPicture;
                                            }
                                        }
                                        else
                                        {
                                            // Regular case
                                            enclosingPrivateArea = null;
                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                clickedSharedObject.Picture;
                                        }
                                    }
                                }
                            }
                            else if (clickedSharedObject == null && GatheringClient == null && foundTheClient && theClient != null && theClient.GrabbedSharedObject == null)
                            { 
                                GatheringClient = mouseEventData.ClientName;
                                GatheringFromLocation = mouseEventData.MouseLocation;
                                GatheringToLocation = mouseEventData.MouseLocation;
                            }
                        }
                        else if (mouseEventData.MouseButton == MouseButtons.Right)
                        {
                            clickedSharedObject = _gameState.GetSharedObjectAt(mouseEventData.MouseLocation);
                            // Check that the clicked shared object is free:
                            if (clickedSharedObject != null && clickedSharedObject.GrabberName == null
                                &&
                                !_gameState.IsInOtherClientPrivateAreaAt(mouseEventData.MouseLocation,
                                    mouseEventData.ClientName))
                            {
                                LogInfo(string.Format("RightClick on free sharedObject Id={0}", clickedSharedObject.Id), 19);

                                if (clickedSharedObject.GameElements.Count == 1)
                                {
                                    // Right click  on a non stacked shared object:
                                    if (clickedSharedObject.GetTopElement() is Card)
                                    {
                                        // Right click on a non stacked card:
                                        var card = clickedSharedObject.GetTopElement() as Card;
                                        if (card != null)
                                        {

                                            foundTheClient = Clients.TryGetValue(mouseEventData.ClientName,
                                                out theClient);
                                            if (foundTheClient && theClient != null &&
                                                theClient.GrabbedSharedObject == null)
                                            {
                                                card.Flip();

                                                //  Set client grabbing data:
                                                theClient.GrabbedSharedObject = clickedSharedObject;
                                                theClient.GrabbedRelativePosition =
                                                    new Point(
                                                        mouseEventData.MouseLocation.X - clickedSharedObject.Location.X,
                                                        mouseEventData.MouseLocation.Y - clickedSharedObject.Location.Y);
                                                // Set the clicked object data:
                                                clickedSharedObject.GrabberName = mouseEventData.ClientName;
                                                clickedSharedObject.GrabbedLocation = clickedSharedObject.Location;
                                                clickedSharedObject.BringToFront();
                                                stateChangeEventDataType.StateChangeEventType =
                                                    StateChangeEventType.SharedObjectMove;
                                                stateChangeEventDataType.SharedObjectId = clickedSharedObject.Id;
                                                stateChangeEventDataType.PrivateOwnerClientName =
                                                    clickedSharedObject.PrivateOwnerName;
                                                stateChangeEventDataType.GrabbingClientName = mouseEventData.ClientName;
                                                stateChangeEventDataType.ReleasingClientName = null;
                                                stateChangeEventDataType.NewSharedObjectLocation =
                                                    clickedSharedObject.Location;
                                                stateChangeEventDataType.SharedObjectSize =
                                                    clickedSharedObject.Rectangle.Size;

                                                // Prepare the shown picture of cards according to the card location on Private Areas
                                                var clickedSharedObjectTopElement = clickedSharedObject.GetTopElement();
                                                if (clickedSharedObjectTopElement != null)
                                                {
                                                    enclosingPrivateArea =
                                                        _gameState.FindEnclosingPrivateArea(clickedSharedObject);
                                                    if (enclosingPrivateArea != null &&
                                                        clickedSharedObject.GameElements.Count == 1 &&
                                                        clickedSharedObjectTopElement is Card)
                                                    {
                                                        // The clickedSharedObject is a Card and is completely inside a PrivateArea
                                                        if ((clickedSharedObjectTopElement as Card).FaceUp)
                                                        {
                                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                                (clickedSharedObjectTopElement as Card)
                                                                    .RectoPicture;
                                                            otherFaceCardPicture =
                                                                (clickedSharedObjectTopElement as Card)
                                                                    .VersoPicture;
                                                        }
                                                        else
                                                        {
                                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                                (clickedSharedObjectTopElement as Card)
                                                                    .VersoPicture;
                                                            otherFaceCardPicture =
                                                                (clickedSharedObjectTopElement as Card)
                                                                    .RectoPicture;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Regular case
                                                        enclosingPrivateArea = null;
                                                        stateChangeEventDataType.NewSharedObjectPicture =
                                                            clickedSharedObject.Picture;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Right click on a stacked shared object:
                                    if (clickedSharedObject.GameElements.First() is Card)
                                    {
                                        // Right click on a stack of cards:
                                        // Grab the card and open contextual menu:

                                        // Check that the client is not already grabbing another object:
                                        foundTheClient = Clients.TryGetValue(mouseEventData.ClientName,
                                                out theClient);
                                        if (foundTheClient && theClient != null &&
                                            theClient.GrabbedSharedObject == null)
                                        {
                                            // Prepare and send state change "OpenContextualMenuOnSharedObject" for the clicked shared object:
                                            StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                            stateChange.StateChangeEventType =
                                                StateChangeEventType.OpenContextualMenuOnSharedObject;
                                            stateChange.SharedObjectId = clickedSharedObject.Id;
                                            stateChange.GrabbingClientName = mouseEventData.ClientName;
                                            stateChange.PrivateOwnerClientName = null;
                                            LogInfo(
                                                string.Format(
                                                    "Broadcasting stateChange (OpenContextualMenuOnSharedObject) SharedObjectId={0}",
                                                    stateChange.SharedObjectId), 39);
                                            BroadcastStateChange(stateChange, null, null);


                                            // Mark the clicked object as grabbed for ContextualMenu opened:
                                            clickedSharedObject.ContextualMenuOpened = true;

                                            // Prepare state change "SharedObjectMove"
                                            // --- From here, code to refactorise with mouse down -----------
                                            //  Set client grabbing data:
                                            theClient.GrabbedSharedObject = clickedSharedObject;
                                            theClient.GrabbedRelativePosition =
                                                new Point(mouseEventData.MouseLocation.X - clickedSharedObject.Location.X,
                                                    mouseEventData.MouseLocation.Y - clickedSharedObject.Location.Y);
                                            // Set the clicked object data:
                                            clickedSharedObject.GrabberName = mouseEventData.ClientName;
                                            clickedSharedObject.GrabbedLocation = clickedSharedObject.Location;
                                            clickedSharedObject.BringToFront();
                                            stateChangeEventDataType.StateChangeEventType =
                                                StateChangeEventType.SharedObjectMove;
                                            stateChangeEventDataType.SharedObjectId = clickedSharedObject.Id;
                                            stateChangeEventDataType.GrabbingClientName = mouseEventData.ClientName;
                                            stateChangeEventDataType.ReleasingClientName = null;
                                            stateChangeEventDataType.PrivateOwnerClientName =
                                                clickedSharedObject.PrivateOwnerName;
                                            stateChangeEventDataType.NewSharedObjectLocation = clickedSharedObject.Location;
                                            stateChangeEventDataType.SharedObjectSize = clickedSharedObject.Rectangle.Size;

                                            // Prepare the shown picture of cards according to the card location on Private Areas
                                            enclosingPrivateArea = _gameState.FindEnclosingPrivateArea(clickedSharedObject);
                                            var clickedSharedObjectTopElement = clickedSharedObject.GetTopElement();
                                            if (clickedSharedObjectTopElement != null)
                                            {
                                                if (enclosingPrivateArea != null && clickedSharedObject.GameElements.Count == 1 &&
                                                    clickedSharedObjectTopElement is Card)
                                                {
                                                    // The grabbedSharedObject is a Card and is completely inside a PrivateArea
                                                    if ((clickedSharedObjectTopElement as Card).FaceUp)
                                                    {
                                                        stateChangeEventDataType.NewSharedObjectPicture =
                                                            (clickedSharedObjectTopElement as Card).RectoPicture;
                                                        otherFaceCardPicture =
                                                            (clickedSharedObjectTopElement as Card).VersoPicture;
                                                    }
                                                    else
                                                    {
                                                        stateChangeEventDataType.NewSharedObjectPicture =
                                                            (clickedSharedObjectTopElement as Card).VersoPicture;
                                                        otherFaceCardPicture =
                                                            (clickedSharedObjectTopElement as Card).RectoPicture;
                                                    }
                                                }
                                                else
                                                {
                                                    // Regular case
                                                    enclosingPrivateArea = null;
                                                    stateChangeEventDataType.NewSharedObjectPicture =
                                                        clickedSharedObject.Picture;
                                                }
                                            }
                                            // --- Till here, code to refactorise with mouse down -----------
                                        }
                                    }
                                }                               
                            }
                        }
                        break;
                    case MouseEventType.MouseMove:
                        if (mouseEventData.MouseButton == MouseButtons.Left)
                        {
                            foundTheClient = Clients.TryGetValue(mouseEventData.ClientName, out theClient);
                            if (foundTheClient && theClient != null && theClient.GrabbedSharedObject != null 
                                && !theClient.GrabbedSharedObject.ContextualMenuOpened )
                            {
                                var grabbedSharedObject = theClient.GrabbedSharedObject;

                                // Set the clicked object data:
                                grabbedSharedObject.MoveTo(
                                    new Point(mouseEventData.MouseLocation.X - theClient.GrabbedRelativePosition.X,
                                        mouseEventData.MouseLocation.Y - theClient.GrabbedRelativePosition.Y));
                                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChangeEventDataType.SharedObjectId = grabbedSharedObject.Id;
                                stateChangeEventDataType.GrabbingClientName = mouseEventData.ClientName;
                                stateChangeEventDataType.ReleasingClientName = null;
                                stateChangeEventDataType.PrivateOwnerClientName =
                                    grabbedSharedObject.PrivateOwnerName;
                                stateChangeEventDataType.NewSharedObjectLocation = grabbedSharedObject.Location;
                                stateChangeEventDataType.SharedObjectSize = grabbedSharedObject.Rectangle.Size;

                                // Prepare the shown picture of cards according to the card location on Private Areas
                                var grabbedSharedObjectTopElement = grabbedSharedObject.GetTopElement();
                                if (grabbedSharedObjectTopElement != null)
                                {
                                    enclosingPrivateArea = _gameState.FindEnclosingPrivateArea(grabbedSharedObject);
                                    if (enclosingPrivateArea != null && grabbedSharedObject.GameElements.Count == 1 &&
                                        grabbedSharedObjectTopElement is Card)
                                    {
                                        // The grabbedSharedObject is a Card and is completely inside a PrivateArea
                                        if ((grabbedSharedObjectTopElement as Card).FaceUp)
                                        {
                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                (grabbedSharedObjectTopElement as Card).RectoPicture;
                                            otherFaceCardPicture = grabbedSharedObject.GrabberName !=
                                                                   enclosingPrivateArea.PrivateOwnerName ||
                                                                   !enclosingPrivateArea.Rectangle.Contains(
                                                                       new Rectangle(
                                                                           grabbedSharedObject.GrabbedLocation.X,
                                                                           grabbedSharedObject.GrabbedLocation.Y,
                                                                           grabbedSharedObject.Rectangle.Width,
                                                                           grabbedSharedObject.Rectangle.Height))
                                                ? null
                                                : (grabbedSharedObjectTopElement as Card).VersoPicture;
                                        }
                                        else
                                        {
                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                (grabbedSharedObjectTopElement as Card).VersoPicture;
                                            otherFaceCardPicture = grabbedSharedObject.GrabberName !=
                                                                   enclosingPrivateArea.PrivateOwnerName ||
                                                                   !enclosingPrivateArea.Rectangle.Contains(
                                                                       new Rectangle(
                                                                           grabbedSharedObject.GrabbedLocation.X,
                                                                           grabbedSharedObject.GrabbedLocation.Y,
                                                                           grabbedSharedObject.Rectangle.Width,
                                                                           grabbedSharedObject.Rectangle.Height))
                                                ? null
                                                : (grabbedSharedObjectTopElement as Card).RectoPicture;
                                        }
                                    }
                                    else
                                    {
                                        // Regular case
                                        enclosingPrivateArea = null;
                                        stateChangeEventDataType.NewSharedObjectPicture = grabbedSharedObject.Picture;
                                    }
                                }
                            }
                            else if (GatheringClient == mouseEventData.ClientName)
                            {
                                GatheringToLocation = mouseEventData.MouseLocation;
                                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.ShowGatheringRectangle;
                                stateChangeEventDataType.GatheringRectangle = new Rectangle(Math.Min(GatheringFromLocation.X, GatheringToLocation.X),
                                    Math.Min(GatheringFromLocation.Y, GatheringToLocation.Y),
                                    Math.Abs(GatheringToLocation.X - GatheringFromLocation.X),
                                    Math.Abs(GatheringToLocation.Y - GatheringFromLocation.Y));
                            }
                        }
                        break;
                    case MouseEventType.MouseUp:
                        if (mouseEventData.MouseButton == MouseButtons.Left || mouseEventData.MouseButton == MouseButtons.Right)
                        {
                            foundTheClient = Clients.TryGetValue(mouseEventData.ClientName, out theClient);
                            if (foundTheClient && theClient != null && theClient.GrabbedSharedObject != null 
                                && !theClient.GrabbedSharedObject.ContextualMenuOpened)
                            {
                                var grabbedSharedObject = theClient.GrabbedSharedObject;
                                //  Set client grabbing data:
                                theClient.GrabbedSharedObject = null;
                                theClient.GrabbedRelativePosition = new Point(0, 0);

                                // Set the clicked object data:
                                grabbedSharedObject.GrabberName = null;

                                // Change the object location on the next magnetic grid step:
                                grabbedSharedObject.MoveTo(ScgGameState.GetMagneticLocation(grabbedSharedObject.Rectangle));

                                // Test if shared object is released outside the game field:
                                if (!new Rectangle(0, 0, ScgGameState.GamefieldDimX, ScgGameState.GamefieldDimY)
                                        .Contains(grabbedSharedObject.Rectangle))
                                {
                                    // If so, put it back at its grabbed location:
                                    grabbedSharedObject.MoveTo(grabbedSharedObject.GrabbedLocation);
                                }
                                grabbedSharedObject.BringToFront();

                                // Handle stacking
                                SharedObject stackObject;
                                if (_gameState.TryToStackOnAnotherSharedObject(grabbedSharedObject, out stackObject))
                                {
                                    // Prepare stateChangeEventType of type DisposeSharedObject:
                                    stateChangeEventDataType.StateChangeEventType = StateChangeEventType.DisposeSharedObject;
                                    stateChangeEventDataType.SharedObjectId = grabbedSharedObject.Id;
                                    stateChangeEventDataType.PrivateOwnerClientName =
                                        grabbedSharedObject.PrivateOwnerName;
                                    stateChangeEventDataType.SharedObjectSize = grabbedSharedObject.Rectangle.Size;
                                    stateChangeEventDataType.GrabbingClientName = null;
                                    stateChangeEventDataType.NewSharedObjectLocation = grabbedSharedObject.Location;
                                    stateChangeEventDataType.NewSharedObjectPicture = null;
                                    stateChangeEventDataType.ReleasingClientName = grabbedSharedObject.GrabberName;
                                    //Broadcast the DisposeSharedObject stateChangeEventType:
                                    BroadcastStateChange(stateChangeEventDataType, null, null);
                                    // The grabbed object is now the stacked object:
                                    grabbedSharedObject = stackObject;
                                }

                                // Prepare the stateChangeEventType to be sent:
                                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChangeEventDataType.SharedObjectId = grabbedSharedObject.Id;
                                stateChangeEventDataType.GrabbingClientName = null;
                                stateChangeEventDataType.ReleasingClientName = mouseEventData.ClientName;
                                stateChangeEventDataType.PrivateOwnerClientName =
                                    grabbedSharedObject.PrivateOwnerName;
                                stateChangeEventDataType.NewSharedObjectLocation = grabbedSharedObject.Location;
                                stateChangeEventDataType.SharedObjectSize = grabbedSharedObject.Rectangle.Size;

                                // Prepare the shown picture of cards according to the card location on Private Areas
                                var grabbedSharedObjectTopElement = grabbedSharedObject.GetTopElement();
                                if (grabbedSharedObjectTopElement != null)
                                {
                                    enclosingPrivateArea = _gameState.FindEnclosingPrivateArea(grabbedSharedObject);
                                    if (enclosingPrivateArea != null && grabbedSharedObject.GameElements.Count == 1 &&
                                        grabbedSharedObjectTopElement is Card)
                                    {
                                        // The grabbedSharedObject is a Card and is completely inside a PrivateArea
                                        if ((grabbedSharedObjectTopElement as Card).FaceUp)
                                        {
                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                (grabbedSharedObjectTopElement as Card).RectoPicture;
                                            otherFaceCardPicture =
                                                (grabbedSharedObjectTopElement as Card).VersoPicture;
                                        }
                                        else
                                        {
                                            stateChangeEventDataType.NewSharedObjectPicture =
                                                (grabbedSharedObjectTopElement as Card).VersoPicture;
                                            otherFaceCardPicture =
                                                (grabbedSharedObjectTopElement as Card).RectoPicture;
                                        }
                                    }
                                    else
                                    {
                                        // Regular case
                                        enclosingPrivateArea = null;
                                        stateChangeEventDataType.NewSharedObjectPicture = grabbedSharedObject.Picture;
                                    }
                                }
                            }
                            else if (GatheringClient == mouseEventData.ClientName)
                            {
                                GatheringClient = null;
                                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.HideGatheringRectangle;
                                stateChangeEventDataType.GatheringRectangle = new Rectangle(Math.Min(GatheringFromLocation.X, GatheringToLocation.X),
                                    Math.Min(GatheringFromLocation.Y, GatheringToLocation.Y),
                                    Math.Abs(GatheringToLocation.X - GatheringFromLocation.X),
                                    Math.Abs(GatheringToLocation.Y - GatheringFromLocation.Y));
                            }
                        }
                        break;
                }

                // if required, Broadcast The state change to each client:
                BroadcastStateChange(stateChangeEventDataType, enclosingPrivateArea, otherFaceCardPicture);
            }
        }

        /// <summary>
        /// Broadcasts the state change to all clients.
        /// </summary>
        /// <param name="stateChangeEventDataType">Type of the state change event data.</param>
        /// <param name="enclosingPrivateArea">The enclosing private area.</param>
        /// <param name="otherFaceCardPicture">The other face card picture.</param>
        public void BroadcastStateChange(StateChangeEventDataType stateChangeEventDataType, SharedObject enclosingPrivateArea, string otherFaceCardPicture)
        {
            // if required, Broadcast The state change to each client:
            if (stateChangeEventDataType.StateChangeEventType != StateChangeEventType.NoStateChange)
            {
                lock (ClientsLocker)
                {
                    var inactiveClients = new List<string>();
                    foreach (var client in Clients)
                    {
                        try
                        {
                            // Change the NewSharedObjectPicture information for card on private area for the PrivateAreaOwner client only:
                            if (stateChangeEventDataType.StateChangeEventType == StateChangeEventType.SharedObjectMove
                                && enclosingPrivateArea != null
                                && otherFaceCardPicture != null
                                && client.Value.Name == enclosingPrivateArea.PrivateOwnerName)
                            {
                                var savedPicture = stateChangeEventDataType.NewSharedObjectPicture;
                                stateChangeEventDataType.NewSharedObjectPicture = otherFaceCardPicture;
                                (client.Value).ScgBroadcastorCallBack.BroadcastToClient(stateChangeEventDataType);
                                stateChangeEventDataType.NewSharedObjectPicture = savedPicture;
                            }
                            else
                            {
                                (client.Value).ScgBroadcastorCallBack.BroadcastToClient(stateChangeEventDataType);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(string.Format("Exception '{0}' raised in ScgBroadcastorService.BroadcastStateChange when calling (client.Value).ScgBroadcastorCallBack.BroadcastToClient(). client.Value.Name = '{1}')", ex.Message , client.Value.Name), 99);
                            inactiveClients.Add(client.Key);
                        }
                    }
                    if (inactiveClients.Count > 0)
                    {
                        foreach (var inactiveClient in inactiveClients)
                        {
                            // Search the leavingClient (Client) corresponding to the inactiveClient (name):
                            Client leavingClient;
                            var foundTheClient = Clients.TryGetValue(inactiveClient, out leavingClient);
                            if (foundTheClient)
                            {
                                // Free a possible grabbed object by the leaving client:
                                if (leavingClient.GrabbedSharedObject != null)
                                {
                                    leavingClient.GrabbedSharedObject.GrabberName = null;
                                    leavingClient.GrabbedSharedObject.ContextualMenuOpened = false;
                                }
                            }

                            // Remove the inactive client from the clients list:
                            Clients.Remove(inactiveClient);
                            LogInfo(string.Format("_clients.Remove(inactiveClient = '{0}')", inactiveClient), 13);

                            // Remove the private area of the inactive client
                            var removedSharedObjectId = _gameState.RemovePrivateArea(inactiveClient);
                            // Note: The removedSharedObjectId should be >= 0
                            if (removedSharedObjectId >= 0)
                            {
                                // Inform other existing clients of the disposed PrivateArea id:
                                foreach (var client in Clients)
                                {
                                    // Prepare StateChangeEventDataType:
                                    var aStateChangeEventDataType =
                                        new StateChangeEventDataType();
                                    aStateChangeEventDataType.StateChangeEventType =
                                        StateChangeEventType.DisposeSharedObject;
                                    aStateChangeEventDataType.SharedObjectId = removedSharedObjectId;
                                    stateChangeEventDataType.PrivateOwnerClientName = inactiveClient;
                                    // Send the StateChangeEventDataType to the client:
                                    client.Value.ScgBroadcastorCallBack.BroadcastToClient(
                                        aStateChangeEventDataType);
                                    LogInfo(
                                        string.Format(
                                            "BroadcastToClient'{0}'(DisposeSharedObject, removedSharedObjectId = '{1}')",
                                            client.Value.Name, removedSharedObjectId), 14);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the server of watchdog event.
        /// (Will be remotely called by the clients)
        /// </summary>
        /// <param name="watchdogData">The watchdog data.</param>
        public void NotifyServerWatchdog(WatchdogDataType watchdogData)
        {
            // Nothing to do. (The watchdog notification is only called regularly to prevent disconnection of the channel)
        }

        /// <summary>
        /// Notifies the server command.
        /// (Will be remotely called by the clients)
        /// </summary>
        /// <param name="clientCommandData">The client command data.</param>
        public void NotifyServerCmd(ClientCommandDataType clientCommandData)
        {
            // Handle the received mouseEventData and update the State of the Shared Card Game:
            switch (clientCommandData.ClientCommandType)
            {
                case ClientCommandType.LeavingCmd:
                    lock (ClientsLocker)
                    {
                        LogInfo(
                            string.Format("NotifyServerCmd (LeavingCmd, clientCommandData.ClientName = '{0}')",
                                clientCommandData.ClientName), 10);
                        Client leavingClient;
                        var foundTheClient = Clients.TryGetValue(clientCommandData.ClientName, out leavingClient);
                        if (foundTheClient)
                        {
                            // Free a possible grabbed object by the leaving client:
                            if (leavingClient.GrabbedSharedObject != null)
                            {
                                leavingClient.GrabbedSharedObject.GrabberName = null;
                                leavingClient.GrabbedSharedObject. ContextualMenuOpened = false;
                            }

                            Clients.Remove(leavingClient.Name);
                            LogInfo(string.Format("_clients.Remove(leavingClient.Name = '{0}')", leavingClient.Name), 11);
                            // Remove the private area of the inactive client
                            var removedSharedObjectId = _gameState.RemovePrivateArea(leavingClient.Name);
                            // Note: The removedSharedObjectId should be >= 0
                            if (removedSharedObjectId >= 0)
                            {
                                // Inform other existing clients of the disposed PrivateArea id:
                                foreach (var client in Clients)
                                {
                                    // Prepare StateChangeEventDataType:
                                    var stateChangeEventDataType =
                                        new StateChangeEventDataType();
                                    stateChangeEventDataType.StateChangeEventType =
                                        StateChangeEventType.DisposeSharedObject;
                                    stateChangeEventDataType.SharedObjectId = removedSharedObjectId;
                                    stateChangeEventDataType.PrivateOwnerClientName = leavingClient.Name;
                                    // Send the StateChangeEventDataType to the client:
                                    client.Value.ScgBroadcastorCallBack.BroadcastToClient(
                                        stateChangeEventDataType);
                                    LogInfo(
                                        string.Format(
                                            "BroadcastToClient'{0}'(DisposeSharedObject, removedSharedObjectId = '{1}')",
                                            client.Value.Name, removedSharedObjectId), 12);
                                }
                            }
                        }
                    }
                    break;
                case ClientCommandType.GatheringCmd:
                    lock (GameStateLocker)
                    {
                        Rectangle gatheringRectangle = clientCommandData.TargetRectangle;
                        LogInfo(string.Format("NotifyServerCmd (GatheringCmd, clientCommandData.ClientName = '{0} gatheringRectangle = [Location={1},Size={2}]')",
                            clientCommandData.ClientName, gatheringRectangle.Location, gatheringRectangle.Size), 20);
                        try
                        {
                            _gameState.GatherCards(gatheringRectangle);
                        }
                        catch (Exception ex)
                        {
                            LogError(string.Format("Unexpected exception '{0}' raised in NotifyServerCmd(GatheringCmd) handling when calling _gameState.GatherCards(gatheringRectangle)", ex.Message), 98);
                            throw;
                        }

                    }
                    break;
                case ClientCommandType.RequestDealingParametersCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format(
                                "NotifyServerCmd (RequestDealingParametersCmd, clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card && stackOfCards.GrabberName == null)
                        {
                            Client theClient;
                            bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                out theClient);
                            if (foundTheClient && theClient != null)
                            {
                                // Do the specific stuff here:

                                // Prepare and send state change for the clicked stack of cards shared object:
                                StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.OpenDealingParametersDialog;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.GrabbingClientName = clientCommandData.ClientName;
                                stateChange.ReleasingClientName = null;
                                stateChange.PrivateOwnerClientName = null;
                                stateChange.DealingClockwise = DealingClockwise;
                                stateChange.NumberOfCardsToDeal = NumberOfCardsToDeal;
                                LogInfo(
                                    "Broadcasting state change (OpenDealingParametersDialog) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // Prepare and send state change for the clicked stack of cards shared object:
                                stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = clientCommandData.ClientName;
                                stateChange.ReleasingClientName = null;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // The stackOfCards is again grabbed:
                                stackOfCards.GrabberName = clientCommandData.ClientName;
                                stackOfCards.ContextualMenuOpened = false;
                                theClient.GrabbedSharedObject = stackOfCards;
                            }
                        }
                    }
                    break;
                case ClientCommandType.CancelDealingParametersDialogCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format(
                                "NotifyServerCmd (CancelDealingParametersDialogCmdl, clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card)
                        {
                            Client theClient;
                            bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                out theClient);
                            if (foundTheClient && theClient != null)
                            {
                                // Do the specific stuff here:

                                // Prepare and send state change for the clicked stack of cards shared object:
                                StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.ReleasingClientName = clientCommandData.ClientName;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // The stackOfCards is no more grabbed:
                                stackOfCards.GrabberName = null;
                                stackOfCards.ContextualMenuOpened = false;
                                theClient.GrabbedSharedObject = null;
                            }
                        }
                    }
                    break;
                case ClientCommandType.ExtractFirstCardCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format("NotifyServerCmd (ExtractFirstCardCmd, clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card)
                        {
                            // Check that no card is already present at the extraction location; 
                            Point extractLocation = new Point(stackOfCards.Location.X + stackOfCards.Rectangle.Width / 2 - ScgGameState.MagneticGridStep - ScgGameState.DimXCard,
                                stackOfCards.Location.Y + stackOfCards.Rectangle.Height / 2 - ScgGameState.MagneticGridStep - ScgGameState.DimYCard);
                            // Vérifier l'extract location !!!

                            SharedObject otherObject =
                                _gameState.GetSharedObjectAt(extractLocation);
                            if (!(otherObject != null &&
                                  otherObject.GameElements.First() is Card &&
                                  otherObject.Location.Equals(extractLocation)))
                            {
                                LogInfo(string.Format("OK, the extraction Location is free ! clientCommandData.ClientName='{0}'", clientCommandData.ClientName), 97);
                                Client theClient;
                                bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                    out theClient);
                                if (foundTheClient && theClient != null)
                                {
                                    // Extract the top most card and create a new shared object:
                                    Card extractedCard = (Card) stackOfCards.GameElements.First();
                                    stackOfCards.GameElements.RemoveAt(0);

                                    // Create a new shared object for the extracted card:
                                    SharedObject newSharedObject =
                                        new SharedObject(_gameState.ZOrderListOfSharedObjects,
                                            _gameState.GetFreeId(),
                                            new Rectangle(
                                                new Point(
                                                    stackOfCards.Location.X -
                                                    ScgGameState.MagneticGridStep,
                                                    stackOfCards.Location.Y -
                                                    ScgGameState.MagneticGridStep),
                                                new Size(ScgGameState.DimXCard, ScgGameState.DimYCard)),
                                            null, new List<GameElement>());
                                    newSharedObject.GameElements.Add(extractedCard);
                                    if (stackOfCards.GameElements.Count == 1)
                                    {
                                        // Resize the clickedSharedObject which owns now a single card:
                                        stackOfCards.Rectangle =
                                            new Rectangle(stackOfCards.Location,
                                                new Size(ScgGameState.DimXCard, ScgGameState.DimYCard));
                                        // Apply magnetic grid:
                                        stackOfCards.Rectangle =
                                            new Rectangle(
                                                ScgGameState.GetMagneticLocation(
                                                    stackOfCards.Rectangle),
                                                stackOfCards.Rectangle.Size);
                                    }

                                    // The stackOfCards is no more grabbed:
                                    stackOfCards.GrabberName = null;
                                    stackOfCards.ContextualMenuOpened = false;

                                    // Prepare and send state change for the clicked shared object:
                                    StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                    stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                    stateChange.SharedObjectId = stackOfCards.Id;
                                    stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                    stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                    stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                    stateChange.GrabbingClientName = null;
                                    stateChange.ReleasingClientName = clientCommandData.ClientName;
                                    stateChange.PrivateOwnerClientName = null;
                                    LogInfo(
                                        "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                        95);
                                    BroadcastStateChange(stateChange, null, null);

                                    // Add the new Shared object to the ZOrderList:
                                    _gameState.ZOrderListOfSharedObjects.Insert(0, newSharedObject);

                                    // Prepare sending the state change for the new Shared object:
                                    stateChange.StateChangeEventType =
                                        StateChangeEventType.NewSharedObject;
                                    stateChange.SharedObjectId = newSharedObject.Id;
                                    stateChange.NewSharedObjectPicture =
                                        newSharedObject.Picture;
                                    stateChange.NewSharedObjectLocation =
                                        newSharedObject.Location;
                                    stateChange.SharedObjectSize =
                                        newSharedObject.Rectangle.Size;
                                    stateChange.GrabbingClientName = null;
                                    stateChange.PrivateOwnerClientName = null;
                                    LogInfo(
                                        string.Format("Broadcasting state change (NewSharedObject) for the extracted card shared object (newSharedObject.Id ={0})",newSharedObject.Id),95);
                                    BroadcastStateChange(stateChange, null, null);

                                    // Mark the new object as not grabbed:
                                    newSharedObject.GrabberName = null;
                                    theClient.GrabbedSharedObject = null;
                                    newSharedObject.BringToFront();
                                }
                            }
                            else
                            {
                                LogWarning(string.Format("NOT OK, the extraction Location is not free ! clientCommandData.ClientName='{0}'", clientCommandData.ClientName), 97);
                            }
                        }
                    }
                    break;
                case ClientCommandType.CountCardsCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format(
                                "NotifyServerCmd (CountCardsCmd clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card)
                        {
                            Client theClient;
                                bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                    out theClient);
                            if (foundTheClient && theClient != null)
                            {
                                // Do the specific stuff here:
                                StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.ShowMessage;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.RecipientClientName = clientCommandData.ClientName;
                                stateChange.ToEveryone = false;
                                stateChange.MessageToDisplay = string.Format("This deck includes {0} cards.", stackOfCards.GameElements.Count);
                                stateChange.MessageBoxTitle = "Deck Card Count";
                                LogInfo(
                                    "Broadcasting state change (ShowMessage) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // Prepare and send state change for the clicked stack of cards shared object:
                                stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.ReleasingClientName = clientCommandData.ClientName;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // The stackOfCards is no more grabbed:
                                stackOfCards.GrabberName = null;
                                stackOfCards.ContextualMenuOpened = false;
                                theClient.GrabbedSharedObject = null;
                            }
                        }
                    }
                    break;
                case ClientCommandType.DealCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format(
                                "NotifyServerCmd (DealCmd, clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card)
                        {
                            Client theDealerClient;
                                bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                    out theDealerClient);
                            if (foundTheClient && theDealerClient != null)
                            {
                                // Do the specific stuff here:
                                DealingClockwise = clientCommandData.DealingClockwise;
                                NumberOfCardsToDeal = clientCommandData.NumberOfCardsToDeal;


                                //DO THE DEALING: TO BE COMPLETED





                                // Code issued from ExtractFirstCard as reference
                                // Extract the top most card and create a new shared object:
                                Card extractedCard = (Card)stackOfCards.GameElements.First();
                                stackOfCards.GameElements.RemoveAt(0);

                                // Create a new shared object for the extracted card:
                                SharedObject newSharedObject =
                                    new SharedObject(_gameState.ZOrderListOfSharedObjects,
                                        _gameState.GetFreeId(),
                                        new Rectangle(
                                            new Point(
                                                stackOfCards.Location.X -
                                                ScgGameState.MagneticGridStep,
                                                stackOfCards.Location.Y -
                                                ScgGameState.MagneticGridStep),
                                            new Size(ScgGameState.DimXCard, ScgGameState.DimYCard)),
                                        null, new List<GameElement>());
                                newSharedObject.GameElements.Add(extractedCard);
                                if (stackOfCards.GameElements.Count == 1)
                                {
                                    // Resize the clickedSharedObject which owns now a single card:
                                    stackOfCards.Rectangle =
                                        new Rectangle(stackOfCards.Location,
                                            new Size(ScgGameState.DimXCard, ScgGameState.DimYCard));
                                    // Apply magnetic grid:
                                    stackOfCards.Rectangle =
                                        new Rectangle(
                                            ScgGameState.GetMagneticLocation(
                                                stackOfCards.Rectangle),
                                            stackOfCards.Rectangle.Size);
                                }

                                // The stackOfCards is no more grabbed:
                                stackOfCards.GrabberName = null;
                                stackOfCards.ContextualMenuOpened = false;

                                // Prepare and send state change for the clicked shared object:
                                StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.ReleasingClientName = clientCommandData.ClientName;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // Add the new Shared object to the ZOrderList:
                                _gameState.ZOrderListOfSharedObjects.Insert(0, newSharedObject);

                                // Prepare sending the state change for the new Shared object:
                                stateChange.StateChangeEventType =
                                    StateChangeEventType.NewSharedObject;
                                stateChange.SharedObjectId = newSharedObject.Id;
                                stateChange.NewSharedObjectPicture =
                                    newSharedObject.Picture;
                                stateChange.NewSharedObjectLocation =
                                    newSharedObject.Location;
                                stateChange.SharedObjectSize =
                                    newSharedObject.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    string.Format("Broadcasting state change (NewSharedObject) for the extracted card shared object (newSharedObject.Id ={0})", newSharedObject.Id), 95);
                                BroadcastStateChange(stateChange, null, null);







                                // Prepare and send state change for the clicked stack of cards shared object:
                                stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.ReleasingClientName = clientCommandData.ClientName;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // The stackOfCards is no more grabbed:
                                stackOfCards.GrabberName = null;
                                stackOfCards.ContextualMenuOpened = false;
                                theDealerClient.GrabbedSharedObject = null;
                            }
                        }
                    }
                    break;
                case ClientCommandType.ShuffleUpCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format(
                                "NotifyServerCmd (ShuffleUpCmd, clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card)
                        {
                            Client theClient;
                                bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                    out theClient);
                            if (foundTheClient && theClient != null)
                            {
                                // Do the specific stuff here:

                                stackOfCards.Shuffle();

                                // Prepare and send state change for the clicked stack of cards shared object:
                                StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.ReleasingClientName = clientCommandData.ClientName;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // The stackOfCards is no more grabbed:
                                stackOfCards.GrabberName = null;
                                stackOfCards.ContextualMenuOpened = false;
                                theClient.GrabbedSharedObject = null;
                            }
                        }
                    }
                    break;
                case ClientCommandType.ContextualMenuClosedCmd:
                    lock (GameStateLocker)
                    {
                        SharedObject stackOfCards = _gameState.GetSharedObjectFromId(clientCommandData.SharedObjectId);
                        LogInfo(
                            string.Format(
                                "NotifyServerCmd (ContextualMenuClosedCmd, clientCommandData.ClientName = '{0}', clientCommandData.SharedObjectId = '{1}')",
                                clientCommandData.ClientName, clientCommandData.SharedObjectId), 31);
                        if (stackOfCards != null && stackOfCards.GameElements.First() is Card)
                        {
                            Client theClient;
                            bool foundTheClient = Clients.TryGetValue(clientCommandData.ClientName,
                                out theClient);
                            if (foundTheClient && theClient != null &&
                                theClient.GrabbedSharedObject != null &&
                                theClient.GrabbedSharedObject.Id == stackOfCards.Id)
                            {
                                // Do the specific stuff here:

                                // Nothing to do.

                                // Prepare and send state change for the clicked stack of cards shared object:
                                StateChangeEventDataType stateChange = new StateChangeEventDataType();
                                stateChange.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                                stateChange.SharedObjectId = stackOfCards.Id;
                                stateChange.NewSharedObjectPicture = stackOfCards.Picture;
                                stateChange.NewSharedObjectLocation = stackOfCards.Location;
                                stateChange.SharedObjectSize = stackOfCards.Rectangle.Size;
                                stateChange.GrabbingClientName = null;
                                stateChange.ReleasingClientName = clientCommandData.ClientName;
                                stateChange.PrivateOwnerClientName = null;
                                LogInfo(
                                    "Broadcasting state change (SharedObjectMove) for the clicked shared object (stackOfCard)",
                                    95);
                                BroadcastStateChange(stateChange, null, null);

                                // The stackOfCards is no more grabbed:
                                stackOfCards.GrabberName = null;
                                stackOfCards.ContextualMenuOpened = false;
                                theClient.GrabbedSharedObject = null;
                            }
                        }
                    }
                    break;
            }
        }

        public void NotifyServerChatMessage(ChatMessageDataType chatMessageData)
        {
            lock (ClientsLocker)
            {
                var inactiveClients = new List<string>();
                foreach (var client in Clients)
                {
                    try
                    {
                        (client.Value).ScgBroadcastorCallBack.BroadcastChatMessageToClient(chatMessageData);
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("Exception '{0}' raised in ScgBroadcastorService.BroadcastStateChange when calling (client.Value).ScgBroadcastorCallBack.BroadcastChatMessageToClient('{2}'). client.Value.Name = '{1}')", ex.Message, client.Value.Name, chatMessageData.ChatMessage), 99);
                        inactiveClients.Add(client.Key);
                    }
                }
                if (inactiveClients.Count > 0)
                {
                    foreach (var inactiveClient in inactiveClients)
                    {
                        // Search the leavingClient (Client) corresponding to the inactiveClient (name):
                        Client leavingClient;
                        var foundTheClient = Clients.TryGetValue(inactiveClient, out leavingClient);
                        if (foundTheClient)
                        {
                            // Free a possible grabbed object by the leaving client:
                            if (leavingClient.GrabbedSharedObject != null)
                            {
                                leavingClient.GrabbedSharedObject.GrabberName = null;
                                leavingClient.GrabbedSharedObject.ContextualMenuOpened = false;
                            }
                        }

                        // Remove the inactive client from the clients list:
                        Clients.Remove(inactiveClient);
                        LogInfo(string.Format("_clients.Remove(inactiveClient = '{0}')", inactiveClient), 13);

                        // Remove the private area of the inactive client
                        var removedSharedObjectId = _gameState.RemovePrivateArea(inactiveClient);
                        // Note: The removedSharedObjectId should be >= 0
                        if (removedSharedObjectId >= 0)
                        {
                            // Inform other existing clients of the disposed PrivateArea id:
                            foreach (var client in Clients)
                            {
                                // Prepare StateChangeEventDataType:
                                var aStateChangeEventDataType =
                                    new StateChangeEventDataType();
                                aStateChangeEventDataType.StateChangeEventType =
                                    StateChangeEventType.DisposeSharedObject;
                                aStateChangeEventDataType.SharedObjectId = removedSharedObjectId;
                                aStateChangeEventDataType.PrivateOwnerClientName = inactiveClient;
                                // Send the StateChangeEventDataType to the client:
                                client.Value.ScgBroadcastorCallBack.BroadcastToClient(
                                    aStateChangeEventDataType);
                                LogInfo(
                                    string.Format(
                                        "BroadcastToClient'{0}'(DisposeSharedObject, removedSharedObjectId = '{1}')",
                                        client.Value.Name, removedSharedObjectId), 14);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Logs an information event message 'eventText' with the id 'eventId' in the Windows event log named
        ///     EventLogName.
        ///     and issued from the source EventLogSourceName.
        /// </summary>
        /// <param name="eventText">The event text.</param>
        /// <param name="eventId">The event id.</param>
        private void LogInfo(string eventText, int eventId)
        {
            if (EventLogActive)
            {
                EventLog.WriteEntry(EventlogSourceName, eventText, EventLogEntryType.Information, eventId);
            }
        }

        /// <summary>
        ///     Logs a warning event message 'eventText' with the id 'eventId' in the Windows event log named EventLogName.
        ///     and issued from the source EventLogSourceName.
        /// </summary>
        /// <param name="eventText">The event text.</param>
        /// <param name="eventId">The event id.</param>
        private void LogWarning(string eventText, int eventId)
        {
            if (EventLogActive)
            {
                EventLog.WriteEntry(EventlogSourceName, eventText, EventLogEntryType.Warning, eventId);
            }
        }

        /// <summary>
        ///     Logs an error event message 'eventText' with the id 'eventId' in the Windows event log named EventLogName.
        ///     and issued from the source EventLogSourceName.
        /// </summary>
        /// <param name="eventText">The event text.</param>
        /// <param name="eventId">The event id.</param>
        public void LogError(string eventText, int eventId)
        {
            if (EventLogActive)
            {
                EventLog.WriteEntry(EventlogSourceName, eventText, EventLogEntryType.Error, eventId);
            }
        }
    }
}