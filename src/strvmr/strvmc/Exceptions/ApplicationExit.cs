using System;

namespace strvmc
{
    [Serializable]
    public class ApplicationExit : Exception
    {
        public ApplicationExit(int exitCode) : base(exitCode.ToString())
        { }
    }
}
