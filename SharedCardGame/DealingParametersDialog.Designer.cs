namespace SharedCardGame
{
    partial class DealingParametersDialog
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
            this.groupBoxDirection = new System.Windows.Forms.GroupBox();
            this.radioButtonCounterClockwiseDirection = new System.Windows.Forms.RadioButton();
            this.radioButtonClockwiseDirection = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownNumberOfCardsToDeal = new System.Windows.Forms.NumericUpDown();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonDeal = new System.Windows.Forms.Button();
            this.groupBoxDirection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfCardsToDeal)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxDirection
            // 
            this.groupBoxDirection.Controls.Add(this.radioButtonCounterClockwiseDirection);
            this.groupBoxDirection.Controls.Add(this.radioButtonClockwiseDirection);
            this.groupBoxDirection.Location = new System.Drawing.Point(12, 12);
            this.groupBoxDirection.Name = "groupBoxDirection";
            this.groupBoxDirection.Size = new System.Drawing.Size(225, 95);
            this.groupBoxDirection.TabIndex = 0;
            this.groupBoxDirection.TabStop = false;
            this.groupBoxDirection.Text = "Direction";
            // 
            // radioButtonCounterClockwiseDirection
            // 
            this.radioButtonCounterClockwiseDirection.AutoSize = true;
            this.radioButtonCounterClockwiseDirection.Location = new System.Drawing.Point(24, 59);
            this.radioButtonCounterClockwiseDirection.Name = "radioButtonCounterClockwiseDirection";
            this.radioButtonCounterClockwiseDirection.Size = new System.Drawing.Size(109, 17);
            this.radioButtonCounterClockwiseDirection.TabIndex = 1;
            this.radioButtonCounterClockwiseDirection.Text = "Counterclockwise";
            this.radioButtonCounterClockwiseDirection.UseVisualStyleBackColor = true;
            // 
            // radioButtonClockwiseDirection
            // 
            this.radioButtonClockwiseDirection.AutoSize = true;
            this.radioButtonClockwiseDirection.Checked = true;
            this.radioButtonClockwiseDirection.Location = new System.Drawing.Point(24, 27);
            this.radioButtonClockwiseDirection.Name = "radioButtonClockwiseDirection";
            this.radioButtonClockwiseDirection.Size = new System.Drawing.Size(73, 17);
            this.radioButtonClockwiseDirection.TabIndex = 0;
            this.radioButtonClockwiseDirection.TabStop = true;
            this.radioButtonClockwiseDirection.Text = "Clockwise";
            this.radioButtonClockwiseDirection.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Cards to deal:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // numericUpDownNumberOfCardsToDeal
            // 
            this.numericUpDownNumberOfCardsToDeal.Location = new System.Drawing.Point(175, 124);
            this.numericUpDownNumberOfCardsToDeal.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.numericUpDownNumberOfCardsToDeal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownNumberOfCardsToDeal.Name = "numericUpDownNumberOfCardsToDeal";
            this.numericUpDownNumberOfCardsToDeal.Size = new System.Drawing.Size(62, 20);
            this.numericUpDownNumberOfCardsToDeal.TabIndex = 2;
            this.numericUpDownNumberOfCardsToDeal.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(175, 173);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(62, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonDeal
            // 
            this.buttonDeal.Location = new System.Drawing.Point(83, 173);
            this.buttonDeal.Name = "buttonDeal";
            this.buttonDeal.Size = new System.Drawing.Size(62, 23);
            this.buttonDeal.TabIndex = 4;
            this.buttonDeal.Text = "&Deal";
            this.buttonDeal.UseVisualStyleBackColor = true;
            this.buttonDeal.Click += new System.EventHandler(this.buttonDeal_Click);
            // 
            // DealingParametersDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 208);
            this.Controls.Add(this.buttonDeal);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.numericUpDownNumberOfCardsToDeal);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBoxDirection);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "DealingParametersDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dealing parameters";
            this.groupBoxDirection.ResumeLayout(false);
            this.groupBoxDirection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownNumberOfCardsToDeal)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDirection;
        private System.Windows.Forms.RadioButton radioButtonCounterClockwiseDirection;
        private System.Windows.Forms.RadioButton radioButtonClockwiseDirection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownNumberOfCardsToDeal;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonDeal;
    }
}