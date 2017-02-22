using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using strobe.runtime.DIF;

namespace difread
{
    class Read
    {
        Instruction[] x;
        public Read(string file)
        {
            x = new DIFFormat().Load(File.ReadAllBytes(file)).CPU();
            Loop();
        }
        private void Loop()
        {
            foreach (Instruction y in x)
                Out(y);
        }

        private void Out(Instruction i)
        {
            string o;
            try
            {
                o = i.Op.ToString().Substring(0, 5).ToLower();
            }
                catch
            {
                o = i.Op.ToString().ToLower();
            }
            Console.WriteLine("{0}\t\t{1}",o, ParseParams(i.Param));
        }

        private string ParseParams(byte[] param)
        {
            string outp = "";
            switch(param.Length)
            {
                case 1:
                    outp = ((int)param[0]).ToString();
                        break;
                case 4:
                    outp = BitConverter.ToInt32(param,0).ToString();
                    break;
                case 9:
                    if (param[4] != 0xfe)
                    {
                        outp = param.ToString();
                        break;
                    }
                    outp = BitConverter.ToInt32(param, 0).ToString() + "\t" + BitConverter.ToInt32(param, 5).ToString();
                    break;
                default:
                    if (param.Length > 4)
                    {
                        if (param[4] == 0xfe)
                        {
                            outp = BitConverter.ToInt32(param, 0).ToString() + "\t" + Encoding.ASCII.GetString(param.Skip(5).ToArray()) + "'";
                            break;
                        }
                    }
                    outp = "'" + Encoding.ASCII.GetString(param) + "'";
                    break;
            }
            return outp;
        }
    }
}
