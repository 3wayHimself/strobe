using System.Collections.Generic;
namespace strobe.runtime.DIF
{
	public class DIFInstruction
	{
		public Instruction.OpType Op {get; private set;}
		public List<byte> Param = new List<byte>();
		public Instruction toInstruction()
		{
			return new Instruction(Op, Param.ToArray());
		}
        public DIFInstruction(Instruction.OpType op)
		{
			Op = op;
		}
	}
}
