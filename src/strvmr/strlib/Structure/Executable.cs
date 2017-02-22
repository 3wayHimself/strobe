namespace strobe.runtime
{
    public class Executable
    {
        public Type type { get; private set; }
        public Executable(Type type)
        {
            this.type = type;
        }
        public enum Type
        {
            DIF,
        };
        public virtual Instruction[] CPU()
        {
            return new Instruction[] { };
        }
    }
}
