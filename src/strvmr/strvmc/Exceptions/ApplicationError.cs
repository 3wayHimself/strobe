using System;

namespace strvmc
{
    [Serializable]
    public class ApplicationError : Exception
    {
        public ApplicationError(int exitCode) : base(exitCode.ToString())
        { }
    }
}
