using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharedCardGame
{
    public partial class DealingParametersDialog : Form
    {
        public DealingParametersDialog()
        {
            InitializeComponent();
        }

        public bool DealingDirectionIsClockwise {
            get
            {
                return radioButtonClockwiseDirection.Checked;
            }

            set
            {
                radioButtonClockwiseDirection.Checked = false;
                radioButtonCounterClockwiseDirection.Checked = false;
                radioButtonClockwiseDirection.Checked = value;
                radioButtonCounterClockwiseDirection.Checked = !value; 
            }
        }

        public int NumberOfCardsToDeal
        {
            get
            {
                return (int)numericUpDownNumberOfCardsToDeal.Value;
            }

            set { numericUpDownNumberOfCardsToDeal.Value = value; }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonDeal_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
