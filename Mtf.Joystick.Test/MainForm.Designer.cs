namespace Mtf.Joystick.Test
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCalibrate = new Button();
            lblX = new Label();
            lblY = new Label();
            lblZ = new Label();
            nudDeviceIndex = new NumericUpDown();
            btnInitializeJoystick = new Button();
            ((System.ComponentModel.ISupportInitialize)nudDeviceIndex).BeginInit();
            SuspendLayout();
            // 
            // btnCalibrate
            // 
            btnCalibrate.Location = new Point(12, 128);
            btnCalibrate.Name = "btnCalibrate";
            btnCalibrate.Size = new Size(75, 23);
            btnCalibrate.TabIndex = 0;
            btnCalibrate.Text = "Calibrate";
            btnCalibrate.UseVisualStyleBackColor = true;
            btnCalibrate.Click += BtnCalibrate_Click;
            // 
            // lblX
            // 
            lblX.AutoSize = true;
            lblX.Location = new Point(12, 51);
            lblX.Name = "lblX";
            lblX.Size = new Size(17, 15);
            lblX.TabIndex = 1;
            lblX.Text = "X:";
            // 
            // lblY
            // 
            lblY.AutoSize = true;
            lblY.Location = new Point(12, 76);
            lblY.Name = "lblY";
            lblY.Size = new Size(17, 15);
            lblY.TabIndex = 2;
            lblY.Text = "Y:";
            // 
            // lblZ
            // 
            lblZ.AutoSize = true;
            lblZ.Location = new Point(12, 100);
            lblZ.Name = "lblZ";
            lblZ.Size = new Size(17, 15);
            lblZ.TabIndex = 3;
            lblZ.Text = "Z:";
            // 
            // nudDeviceIndex
            // 
            nudDeviceIndex.Location = new Point(12, 14);
            nudDeviceIndex.Name = "nudDeviceIndex";
            nudDeviceIndex.Size = new Size(52, 23);
            nudDeviceIndex.TabIndex = 4;
            // 
            // btnInitializeJoystick
            // 
            btnInitializeJoystick.Location = new Point(70, 14);
            btnInitializeJoystick.Name = "btnInitializeJoystick";
            btnInitializeJoystick.Size = new Size(115, 23);
            btnInitializeJoystick.TabIndex = 5;
            btnInitializeJoystick.Text = "Initialize joystick";
            btnInitializeJoystick.UseVisualStyleBackColor = true;
            btnInitializeJoystick.Click += BtnInitializeJoystick_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(328, 216);
            Controls.Add(btnInitializeJoystick);
            Controls.Add(nudDeviceIndex);
            Controls.Add(lblZ);
            Controls.Add(lblY);
            Controls.Add(lblX);
            Controls.Add(btnCalibrate);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Joystick Test";
            FormClosing += MainForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)nudDeviceIndex).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCalibrate;
        private Label lblX;
        private Label lblY;
        private Label lblZ;
        private NumericUpDown nudDeviceIndex;
        private Button btnInitializeJoystick;
    }
}
