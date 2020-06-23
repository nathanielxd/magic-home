using System;
using System.Collections.Generic;
using System.Text;

namespace MagicHome
{
    public class MagicHomeException : Exception
    {
        new public string Message { get; private set; }

        public MagicHomeException(string message) => Message = message;
    }
}
