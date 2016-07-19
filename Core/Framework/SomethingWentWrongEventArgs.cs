using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class SomethingWentWrongEventArgs:EventArgs
    {
        public string Message
        {
            get;
            private set;
        }

        public SomethingWentWrongEventArgs(string Message)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(Message));
            this.Message = Message;
        }
    }
}
