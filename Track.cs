using System;
using System.Collections.Generic;
using System.Text;

namespace MapsetVerifierFramework
{
    internal class Track
    {
        private readonly string message;

        public Track(string aMessage)
        {
            message = aMessage;
            Checker.OnLoadStart(message);
        }

        public void Complete()
        {
            Checker.OnLoadComplete(message);
        }
    }
}
