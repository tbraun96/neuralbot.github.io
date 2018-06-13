using NeuralBot.Neural;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralBot.Window
{
    public partial class GraphWindow : Form
    {
        public GraphWindow()
        {
            InitializeComponent();
            Form.CheckForIllegalCrossThreadCalls = false;
            this.chart1.SuppressExceptions = true;
        }

        public void HookToBrain(Brain brain)
        {
            brain.GetNetwork().MseSeries = this.chart1.Series[0];
            brain.GetNetwork().OtherSeries = this.chart1.Series[1];
        }

    }
}
