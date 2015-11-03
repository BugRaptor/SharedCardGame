using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.VisualStyles;

namespace ScgServiceLibrary
{
    /// <summary>
    ///     Class ScgGameState.
    ///     Defines the state of the game table with the list of shared objects
    /// </summary>
    internal class ScgGameState
    {
        #region Public Constants
        /// <summary>
        /// The magnetic grid step in pixels.
        /// </summary>
        public const int MagneticGridStep = 9;
        /// <summary>
        /// The gamefield dim x
        /// </summary>
        public const int GamefieldDimX = 1024;
        /// <summary>
        /// The gamefield dim y
        /// </summary>
        public const int GamefieldDimY = 620;

        public const int DimXCard = 43;
        public const int DimYCard = 58;
        public const int DimXStackOfCards = 47;
        public const int DimYStackOfCards = 62;
        #endregion

        #region Public Static Methods
        public static Point GetMagneticLocation(Rectangle rectangle)
        {
            int step = MagneticGridStep;
            return new Point((rectangle.X + rectangle.Height / 2) / step * step + (step - rectangle.Height) / 2,
                             (rectangle.Y + rectangle.Width / 2) / step * step + (step - rectangle.Width) / 2);
        }
        #endregion

        private ScgBroadcastorService ScgBroadcastorService { get; set; }
  

