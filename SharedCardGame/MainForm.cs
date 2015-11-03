using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Security;
using System.ServiceModel;
using System.Windows.Forms;
using ScgServiceLibrary;
using SharedCardGame.Properties;
using SharedCardGame.ScgServiceLibrary;

namespace SharedCardGame
{
    public partial class MainForm : Form
    {
        private const string EventlogSourceName = "SCG Client";
        private const string EventlogServiceSourceName = "SCG BrodcastorService";
        private const string EventlogLogName = "Application";
        //private MouseEventTransparentPictureBox _pictureBoxRaptor;
        private ScgBroadcastorServiceClient _client; // Reference to the broadcastor service proxy object
        private DateTime _lastNotifyServerDateTime;
        private Dictionary<string, MouseEventTransparentVectorControl> _playerVectors;
        private MouseEventTransparentGatheringRectangleControl _gatheringRectangleControl;
        private ChatForm _chatForm;
        private Control _controlOwningContextMenuStrip;
        private DealingParametersDialog _dealingParametersDialog;
        #region Private Properties
        public string ClientRegisteredName { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // Raptor logo:
            //_pictureBoxRaptor = new MouseEventTransparentPictureBox();
            //_pictureBoxRaptor.Image = global::SharedCardGame.Properties.Resources.Raptor_Development_Transp_Texte;
            //_pictureBoxRaptor.Location = new System.Drawing.Point(350, 124);
            //_pictureBoxRaptor.Name = "_pictureBoxRaptor";
            //_pictureBoxRaptor.Size = new System.Drawing.Size(402, 350);
            //_pictureBoxRaptor.TabIndex = 0;
            //_pictureBoxRaptor.TabStop = false;
            //RedirectMouseEventsToGameFieldForControl(_pictureBoxRaptor);
            //_pictureBoxRaptor.BackColor = Color.Transparent;
            //_pictureBoxRaptor.SizeMode = PictureBoxSizeMode.StretchImage;
            //_pictureBoxRaptor.Parent = splitContainerGameField.Panel2;

            _dealingParametersDialog = new DealingParametersDialog();

            _chatForm = new ChatForm(this);
            _chatForm.Visible = false;
            _chatForm.BringToFront();
            _playerVectors = new Dictionary<string, MouseEventTransparentVectorControl>();
            try
            {
                if (!EventLog.SourceExists(EventlogSourceName))
                {
                    // Warning: Requires to be run as Administrator to create the Event Source (once):
                    EventLog.CreateEventSource(EventlogSourceName, EventlogLogName);

                    // Create the EventSource for the BroadcastorService as it cannot create it itself: (requires Administrator privileges)
                    if (!EventLog.SourceExists(EventlogServiceSourceName))
                    {
                        EventLog.CreateEventSource(EventlogServiceSourceName, EventlogLogName);
                    }
                }
                EventLogActive = true;
            }
            catch (SecurityException)
            {
                // Was not run as administrator and source does not exist:
                EventLogActive = false;
            }
            LogInfo("Client started.", 0);
        }

