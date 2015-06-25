using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
namespace PacketReader
{
    public partial class Form1 : Form
    {
        PacketReader reader;
        Thread t;
        private void refreshClientList()
        {
            comboClients.Items.Clear();
            foreach (Process p in Process.GetProcessesByName("Tibia"))
            {
                comboClients.Items.Add(p.MainWindowTitle + " [" + p.Id + "]");
            }
            if (comboClients.Items.Count >0)
            {
                comboClients.Text = comboClients.GetItemText(comboClients.Items[0]);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process pr = null;
            foreach (Process p in Process.GetProcessesByName("Tibia"))
            {
                string[] id = comboClients.Text.Split(new Char [] {'['});
                if (p.Id ==  Convert.ToInt32(id[id.Length - 1].Substring(0,id[id.Length - 1].Length-1)))
                {
                    pr = p;
                    break;
                }
               
            }
            if (pr == null)
            {
                MessageBox.Show("Error finding client", "Error");
                return;
            }
            reader = new PacketReader(pr);
            if (!reader.Inject())
            {
                MessageBox.Show("Error injecting dll", "Error");
            }
            else
            {
                button1.Text = "Injected!";
                tmrRead.Enabled = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            refreshClientList();
 
            
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            refreshClientList();
        }
        private void tmrRead_Tick(object sender, EventArgs e)
        {
            List<byte> buff = new List<byte>();
            reader.readPacket(ref buff);
            if (buff.Count > 0)
            {
                listBox1.Items.Add(BitConverter.ToString(buff.ToArray(),8));
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                Thread.Sleep(100);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                Clipboard.SetText(Convert.ToString(listBox1.Items[listBox1.SelectedIndex]));
            }
        }
    }
}
