using strobe.runtime;
using System;
using System.Collections.Generic;

namespace strvmc
{
    public class Process
    {
        API api;
        Processor proc;
        VariableManager Vars;
        Instruction[] Instructions;
        int Current;
        bool Running;
        int[] Labels;

        public int User { private set; get; }
        public string Executable { private set; get; }

        bool hasData;

        public void SetData(int User, string Executable)
        {
            if (!hasData)
            {
                this.User = User;
                this.Executable = Executable;
            }
            else throw new ApplicationError(2);
        }

        public Process(Executable File, int MemorySize, API APIs)
        {
            hasData = false;
            api = APIs;
            Vars = new VariableManager(MemorySize);
            proc = new Processor(Vars);
            Labels = new int[MemorySize / 8];
            for (int i = 0; i < Labels.Length; i++)
                Labels[i] = -1;
            Running = true;
            Current = 0;
            Instructions = File.CPU();
        }

        public bool IsRunning()
        {
            return Running;
        }

        public void Step()
        {
            if (!Running)
                throw new ApplicationNotRunning();
            if (Current >= Instructions.Length)
                throw new ApplicationExit(0);

            int[] args = proc.TwoArgs(Instructions[Current].Param);

            switch (Instructions[Current].Op)
            {
                case Instruction.OpType.Interrupt:
                    Vars.AMem(0, api.Interrupt(Instructions[Current].Param[0], Vars.Get(16)));
                    break;
                case Instruction.OpType.Assign:
                    var by = new List<byte>();
                    bool use = false;
                    foreach (byte z in Instructions[Current].Param)
                    {
                        if (use)
                        {
                            by.Add(z);
                        }
                        use |= z == 254;
                    }
                    Vars.AMem(args[0], by.ToArray());
                    break;
                case Instruction.OpType.Addr:
                    Vars.AMem(args[0], args[1]);
                    break;
                case Instruction.OpType.Move:
                    Vars.AMem(args[0], Vars.AMem(args[1]));
                    break;
                case Instruction.OpType.Allocate:
                    Vars.AMem(args[0], Vars.FreeAddr, args[1]);
                    break;
                case Instruction.OpType.Label:
                    Labels[BitConverter.ToInt32(Vars.AMem(args[0]), 0)] = Current;
                    break;
                case Instruction.OpType.Goto:
                    if (Labels[BitConverter.ToInt32(Vars.AMem(args[0]), 0)] != -1)
                    {
                        if (Vars.AMem(args[1])[0] == 0x1)
                            Current = Labels[BitConverter.ToInt32(Vars.AMem(args[0]), 0)];
                    }
                    break;
                case Instruction.OpType.Clear:
                    Vars.AMemClear(args[0]);
                    break;
                default:
                    Vars.AMem(0, proc.Execute(Instructions[Current]));
                    break;
            }
            Current++;
        }

        public void Kill()
        {
            Running = false;
            Vars.Destroy();
        }
    }
}