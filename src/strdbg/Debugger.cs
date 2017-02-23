using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace strdbg
{
    public class Debugger
    {
        Process process = new Process();
        Thread readTh, errTh;
        List<string> error, output;
        bool abort;
        public Debugger()
        {
            error = new List<string>();
            output = new List<string>();
            abort = false;
            process.StartInfo.FileName = "strdbg.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
        }

        public bool isRunning()
        {
            return !process.HasExited;
        }

        public void Send(string message)
        {
            if (process.HasExited)
                throw new Exception("Debugger is not running");
            writeIn(message);
        }

        public string Read(bool isError = false)
        {
            string ret = "";
            if (isError == true)
            {
                foreach (string s in error)
                    ret += s;
                error.Clear();
            }
            else
            {
                foreach (string s in output)
                    ret += s;
                output.Clear();
            }
            return ret;
        }

        public void Start()
        {
            readTh = new Thread(readOut);
            readTh.Name = "CmdStdOutTh";
            errTh = new Thread(readErr);
            errTh.Name = "CmdStdErrTh";

            process.Start();
            readTh.Start();
            errTh.Start();
        }

        void processError(string e)
        {
            error.Add(e);
        }

        void processOutput(string o)
        {
            output.Add(o);
        }

        void writeIn(string write)
        {
            bool isDone = false;
            while(!isDone)
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    if (thread.ThreadState == System.Diagnostics.ThreadState.Wait && thread.WaitReason == ThreadWaitReason.UserRequest)
                    {
                        process.StandardInput.Write(write);
                        isDone = true;
                        break;
                    }
                }
            }
        }
        void readErr()
        {
            char[] buf = new char[256];
            while ((!process.HasExited || (process.StandardError.Peek() >= 0)) && !abort)
            {
                int n = process.StandardError.Read(buf, 0, buf.Length - 1);
                buf[n] = '\0';
                if (n > 0)
                    processError(new string(buf));
                Thread.Sleep(0);
            }
        }
        void readOut()
        {
            char[] buf = new char[256];
            int n = 0;
            while ((!process.HasExited || (process.StandardOutput.Peek() >= 0)) && !abort)
            {
                n = process.StandardOutput.Read(buf, 0, buf.Length - 1);
                buf[n] = '\0';
                if (n > 0)
                    processOutput(new string(buf));
                Thread.Sleep(0);
            }
        }
    }
}
