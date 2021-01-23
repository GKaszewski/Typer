using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Typer
{
    public partial class Form1 : Form
    {
        private float interval = 0.0f;
        private Queue<string> data = new Queue<string>();

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        public Form1()
        {
            InitializeComponent();

            int startHotKeyID = 1;
            int stopHotKeyID = 2;
            int startHotkeyCode = (int)Keys.F5;
            int stopHotkeyCode = (int)Keys.F6;
            var f5Registered = RegisterHotKey(this.Handle, startHotKeyID, 0x0000, startHotkeyCode);
            var f6Registered = RegisterHotKey(this.Handle, stopHotKeyID, 0x0000, stopHotkeyCode);

            if (!f5Registered) statusLabel.Text = "Couldn't set f5 shortcut.";
            if (!f6Registered) statusLabel.Text = "Couldn't set f6 shortcut.";

            interval = getIntervalFromSecsToMs(intervalSlider.Value);
            intervalLabel.Text = $"{interval} ms";
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                var id = m.WParam.ToInt32();
                if (id == 1)
                {
                    initializeData();
                    timer1.Start();
                }

                if (id == 2)
                {
                    timer1.Stop();
                }
            }

            base.WndProc(ref m);
        }

        private float getIntervalFromSecsToMs(float val)
        {
            return (val / 1000.0f) * 1000.0f;
        }

        private void initializeData()
        {
            data.Clear();
            var content = contentTextbox.Text;
            foreach (var ch in content)
            {
                var s = Convert.ToString(ch);
                
                data.Enqueue(s);
            }
        }

        private void intervalSlider_Scroll(object sender, EventArgs e)
        {
            interval = getIntervalFromSecsToMs(intervalSlider.Value);
            timer1.Interval = (int)interval;
            intervalLabel.Text = $"{interval} ms";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var result = data.TryDequeue(out string s);

            if (result)
            {
                s = Regex.Replace(s, "[+^%~(){}]", "{$0}");
                SendKeys.Send(s);
            }
            else
            {
                timer1.Stop();
            }

        }


        private void startButton_Click(object sender, EventArgs e)
        {
            initializeData();

            timer1.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void resetButton_Click(object sender, EventArgs e)
        { 
            initializeData();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            contentTextbox.Text = "";
            data.Clear();
        }
    }
}
