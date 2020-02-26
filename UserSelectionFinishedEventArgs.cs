using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginTest02
{

    public class UserSelectionFinishedEventArgs : System.EventArgs
    {
        public int userID
        {
            get;
            private set;
        }

        public UserSelectionFinishedEventArgs(int userID)
        {
            this.userID = userID;
        }
    }
}