        public void SendChatMessage(string chatMessage)
        {
            if (_client != null)
            {
                _client.NotifyServerChatMessage(
                    new ChatMessageDataType
                    {
                        ClientName = textBoxPlayerName.Text,
                        ChatMessage = chatMessage
                    });
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the event logging is active.
        /// </summary>
        /// <value>
        ///     <c>true</c> if event logging is active; otherwise, <c>false</c>.
        /// </value>
        private bool EventLogActive { get; set; }

        /// <summary>
        ///     Handles a broadcast callback call received from the ScgBroadcastService.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void HandleBroadcast(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new HandleBroadcastCallback(HandleBroadcast), sender, e);
            }
            else
            {
                if (sender is StateChangeEventDataType)
                {
                    var stateChangeEventData = (StateChangeEventDataType) sender;
                    // Handle the received stateChangeEventData and report the state change to the GUI elements:
                    try
                    {
                        // getting the event type:
                        var stateChangeEventType = stateChangeEventData.StateChangeEventType;
                        //var id = stateChangeEventData.SharedObjectId;
                        ResourceManager rm;
                        switch (stateChangeEventType)
                        {
                            case StateChangeEventType.NewSharedObject:
                                try
                                {
                                    LogInfo(
                                        string.Format(
                                            "Handling stateChangeEventData (stateChangeEventType = StateChangeEventType.NewSharedObject, stateChangeEventData.NewSharedObjectPicture = '{0}', stateChangeEventData.SharedObjectId = {1}",
                                            stateChangeEventData.NewSharedObjectPicture,
                                            stateChangeEventData.SharedObjectId),
                                        1);

                                    // Test if first NewSharedObject state change received from registration  
                                    if (timerRegisteringTimeOut.Enabled)
                                    {
                                        // So the registration has been accepted. - Collapse the register panel:
                                        splitContainerGameField.Panel1Collapsed = true;
                                        // Update the main form title with player name as suffix:
                                        Text = Resources.Form1_HandleBroadcast_Shared_Card_Game___ +
                                               ClientRegisteredName;
                                        // Disable the registering timeout:
                                        timerRegisteringTimeOut.Enabled = false;
                                        // Set the chat form visible:
                                        _chatForm.Visible = true;
                                    }

                                    // Create the asked new shared object:
                                    MouseEventTransparentPictureBox newMouseEventTransparentPictureBox =
                                        new MouseEventTransparentPictureBox();
                                    RedirectMouseEventsToGameFieldForControl(newMouseEventTransparentPictureBox);
                                    newMouseEventTransparentPictureBox.BackColor = Color.Transparent;
                                    rm = Resources.ResourceManager;
                                    newMouseEventTransparentPictureBox.Image =
                                        (Bitmap) rm.GetObject(stateChangeEventData.NewSharedObjectPicture);
                                    newMouseEventTransparentPictureBox.Location =
                                        stateChangeEventData.NewSharedObjectLocation;
                                    newMouseEventTransparentPictureBox.Size = stateChangeEventData.SharedObjectSize;
                                    newMouseEventTransparentPictureBox.Name =
                                        stateChangeEventData.SharedObjectId.ToString();
                                    newMouseEventTransparentPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                                    newMouseEventTransparentPictureBox.TabIndex = 1;
                                    newMouseEventTransparentPictureBox.TabStop = false;
                                    newMouseEventTransparentPictureBox.Parent = splitContainerGameField.Panel2;

                                    // Handle new private areas:
                                    if (stateChangeEventData.PrivateOwnerClientName != null)
                                    {
                                        // Create the label of the new private area:
                                        MouseEventTransparentLabel labelPrivateArea = new MouseEventTransparentLabel();
                                        RedirectMouseEventsToGameFieldForControl(labelPrivateArea);
                                        labelPrivateArea.BackColor = Color.Ivory;
                                        labelPrivateArea.Text = stateChangeEventData.PrivateOwnerClientName;
                                        labelPrivateArea.TextAlign = ContentAlignment.MiddleCenter;
                                        labelPrivateArea.Top = 0;
                                        labelPrivateArea.Width = newMouseEventTransparentPictureBox.Width;
                                        labelPrivateArea.Height = 12;
                                        newMouseEventTransparentPictureBox.Controls.Add(labelPrivateArea);
                                        // The new private area must be the topmost private area
                                        newMouseEventTransparentPictureBox.BringToFront();
                                        // But other regular shared object must stay on top of it if they are intersecting with it:
                                        BringToFrontRegularSharedObjectControlsOnPrivateArea(newMouseEventTransparentPictureBox);

                                        // Prepare the player vector control: (not visible at the moment but ready) 
                                        Point playerLocation = stateChangeEventData.NewSharedObjectLocation;
                                        playerLocation.X += stateChangeEventData.SharedObjectSize.Width/2;
                                        playerLocation.Y += stateChangeEventData.SharedObjectSize.Height/2;
                                        MouseEventTransparentVectorControl playerVector =
                                            new MouseEventTransparentVectorControl(playerLocation, playerLocation);
                                        RedirectMouseEventsToGameFieldForControl(playerVector);
                                        playerVector.UpdateLocation();
                                        playerVector.Parent = splitContainerGameField.Panel2;

                                        // Add the new player vector to the _playerVectors list:
                                        _playerVectors.Add(stateChangeEventData.PrivateOwnerClientName, playerVector);
                                        SendPlayerVectorsToBack();

                                        // Test if the new private area is our:
                                        if (stateChangeEventData.PrivateOwnerClientName == ClientRegisteredName)
                                        {
                                            // Display the welcome message:
                                            MessageBoxEx.Show(this,
                                                string.Format(
                                                    Resources.Form1_HandleBroadcast_Welcome_in_the_game___0___ +
                                                    Environment.NewLine + Environment.NewLine +
                                                    Resources
                                                        .Form1_HandleBroadcast_Please__choose_your_playing_position_by_dragging_your_private_area_from_the_top_left_corner_to_your_wished_position_around_the_game_field_,
                                                    ClientRegisteredName), Resources.Form1_HandleBroadcast_Welcome);
                                        }
                                    }

                                    // Update the playerVector if the new shared object is grabbed by a player (the new
                                       // shared object has been extracted from a stack, he is grabbed until button mouse release)
                                    if (stateChangeEventData.GrabbingClientName != null)
                                    {
                                        // No playor vector displayed on our own display:
                                        if (stateChangeEventData.GrabbingClientName != ClientRegisteredName)
                                        {
                                            // Test if the event is about a private area:
                                            if (stateChangeEventData.PrivateOwnerClientName == null)
                                            {
                                                // not a private area, prepare to show the player vector: 
                                                MouseEventTransparentVectorControl playerVector;
                                                // Try to get the playerVector control of the grabbing player:
                                                if (_playerVectors.TryGetValue(stateChangeEventData.GrabbingClientName,
                                                    out playerVector))
                                                {
                                                    // Show the playerVector from the grabbing player's private area
                                                    // to the grabbed shared object location and set it visible:
                                                    Point to = stateChangeEventData.NewSharedObjectLocation;
                                                    to.X += stateChangeEventData.SharedObjectSize.Width/2;
                                                    to.Y += stateChangeEventData.SharedObjectSize.Height/2;
                                                    playerVector.To = to;
                                                    playerVector.UpdateLocation();
                                                    playerVector.Visible = true;
                                                    SendPlayerVectorsToBack();
                                                    playerVector.Invalidate();
                                                }
                                            }
                                            else
                                            {
                                                // The event is about the private area, just update the
                                                // location of the playerVector but do not show it:
                                                MouseEventTransparentVectorControl playerVector;
                                                if (
                                                    _playerVectors.TryGetValue(
                                                        stateChangeEventData.PrivateOwnerClientName,
                                                        out playerVector))
                                                {
                                                    Point pt = stateChangeEventData.NewSharedObjectLocation;
                                                    pt.X += stateChangeEventData.SharedObjectSize.Width/2;
                                                    pt.Y += stateChangeEventData.SharedObjectSize.Height/2;
                                                    playerVector.From = pt;
                                                    playerVector.To = pt;
                                                    playerVector.UpdateLocation();
                                                    playerVector.Visible = false;
                                                    playerVector.Invalidate();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // The event concerned shared object is not grabbed:
                                        // Test if it's a private area or not:
                                        if (stateChangeEventData.PrivateOwnerClientName == null)
                                        {
                                            // the event concerned shared object is not a private area and is not (or no more)
                                            // grabbed: hide the eventualy visible playerVector:
                                            MouseEventTransparentVectorControl playerVector;
                                            if (stateChangeEventData.ReleasingClientName != null &&
                                                _playerVectors.TryGetValue(stateChangeEventData.ReleasingClientName,
                                                    out playerVector))
                                            {
                                                playerVector.To = playerVector.From;
                                                playerVector.UpdateLocation();
                                                playerVector.Visible = false;
                                                playerVector.Invalidate();
                                            }
                                        }
                                    }
                                    newMouseEventTransparentPictureBox.BringToFront();
                                }
                                catch (Exception ex)
                                {
                                    // Log any unexpected exception here:
                                    LogError(string.Format(
                                        "Unexpected exception '{0}' raised in NewSharedObject handling.", ex.Message),
                                        21);
                                    // and rethrow it.
                                    throw;
                                }
                                break;
                            case StateChangeEventType.SharedObjectMove:
                                try
                                {
                                    foreach (Control control in splitContainerGameField.Panel2.Controls)
                                    {
                                        if (control is MouseEventTransparentPictureBox &&
                                            control.Name == stateChangeEventData.SharedObjectId.ToString())
                                        {
                                            control.Location = stateChangeEventData.NewSharedObjectLocation;
                                            control.Size = stateChangeEventData.SharedObjectSize;
                                            rm = Resources.ResourceManager;

                                            (control as MouseEventTransparentPictureBox).Image =
                                                (Bitmap) rm.GetObject(stateChangeEventData.NewSharedObjectPicture);
                                            control.BringToFront();

                                            // Update the playerVector:
                                            if (stateChangeEventData.GrabbingClientName != null)
                                            {
                                                if (stateChangeEventData.GrabbingClientName != ClientRegisteredName)
                                                {
                                                    if (stateChangeEventData.PrivateOwnerClientName == null)
                                                    {
                                                        MouseEventTransparentVectorControl playerVector;
                                                        if (
                                                            _playerVectors.TryGetValue(
                                                                stateChangeEventData.GrabbingClientName,
                                                                out playerVector))
                                                        {
                                                            Point to = stateChangeEventData.NewSharedObjectLocation;
                                                            to.X += stateChangeEventData.SharedObjectSize.Width/2;
                                                            to.Y += stateChangeEventData.SharedObjectSize.Height/2;
                                                            playerVector.To = to;
                                                            playerVector.UpdateLocation();
                                                            playerVector.Visible = true;
                                                            SendPlayerVectorsToBack();
                                                            playerVector.Invalidate();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MouseEventTransparentVectorControl playerVector;
                                                        if (
                                                            _playerVectors.TryGetValue(
                                                                stateChangeEventData.PrivateOwnerClientName,
                                                                out playerVector))
                                                        {
                                                            Point pt = stateChangeEventData.NewSharedObjectLocation;
                                                            pt.X += stateChangeEventData.SharedObjectSize.Width/2;
                                                            pt.Y += stateChangeEventData.SharedObjectSize.Height/2;
                                                            playerVector.From = pt;
                                                            playerVector.To = pt;
                                                            playerVector.UpdateLocation();
                                                            playerVector.Visible = false;
                                                            playerVector.Invalidate();
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (stateChangeEventData.PrivateOwnerClientName == null)
                                                {
                                                    MouseEventTransparentVectorControl playerVector;
                                                    if (stateChangeEventData.ReleasingClientName != null &&
                                                        _playerVectors.TryGetValue(
                                                            stateChangeEventData.ReleasingClientName,
                                                            out playerVector))
                                                    {
                                                        playerVector.To = playerVector.From;
                                                        playerVector.UpdateLocation();
                                                        playerVector.Visible = false;
                                                        playerVector.Invalidate();
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(string.Format(
                                        "Unexpected exception '{0}' raised in SharedObjectMove handling.", ex.Message),
                                        22);
                                    throw;
                                }
                                break;
                            case StateChangeEventType.DisposeSharedObject:
                                LogInfo(
                                    string.Format(
                                        "Handling stateChangeEventData (stateChangeEventType = StateChangeEventType.DisposeSharedObject, stateChangeEventData.SharedObjectId = {0}",
                                        stateChangeEventData.SharedObjectId), 2);
                                try
                                {
                                    foreach (Control control in splitContainerGameField.Panel2.Controls)
                                    {
                                        if (control is MouseEventTransparentPictureBox &&
                                            control.Name == stateChangeEventData.SharedObjectId.ToString())
                                        {
                                            if (stateChangeEventData.PrivateOwnerClientName != null)
                                            {
                                                // It's a private area, the player stateChangeEventData.PrivateOwnerClientName has left.
                                                // Remove its playerVector from the _playerVectors list and dispose it:
                                                MouseEventTransparentVectorControl playerVector;
                                                if (_playerVectors.TryGetValue(stateChangeEventData.PrivateOwnerClientName, out playerVector))
                                                {
                                                    playerVector.Dispose();
                                                }
                                                _playerVectors.Remove(stateChangeEventData.PrivateOwnerClientName);
                                            }
                                            var i = splitContainerGameField.Panel2.Controls.IndexOf(control);
                                            splitContainerGameField.Panel2.Controls.RemoveAt(i);
                                            control.Dispose();
                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(
                                        string.Format(
                                            "Unexpected exception '{0}' raised in DisposeSharedObject handling.",
                                            ex.Message), 23);
                                    throw;
                                }
                                break;
                            case StateChangeEventType.ShowGatheringRectangle:
                                try
                                {
                                    if (_gatheringRectangleControl == null)
                                    {
                                        _gatheringRectangleControl =
                                            new MouseEventTransparentGatheringRectangleControl(
                                                stateChangeEventData.GatheringRectangle);
                                        RedirectMouseEventsToGameFieldForControl(_gatheringRectangleControl);
                                        _gatheringRectangleControl.Parent = splitContainerGameField.Panel2;
                                    }
                                    else
                                    {
                                        _gatheringRectangleControl.Rectangle = stateChangeEventData.GatheringRectangle;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(string.Format(
                                        "Unexpected exception '{0}' raised in ShowGatheringRectangle handling.",
                                        ex.Message),
                                        21);
                                    throw;
                                }
                                break;
                            case StateChangeEventType.HideGatheringRectangle:
                                try
                                {
                                    if (_gatheringRectangleControl != null)
                                    {
                                        splitContainerGameField.Panel2.Controls.Remove(_gatheringRectangleControl);
                                        _gatheringRectangleControl = null;
                                        // Send the gathering command to the server 
                                        _client.NotifyServerCmd(
                                            new ClientCommandDataType
                                            {
                                                ClientName = ClientRegisteredName,
                                                ClientCommandType = ClientCommandType.GatheringCmd,
                                                TargetRectangle = stateChangeEventData.GatheringRectangle
                                            });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError(string.Format(
                                        "Unexpected exception '{0}' raised in HideGatheringRectangle handling.",
                                        ex.Message),
                                        21);
                                    throw;
                                }
                                break;
                            case StateChangeEventType.OpenContextualMenuOnSharedObject:
                                LogInfo(string.Format(
                                    "Handling stateChangeEventData (stateChangeEventType = StateChangeEventType.OpenContextualMenuOnSharedObject, stateChangeEventData.SharedObjectId = {0}",
                                    stateChangeEventData.SharedObjectId), 4);
                                if (stateChangeEventData.GrabbingClientName == ClientRegisteredName)
                                {
                                    // The local client has right clicked on the Shared object and has grabbed it 
                                    // He can now open the contextual menu:
                                    try
                                    {
                                        // Search for the MouseEventTransparentControl corresponding to the shared object:
                                        foreach (Control control in splitContainerGameField.Panel2.Controls)
                                        {
                                            if (control is MouseEventTransparentPictureBox &&
                                                control.Name == stateChangeEventData.SharedObjectId.ToString())
                                            {
                                                LogInfo("Opening contextual menu.",5);
                                                // Open the contextual Menu by setting it to the found control
                                                control.ContextMenuStrip = contextMenuStripStackOfCards;
                                                _controlOwningContextMenuStrip = control;
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(string.Format(
                                            "Unexpected exception '{0}' raised in OpenContextualMenuOnSharedObject handling.",
                                            ex.Message),
                                            28);
                                        throw;
                                    }
                                }
                                break;
                            case StateChangeEventType.OpenDealingParametersDialog:
                                LogInfo(string.Format(
                                    "Handling stateChangeEventData (stateChangeEventType = StateChangeEventType.OpenDealingParametersDialog, stateChangeEventData.SharedObjectId = {0}",
                                    stateChangeEventData.SharedObjectId), 27);
                                if (stateChangeEventData.GrabbingClientName == ClientRegisteredName)
                                {
                                    // The local client has Requested to open the DealingParametersDialog 
                                    // He can now do this:
                                    try
                                    {
                                        // Search for the MouseEventTransparentControl corresponding to the shared object:
                                        foreach (Control control in splitContainerGameField.Panel2.Controls)
                                        {
                                            if (control is MouseEventTransparentPictureBox &&
                                                control.Name == stateChangeEventData.SharedObjectId.ToString())
                                            {
                                                LogInfo("Opening DealingParametersDialog.", 5);
                                                //Opening DealingParametersDialog
                                                _dealingParametersDialog.DealingDirectionIsClockwise =
                                                    stateChangeEventData.DealingClockwise;
                                                _dealingParametersDialog.NumberOfCardsToDeal =
                                                    stateChangeEventData.NumberOfCardsToDeal;

                                                _dealingParametersDialog.ShowDialog();
                                                if (_dealingParametersDialog.DialogResult == DialogResult.OK)
                                                {
                                                    // A CORRIGER: Notify the server of the Dealing
                                                    try
                                                    {
                                                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.RequestDealingParameters, SharedObjectId={1}", ClientRegisteredName, stateChangeEventData.SharedObjectId), 34);
                                                        _client.NotifyServerCmd(
                                                            new ClientCommandDataType
                                                            {
                                                                ClientName = ClientRegisteredName,
                                                                ClientCommandType = ClientCommandType.DealCmd,
                                                                DealingClockwise = _dealingParametersDialog.DealingDirectionIsClockwise,
                                                                NumberOfCardsToDeal = _dealingParametersDialog.NumberOfCardsToDeal,
                                                                SharedObjectId = stateChangeEventData.SharedObjectId
                                                            });
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd", ex.Message), 34);
                                                        // ignore
                                                    }

                                                }
                                                else
                                                {
                                                    // A CORRIGER (Notify the server of the cancelling)
                                                    try
                                                    {
                                                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.CancelDealingParametersDialogCmd, SharedObjectId={1}", ClientRegisteredName, stateChangeEventData.SharedObjectId), 34);
                                                        _client.NotifyServerCmd(
                                                            new ClientCommandDataType
                                                            {
                                                                ClientName = ClientRegisteredName,
                                                                ClientCommandType = ClientCommandType.CancelDealingParametersDialogCmd,
                                                                SharedObjectId = stateChangeEventData.SharedObjectId
                                                            });
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd", ex.Message), 34);
                                                        // ignore
                                                    }
                                                    
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(string.Format(
                                            "Unexpected exception '{0}' raised in OpenDealingParametersDialog handling.",
                                            ex.Message),
                                            28);
                                        throw;
                                    }
                                }
                                break;

                            case StateChangeEventType.ShowMessage:
                                if (stateChangeEventData.ToEveryone ||
                                    stateChangeEventData.RecipientClientName == ClientRegisteredName)
                                {
                                    LogInfo(
                                        string.Format(
                                            "Handling stateChangeEventData (stateChangeEventType = StateChangeEventType.ShowMessage, stateChangeEventData.NewSharedObjectPicture = '{0}', stateChangeEventData.SharedObjectId = {1}, MessageToDisplay='{2}'",
                                            stateChangeEventData.NewSharedObjectPicture,
                                            stateChangeEventData.SharedObjectId,
                                            stateChangeEventData.MessageToDisplay),
                                        1);
                                    MessageBoxEx.Show(this, stateChangeEventData.MessageToDisplay,
                                        stateChangeEventData.MessageBoxTitle);
                                }
                                break;
                        }
                        //_pictureBoxRaptor.SendToBack();
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("Unexpected exception '{0}' raised in HandleBroadcast.", ex.Message), 25);
                        throw;
                    }
                }
                else if (sender is ChatMessageDataType)
                {
                    ChatMessageDataType chatMessageData = (ChatMessageDataType)sender;
                    // Handle the received chatMessageData and update the Chat Window:
                    try
                    {
                        if (_chatForm != null)
                        {
                            _chatForm.LogReceivedMessage(chatMessageData);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(
                            string.Format(
                                "Unexpected exception '{0}' raised in HandleBroadcast when handling a ChatMessageDataType.",
                                ex.Message), 45);
                        throw;
                    }
                }
                else if (sender is int)
                {
                    // Bad Client Protolcol Version
                    // Handle the received required serverProtocolVersion and Display an error message:
                    int serverProtocolVersion = (int) sender;
                    LogError(
                        string.Format(
                            "Bad client protocol version {0}. The server requires version '{1}'.",
                            Common.ClientServerProtocolVersion, serverProtocolVersion), 46);
                    timerRegisteringTimeOut.Enabled = false;
                    MessageBoxEx.Show(this, string.Format("ERROR: BAD COMMUNICATION PROTOCOL VERSION:" + Environment.NewLine + Environment.NewLine
                        + "The WCF service running on the server requires the communication protocol version {0}. You are using an obsolete SharedCardGame.exe client implementing communication protocol version {1}." + Environment.NewLine
                        + "To succesfully connect to this server, please download the latest SharedCardGame.exe client version at:" + Environment.NewLine + Environment.NewLine 
                        + "http://www.raptordev.ch/SharedCardGame_Client.zip",serverProtocolVersion, Common.ClientServerProtocolVersion));
                    if (_client != null)
                    {
                        _client.Abort();
                        _client = null;
                    }  
                    Close();
                }
            }
        }

        /// <summary>
        /// Brings to front regular shared object controls (not private areas) 
        /// that intersect with the given private area.
        /// </summary>
        /// <param name="privateArea">The private area.</param>
        private void BringToFrontRegularSharedObjectControlsOnPrivateArea(MouseEventTransparentPictureBox privateArea)
        {
            List<Control> listOfControlsToBringToFront = (from Control control in splitContainerGameField.Panel2.Controls 
                                                          where control is IMouseEventTransparentControl 
                                                          && control != privateArea 
                                                          && control.Width != privateArea.Width 
                                                          && control.Height != privateArea.Height 
                                                          let rectanglePrivateArea = new Rectangle(privateArea.Location, privateArea.Size) 
                                                          let rectangleControl = new Rectangle(privateArea.Location, privateArea.Size) 
                                                          where rectangleControl.IntersectsWith(rectanglePrivateArea) 
                                                          select control).ToList();
            foreach (Control control in listOfControlsToBringToFront)
            {
                control.BringToFront();
            }
        }

        /// <summary>
        ///     Logs an information event message 'eventText' with the id 'eventId' in the Windows event log named
        ///     EventlogLogName.
        ///     and issued from the source EventlogSourceName.
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

        ///// <summary>
        /////     Logs a warning event message 'eventText' with the id 'eventId' in the Windows event log named EventlogLogName.
        /////     and issued from the source EventlogSourceName.
        ///// </summary>
        ///// <param name="eventText">The event text.</param>
        ///// <param name="eventId">The event id.</param>
        //private void LogWarning(string eventText, int eventId)
        //{
        //    if (EventLogActive)
        //    {
        //        EventLog.WriteEntry(EventlogSourceName, eventText, EventLogEntryType.Warning, eventId);
        //    }
        //}

        /// <summary>
        ///     Logs an error event message 'eventText' with the id 'eventId' in the Windows event log named EVENTLOG_LOG_NAME.
        ///     and isssued from the source EVENTLOG_SOURCE_NAME.
        /// </summary>
        /// <param name="eventText">The event text.</param>
        /// <param name="eventId">The event id.</param>
        private void LogError(string eventText, int eventId)
        {
            if (EventLogActive)
            {
                EventLog.WriteEntry(EventlogSourceName, eventText, EventLogEntryType.Error, eventId);
            }
        }

        /// <summary>
        /// Registers the client.
        /// </summary>
        private void RegisterClient()
        {
            // Dispose all currently MouseEventTransparentPictureBox controls representing 
            // sharable objects issued from previous session:
            var controlsToDispose = new List<Control>();
            foreach (Control control in splitContainerGameField.Panel2.Controls)
            {
                if (control is IMouseEventTransparentControl && control.Name != "_pictureBoxRaptor")
                {
                    controlsToDispose.Add(control);
                }
            }
            foreach (var control in controlsToDispose)
            {
                control.Dispose();
            }

            // Abort previous session:
            if ((_client != null))
            {
                LogInfo("Aborting previous session: _client.Abort()", 0);
                try
                {
                    _client.Abort();
                    _client = null;
                }
                catch (Exception ex)
                {
                    LogError(string.Format("Unexpected exception '{0}' was raised when aborting previous session.", ex.Message), 0);
                    return;
                }
            }

            // Prepare the new _ScgBroadcastorServiceClient:
            var cb = new ScgBroadcastorCallback();
            cb.SetHandler(HandleBroadcast);
            var context = new InstanceContext(cb);
            _client = new ScgBroadcastorServiceClient(context);

            // Try to register to the server:
            LogInfo(string.Format("_client.RegisterClient(clientName='{0}')", textBoxPlayerName.Text), 0);
            try
            {
                _client.RegisterClient(textBoxPlayerName.Text, Common.ClientServerProtocolVersion);
            }
            catch (Exception ex)
            {
                LogError(string.Format("Unexpected exception '{0}' was raised when registering client '{1}'.", ex.Message, textBoxPlayerName.Text), 0);
                MessageBoxEx.Show(this, string.Format("Unexpected exception '{0}' was raised when registering client '{1}'." + Environment.NewLine
                    + "An external corporate firewall might block outgoing TCP requests.", ex.Message, textBoxPlayerName.Text));
                return;
            }

            ClientRegisteredName = textBoxPlayerName.Text;
            // Enable registering timeout
            timerRegisteringTimeOut.Enabled = false;
            timerRegisteringTimeOut.Enabled = true;
            labelWarningNotRegistered.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the buttonRegisterPlayer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonRegisterPlayer_Click(object sender, EventArgs e)
        {
            if (textBoxPlayerName.Text == "" || textBoxPlayerName.Text == Resources.Form1_buttonRegisterPlayer_Click_Type_your_name)
            {
                MessageBoxEx.Show(this, Resources.Form1_buttonRegisterPlayer_Click_Player_name_cannot_be_empty__Type_in_your_player_name_and_register_);
                return;
            }
            RegisterClient();
        }


        /// <summary>
        /// Handles the MouseDown events of the MouseEventTransparentControl controls
        /// and redirect them to the GameField panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void MouseEventTransparentControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (sender is IMouseEventTransparentControl)
            {
                Control control = (Control)sender;
                int xTrans = e.X + control.Location.X;
                int yTrans = e.Y + control.Location.Y;
                control = control.Parent;

                while (control != splitContainerGameField.Panel2 && control != null)
                {
                    xTrans += control.Location.X;
                    yTrans += control.Location.Y;
                    control = control.Parent;
                }

                if (control == splitContainerGameField.Panel2)
                {
                    MouseEventArgs eTrans = new MouseEventArgs(e.Button, e.Clicks, xTrans, yTrans, e.Delta);
                    GameField_MouseDown(splitContainerGameField.Panel2, eTrans);
                }
            }
        }

        /// <summary>
        /// Handles the MouseMove events of the MouseEventTransparentControl controls
        /// and redirect them to the GameField panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>        
        private void MouseEventTransparentControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is IMouseEventTransparentControl)
            {
                Control control = (Control)sender;
                int xTrans = e.X + control.Location.X;
                int yTrans = e.Y + control.Location.Y;
                control = control.Parent;

                while (control != splitContainerGameField.Panel2 && control != null)
                {
                    xTrans += control.Location.X;
                    yTrans += control.Location.Y;
                    control = control.Parent;
                }

                if (control == splitContainerGameField.Panel2)
                {
                    MouseEventArgs eTrans = new MouseEventArgs(e.Button, e.Clicks, xTrans, yTrans, e.Delta);
                    GameField_MouseMove(splitContainerGameField.Panel2, eTrans);
                }
            }
        }


        /// <summary>
        /// Handles the MouseUp events of the MouseEventTransparentControl controls
        /// and redirect them to the GameField panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>        
        private void MouseEventTransparentControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (sender is IMouseEventTransparentControl)
            {
                Control control = (Control)sender;
                int xTrans = e.X + control.Location.X;
                int yTrans = e.Y + control.Location.Y;
                control = control.Parent;

                while (control != splitContainerGameField.Panel2 && control != null)
                {
                    xTrans += control.Location.X;
                    yTrans += control.Location.Y;
                    control = control.Parent;
                }

                if (control == splitContainerGameField.Panel2)
                {
                    MouseEventArgs eTrans = new MouseEventArgs(e.Button, e.Clicks, xTrans, yTrans, e.Delta);
                    GameField_MouseUp(splitContainerGameField.Panel2, eTrans);
                }
            }
        }

        /// <summary>
        /// Redirects the mouse events to game field for control.
        /// </summary>
        /// <param name="control">The control.</param>
        private void RedirectMouseEventsToGameFieldForControl(IMouseEventTransparentControl control)
        {
            control.RedirectMouseEvents(MouseEventTransparentControl_MouseDown, MouseEventTransparentControl_MouseMove, MouseEventTransparentControl_MouseUp);
        }


        /// <summary>
        /// Shows the connection error.
        /// </summary>
        private void ShowConnectionError()
        {
            labelWarningNotRegistered.Visible = true;
            if (splitContainerGameField.Panel1Collapsed)
            {
                Console.Beep(5000, 10);
                splitContainerGameField.Panel1Collapsed = false;
            }
        }

        /// <summary>
        /// Handles the MouseDown event of the GameField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void GameField_MouseDown(object sender, MouseEventArgs e)
        {
            if (_client == null)
            {
                ShowConnectionError();
                return;
            }


            try
            {
                _client.NotifyServer(
                    new MouseEventDataType
                    {
                        ClientName = textBoxPlayerName.Text,
                        MouseEventType = MouseEventType.MouseDown,
                        MouseLocation = e.Location,
                        MouseButton = e.Button
                    });
                ResetWatchdog();
            }
            catch (Exception)
            {
                ShowConnectionError();
                RegisterClient();
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the GameField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void GameField_MouseMove(object sender, MouseEventArgs e)
        {
            if (_client == null)
            {
                ShowConnectionError();
                return;
            }

            try
            {
                _client.NotifyServer(
                    new MouseEventDataType
                    {
                        ClientName = textBoxPlayerName.Text,
                        MouseEventType = MouseEventType.MouseMove,
                        MouseLocation = e.Location,
                        MouseButton = e.Button
                    });
                ResetWatchdog();
            }
            catch (Exception)
            {
                ShowConnectionError(); 
                RegisterClient();
            }
        }
 
        /// <summary>
        /// Handles the MouseUp event of the GameField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void GameField_MouseUp(object sender, MouseEventArgs e)
        {
            if (_client == null)
            {
                ShowConnectionError();
                return;
            }

            try
            {
                _client.NotifyServer(
                    new MouseEventDataType
                    {
                        ClientName = textBoxPlayerName.Text,
                        MouseEventType = MouseEventType.MouseUp,
                        MouseLocation = e.Location,
                        MouseButton = e.Button
                    });
                ResetWatchdog();
            }
            catch (Exception)
            {
                ShowConnectionError();
                RegisterClient();
            }
        }

        /// <summary>
        /// Resets the watchdog.
        /// </summary>
        private void ResetWatchdog()
        {
            _lastNotifyServerDateTime = DateTime.Now;
        }

        /// <summary>
        /// Handles the Tick event of the timerWatchdog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void timerWatchdog_Tick(object sender, EventArgs e)
        {
            if (_client != null && DateTime.Now - _lastNotifyServerDateTime >= new TimeSpan(0, 0, 1, 0))
            {
                try
                {
                    _client.NotifyServerWatchdog(
                        new WatchdogDataType
                        {
                            ClientName = textBoxPlayerName.Text
                        });
                    ResetWatchdog();
                }
                catch (Exception)
                {
                    ShowConnectionError(); 
                    RegisterClient();
                }
            }
        }

        /// <summary>
        /// Handles the FormClosing event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogInfo("Form1_FormClosing", 0);
            e.Cancel = false;

            LogInfo(
                string.Format(
                    "_clientNotifyServerCmd(new ClientCommandDataType [ClientName='{0}', ClientCommandType = ClientCommandType.LeavingCmd] )",
                    textBoxPlayerName.Text), 0);

            // Inform the server we are leaving:
            if (_client != null)
            {
                try
                {
                    _client.NotifyServerCmd(
                        new ClientCommandDataType
                        {
                            ClientName = textBoxPlayerName.Text,
                            ClientCommandType = ClientCommandType.LeavingCmd
                        });
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
    

        /// <summary>
        /// Handles the FormClosed event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosedEventArgs"/> instance containing the event data.</param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            LogInfo("Client terminated.", 0);
        }

        private delegate void HandleBroadcastCallback(object sender, EventArgs e);

        /// <summary>
        /// Handles the Tick event of the timerRegisteringTimeOut control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void timerRegisteringTimeOut_Tick(object sender, EventArgs e)
        {
            timerRegisteringTimeOut.Enabled = false;
            MessageBoxEx.Show(this,
                string.Format(Resources.Form1_timerRegisteringTimeOut_Tick_Registration_Failed_Message,
                    textBoxPlayerName.Text), Resources.Form1_timerRegisteringTimeOut_Tick_Registration_Failed);
        }

        private void textBoxPlayerName_TextChanged(object sender, EventArgs e)
        {
            if (textBoxPlayerName.ForeColor.Name == "GrayText" && textBoxPlayerName.Text.Contains(Resources.Form1_buttonRegisterPlayer_Click_Type_your_name))
            {
                textBoxPlayerName.Text = textBoxPlayerName.Text.Remove(textBoxPlayerName.Text.IndexOf(Resources.Form1_buttonRegisterPlayer_Click_Type_your_name, StringComparison.Ordinal), Resources.Form1_buttonRegisterPlayer_Click_Type_your_name.Length);
            }
            textBoxPlayerName.ForeColor = Color.Black;
            textBoxPlayerName.Select(textBoxPlayerName.Text.Length, 0);
        }

        private void MainForm_LocationChanged(object sender, EventArgs e)
        {
            if (_chatForm != null)
            {
                _chatForm.Width = 220;
                _chatForm.Height = 190;
                _chatForm.Left = Left;
                _chatForm.Top = Bottom - _chatForm.Height;
                _chatForm.BringToFront();
            }
        }

        private void toolStripMenuItemExtractFirstCard_Click(object sender, EventArgs e)
        {           
            if (contextMenuStripStackOfCards.SourceControl is MouseEventTransparentPictureBox)
            {
                int sharedObjectId = Convert.ToInt32(((MouseEventTransparentPictureBox)contextMenuStripStackOfCards.SourceControl).Name);
                LogInfo(string.Format("toolStripMenuItemExtractFirstCard_Click shareObjectId={0}",sharedObjectId),33);
                // Notify the server of the ExtractFirstCard command:
                if (_client != null)
                {
                    try
                    {
                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.ExtractFirstCardCmd, SharedObjectId={1}",ClientRegisteredName, sharedObjectId), 34);
                        _client.NotifyServerCmd(
                            new ClientCommandDataType
                            {
                                ClientName = ClientRegisteredName,
                                ClientCommandType = ClientCommandType.ExtractFirstCardCmd,
                                SharedObjectId = sharedObjectId
                            });
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd",ex.Message),34);
                        // ignore
                    }
                }
                
            }
        }

        private void toolStripMenuItemCountCards_Click(object sender, EventArgs e)
        {
            if (contextMenuStripStackOfCards.SourceControl is MouseEventTransparentPictureBox)
            {
                int sharedObjectId = Convert.ToInt32(((MouseEventTransparentPictureBox)contextMenuStripStackOfCards.SourceControl).Name);
                LogInfo(string.Format("toolStripMenuItemCountCards_Click shareObjectId={0}", sharedObjectId), 33);
                // Notify the server of the ExtractFirstCard command:
                if (_client != null)
                {
                    try
                    {
                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.CountCardsCmd, SharedObjectId={1}", ClientRegisteredName, sharedObjectId), 34);
                        _client.NotifyServerCmd(
                            new ClientCommandDataType
                            {
                                ClientName = ClientRegisteredName,
                                ClientCommandType = ClientCommandType.CountCardsCmd,
                                SharedObjectId = sharedObjectId
                            });
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd", ex.Message), 34);
                        // ignore
                    }
                }

            }
        }

        private void toolStripMenuItemShuffleUp_Click(object sender, EventArgs e)
        {
            if (contextMenuStripStackOfCards.SourceControl is MouseEventTransparentPictureBox)
            {
                int sharedObjectId = Convert.ToInt32(((MouseEventTransparentPictureBox)contextMenuStripStackOfCards.SourceControl).Name);
                LogInfo(string.Format("toolStripMenuItemShuffleUp_Click shareObjectId={0}", sharedObjectId), 33);
                // Notify the server of the ExtractFirstCard command:
                if (_client != null)
                {
                    try
                    {
                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.ShuffleUpCmd, SharedObjectId={1}", ClientRegisteredName, sharedObjectId), 34);
                        _client.NotifyServerCmd(
                            new ClientCommandDataType
                            {
                                ClientName = ClientRegisteredName,
                                ClientCommandType = ClientCommandType.ShuffleUpCmd,
                                SharedObjectId = sharedObjectId
                            });
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd", ex.Message), 34);
                        // ignore
                    }
                }

            }
        }

        private void toolStripMenuItemDeal_Click(object sender, EventArgs e)
        {
            if (contextMenuStripStackOfCards.SourceControl is MouseEventTransparentPictureBox)
            {
                int sharedObjectId = Convert.ToInt32(((MouseEventTransparentPictureBox)contextMenuStripStackOfCards.SourceControl).Name);
                LogInfo(string.Format("toolStripMenuItemDeal_Click shareObjectId={0}", sharedObjectId), 33);
                // Notify the server of the ExtractFirstCard command:
                if (_client != null)
                {
                    // MessageBoxEx.Show(this, "Sorry, the dealing function is not yet implemented.", "Warning");

                    try
                    {
                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.RequestDealingParameters, SharedObjectId={1}", ClientRegisteredName, sharedObjectId), 34);
                        _client.NotifyServerCmd(
                            new ClientCommandDataType
                            {
                                ClientName = ClientRegisteredName,
                                ClientCommandType = ClientCommandType.RequestDealingParametersCmd,
                                SharedObjectId = sharedObjectId
                            });
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd", ex.Message), 34);
                        // ignore
                    }
                }

            }
        }

        private void contextMenuStripStackOfCards_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (contextMenuStripStackOfCards.SourceControl is MouseEventTransparentPictureBox)
            {
                if (_controlOwningContextMenuStrip != null)
                {
                    _controlOwningContextMenuStrip.ContextMenuStrip = null;
                }
                int sharedObjectId = Convert.ToInt32(((MouseEventTransparentPictureBox)contextMenuStripStackOfCards.SourceControl).Name);
                LogInfo(string.Format("toolStripStackOfCards_Closed shareObjectId={0}", sharedObjectId), 33);
                // Notify the server of the ExtractFirstCard command:
                if (_client != null)
                {
                    try
                    {
                        LogInfo(string.Format("_client.NotifyServerCmd ClientName={0}, ClientCommandType=ClientCommandType.ContextualMenuClosedCmd, SharedObjectId={1}", ClientRegisteredName, sharedObjectId), 34);
                        _client.NotifyServerCmd(
                            new ClientCommandDataType
                            {
                                ClientName = ClientRegisteredName,
                                ClientCommandType = ClientCommandType.ContextualMenuClosedCmd,
                                SharedObjectId = sharedObjectId
                            });
                    }
                    catch (Exception ex)
                    {
                        LogError(string.Format("ERROR: Unexpected exception '{0} raised on _client.NotifyServerCmd", ex.Message), 34);
                        // ignore
                    }
                }

            }
        
        }

        private void SendPlayerVectorsToBack()
        {
            foreach (Control control in splitContainerGameField.Panel2.Controls)
            {
                if (control is MouseEventTransparentVectorControl)
                {
                    control.SendToBack();
                }
            }
        }
    }
}