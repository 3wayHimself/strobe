using System.Collections.Generic;
using System;
namespace strvmc
{
    public class Processor
    {
        VariableManager var;
        public Processor(VariableManager var)
        {
            this.var = var;
        }
        public byte[] Execute(Instruction i)
        {
            switch (i.Op)
            {
                case Instruction.OpType.Compare:
                    return Comp(i.Param);
                case Instruction.OpType.Subtract:
                    return Sub(i.Param);
                case Instruction.OpType.Mutiply:
                    return Mul(i.Param);
                case Instruction.OpType.Add:
                    return Add(i.Param);
                case Instruction.OpType.Divide:
                    return Div(i.Param);
                default:
                    throw new ApplicationError(0);
            }
        }

        byte[] Comp(byte[] ar)
        {
            if (ar.Length < 3)
            {
                throw new ApplicationError(4);
            }
            switch (ar[0])
            {
                case 0:
                    return Equ(RemoveFirst(ar));
                case 1:
                    return Neq(RemoveFirst(ar));
                case 2:
                    return Lss(RemoveFirst(ar));
                case 3:
                    return Mor(RemoveFirst(ar));
                default:
                    throw new ApplicationError(5);

            }
        }

        byte[] RemoveFirst(byte[] arr)
        {
            List<byte> n = new List<byte>();
            for (int i = 0; i < arr.Length; i++)
                n.Add(arr[i]);
            n.RemoveAt(0);
            return n.ToArray();
        }

        public void Error(int i)
        {
            throw new ApplicationError(i);
        }

        byte[] Equ(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] == ret[1]);
        }

        public void Halt()
        {
            throw new ApplicationExit(1);
        }

        byte[] Neq(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] != ret[1]);
        }

        byte[] Mor(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] > ret[1]);
        }

        byte[] Lss(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] < ret[1]);
        }

        byte[] Sub(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] - ret[1]);
        }

        byte[] Mul(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] * ret[1]);
        }

        byte[] Div(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] / ret[1]);
        }

        byte[] Add(byte[] ar)
        {
            int[] ret = TwoArgs(ar);
            ret[0] = BitConverter.ToInt32(var.AMem(ret[0]), 0);
            ret[1] = BitConverter.ToInt32(var.AMem(ret[1]), 0);
            return BitConverter.GetBytes(ret[0] + ret[1]);
        }

        public int[] TwoArgs(byte[] ar)
        {
            bool isTwo = false;
            List<byte> one = new List<byte>();
            List<byte> two = new List<byte>();
            foreach (byte b in ar)
            {
                if (b == 254)
                {
                    isTwo = true;
                    continue;
                }
                if (isTwo == false)
                {
                    one.Add(b);
                }
                else
                {
                    two.Add(b);
                }
            }
            while (one.Count < 4)
            {
                one.Add(0);
            }
            while (two.Count < 4)
            {
                two.Add(0);
            }
            while (one.Count > 4)
            {
                one.RemoveAt(one.Count - 1);
            }
            while (two.Count > 4)
            {
                two.RemoveAt(two.Count - 1);
            }
            int x = BitConverter.ToInt32(one.ToArray(), 0);
            int y = BitConverter.ToInt32(two.ToArray(), 0);
            int[] ret = new int[2];
            ret[0] = x;
            ret[1] = y;
            return ret;
        }
    }
}