namespace SharedCardGame
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.splitContainerGameField = new System.Windows.Forms.SplitContainer();
            this.labelWarningNotRegistered = new System.Windows.Forms.Label();
            this.buttonRegisterPlayer = new System.Windows.Forms.Button();
            this.textBoxPlayerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.timerWatchdog = new System.Windows.Forms.Timer(this.components);
            this.timerRegisteringTimeOut = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripStackOfCards = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemExtractFirstCard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCountCards = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemShuffleUp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemDeal = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGameField)).BeginInit();
            this.splitContainerGameField.Panel1.SuspendLayout();
            this.splitContainerGameField.SuspendLayout();
            this.contextMenuStripStackOfCards.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerGameField
            // 
            this.splitContainerGameField.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerGameField.ForeColor = System.Drawing.Color.Black;
            this.splitContainerGameField.Location = new System.Drawing.Point(0, 0);
            this.splitContainerGameField.Name = "splitContainerGameField";
            this.splitContainerGameField.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerGameField.Panel1
            // 
            this.splitContainerGameField.Panel1.Controls.Add(this.labelWarningNotRegistered);
            this.splitContainerGameField.Panel1.Controls.Add(this.buttonRegisterPlayer);
            this.splitContainerGameField.Panel1.Controls.Add(this.textBoxPlayerName);
            this.splitContainerGameField.Panel1.Controls.Add(this.label1);
            this.splitContainerGameField.Panel1MinSize = 0;
            // 
            // splitContainerGameField.Panel2
            // 
            this.splitContainerGameField.Panel2.BackColor = System.Drawing.Color.DarkGreen;
            this.splitContainerGameField.Panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GameField_MouseDown);
            this.splitContainerGameField.Panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GameField_MouseMove);
            this.splitContainerGameField.Panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GameField_MouseUp);
            this.splitContainerGameField.Size = new System.Drawing.Size(1013, 631);
            this.splitContainerGameField.SplitterDistance = 31;
            this.splitContainerGameField.TabIndex = 0;
            // 
            // labelWarningNotRegistered
            // 
            this.labelWarningNotRegistered.AutoSize = true;
            this.labelWarningNotRegistered.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWarningNotRegistered.ForeColor = System.Drawing.Color.Red;
            this.labelWarningNotRegistered.Location = new System.Drawing.Point(451, 9);
            this.labelWarningNotRegistered.Name = "labelWarningNotRegistered";
            this.labelWarningNotRegistered.Size = new System.Drawing.Size(290, 13);
            this.labelWarningNotRegistered.TabIndex = 3;
            this.labelWarningNotRegistered.Text = "Player is not registred ! Register Player to server !";
            this.labelWarningNotRegistered.Visible = false;
            // 
            // buttonRegisterPlayer
            // 
            this.buttonRegisterPlayer.Location = new System.Drawing.Point(329, 5);
            this.buttonRegisterPlayer.Name = "buttonRegisterPlayer";
            this.buttonRegisterPlayer.Size = new System.Drawing.Size(106, 23);
            this.buttonRegisterPlayer.TabIndex = 2;
            this.buttonRegisterPlayer.Text = "Register Player";
            this.buttonRegisterPlayer.UseVisualStyleBackColor = true;
            this.buttonRegisterPlayer.Click += new System.EventHandler(this.buttonRegisterPlayer_Click);
            // 
            // textBoxPlayerName
            // 
            this.textBoxPlayerName.ForeColor = System.Drawing.SystemColors.GrayText;
            this.textBoxPlayerName.Location = new System.Drawing.Point(144, 6);
            this.textBoxPlayerName.Name = "textBoxPlayerName";
            this.textBoxPlayerName.Size = new System.Drawing.Size(179, 20);
            this.textBoxPlayerName.TabIndex = 1;
            this.textBoxPlayerName.Text = "Type your name";
            this.textBoxPlayerName.TextChanged += new System.EventHandler(this.textBoxPlayerName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Player Name/Nick name:";
            // 
            // timerWatchdog
            // 
            this.timerWatchdog.Enabled = true;
            this.timerWatchdog.Interval = 5000;
            this.timerWatchdog.Tick += new System.EventHandler(this.timerWatchdog_Tick);
            // 
            // timerRegisteringTimeOut
            // 
            this.timerRegisteringTimeOut.Interval = 3000;
            this.timerRegisteringTimeOut.Tick += new System.EventHandler(this.timerRegisteringTimeOut_Tick);
            // 
            // contextMenuStripStackOfCards
            // 
            this.contextMenuStripStackOfCards.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemExtractFirstCard,
            this.toolStripMenuItemCountCards,
            this.toolStripMenuItemShuffleUp,
            this.toolStripMenuItemDeal});
            this.contextMenuStripStackOfCards.Name = "contextMenuStripStackOfCards";
            this.contextMenuStripStackOfCards.Size = new System.Drawing.Size(201, 114);
            this.contextMenuStripStackOfCards.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStripStackOfCards_Closed);
            // 
            // toolStripMenuItemExtractFirstCard
            // 
            this.toolStripMenuItemExtractFirstCard.Name = "toolStripMenuItemExtractFirstCard";
            this.toolStripMenuItemExtractFirstCard.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemExtractFirstCard.Text = "Extract first card of deck";
            this.toolStripMenuItemExtractFirstCard.Click += new System.EventHandler(this.toolStripMenuItemExtractFirstCard_Click);
            // 
            // toolStripMenuItemCountCards
            // 
            this.toolStripMenuItemCountCards.Name = "toolStripMenuItemCountCards";
            this.toolStripMenuItemCountCards.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemCountCards.Text = "Count cards in deck";
            this.toolStripMenuItemCountCards.Click += new System.EventHandler(this.toolStripMenuItemCountCards_Click);
            // 
            // toolStripMenuItemShuffleUp
            // 
            this.toolStripMenuItemShuffleUp.Name = "toolStripMenuItemShuffleUp";
            this.toolStripMenuItemShuffleUp.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemShuffleUp.Text = "Shuffle up the deck";
            this.toolStripMenuItemShuffleUp.Click += new System.EventHandler(this.toolStripMenuItemShuffleUp_Click);
            // 
            // toolStripMenuItemDeal
            // 
            this.toolStripMenuItemDeal.Name = "toolStripMenuItemDeal";
            this.toolStripMenuItemDeal.Size = new System.Drawing.Size(200, 22);
            this.toolStripMenuItemDeal.Text = "Deal cards of deck...";
            this.toolStripMenuItemDeal.Click += new System.EventHandler(this.toolStripMenuItemDeal_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1013, 631);
            this.Controls.Add(this.splitContainerGameField);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Shared Card Game";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.LocationChanged += new System.EventHandler(this.MainForm_LocationChanged);
            this.splitContainerGameField.Panel1.ResumeLayout(false);
            this.splitContainerGameField.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGameField)).EndInit();
            this.splitContainerGameField.ResumeLayout(false);
            this.contextMenuStripStackOfCards.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainerGameField;
        private System.Windows.Forms.Button buttonRegisterPlayer;
        private System.Windows.Forms.TextBox textBoxPlayerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelWarningNotRegistered;
        private System.Windows.Forms.Timer timerWatchdog;
        private System.Windows.Forms.Timer timerRegisteringTimeOut;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripStackOfCards;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExtractFirstCard;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCountCards;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShuffleUp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemDeal;
    }
}

