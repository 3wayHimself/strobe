using System.Collections.Generic;
namespace strobe.runtime.DIF
{
	public class DIFExecuteable : Executable
	{
		public List<DIFInstruction> insts { get; private set; }

		public DIFExecuteable() : base(Type.DIF)
		{
			insts = new List<DIFInstruction>();
		}

        public void AddInst(DIFInstruction ins)
		{
			insts.Add(ins);
		}

		public override Instruction[] CPU()
		{
			List<Instruction> inst = new List<Instruction>();
			foreach (DIFInstruction i in insts)
			{
				inst.Add(i.toInstruction());
			}
			return inst.ToArray();
		}
	}
}
