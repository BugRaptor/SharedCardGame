using System;
using System.Windows.Forms;
using SharedCardGame.ScgServiceLibrary;

namespace SharedCardGame
{
    public partial class ChatForm : Form
    {
        private MainForm _mainForm;
        public ChatForm(MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
            textBoxChatMessage.KeyDown += textBoxChatMessage_KeyDown;
        }

        public void LogReceivedMessage(ChatMessageDataType chatMessageData)
        {
            if (chatMessageData.ClientName != _mainForm.ClientRegisteredName)
            {
                Console.Beep(3000, 50);
                if (WindowState == FormWindowState.Minimized)
                {
                    WindowState = FormWindowState.Normal;
                    Width = 220;
                    Height = 190;
                    Left = _mainForm.Left;
                    Top = _mainForm.Bottom - Height;
                }
                BringToFront();
            }
            textBoxChat.AppendText(string.Format("{0}: {1}" + Environment.NewLine,chatMessageData.ClientName, chatMessageData.ChatMessage));
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (_mainForm != null && textBoxChatMessage.Text != string.Empty)
            {
                _mainForm.SendChatMessage(textBoxChatMessage.Text);
                textBoxChatMessage.Text = string.Empty;
            }
        }


        void textBoxChatMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //enter key is down
                buttonSend_Click(sender, e);
            }
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
