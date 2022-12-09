using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HooverGOL122022
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }
        public int TimerMiliseconds //this is a proper way to do a property
        {
            get { return (int)numericUpDownTimer.Value; }
            set { numericUpDownTimer.Value = value; }
        }

        public int WidthUniverse
        {
            get { return (int)numericUpDownWidthOfUniverse.Value; }
            set { numericUpDownWidthOfUniverse.Value = value; }
        }
        public int HeightUniverse
        {
            get { return (int)numericUpDownHeightofUniverse.Value; }
            set { numericUpDownHeightofUniverse.Value = value; }
        }

        private void numericUpDownTimer_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
