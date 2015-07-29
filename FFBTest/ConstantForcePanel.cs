using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace FFBTest
{
    public class ConstantForcePanel : EffectPanel
    {
        private Label lblMagnitude = new Label();
        private TextBox tboxMagnitude = new TextBox();
        private ToolTip tipMagnitude = new ToolTip();

        private DataClasses.ConstantEffectTypeData efd = new DataClasses.ConstantEffectTypeData();

        public ConstantForcePanel()
        {
            lblMagnitude.Text = "Magnitude:";
            tboxMagnitude.Text = "5000";
            tipMagnitude.SetToolTip(lblMagnitude, FFBInspector.Properties.Resources.tip_constantMagnitude);

            this.Controls.Add(lblMagnitude);
            this.Controls.Add(tboxMagnitude);
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
            catch (OverflowException)
            {
                MessageBox.Show("Magnitude value out of range.", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                this.tboxMagnitude.Text = "10000";
            }
            else if (efd.magnitude < -10000)
            {
                efd.magnitude = -10000;
                this.tboxMagnitude.Text = "-10000";
            }
        }
        
    }
}
