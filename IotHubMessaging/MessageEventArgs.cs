using System;

namespace Safern.Hub.Reader
{
    public class MessageEventArgs : EventArgs
    {
        public string Data { get; }

        public MessageEventArgs(string data)
        {
            Data = data;
        }
    }
}