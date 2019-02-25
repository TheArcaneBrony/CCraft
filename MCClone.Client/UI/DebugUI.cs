using System;
using System.Windows.Forms;

namespace MCClone.UI
{
    public partial class DebugUI : Form
    {
        public DebugUI()
        {
            InitializeComponent();
        }
        private void DebugUI_Load(object sender, EventArgs e)
        {

        }
        public void UpdateUI(string Input)
        {
            Invoke((MethodInvoker)(()=>{
                testLabel.Text = Input;
            }));
        }
    }
}
