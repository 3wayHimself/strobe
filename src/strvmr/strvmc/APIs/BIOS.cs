using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace strvmc.APIs

{
    public class BIOS
    {
        public string GetPassword()
        {
            List<char> pwd = new List<char>();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Count > 0)
                    {
                        pwd.RemoveAt(pwd.Count - 1);
                    }
                }
                else
                {
                    pwd.Add(i.KeyChar);
                }
            }
            Console.Write("\n");

            string s = "";
            foreach (char c in pwd.ToArray())
                s += c;

            return s;
        }
        public byte[] GetBytes(string s)
        {
            byte[] ret = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
                ret[i] = (byte)s[i];
            return ret;
        }
        public byte[] ReadFile(byte[] file)
        {
            return File.ReadAllBytes(Encoding.ASCII.GetString(file));
        }

        public void WriteFile(byte[] file, byte[] content)
        {
            File.WriteAllBytes(Encoding.ASCII.GetString(file),
                               content);
        }

        public void DeleteFile(byte[] file)
        {
            File.Delete(Encoding.ASCII.GetString(file));
        }

        public void DeleteFolder(byte[] folder)
        {
            Directory.Delete(Encoding.ASCII.GetString(folder));
        }

        public byte[] FolderExists(byte[] folder)
        {
            if (Directory.Exists(Encoding.ASCII.GetString(folder)))
            {
                return new byte[] { 0x1 };
            }
            return new byte[] { 0x0 };
        }

        public byte[] FileExists(byte[] file)
        {
            if (File.Exists(Encoding.ASCII.GetString(file)))
            {
                return new byte[] { 0x1 };
            }
            return new byte[] { 0x0 };
        }

        public void CreateFolder(byte[] folder)
        {
            Directory.CreateDirectory(Encoding.ASCII.GetString(folder));
        }

        public void AppendFile(byte[] file, byte[] content)
        {
            List<byte> c = new List<byte>();
            byte[] one = ReadFile(file);
            foreach (byte b in one)
                c.Add(b);
            foreach (byte b in content)
                c.Add(b);
            WriteFile(file, c.ToArray());
        }

        public byte[] ReadLine()
        {
            return GetBytes(Console.ReadLine());
        }

        public byte[] Read()
        {
            return BitConverter.GetBytes(Console.Read());
        }

        public byte[] ReadKey()
        {
            ConsoleKeyInfo x = Console.ReadKey();
            return BitConverter.GetBytes(x.KeyChar);
        }

        public void Write(byte[] ba, byte[] c)
        {
            if (c[0] == 1)
                Console.Write(((int)ba[0]).ToString());
            else
                Console.Write(Encoding.ASCII.GetString(ba));
        }


        public void Display()
        {
            // Not needed, but kept for compatibility.
        }
    }
}
