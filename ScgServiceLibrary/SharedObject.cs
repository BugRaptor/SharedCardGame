using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ScgServiceLibrary
{
    /// <summary>
    /// Class SharedObject. A shared object is an object placed on the shared game area and that can be displaced by the concurrent players.
    /// Each client will display it with a MouseEventTransparentPictureBox (whose image resource name is given by the Picture property). 
    /// This picture can change according to the state of the represented game element(s), and at a location given by the Rectangle property that
    /// obviously can change according to the moves imposed to the shared object by all the concurrent players.
    /// The represented game element can be a unic GameElement object or a list of stacked game elements. 
    /// </summary>
    public class SharedObject
    {
        private List<SharedObject> _zOrderList;

        #region Public Properties
        /// <summary>
        /// Gets the identifier of the shared object.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the rectangle area occupied by the shared object.
        /// </summary>
        /// <value>The rectangle.</value>
        public Rectangle Rectangle { get; set; }

        /// <summary>
        /// Gets the name of the private owner. If not null the Shared object is a private area and may be displaced by the Private Owner only.
        /// </summary>
        /// <value>The name of the private owner.</value>
        public string PrivateOwnerName { get; private set; }

        /// <summary>
        /// Gets the picture representing the image of the shared object.
        /// </summary>
        /// <value>The picture.</value>
        public string Picture
        {
            get
            {
                string picture = string.Empty;
                if (GameElements.Count > 1 && GameElements.First() is IStackable)
                {
                    picture = (GameElements.First() as IStackable).StackedPicturePrefix;
                }
                GameElement topElement = GetTopElement();
                if (topElement != null)
                {
                    return picture + topElement.VisiblePicture;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the game elements.
        /// A shared object includes on or many stacked game elements.
        /// If many game elements are stacke, the top most one is at index 0 of the List.
        /// </summary>
        /// <value>The list of game elements.</value>
        public List<GameElement> GameElements { get; set; }

        /// <summary>
        /// Gets the Top Left location of the shared object.
        /// </summary>
        /// <value>The Top Left location of the shared object.</value>
        public Point Location
        {
            get { return new Point(Rectangle.Left, Rectangle.Top); }
        }

        /// <summary>
        /// Gets or sets the name of the owner (client that grabs the shared object).
        /// if null, the object is currently free to be grabbed.
        /// </summary>
        /// <value>The name of the owner grabbing the shared element.</value>
        public string GrabberName { get; set; }

        /// <summary>
        /// Gets the Top Left location when the shared object was grabbed.
        /// </summary>
        /// <value>The grabbed location.</value>
        public Point GrabbedLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the contextual menu is opened.
        /// (by the grabber client)
        /// </summary>
        /// <value><c>true</c> if [contextual menu opened]; otherwise, <c>false</c>.</value>
        public bool ContextualMenuOpened { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SharedObject" /> class.
        /// </summary>
        /// <param name="zOrderList">The z order list.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="rectangle">The rectangle.</param>
        /// <param name="privateOwnerName">Name of the private owner.</param>
        /// <param name="gameElements">The game elements.</param>
        public SharedObject(List<SharedObject> zOrderList, int id, Rectangle rectangle, string privateOwnerName, List<GameElement> gameElements )
        {
            _zOrderList = zOrderList;
            Id = id;
            // Apply magnetic grid at creation:
            rectangle.Location = ScgGameState.GetMagneticLocation(rectangle);
            Rectangle = rectangle;
            GrabbedLocation = new Point(Rectangle.Left, Rectangle.Top);
            PrivateOwnerName = privateOwnerName;
            GameElements = gameElements;
            GrabberName = null;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Brings to front the shared object.
        /// </summary>
        public void BringToFront()
        {
            int myIndex = _zOrderList.IndexOf(this);
            _zOrderList.RemoveAt(myIndex);
            _zOrderList.Insert(0, this);
        }

        /// <summary>
        /// Sends to back the shared object.
        /// </summary>
        public void SendToBack()
        {
            int myIndex = _zOrderList.IndexOf(this);
            _zOrderList.RemoveAt(myIndex);
            _zOrderList.Add(this);
        }

        /// <summary>
        /// Moves the shared object to a new location.
        /// </summary>
        /// <param name="newLocation">The new location.</param>
        public void MoveTo(Point newLocation)
        {
            BringToFront();
            Rectangle  = new Rectangle(newLocation, Rectangle.Size);
        }

        /// <summary>
        /// Sets all cards same face.
        /// </summary>
        /// <param name="faceUp">if set to <c>true</c> [face up].</param>
        public void SetAllCardsSameFace(bool faceUp)
        {
            if (GameElements.First() is Card)
            {
                foreach (Card card in GameElements)
                {
                    if (card.FaceUp != faceUp)
                    {
                        card.Flip();
                    }
                }
            }
        }

        public void Shuffle()
        {
            List<GameElement> sortingList = new List<GameElement>();
            var rnd = new Random();


            foreach (var gameElement in GameElements)
            {
                sortingList.Insert(rnd.Next(sortingList.Count + 1), gameElement);
            }
            GameElements.Clear();
            foreach (var gameElement in sortingList)
            {
                GameElements.Add(gameElement);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets the top most element of the list of game elements.
        /// </summary>
        /// <returns>The top most GameElement.</returns>
        public GameElement GetTopElement()
        {
            if (GameElements.Count > 0)
            {
                return GameElements.First();
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
