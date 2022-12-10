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
    public partial class RandomSeed : Form
    {
        public RandomSeed()
        {
            InitializeComponent();
        }
        public int Seed
        {
            get { return (int)numericUpDownSeed.Value; }
            set { numericUpDownSeed.Value = value; }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
