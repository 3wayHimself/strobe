namespace strobe.runtime
{
    public abstract class Format
    {
        public abstract Executable Load(byte[] Input);
        public abstract byte[] GetBytes(Executable Input);
    }
}
