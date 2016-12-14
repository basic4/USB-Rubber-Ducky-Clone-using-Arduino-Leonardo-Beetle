using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DuckyEncoder
{
    public partial class frm_Main : Form
    {
        public frm_Main()
        {
            InitializeComponent();
        }

        String ScriptToExecute = "";
        BinaryWriter dataStreamWriter;
        String mapType = "US";

        #region "Command Processing"

        private void processLine(String comline)
        {
            String[] parts;
            if (comline.StartsWith("STRING"))
            {
                //Send the string chars one at a time
                String resultant = comline.Substring(7);
                sendData(resultant);
                return;
            }
            else if (comline.StartsWith("DELAY"))
            {
                //Wait given millsecs
                int delTime = Convert.ToInt32(comline.Substring(6));
                if (delTime > 0)
                {
                    sendData(0xff);  //delay command code
                    int d = delTime;
                    int dex = 0;
                    //in units of 50ms
                    while (d > 0)
                    {
                        dex++;
                        d = d - 50;
                    }
                    sendData(dex);
                    sendData(0x32);
                }
                return;
            }
            else if(comline.StartsWith("REM"))
            {
                //Remark line - just ignore it.
                return;
            }
            else if (comline.StartsWith("ENTER"))
            {
                //Send a Return
                byte k = (byte)176;  //'B0'
                sendData(k);
                return;
            }
            else if (comline.StartsWith("COMD"))
            {
                //Send full command line plus return
                String resultant = comline.Substring(5);
                sendCommandData(resultant);
                return;
            }
            else if (comline.StartsWith("KEY"))
            {
                //Send a windows ALT key combo (eg. 'ALT + 0124')
                //Windows ALT keys(
                sendData(252 & 0xff);
                //numberpad keys inputs
                String nums = comline.Substring(4);
                nums = nums.Replace(" ", "");
                nums = nums.Replace(System.Environment.NewLine, "");
                char[] bc = nums.ToCharArray();
                for (int x = 0; x < nums.Length; x++)
                {
                    int ky = getNumericPad(bc[x]);
                    sendData(ky & 0xff);
                }
                //signal sequence end
                sendData(253 & 0xff);
                return;
            }
            else if (comline.StartsWith("MAP"))
            {
                String typx = comline.Substring(4);
                typx = typx.Replace(" ", "");
                typx = typx.Replace(System.Environment.NewLine, "");
                mapType = typx;
                return;
            }
            else
            {
                //Compound Command or Special Key
                String[] dif = new String[] { "+" };
                if (comline.IndexOf(dif[0]) > 0)
                {
                    //Multi-Key Special Character
                    comline = comline.Replace("\\r?\\n", "");
                    parts = comline.Split(dif, StringSplitOptions.RemoveEmptyEntries);

                    sendData(251 & 0xff); //signal multi start
                    foreach (String part in parts)
                    {
                        String mako = part.Replace(" ", "");
                        if (getKeyCode(mako) > 0)
                        {
                            sendData(getKeyCode(mako) & 0xff);
                        }
                        else
                        {
                            sendData(mako);
                        }
                    }
                    sendData(254 & 0xff); //signal multi end
                    return;
                }
                else
                {
                    //Single Special Key
                    parts = new String[1];
                    parts[0] = comline.ToString();
                    String sect = parts[0].Replace(" ", "");
                    if (getKeyCode(sect) > 0)
                    {
                        sendData(getKeyCode(sect) & 0xff);
                    }
                    return;
                }

            }
        }
        private int getKeyCode(String subcom)
        {
            //Get the correct keycode
            int resultant = 0;
            int keyVal = -1;
            switch (subcom)
            {
                case "CTRL":
                    resultant = 128;
                    break;
                case "SHIFT":
                    //left shifttest
                    resultant = 129;
                    break;
                case "ALT":
                    resultant = 130;
                    break;
                case "TAB":
                    resultant = 179;
                    break;
                case "GUI":
                    //left GUI (windows)
                    resultant = 131;
                    break;
                case "GUI_R":
                    resultant = 135;
                    break;
                case "ESC":
                    resultant = 177;
                    break;
                case "MENU":
                    resultant = 237;
                    break;
                case "BACKSPACE":
                    resultant = 178;
                    break;
                case "INS":
                    resultant = 209;
                    break;
                case "DEL":
                    resultant = 212;
                    break;
                case "HOME":
                    resultant = 210;
                    break;
                case "ALTGR":
                    resultant = 134;
                    break;
                case "CTRLR":
                    resultant = 132;
                    break;
                case "SHIFTR":
                    resultant = 133;
                    break;
                case "F1":
                    resultant = 194;
                    break;
                case "F2":
                    resultant = 195;
                    break;
                case "F3":
                    resultant = 196;
                    break;
                case "F4":
                    resultant = 197;
                    break;
                case "F5":
                    resultant = 198;
                    break;
                case "F6":
                    resultant = 199;
                    break;
                case "F7":
                    resultant = 200;
                    break;
                case "F8":
                    resultant = 201;
                    break;
                case "F9":
                    resultant = 202;
                    break;
                case "F10":
                    resultant = 203;
                    break;
                case "F11":
                    resultant = 204;
                    break;
                case "F12":
                    resultant = 205;
                    break;
                case "CAPS_LOCK":
                    resultant = 193;
                    break;
                case "PAGE_UP":
                    resultant = 211;
                    break;
                case "PAGE_DOWN":
                    resultant = 214;
                    break;
                case "UP":
                    resultant = 218;
                    break;
                case "DWN":
                    resultant = 217;
                    break;
                case "LFT":
                    resultant = 216;
                    break;
                case "RHT":
                    resultant = 215;
                    break;
                default:
                    resultant = keyVal;
                    break;
            }
            return (resultant);
        }
        private char replaceKey(char inp)
        {
            //Needed because of the keycode differences between
            //US and UK keyboards. Others are not supported
            char repKey = inp;
            switch (mapType)
            {
                case "UK":
                    switch ((int)inp)
                    {
                        case 64:
                            //@
                            repKey = (char)34;
                            break;
                        case 34:
                            // "
                            repKey = (char)64;
                            break;
                        case 35:
                            //#
                            repKey = (char)186;
                            break;
                        case 126:
                            //~
                            repKey = (char)124;
                            break;
                        case 47:
                            // Forward slash (/)
                            repKey = (char)192;
                            break;
                        case 92:
                            // Back slash (\)
                            repKey = (char)0xec;
                            break;
                        default:
                            repKey = inp;
                            break;
                    }

                    return (repKey);
            }
            return (repKey);
        }

        int getNumericPad(char inx)
        {
            //Ruturn the corresponding numeric pad
            //keycode
            int vx = (int)inx;
            if (vx > 48)
            {
                vx = vx - 48;
                return (vx + 224);
            }
            else
            {
                return (234);
            }
        }
        #endregion


        #region   "Stream IO methods"
        void sendCommandData(String inputx)
        {
            try
            {
                String msg = inputx;
                msg += "\n";
                foreach (byte b in msg.ToCharArray())
                {
                    if (mapType == "UK" && b == 0x7C)
                    {
                        sendUKPipe();
                    }
                    else
                    {
                        byte k = (byte)replaceKey((char)(b & 0xff));
                        dataStreamWriter.Write((sbyte)k);
                    }
                }
            }
            catch (Exception ex)
            {
                errDisp(ex);
            }
        }

        private void sendData(String inpx)
        {
            try
            {
                foreach (byte b in inpx.ToCharArray())
                {
                    if (mapType == "UK" && b == 0x7C)
                    {
                        sendUKPipe();
                    }
                    else
                    {
                        char t = replaceKey((char)b);
                        dataStreamWriter.Write((sbyte)t);
                    }
                }
            }
            catch (Exception ex)
            {
                errDisp(ex);
            }
        }

        private void sendData(byte inx)
        {
            try
            {
                if (mapType == "UK" && inx == 0x7C)
                {
                    sendUKPipe();
                }
                else
                {
                    dataStreamWriter.Write((sbyte)inx);
                }
            }
            catch (Exception ex)
            {
                errDisp(ex);
            }
        }

        private void sendData(int ipx)
        {
            try
            {
                ipx = ipx & 0xff;
                dataStreamWriter.Write((sbyte)ipx);
            }
            catch (Exception ex)
            {
                errDisp(ex);
            }
        }

        private void sendUKPipe()
        {
            sendData(251 & 0xff);
            sendData(129 & 0xff);
            sendData(0xec & 0xff);
            sendData(254 & 0xff);
        }
        
        #endregion


        private void encodeScript(String file)
        {
            //execute a laoded script line by line
            try
            {
                String[] parts = file.Split(new string[1] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String part in parts)
                {
                    processLine(part);
                }
            }
            catch (Exception ex)
            {
                errDisp(ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Open the script load screen
            this.label2.Text = "";
            openFileDialog1.Multiselect = false;
            openFileDialog1.Title = "Load Ducky Script File";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    System.IO.StreamReader sr = new
                    System.IO.StreamReader(openFileDialog1.FileName);
                    ScriptToExecute = sr.ReadToEnd();
                    sr.Close();
                    textBox1.Text = openFileDialog1.FileName.ToString();
                    this.label2.Text = "PRESS 'ENCODE' TO MAKE SCRIPT";
                    this.label2.ForeColor = Color.Blue;
                }
                catch (Exception ex)
                {
                    errDisp(ex);
                }

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
                if(textBox1.Text.Length > 5)
                {
                    try
                    {
                        if (File.Exists("sctipt.bin"))
                        {
                            File.Delete("script.bin");
                        }
                        dataStreamWriter = new BinaryWriter(new FileStream("script.bin", FileMode.Create));
                        encodeScript(ScriptToExecute);
                        dataStreamWriter.Close();
                        this.label2.Text = "DUCKY SCRIPT GENERATED OK";
                        this.label2.ForeColor = Color.Red;

                    }
                    catch (Exception ex)
                    {
                        errDisp(ex);
                    }
                }
                else
                {
                    this.label2.Text = "PLEASE 'LOAD' A SOURCE FILE ABOVE";
                    this.label2.ForeColor = Color.Red;
                }
        }

        private void errDisp(Exception ex)
        {
            this.label2.Text = "ERROR";
            this.label2.ForeColor = Color.Red;
            MessageBox.Show(ex.Message.ToString());

        }

        private void frm_Main_Load(object sender, EventArgs e)
        {
            this.label2.Text = "PRESS 'LOAD' TO SELECT FILE";
            this.label2.ForeColor = Color.Blue;
        }

    }
}
