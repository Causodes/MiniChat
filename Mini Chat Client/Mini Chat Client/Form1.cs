using System;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;

namespace Mini_Chat_Client
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        NetworkStream serverStream = default(NetworkStream);
        string readData = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendButtonClicked();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            readData = "Conected to Chat Server ...";
            msg();
            clientSocket.Connect(textBox4.Text.Trim(), 8080);
            serverStream = clientSocket.GetStream();

            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox3.Text );
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            Thread ctThread = new Thread(getMessage);
            ctThread.Start();

            textBox3.Enabled = false;
            textBox4.Enabled = false;
            button2.Enabled = false;
        }

        private void getMessage()
        {
            while (true)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = 0;
                byte[] inStream = new byte[10025];
                buffSize = clientSocket.ReceiveBufferSize;
                serverStream.Read(inStream, 0, buffSize);
                string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                readData = "" + returndata;
                msg();
            }
        }

        private void msg()
        {
            if (this.InvokeRequired)
            {
                try
                {
                    this.Invoke(new MethodInvoker(msg));
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                textBox1.Text = textBox1.Text + Environment.NewLine + " >> " + readData;
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendButtonClicked();
                //textBox2.Text = "";
            }
        }
        
        private void SendButtonClicked()
        {
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes(textBox2.Text);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
            textBox2.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes("$$$left$$$");
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();
            }
            catch (Exception ex)
            {

            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
