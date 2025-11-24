using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace BatterylessPatcher
{
    public class ControlWriter : TextWriter
    {
        public bool run = true;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        private const int WM_VSCROLL = 277;
        private const int SB_PAGEBOTTOM = 7;

        public static void ScrollToBottom(RichTextBox MyRichTextBox)
        {
            SendMessage(MyRichTextBox.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);
        }
        private RichTextBox textbox;
        private String buffer = "";
        public ControlWriter(RichTextBox textbox)
        {
            this.textbox = textbox;
            System.Timers.Timer refreshTimer = new System.Timers.Timer();
            refreshTimer.Elapsed += new ElapsedEventHandler(refreshText);
            refreshTimer.Interval = 100; 
            refreshTimer.Enabled = true;
        }
        public readonly Mutex logMutex = new Mutex();
        public bool dataChanged = false;

        private void refreshText(object source, ElapsedEventArgs e)
        {
            try
            {
                logMutex.WaitOne();
                if (dataChanged)
                {
                    textbox.Invoke(new Action(() =>
                    {
                        textbox.Text = buffer;
                        ScrollToBottom(textbox);
                        textbox.Refresh();
                    }));
                    dataChanged = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                logMutex.ReleaseMutex();
            }
            
        }

        public override void Write(char value)
        {
            try
            {
                logMutex.WaitOne();
                buffer += value;
                dataChanged = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                logMutex.ReleaseMutex();
            }
        }

        public override void Write(string value)
        {
            try
            {
                logMutex.WaitOne();
                int lastLineStart = buffer.LastIndexOf("\n");
                if (lastLineStart >= 0)
                {
                    if (value.StartsWith("\r") && !(value.StartsWith("\r\n")))
                    {
                        buffer = buffer.Remove(lastLineStart + 1);
                        buffer += value.Substring(1);
                    }
                    else
                    {
                        buffer += value;
                    }
                }
                else
                {
                    buffer += value;
                }
                dataChanged = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                logMutex.ReleaseMutex();
            }
           
        }


        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }


}
