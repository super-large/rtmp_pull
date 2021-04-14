using RtmpService.Pull;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RtmpClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private RtmpPacketPull _rtmpPull;
        private void button1_Click(object sender, EventArgs e)
        {
            string url = "rtmp://58.200.131.2:1935/livetv/cctv1";
            _rtmpPull = new RtmpPacketPull(url);
            _rtmpPull.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _rtmpPull.Stop();
        }
    }
}
