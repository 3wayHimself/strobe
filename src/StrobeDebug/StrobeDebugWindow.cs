using System;
using strdbg;
using System.Windows.Forms;
using System.Threading;

namespace StrobeDebug
{
    public partial class StrobeDebugWindow : Form
    {
        Debugger debug;
        Thread updater;
        public StrobeDebugWindow()
        {
            updater = new Thread(update);
            debug = new Debugger();
            InitializeComponent();
        }

        private void StrobeDebugWindow_Load(object sender, EventArgs e)
        {
            debug.Start();
            updater.Start();
        }

        void update()
        {
            while (true)
            {
                if (debug.isRunning())
                {
                    string text = "";
                    text += "[OUT] " + debug.Read(false);
                    text += "[ERR] " + debug.Read(true);

                    try
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            ConsoleWindow.Text += text;
                            SendButton.Enabled = true;
                            Command.Enabled = true;
                        });
                    }
                    catch (Exception) { }
                }
                else
                {
                    Invoke((MethodInvoker)delegate
                    {
                        SendButton.Enabled = false;
                        Command.Enabled = false;
                    });
                }
                Thread.Sleep(10);
            }
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (debug.isRunning())
            {
                debug.Send(Command.Text);
                Command.Text = "";
            }
        }
    }
}
