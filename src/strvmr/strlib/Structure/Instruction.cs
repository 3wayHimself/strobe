public class Instruction
{
    public OpType Op { get; private set; }
    public byte[] Param { get; private set; }
    public Instruction(OpType Op, byte[] Param)
    {
        this.Op = Op;
        this.Param = Param;
    }
    public enum OpType
    {
        Null,
        Add,
        Subtract,
        Divide,
        Mutiply,
        Allocate,
        Assign,
        Interrupt,
        Compare,
        Move,
        Addr,
        Goto,
        Label,
        Clear,
    }
}