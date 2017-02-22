using strobe.runtime;
using System;

namespace strvmc.APIs
{
    class StrobeAPI : API
    {
        BIOS bios;
        public StrobeAPI()
        {
            Name = "StrobeAPI";
            bios = new BIOS();
            Start = 0;
            End = 14;
        }
        public override byte[] Interrupt(byte Current, byte[][] Data)
        {
            switch (Current)
            {
                case 0:
                    throw new ApplicationExit(BitConverter.ToInt32(Data[1], 0));
                case 1:
                    bios.Write(Data[1], Data[2]);
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 2:
                    bios.Display();
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 3:
                    Data[0] = bios.Read();
                    break;
                case 4:
                    Data[0] = bios.ReadLine();
                    break;
                case 5:
                    Data[0] = bios.ReadKey();
                    break;
                case 6:
                    Data[0] = bios.ReadFile(Data[1]);
                    break;
                case 7:
                    bios.WriteFile(Data[1], Data[2]);
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 8:
                    bios.AppendFile(Data[1], Data[2]);
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 9:
                    bios.CreateFolder(Data[1]);
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 10:
                    Data[0] = bios.FileExists(Data[1]);
                    break;
                case 11:
                    Data[0] = bios.FolderExists(Data[1]);
                    break;
                case 12:
                    bios.DeleteFile(Data[1]);
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 13:
                    bios.DeleteFolder(Data[1]);
                    Data[0][0] = 0;
                    Data[0][1] = 0;
                    Data[0][2] = 0;
                    Data[0][3] = 0;
                    break;
                case 14:
                    Data[0] = bios.GetBytes(bios.GetPassword());
                    break;
            }
            return Data[0];
        }
    }
}
