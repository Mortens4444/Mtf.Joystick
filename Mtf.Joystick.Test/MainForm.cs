using Mtf.MessageBoxes;
using Timer = System.Windows.Forms.Timer;

namespace Mtf.Joystick.Test
{
    public partial class MainForm : Form
    {
        private Timer joystickUpdateTimer;

        public MainForm()
        {
            InitializeComponent();
        }

        private void StartUpdater()
        {
            try
            {
                joystickUpdateTimer = new Timer
                {
                    Interval = 100
                };
                joystickUpdateTimer.Tick += JoystickUpdateTimer_Tick;
                joystickUpdateTimer.Start();
            }
            catch (Exception ex)
            {
                ErrorBox.Show(ex);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopJoystickUpdater();
            JoystickHandler.StopJoystick();
        }

        private void BtnInitializeJoystick_Click(object sender, EventArgs e)
        {
            JoystickHandler.InitializeJoystick(
                () => (int)nudDeviceIndex.Value);
            StartUpdater();
        }

        private void BtnCalibrate_Click(object sender, EventArgs e)
        {
            JoystickHandler.CalibrateJoystick();
        }

        private void JoystickUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() =>
                {
                    UpdateJoystickData();
                }));
            }
            else
            {
                UpdateJoystickData();
            }
        }

        private void UpdateJoystickData()
        {
            if (JoystickHandler.State == null)
            {
                StopJoystickUpdater();
                ErrorBox.Show("General error", $"No joystick found with the desired index {JoystickHandler.DeviceIndex}.");
            }
            else
            {
                lblX.Text = $"X: {JoystickHandler.State.X}";
                lblY.Text = $"Y: {JoystickHandler.State.Y}";
                lblZ.Text = $"Z: {JoystickHandler.State.Z}";
            }
        }

        private void StopJoystickUpdater()
        {
            joystickUpdateTimer?.Stop();
            joystickUpdateTimer?.Dispose();
        }
    }
}
