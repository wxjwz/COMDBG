﻿/**
 
 * Copyright (c) 2014, Wenhuix, All rights reserved.

 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.

 * Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.

 * Neither the name of COMDBG nor the names of its contributors may 
 * be used to endorse or promote products derived from this software without 
 * specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
 * THE POSSIBILITY OF SUCH DAMAGE.
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows;
using System.Windows.Forms;

namespace COMDBG
{
    public interface IView
    {
        void SetController(IController controller);
        void OpenComEvent(Object sender, SerialPortEventArgs e);
        void CloseComEvent(Object sender, SerialPortEventArgs e);
        void ComReceiveDataEvent(Object sender, SerialPortEventArgs e);
    }

    public partial class MainForm : Form, IView
    {
        private IController controller;
        private int sendBytesCount = 0;
        private int receiveBytesCount = 0;

        delegate void ComCallBack(Object model, SerialPortEventArgs e);

        public MainForm()
        {
            InitializeComponent();
            InitializeCOMCombox();
            this.statusTimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.toolStripStatusTx.Text = "Sent: 0";
            this.toolStripStatusRx.Text = "Received: 0";
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        public void SetController(IController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// Initialize serial port infomation
        /// </summary>
        private void InitializeCOMCombox()
        {
            //BaudRate
            baudRateCbx.Items.Add(4800);
            baudRateCbx.Items.Add(9600);
            baudRateCbx.Items.Add(19200);
            baudRateCbx.Items.Add(38400);
            baudRateCbx.Items.Add(57600);
            baudRateCbx.Items.Add(115200);
            baudRateCbx.Items.ToString();
            //get 9600 print in text
            baudRateCbx.Text = baudRateCbx.Items[1].ToString();

            //Data bits
            dataBitsCbx.Items.Add(7);
            dataBitsCbx.Items.Add(8);
            //get the 8bit item print it in the text 
            dataBitsCbx.Text = dataBitsCbx.Items[1].ToString();

            //Stop bits
            stopBitsCbx.Items.Add("One");
            stopBitsCbx.Items.Add("OnePointFive");
            stopBitsCbx.Items.Add("Two");
            //get the One item print in the text
            stopBitsCbx.Text = stopBitsCbx.Items[0].ToString();

            //Parity
            parityCbx.Items.Add("None");
            parityCbx.Items.Add("Even");
            parityCbx.Items.Add("Mark");
            parityCbx.Items.Add("Odd");
            parityCbx.Items.Add("Space");
            //get the first item print in the text
            parityCbx.Text = parityCbx.Items[0].ToString();

            //Com Ports
            string[] ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length == 0)
            {
                statuslabel.Text = "No COM found !";
                openCloseSpbtn.Enabled = false;
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comListCbx.Items.Add(ArrayComPortsNames[i]);
                }
                comListCbx.Text = ArrayComPortsNames[0];
                openCloseSpbtn.Enabled = true;
            }
        }

        /// <summary>
        /// update status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenComEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.openCloseSpbtn.InvokeRequired || sendbtn.InvokeRequired)
            {
                ComCallBack cb = new ComCallBack(OpenComEvent);
                this.Invoke(cb, new object[] { sender, e });
            }
            else
            {
                if (e.isOpend)  //Open successfully
                {
                    statuslabel.Text = comListCbx.Text + " Opend";
                    openCloseSpbtn.Text = "Close";
                    sendbtn.Enabled = true;
                    autoSendcbx.Enabled = true;

                    comListCbx.Enabled = false;
                    baudRateCbx.Enabled = false;
                    dataBitsCbx.Enabled = false;
                    stopBitsCbx.Enabled = false;
                    parityCbx.Enabled = false;
                    refreshbtn.Enabled = false;

                    if (autoSendcbx.Checked)
                    {
                        autoSendtimer.Start();
                        sendtbx.ReadOnly = true;
                    }
                }
                else    //Open failed
                {
                    statuslabel.Text = "Open failed !";
                    sendbtn.Enabled = false;
                    autoSendcbx.Enabled = false;
                }
            }
        }

        /// <summary>
        /// update status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CloseComEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.openCloseSpbtn.InvokeRequired || sendbtn.InvokeRequired)
            {
                ComCallBack cb = new ComCallBack(CloseComEvent);
                this.Invoke(cb, new object[] { sender, e });
            }
            else
            {
                if (!e.isOpend) //close successfully
                {
                    statuslabel.Text = comListCbx.Text + " Closed";
                    openCloseSpbtn.Text = "Open";

                    sendbtn.Enabled = false;
                    sendtbx.ReadOnly = false;
                    autoSendcbx.Enabled = false;
                    autoSendtimer.Stop();

                    comListCbx.Enabled = true;
                    baudRateCbx.Enabled = true;
                    dataBitsCbx.Enabled = true;
                    stopBitsCbx.Enabled = true;
                    parityCbx.Enabled = true;
                    refreshbtn.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Display received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComReceiveDataEvent(Object sender, SerialPortEventArgs e)
        {
            if (this.receivetbx.InvokeRequired)
            {
                ComCallBack cb = new ComCallBack(ComReceiveDataEvent);
                try
                {
                    this.Invoke(cb, new object[] { sender, e });
                }
                catch (System.Exception)
                {
                	//disable form destroy exception
                }
                
            }
            else
            {
                if (recStrRadiobtn.Checked) //display as string
                {
                    receivetbx.AppendText(e.receivedString);
                }
                else //display as hex
                {
                    if (receivetbx.Text.Length > 0)
                    {
                        receivetbx.AppendText("-");
                    }
                    receivetbx.AppendText(IController.String2Hex(e.receivedString));
                }
                //update status bar
                receiveBytesCount += e.receivedString.Length;
                toolStripStatusRx.Text = "Received: "+receiveBytesCount.ToString();
            }

        }

        /// <summary>
        /// Auto scroll in receive textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivetbx_TextChanged(object sender, EventArgs e)
        {
            receivetbx.SelectionStart = receivetbx.Text.Length;
            receivetbx.ScrollToCaret();
        }

        /// <summary>
        /// update time in status bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void statustimer_Tick(object sender, EventArgs e)
        {
            this.statusTimeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        /// <summary>
        /// open or close serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openCloseSpbtn_Click(object sender, EventArgs e)
        {
            if (openCloseSpbtn.Text == "Open")
            {
                controller.OpenSerialPort(comListCbx.Text, baudRateCbx.Text,
                    dataBitsCbx.Text, stopBitsCbx.Text, parityCbx.Text);
            } 
            else
            {
                controller.CloseSerialPort();
            }
        }

        /// <summary>
        /// Refresh soft to find Serial port device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshbtn_Click(object sender, EventArgs e)
        {
            comListCbx.Items.Clear();
            //Com Ports
            string[] ArrayComPortsNames = SerialPort.GetPortNames();
            if (ArrayComPortsNames.Length == 0)
            {
                statuslabel.Text = "No COM found !";
                openCloseSpbtn.Enabled = false;
            }
            else
            {
                Array.Sort(ArrayComPortsNames);
                for (int i = 0; i < ArrayComPortsNames.Length; i++)
                {
                    comListCbx.Items.Add(ArrayComPortsNames[i]);
                }
                comListCbx.Text = ArrayComPortsNames[0];
                openCloseSpbtn.Enabled = true;
                statuslabel.Text = "OK !";
            }
            
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendbtn_Click(object sender, EventArgs e)
        {
            String sendText = sendtbx.Text;
            if (sendText == null)
            {
                return;
            }
            //If hex radio checked, should convert to string first
            if (sendHexRadiobtn.Checked)
            {
                sendText = IController.Hex2String(sendText);
            }
            //send data to serial port
            controller.SendDataToCom(sendText);
            sendBytesCount += sendText.Length;
            toolStripStatusTx.Text = "Sent: " + sendBytesCount.ToString();
        }

        /// <summary>
        /// clear text in send area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearSendbtn_Click(object sender, EventArgs e)
        {
            sendtbx.Text = "";
            toolStripStatusTx.Text = "Sent: 0";
            sendBytesCount = 0;
        }

        /// <summary>
        /// clear receive text in receive area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearReceivebtn_Click(object sender, EventArgs e)
        {
            receivetbx.Text = "";
            toolStripStatusRx.Text = "Received: 0";
            receiveBytesCount = 0;
        }

        /// <summary>
        /// String to hex
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recHexRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (recHexRadiobtn.Checked)
            {
                if (receivetbx.Text == null)
                {
                    return;
                }
                receivetbx.Text = IController.String2Hex(receivetbx.Text);
            }
        }

        /// <summary>
        /// Hex to string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recStrRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (recStrRadiobtn.Checked)
            {
                if (receivetbx.Text == null)
                {
                    return;
                }
                receivetbx.Text = IController.Hex2String(receivetbx.Text);
            }
        }

        /// <summary>
        /// String to Hex
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendHexRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (sendHexRadiobtn.Checked)
            {
                if (sendtbx.Text == null)
                {
                    return;
                }
                sendtbx.Text = IController.String2Hex(sendtbx.Text);
            }
        }

        /// <summary>
        /// Hex to string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendStrRadiobtn_CheckedChanged(object sender, EventArgs e)
        {
            if (sendStrRadiobtn.Checked)
            {
                if (sendtbx.Text == null)
                {
                    return;
                }
                sendtbx.Text = IController.Hex2String(sendtbx.Text);
            }
        }

        /// <summary>
        /// Filter illegal input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendtbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Input Hex, should like: AF-1B-09
            if (sendHexRadiobtn.Checked)
            {
                e.Handled = true;
                int length = sendtbx.SelectionStart;
                switch (length % 3)
                {
                case 0:
                case 1:
                    if ((e.KeyChar >= 'a' && e.KeyChar <= 'f')
                        || (e.KeyChar >= 'A' && e.KeyChar <= 'F')
                        || char.IsDigit(e.KeyChar)
                        || (char.IsControl(e.KeyChar) && e.KeyChar != (char)13))
                    {
                        e.Handled = false;
                    }
                    break;
                case 2:
                    if (e.KeyChar == '-'
                        || (char.IsControl(e.KeyChar) && e.KeyChar != (char)13))
                    {
                        e.Handled = false;
                    }
                    break;
                }
            }
        }


        /// <summary>
        /// Auto send data to serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoSendcbx_CheckedChanged(object sender, EventArgs e)
        {
            if (autoSendcbx.Checked)
            {
                autoSendtimer.Enabled = true;
                autoSendtimer.Interval = int.Parse(sendtimetbx.Text);
                autoSendtimer.Start();

                //disable send botton and textbox
                sendtimetbx.Enabled = false;
                sendtbx.ReadOnly = true;
                sendbtn.Enabled = false;
            }
            else
            {
                autoSendtimer.Enabled = false;
                autoSendtimer.Stop();

                //enable send botton and textbox
                sendtimetbx.Enabled = true;
                sendtbx.ReadOnly = false;
                sendbtn.Enabled = true;
            }
        }

        private void autoSendtimer_Tick(object sender, EventArgs e)
        {
            sendbtn_Click(sender, e);
        }

        /// <summary>
        /// filter illegal input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendtimetbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// save received data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void receivedDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "txt file|*.txt";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "received.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = saveFileDialog.FileName;
                System.IO.File.WriteAllText(fName, receivetbx.Text);
            }
        }

        /// <summary>
        /// save send data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Filter = "txt file|*.txt";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.FileName = "send.txt";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                String fName = saveFileDialog.FileName;
                System.IO.File.WriteAllText(fName, sendtbx.Text);
            }
        }

        /// <summary>
        /// Quit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// about me
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.StartPosition = FormStartPosition.CenterParent;
            about.Show();

            if (about.StartPosition == FormStartPosition.CenterParent)
            {
                var x = Location.X + (Width - about.Width) / 2;
                var y = Location.Y + (Height - about.Height) / 2;
                about.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
            }
        }

        /// <summary>
        /// Help
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm help = new HelpForm();
            help.StartPosition = FormStartPosition.CenterParent;
            help.Show();

            if (help.StartPosition == FormStartPosition.CenterParent)
            {
                var x = Location.X + (Width - help.Width) / 2;
                var y = Location.Y + (Height - help.Height) / 2;
                help.Location = new Point(Math.Max(x, 0), Math.Max(y, 0));
            }
        }

    }
}
