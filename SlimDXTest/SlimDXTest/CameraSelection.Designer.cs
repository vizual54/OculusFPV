namespace SlimDXTest
{
    partial class CameraSelection
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
            this.leftCameraSelector = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rightCameraSelector = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.fullScreenCheckBox = new System.Windows.Forms.CheckBox();
            this.shaderCombox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // leftCameraSelector
            // 
            this.leftCameraSelector.FormattingEnabled = true;
            this.leftCameraSelector.Location = new System.Drawing.Point(93, 13);
            this.leftCameraSelector.Name = "leftCameraSelector";
            this.leftCameraSelector.Size = new System.Drawing.Size(328, 21);
            this.leftCameraSelector.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Left Camera:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Right Camera:";
            // 
            // rightCameraSelector
            // 
            this.rightCameraSelector.FormattingEnabled = true;
            this.rightCameraSelector.Location = new System.Drawing.Point(93, 50);
            this.rightCameraSelector.Name = "rightCameraSelector";
            this.rightCameraSelector.Size = new System.Drawing.Size(328, 21);
            this.rightCameraSelector.TabIndex = 3;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(265, 111);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(346, 111);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // fullScreenCheckBox
            // 
            this.fullScreenCheckBox.AutoSize = true;
            this.fullScreenCheckBox.Location = new System.Drawing.Point(15, 90);
            this.fullScreenCheckBox.Name = "fullScreenCheckBox";
            this.fullScreenCheckBox.Size = new System.Drawing.Size(74, 17);
            this.fullScreenCheckBox.TabIndex = 6;
            this.fullScreenCheckBox.Text = "Fullscreen";
            this.fullScreenCheckBox.UseVisualStyleBackColor = true;
            this.fullScreenCheckBox.CheckedChanged += new System.EventHandler(this.fullScreenCheckBox_CheckedChanged);
            // 
            // shaderCombox
            // 
            this.shaderCombox.FormattingEnabled = true;
            this.shaderCombox.Location = new System.Drawing.Point(93, 111);
            this.shaderCombox.Name = "shaderCombox";
            this.shaderCombox.Size = new System.Drawing.Size(121, 21);
            this.shaderCombox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Shader:";
            // 
            // CameraSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 146);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.shaderCombox);
            this.Controls.Add(this.fullScreenCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.rightCameraSelector);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.leftCameraSelector);
            this.Name = "CameraSelection";
            this.Text = "CameraSelection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox leftCameraSelector;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox rightCameraSelector;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox fullScreenCheckBox;
        private System.Windows.Forms.ComboBox shaderCombox;
        private System.Windows.Forms.Label label3;
    }
}