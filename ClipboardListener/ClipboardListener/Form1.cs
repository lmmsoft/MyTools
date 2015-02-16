using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardListener
{
    public partial class Form1 : Form
    {
        IntPtr NextClipHwnd;
        string previous;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //获得观察链中下一个窗口句柄
            NextClipHwnd = SetClipboardViewer(this.Handle);
        }

        // 撤消自己定义的观察者，并通知下一个观察者
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //从观察链中删除本观察窗口（第一个参数：将要删除的窗口的句柄；第二个参数：//观察链中下一个窗口的句柄 ）
            ChangeClipboardChain(this.Handle, NextClipHwnd);
            //将变动消息WM_CHANGECBCHAIN消息传递到下一个观察链中的窗口 
            SendMessage(NextClipHwnd, WM_CHANGECBCHAIN, this.Handle, NextClipHwnd);
        }

        // 监视剪切板，并把剪切板变化的消息发送给下一个观察者
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    //将WM_DRAWCLIPBOARD消息传递到下一个观察链中的窗口
                    SendMessage(NextClipHwnd, m.Msg, m.WParam, m.LParam);
                    IDataObject iData = Clipboard.GetDataObject();
                    //检测文本
                    if (iData.GetDataPresent(DataFormats.Text) | iData.GetDataPresent(DataFormats.OemText))
                    {
                        string input = (String)iData.GetData(DataFormats.Text);

                        // 防止复制回剪切板之后引发反复调用
                        if (input != previous)
                        {
                            string output = ParserURL(input);

                            this.textBox1.Text = input;
                            this.textBox2.Text = output;

                            if (output.Length > 0)
                            {
                                previous = output;
                                Clipboard.SetDataObject(output);//放回剪切板
                                pictureBox1.ImageLocation = output; //图片预览
                            }
                        }

                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private string ParserURL(string input)
        {
            string str2 = "";
            Match match = new Regex(textBox3.Text).Match(input);
            if (match.Success)
            {
                str2 = match.Groups[1].Value;
                str2 = string.Format("\n\n![]({0})\n\n", str2);
            }
            return str2;
        }


        /// C#中的剪切板编程 http://blog.csdn.net/waxic/article/details/1326234

        // 用于往观察链中添加一个窗口句柄，这个窗口就可成为观察链中的一员了，返回值指向下一个观察者。
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr SetClipboardViewer(IntPtr hwnd);

        // 删除由hwnd指定的观察链成员，这是一个窗口句柄，第二个参数hWndNext是观察链中下一个窗口的句柄
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern IntPtr ChangeClipboardChain(IntPtr hwnd, IntPtr hWndNext);

        // 发送消息
        [System.Runtime.InteropServices.DllImport("user32")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        //两个消息常量
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;

    }
}
