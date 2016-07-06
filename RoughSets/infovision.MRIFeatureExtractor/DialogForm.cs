using System.Windows.Forms;

namespace Infovision.MRI.UI
{
    public class DialogForm : Form
    {
        public DialogForm()
            : base()
        {
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // DialogForm
            //
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "DialogForm";
            this.ResumeLayout(false);
        }
    }
}