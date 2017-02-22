using System;

namespace strvmc
{
    [Serializable]
    public class ApplicationNotRunning : Exception
    {
        public ApplicationNotRunning() : base()
        { }
    }
}