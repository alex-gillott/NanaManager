using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanaManagerAPI.EventArgs
{
    public sealed class TagCheckEventArgs : System.EventArgs
    {
        public bool IsActive;
        public int TagIndex;
    }
}
