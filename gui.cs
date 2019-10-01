using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using iRSDKSharp;

namespace iRacingSLI {
    public partial class frmMain : Form {

        static SerialPort SP;
        iRacingSDK sdk = new iRacingSDK();

        int Gear;
        double Speed, RPM, Fuel, Shift;
        short iRPM, iFuel, iShift;
        byte Engine;
        byte[] serialdata = new byte[8];
        byte[] shiftlights = new byte[16];

        System.Random randnum = new System.Random();

        public frmMain() {
            InitializeComponent();
        }

        private void tmr_Tick(object sender, EventArgs e) {
            if (chkDebug.Checked == true) {
                lblConn.Text = "Debug mode active!";
                lblColor.BackColor = Color.FromArgb(255, 129, 0);

                Gear = trkGear.Value;
                Speed = randnum.Next(0, 255);
                RPM = randnum.Next(4253, 17954);
                Fuel = trkFuel.Value;
                Shift = trkShift.Value;

                if (chkPit.Checked == true)
                    Engine = 0x10;
                else
                    Engine = 0x00;

                iRPM = Convert.ToInt16(RPM);
                iFuel = Convert.ToByte(Math.Round(Fuel));
                iShift = Convert.ToByte(Math.Round((Shift * 16) / 100));

                Console.Out.WriteLine("iRPM: " + iRPM);
                int c = (iRPM >> 8) & 0x00FF;

                
                serialdata[0] = 255;
                serialdata[1] = Convert.ToByte(Gear + 1);
                serialdata[2] = Convert.ToByte(Speed);
                serialdata[3] = Convert.ToByte((iRPM >> 8) & 0x00FF);
                serialdata[4] = Convert.ToByte(iRPM & 0x00FF);
                serialdata[5] = Convert.ToByte(iFuel);
                serialdata[6] = Convert.ToByte(iShift);
                serialdata[7] = Engine;

                SP.Write(serialdata, 0, 8);

            } else {
                if (sdk.IsConnected()) {
                    lblConn.Text = "Connected to iRacing API";
                    lblColor.BackColor = Color.FromArgb(0, 200, 0);

                    Gear = Convert.ToInt32(sdk.GetData("Gear"));
                    Speed = Convert.ToDouble(sdk.GetData("Speed")) * 2.23693629;
                    RPM = Convert.ToDouble(sdk.GetData("RPM"));
                    Fuel = Convert.ToDouble(sdk.GetData("FuelLevelPct"));
                    Shift = Convert.ToDouble(sdk.GetData("ShiftIndicatorPct"));
                    Engine = Convert.ToByte(sdk.GetData("EngineWarnings"));

                    this.Text = Shift.ToString();

                    iRPM = Convert.ToInt16(RPM);
                    iFuel = Convert.ToByte(Math.Round(Fuel * 100));
                    iShift = Convert.ToByte(Math.Round((Shift * 100 * 16) / 100));

                    serialdata[0] = 255;
                    serialdata[1] = Convert.ToByte(Gear + 1);
                    serialdata[2] = Convert.ToByte(Speed);
                    serialdata[3] = Convert.ToByte((iRPM >> 8) & 0x00FF);
                    serialdata[4] = Convert.ToByte(iRPM & 0x00FF);
                    serialdata[5] = Convert.ToByte(iFuel);
                    serialdata[6] = Convert.ToByte(iShift);
                    serialdata[7] = Engine;

                    SP.Write(serialdata, 0, 8);
                } else if (sdk.IsInitialized) {
                    lblConn.Text = "No connection with iRacing API";
                    lblColor.BackColor = Color.FromArgb(200, 0, 0);

                    sdk.Shutdown();
                } else {
                    lblConn.Text = "No connection with iRacing API";
                    lblColor.BackColor = Color.FromArgb(200, 0, 0);

                    sdk.Startup();
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e) {
            String[] ports = SerialPort.GetPortNames();
            cboPorts.Items.AddRange(ports);
            cboPorts.SelectedIndex = 0;

            lblConn.Text = "No connection with iRacing API";
        }

        private void cmbSerial_Click(object sender, EventArgs e) {
            if (cmbSerial.Text == "Start serial port") {
                SP = new SerialPort(cboPorts.Text, 9600, Parity.None, 8);
                SP.Open();
                tmr.Enabled = true;
                cmbSerial.Text = "Stop serial port";
                chkDebug.Enabled = true;
            } else {
                SP.Close();
                tmr.Enabled = false;
                cmbSerial.Text = "Start serial port";
                chkDebug.Enabled = false;
            }
        }

        private void chkDebug_CheckedChanged(object sender, EventArgs e) {
            if (chkDebug.Checked == true) {
                this.Height = 424;
            } else {
                this.Height = 192;
            }

        }

    }
}
