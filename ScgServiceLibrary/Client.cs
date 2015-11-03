using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScgServiceLibrary
{
    class Client
    {
        public string Name { get; private set; }
        public SharedObject GrabbedSharedObject { get; set; }
        public Point GrabbedRelativePosition { get; set; }
        public IScgBroadcastorCallBack ScgBroadcastorCallBack { get; set; }
   
        public Client(string name, IScgBroadcastorCallBack scgBroadcastorCallBack)
        {
            Name = name;
            GrabbedSharedObject = null;
            ScgBroadcastorCallBack = scgBroadcastorCallBack;
        }
    }
}
