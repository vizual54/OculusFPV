namespace MavLinkTest
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comPortComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.yawLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.rollLabel = new System.Windows.Forms.Label();
            this.pitchLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.yawTrackBar = new System.Windows.Forms.TrackBar();
            this.rollTrackBar = new System.Windows.Forms.TrackBar();
            this.pitchTrackBar = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.disconnectButton = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.baudRateComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.mavLinkTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yawTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rollTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitchTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // comPortComboBox
            // 
            this.comPortComboBox.FormattingEnabled = true;
            this.comPortComboBox.Location = new System.Drawing.Point(70, 11);
            this.comPortComboBox.Name = "comPortComboBox";
            this.comPortComboBox.Size = new System.Drawing.Size(121, 21);
            this.comPortComboBox.TabIndex = 0;
            this.comPortComboBox.SelectedIndexChanged += new System.EventHandler(this.comPortComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Com port:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            this.splitContainer1.Panel1.Controls.Add(this.disconnectButton);
            this.splitContainer1.Panel1.Controls.Add(this.connectButton);
            this.splitContainer1.Panel1.Controls.Add(this.baudRateComboBox);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.comPortComboBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.mavLinkTextBox);
            this.splitContainer1.Size = new System.Drawing.Size(598, 424);
            this.splitContainer1.SplitterDistance = 134;
            this.splitContainer1.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.yawLabel);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.rollLabel);
            this.groupBox1.Controls.Add(this.pitchLabel);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.yawTrackBar);
            this.groupBox1.Controls.Add(this.rollTrackBar);
            this.groupBox1.Controls.Add(this.pitchTrackBar);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(8, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(579, 82);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Camera Controls";
            // 
            // yawLabel
            // 
            this.yawLabel.AutoSize = true;
            this.yawLabel.Location = new System.Drawing.Point(483, 28);
            this.yawLabel.Name = "yawLabel";
            this.yawLabel.Size = new System.Drawing.Size(13, 13);
            this.yawLabel.TabIndex = 15;
            this.yawLabel.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(449, 28);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Yaw";
            // 
            // rollLabel
            // 
            this.rollLabel.AutoSize = true;
            this.rollLabel.Location = new System.Drawing.Point(292, 28);
            this.rollLabel.Name = "rollLabel";
            this.rollLabel.Size = new System.Drawing.Size(13, 13);
            this.rollLabel.TabIndex = 13;
            this.rollLabel.Text = "0";
            // 
            // pitchLabel
            // 
            this.pitchLabel.AutoSize = true;
            this.pitchLabel.Location = new System.Drawing.Point(101, 28);
            this.pitchLabel.Name = "pitchLabel";
            this.pitchLabel.Size = new System.Drawing.Size(13, 13);
            this.pitchLabel.TabIndex = 12;
            this.pitchLabel.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(261, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Roll";
            // 
            // yawTrackBar
            // 
            this.yawTrackBar.Location = new System.Drawing.Point(386, 49);
            this.yawTrackBar.Maximum = 90;
            this.yawTrackBar.Minimum = -90;
            this.yawTrackBar.Name = "yawTrackBar";
            this.yawTrackBar.Size = new System.Drawing.Size(184, 45);
            this.yawTrackBar.TabIndex = 10;
            this.yawTrackBar.TickFrequency = 10;
            this.yawTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.yawTrackBar.Scroll += new System.EventHandler(this.yawTrackBar_Scroll);
            // 
            // rollTrackBar
            // 
            this.rollTrackBar.Location = new System.Drawing.Point(196, 49);
            this.rollTrackBar.Maximum = 90;
            this.rollTrackBar.Minimum = -90;
            this.rollTrackBar.Name = "rollTrackBar";
            this.rollTrackBar.Size = new System.Drawing.Size(184, 45);
            this.rollTrackBar.TabIndex = 9;
            this.rollTrackBar.TickFrequency = 10;
            this.rollTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.rollTrackBar.Scroll += new System.EventHandler(this.rollTrackBar_Scroll);
            // 
            // pitchTrackBar
            // 
            this.pitchTrackBar.Location = new System.Drawing.Point(6, 49);
            this.pitchTrackBar.Maximum = 90;
            this.pitchTrackBar.Minimum = -90;
            this.pitchTrackBar.Name = "pitchTrackBar";
            this.pitchTrackBar.Size = new System.Drawing.Size(184, 45);
            this.pitchTrackBar.TabIndex = 7;
            this.pitchTrackBar.TickFrequency = 10;
            this.pitchTrackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.pitchTrackBar.Scroll += new System.EventHandler(this.pitchTrackBar_Scroll);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(64, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Pitch";
            // 
            // disconnectButton
            // 
            this.disconnectButton.Enabled = false;
            this.disconnectButton.Location = new System.Drawing.Point(507, 9);
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(75, 23);
            this.disconnectButton.TabIndex = 5;
            this.disconnectButton.Text = "Disconnect";
            this.disconnectButton.UseVisualStyleBackColor = true;
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(426, 9);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 4;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // baudRateComboBox
            // 
            this.baudRateComboBox.FormattingEnabled = true;
            this.baudRateComboBox.Location = new System.Drawing.Point(283, 10);
            this.baudRateComboBox.Name = "baudRateComboBox";
            this.baudRateComboBox.Size = new System.Drawing.Size(121, 21);
            this.baudRateComboBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Baud Rate:";
            // 
            // mavLinkTextBox
            // 
            this.mavLinkTextBox.DetectUrls = false;
            this.mavLinkTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mavLinkTextBox.Location = new System.Drawing.Point(0, 0);
            this.mavLinkTextBox.Name = "mavLinkTextBox";
            this.mavLinkTextBox.ReadOnly = true;
            this.mavLinkTextBox.Size = new System.Drawing.Size(598, 286);
            this.mavLinkTextBox.TabIndex = 0;
            this.mavLinkTextBox.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(598, 424);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yawTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rollTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pitchTrackBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comPortComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox mavLinkTextBox;
        private System.Windows.Forms.ComboBox baudRateComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button disconnectButton;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.TrackBar pitchTrackBar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TrackBar rollTrackBar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar yawTrackBar;
        private System.Windows.Forms.Label yawLabel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label rollLabel;
        private System.Windows.Forms.Label pitchLabel;
        private System.Windows.Forms.Label label4;

    }
}

