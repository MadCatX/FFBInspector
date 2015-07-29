using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace FFBTest
{
    public class PeriodicForcePanel : EffectPanel
    {
        private Label lblMagnitude = new Label();
        private Label lblOffset = new Label();
        private Label lblPeriod = new Label();
        private Label lblPhase = new Label();
        private TextBox tboxMagnitude = new TextBox();
        private TextBox tboxOffset = new TextBox();
        private TextBox tboxPeriod = new TextBox();
        private TextBox tboxPhase = new TextBox();
        private ToolTip tipMagnitude = new ToolTip();
        private ToolTip tipOffset = new ToolTip();
        private ToolTip tipPeriod = new ToolTip();
        private ToolTip tipPhase = new ToolTip();

        private DataClasses.PeriodicEffectTypeData efd = new DataClasses.PeriodicEffectTypeData();

        public PeriodicForcePanel()
        {
            lblMagnitude.Text = "Magnitude:";
            lblOffset.Text = "Offset:";
            lblPeriod.Text = "Period:";
            lblPhase.Text = "Phase:";
            tboxMagnitude.Text = "5000";
            tboxOffset.Text = "0";
            tboxPeriod.Text = "1000000";
            tboxPhase.Text = "0";

            tipMagnitude.SetToolTip(lblMagnitude, FFBInspector.Properties.Resources.tip_periodicMagnitude);
            tipOffset.SetToolTip(lblOffset, FFBInspector.Properties.Resources.tip_periodicOffset);
            tipPeriod.SetToolTip(lblPeriod, FFBInspector.Properties.Resources.tip_periodicPeriod);
            tipPhase.SetToolTip(lblPhase, FFBInspector.Properties.Resources.tip_periodicPhase);

            this.Controls.Add(lblMagnitude);
            this.Controls.Add(tboxMagnitude);
            this.Controls.Add(lblOffset);
            this.Controls.Add(tboxOffset);
            this.Controls.Add(lblPeriod);
            this.Controls.Add(tboxPeriod);
            this.Controls.Add(lblPhase);
            this.Controls.Add(tboxPhase);
        }

        public override DataClasses.EffectTypeData PassEffectData()
        {
            try
            {
                efd.magnitude = Convert.ToInt32(tboxMagnitude.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid magnitude value.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            try
            {
                efd.offset= Convert.ToInt32(tboxOffset.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid offset value.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            try
            {
                efd.period = Convert.ToInt32(tboxPeriod.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid period value.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            try
            {
                efd.phase = Convert.ToInt32(tboxPhase.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid phase value.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            EnsureValidity();
            return efd;
        }

        private void EnsureValidity()
        {
            if (efd.magnitude > 10000)
            {
                efd.magnitude = 10000;
                tboxMagnitude.Text = "10000";
            }
            else if (efd.magnitude < 0)
            {
                efd.magnitude = 0;
                tboxMagnitude.Text = "0";
            }

            if (efd.period < 1)
            {
                efd.period = 1;
                tboxPeriod.Text = "1";
            }

            if (efd.phase > 35999)
            {
                efd.phase = 35999;
                tboxPhase.Text = "35999";
            }
            else if (efd.phase < 0)
            {
                efd.phase = 0;
                tboxPhase.Text = "0";
            }
        }
    }
}
