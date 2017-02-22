using System;

namespace strobe.runtime
{
    public class Framework : API
    {
        API[] APIs;
        int[] Interrupts = new int[255];
        public Framework(API[] apr, string name = "Strobe Framework")
        {
            Name = name;
            Start = 0;
            End = 255;
            APIs = apr;
            for (int r = 0; r < APIs.Length; r++)
            {
                for (int i = APIs[r].Start; i <= APIs[r].End; i++)
                {
                    Interrupts[i] = r;
                }
            }

        }
        public override byte[] Interrupt(byte ic, byte[][] Data)
        {
            return APIs[Interrupts[ic]].Interrupt(ic, Data);
        }
    }
}