        #region Constructors
        public ScgGameState(ScgBroadcastorService scgBroadcastorService)
        {
            ScgBroadcastorService = scgBroadcastorService;
            ZOrderListOfSharedObjects = new List<SharedObject>();

            char[] suitChars =
            {
                'S',
                'H',
                'D',
                'C'
            };
            char[] heightChars =
            {
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'T',
                'J',
                'Q',
                'K',
                'A'
            };
            var intTable = new List<int>();
            for (var i = 0; i < 52; i++)
            {
                intTable.Add(i);
            }

            /* 
            //To shuffle up the initial positions of cards
            var shuffelingTable = new int[52];
            var rnd = new Random();
            for (var i = 0; i < 52; i++)
            {
                var index = rnd.Next(intTable.Count);
                shuffelingTable[i] = intTable[index];
                intTable.RemoveAt(index);
            }
            */

            var id = 0;
            //int iSuitShuffeled;
            //int iHeightShuffeled;
            for (var iSuit = 0; iSuit < 4; iSuit++)
            {
                for (var iHeight = 0; iHeight < 13; iHeight++)
                {
                    var picture = "Crd" + heightChars[iHeight] + suitChars[iSuit];
                    /*
                    //To shuffle up the initial positions of cards
                    var cardIndex = shuffelingTable[iSuit*13 + iHeight];
                    iHeightShuffeled = cardIndex/4;
                    iSuitShuffeled = cardIndex%4;

                    // Expose the cards face down and shuffled up
                    ZOrderListOfSharedObjects.Insert(0,
                        new SharedObject(ZOrderListOfSharedObjects, id,
                            new Rectangle(220 + iHeightShuffeled * MagneticGridStep * 5, 180 + iSuitShuffeled * MagneticGridStep * 7, DimXCard, DimYCard), null,
                            new List<GameElement>(new[] {new Card(picture, "CrdBackB", "Stk", false)})));
                    */

                    // Expose the cards face up and not shuffled up
                    ZOrderListOfSharedObjects.Insert(0,
                        new SharedObject(ZOrderListOfSharedObjects, id,
                            new Rectangle(220 + iHeight * MagneticGridStep * 5, 180 + iSuit * MagneticGridStep * 7, DimXCard, DimYCard), null,
                            new List<GameElement>(new[] { new Card(picture, "CrdBackB", "Stk", true) })));
                    id++;
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the z order list of shared objects.
        ///     The top most object is at index 0
        /// </summary>
        /// <value>The z order list of shared objects.</value>
        public List<SharedObject> ZOrderListOfSharedObjects { get; set; }

        #endregion

        #region Public Methods
        /// <summary>
        /// Finds the private area of a client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <returns>The private area SharedObject.</returns>
        public SharedObject FindPrivateArea(string clientName)
        {
            foreach (var sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.PrivateOwnerName == clientName)
                {
                    return sharedObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the minimum free identifier.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetFreeId()
        {
            var id = 0;
            bool foundId;
            do
            {
                foundId = false;
                foreach (var sharedObject in ZOrderListOfSharedObjects)
                {
                    if (sharedObject.Id == id)
                    {
                        foundId = true;
                        break;
                    }
                }
                if (foundId)
                {
                    id++;
                }
            } while (foundId);
            return id;
        }

        /// <summary>
        /// Creates a private area for the given client.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <param name="newSharedObject">if set to <c>true</c> the private area was created; otherwise it was already existing.</param>
        /// <returns>SharedObject.</returns>
        public SharedObject CreatePrivateArea(string clientName, out bool newSharedObject)
        {
            newSharedObject = false;
            var foundSharedObject = FindPrivateArea(clientName);
            if (foundSharedObject == null)
            {
                newSharedObject = true;
                var id = GetFreeId();
                var theNewSharedObject = new SharedObject(ZOrderListOfSharedObjects, id,
                    new Rectangle(3, 3, 180, 100), clientName, new List<GameElement>(new[] {new GameElement("wood")}));
                ZOrderListOfSharedObjects.Insert(id, theNewSharedObject);
                theNewSharedObject.BringToFront();
                return theNewSharedObject;
            }
            return foundSharedObject;
        }

        /// <summary>
        /// Removes one client's private area of the ZOrderList of shared objects.
        /// </summary>
        /// <param name="clientName">Name of the client.</param>
        /// <returns>System.Int32.</returns>
        public int RemovePrivateArea(string clientName)
        {
            SharedObject foundSharedObject = null;
            foreach (var sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.PrivateOwnerName == clientName)
                {
                    foundSharedObject = sharedObject;
                    break;
                }
            }
            if (foundSharedObject != null)
            {
                ZOrderListOfSharedObjects.RemoveAt(ZOrderListOfSharedObjects.IndexOf(foundSharedObject));
                return foundSharedObject.Id;
            }
            return -1;
        }

        /// <summary>
        /// Gets the top most shared object occupying a given gamefield location.
        /// </summary>
        /// <param name="pt">The pt. where to search a shared object</param>
        /// <returns>SharedObject.</returns>
        public SharedObject GetSharedObjectAt(Point pt)
        {
            foreach (SharedObject sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.Rectangle.Contains(pt))
                {
                    return sharedObject;
                }
            }
            return null;
        }

        public SharedObject GetSharedObjectFromId(int id)
        {
            foreach (SharedObject sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.Id == id)
                {
                    return sharedObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Determines whether the specified pt is in an other client's private area.
        /// </summary>
        /// <param name="pt">The pt location.</param>
        /// <param name="clientName">Name of the asking client.</param>
        /// <returns></returns>
        public bool IsInOtherClientPrivateAreaAt(Point pt, string clientName)
        {
            foreach (var sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.Rectangle.Contains(pt) && sharedObject.PrivateOwnerName != null)
                {
                    return sharedObject.PrivateOwnerName != clientName;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if the given private area contains other shared objects.
        /// </summary>
        /// <param name="privateArea">The private area.</param>
        /// <returns>true if the given private area contains other shared objects</returns>
        public bool PrivateAreaIsNotEmpty(SharedObject privateArea)
        {
            if (privateArea.PrivateOwnerName == null)
            {
                return true; // Not a private area
            }

            foreach (var sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject == privateArea)
                {
                    return false;
                }
                if (sharedObject != privateArea && privateArea.Rectangle.Contains(sharedObject.Rectangle))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Finds the enclosing private area.
        /// </summary>
        /// <param name="enclosedSharedObject">The enclosed shared object.</param>
        /// <returns>null or the enclosing shared object if found.</returns>
        public SharedObject FindEnclosingPrivateArea(SharedObject enclosedSharedObject)
        {
            foreach (var sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.PrivateOwnerName != null &&
                    sharedObject.Rectangle.Contains(enclosedSharedObject.Rectangle))
                {
                    return sharedObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to stack the grabbedSharedObject on another shared object.
        /// The grabbedShared object has to be composed of stackable elements and to be brought to front.
        /// The first shared object found under the grabbed shared object has also to be composed of stackable 
        /// elements of the same kind and must be placed at the same location.
        /// </summary>
        /// <param name="grabbedSharedObject">The grabbed shared object.</param>
        /// <param name="stackObject">The stack object.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool TryToStackOnAnotherSharedObject(SharedObject grabbedSharedObject, out SharedObject stackObject)
        {
            stackObject = null;
            if (!(grabbedSharedObject.GameElements.First() is IStackable))
            {
                return false;
            }
            if (ZOrderListOfSharedObjects.First() != grabbedSharedObject)
            {
                return false;
            }
            foreach (SharedObject sharedObject in ZOrderListOfSharedObjects)
            {
                if (sharedObject.Rectangle.IntersectsWith(grabbedSharedObject.Rectangle) && sharedObject != grabbedSharedObject)
                {
                    // sharedObject is the first shared object found under the grabbed one. 
                    Point centerSharedObject = new Point(sharedObject.Location.X + sharedObject.Rectangle.Width / 2,
                        sharedObject.Location.Y + sharedObject.Rectangle.Height / 2);
                    Point centerGrabbedObject = new Point(grabbedSharedObject.Location.X + grabbedSharedObject.Rectangle.Width / 2,
                        grabbedSharedObject.Location.Y + grabbedSharedObject.Rectangle.Height / 2);
                    if (centerSharedObject.Equals(centerGrabbedObject) &&
                        sharedObject.GameElements.First() is IStackable &&
                        sharedObject.GameElements.First().GetType() == grabbedSharedObject.GameElements.First().GetType())
                    {
                        // stacking is possible:
                        stackObject = sharedObject;
                    }
                    break;
                }
            }
            if (stackObject != null)
            {
                if (stackObject.GameElements.Count < 2 && stackObject.GameElements.First() is Card)
                {
                    // The stack object was a single card. Change its size for a stack of cards: 
                    stackObject.Rectangle = new Rectangle(stackObject.Rectangle.X, stackObject.Rectangle.Y, DimXStackOfCards, DimYStackOfCards);
                    // and apply again magnetic grid:
                    stackObject.Rectangle = new Rectangle(GetMagneticLocation(stackObject.Rectangle),stackObject.Rectangle.Size);
                }
                // Stack the elements
                for (int i = grabbedSharedObject.GameElements.Count - 1; i >= 0; i--)
                {
                    GameElement gameElement = grabbedSharedObject.GameElements[i];
                    stackObject.GameElements.Insert(0, gameElement);
                }
                // Remove the grabbed shared object from the ZList:
                ZOrderListOfSharedObjects.RemoveAt(0);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gathers the cards in the specified rectangle.
        /// </summary>
        /// <param name="gatheringRectangle">The gathering rectangle.</param>
        public void GatherCards(Rectangle gatheringRectangle)
        {
            SharedObject gatheringStackSharedObject = null;
            for (int i = ZOrderListOfSharedObjects.Count - 1; i >= 0; i--)
            {
                SharedObject sharedObject = ZOrderListOfSharedObjects[i];
                if (gatheringRectangle.Contains(sharedObject.Rectangle) && sharedObject.GrabberName == null &&
                    sharedObject.GameElements.First() is Card)
                {
                    gatheringStackSharedObject = sharedObject;
                    break;
                }
            }
            if (gatheringStackSharedObject != null)
            {
                gatheringStackSharedObject.SetAllCardsSameFace(false);
                List<SharedObject> listSharedObjectsToGather = new List<SharedObject>();
                for (int i = ZOrderListOfSharedObjects.Count - 1; i >= 0; i--)
                {
                    SharedObject sharedObject = ZOrderListOfSharedObjects[i];
                    if (gatheringRectangle.Contains(sharedObject.Rectangle) && sharedObject.GrabberName == null &&
                        sharedObject.GameElements.First() is Card && sharedObject != gatheringStackSharedObject)
                    {
                        sharedObject.SetAllCardsSameFace(false);
                        listSharedObjectsToGather.Add(sharedObject);
                    }
                }
                foreach (SharedObject sharedObject in listSharedObjectsToGather)
                {
                    GatherCards(sharedObject, gatheringStackSharedObject);
                }

                // gatheringStackSharedObject.Shuffle();

                // Broadcast the gathering shared object move event:
                StateChangeEventDataType stateChangeEventDataType = new StateChangeEventDataType();
                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                stateChangeEventDataType.SharedObjectId = gatheringStackSharedObject.Id;
                stateChangeEventDataType.NewSharedObjectLocation = gatheringStackSharedObject.Location;
                stateChangeEventDataType.SharedObjectSize = gatheringStackSharedObject.Rectangle.Size;
                stateChangeEventDataType.NewSharedObjectPicture = gatheringStackSharedObject.Picture;
                ScgBroadcastorService.BroadcastStateChange(stateChangeEventDataType, null, null);

            }   
        }

        /// <summary>
        /// Gathers the cards of sharedObject in the GameElments of gatheringObject.
        /// </summary>
        /// <param name="sharedObject">The shared object.</param>
        /// <param name="gatheringObject">The gathering object.</param>
        public void GatherCards(SharedObject sharedObject, SharedObject gatheringObject)
        {
            if (sharedObject.GameElements.First() is Card && gatheringObject.GameElements.First() is Card)
            {
                if (gatheringObject.GameElements.Count == 1)
                {
                    // The gathering object was a single card. Change its size for a stack of cards: 
                    gatheringObject.Rectangle = new Rectangle(gatheringObject.Rectangle.X, gatheringObject.Rectangle.Y, DimXStackOfCards, DimYStackOfCards);
                    // and apply again magnetic grid:
                    gatheringObject.Rectangle = new Rectangle(GetMagneticLocation(gatheringObject.Rectangle), gatheringObject.Rectangle.Size);
                   
                }
                // Transfer the cards:
                try
                {
                    foreach (Card card in sharedObject.GameElements)
                    {
                        gatheringObject.GameElements.Insert(0, card);
                    }
                }
                catch (Exception ex)
                {
                    ScgBroadcastorService.LogError(string.Format("Unexpected exception '{0}' in GameState.GatherCards in foreach loop doing gatheringObject.GameElements.Add(card);", ex.Message), 97);
                    throw;
                }

                // Broadcast the gathering shared object move event:
                StateChangeEventDataType stateChangeEventDataType = new StateChangeEventDataType();
                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.SharedObjectMove;
                stateChangeEventDataType.SharedObjectId = gatheringObject.Id;
                stateChangeEventDataType.NewSharedObjectLocation = gatheringObject.Location;
                stateChangeEventDataType.SharedObjectSize = gatheringObject.Rectangle.Size;
                stateChangeEventDataType.NewSharedObjectPicture = gatheringObject.Picture;
                ScgBroadcastorService.BroadcastStateChange(stateChangeEventDataType, null, null);

                // Remove the source shared object and broadcast the dispose event:
                ZOrderListOfSharedObjects.Remove(sharedObject);
                stateChangeEventDataType.StateChangeEventType = StateChangeEventType.DisposeSharedObject;
                stateChangeEventDataType.SharedObjectId = sharedObject.Id;
                ScgBroadcastorService.BroadcastStateChange(stateChangeEventDataType, null, null);
            }
        }
        #endregion
    }
}