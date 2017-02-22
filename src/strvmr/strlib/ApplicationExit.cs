using System;

namespace strobe.runtime
{
    [Serializable]
    public class ApplicationExit : Exception
    {
        public ApplicationExit(int exitCode) : base(exitCode.ToString())
        { }
    }
}
