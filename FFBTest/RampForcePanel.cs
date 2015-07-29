using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace FFBTest
{
    public class RampForcePanel : EffectPanel
    {
        private Label lblStart = new Label();
        private Label lblEnd = new Label();
        private TextBox tboxStart = new TextBox();
        private TextBox tboxEnd = new TextBox();

        private DataClasses.RampEffectTypeData efd = new DataClasses.RampEffectTypeData();

        public RampForcePanel()
        {
            lblStart.Text = "Initial force:";
            lblEnd.Text = "End force:";
            tboxStart.Text = "0";
            tboxEnd.Text = "0";

            this.Controls.Add(lblStart);
            this.Controls.Add(tboxStart);
            this.Controls.Add(lblEnd);
            this.Controls.Add(tboxEnd);
        }

        public override DataClasses.EffectTypeData PassEffectData()
        {
            try
            {
                efd.start = Convert.ToInt32(tboxStart.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid initial force value.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            try
            {
                efd.end = Convert.ToInt32(tboxEnd.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid end force value.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            EnsureValidity();
            return efd;
        }

        private void EnsureValidity()
        {
            if (efd.start > 10000)
            {
                efd.start = 10000;
                tboxStart.Text = "10000";
            }
            else if (efd.start < -10000)
            {
                efd.start = -10000;
                tboxStart.Text = "-10000";
            }
            
            if (efd.end > 10000)
            {
                efd.end = 10000;
                tboxEnd.Text = "10000";
            }
            else if (efd.end < -10000)
            {
                efd.end = -10000;
                tboxEnd.Text = "-10000";
            }
        }
    }
}
