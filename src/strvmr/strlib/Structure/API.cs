namespace strobe.runtime
{
    public abstract class API
    {
        public byte Start;
        public byte End;
        public string Name;
        public abstract byte[] Interrupt(byte Current, byte[][] Data);
    }
}
