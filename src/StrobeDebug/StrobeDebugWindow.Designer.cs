namespace StrobeDebug
{
    partial class StrobeDebugWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConsoleWindow = new System.Windows.Forms.TextBox();
            this.Command = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConsoleWindow
            // 
            this.ConsoleWindow.Location = new System.Drawing.Point(12, 12);
            this.ConsoleWindow.MaxLength = 2147483647;
            this.ConsoleWindow.Multiline = true;
            this.ConsoleWindow.Name = "ConsoleWindow";
            this.ConsoleWindow.ReadOnly = true;
            this.ConsoleWindow.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.ConsoleWindow.Size = new System.Drawing.Size(538, 159);
            this.ConsoleWindow.TabIndex = 0;
            // 
            // Command
            // 
            this.Command.Location = new System.Drawing.Point(12, 177);
            this.Command.Name = "Command";
            this.Command.Size = new System.Drawing.Size(457, 20);
            this.Command.TabIndex = 1;
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(475, 176);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(75, 23);
            this.SendButton.TabIndex = 2;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // StrobeDebugWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 211);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.Command);
            this.Controls.Add(this.ConsoleWindow);
            this.Name = "StrobeDebugWindow";
            this.Text = "Strobe Debugger";
            this.Load += new System.EventHandler(this.StrobeDebugWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ConsoleWindow;
        private System.Windows.Forms.TextBox Command;
        private System.Windows.Forms.Button SendButton;
    }
}

