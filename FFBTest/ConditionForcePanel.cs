using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace FFBTest
{
    public class ConditionForcePanel : EffectPanel
    {
        private Label lblDeadBand = new Label();
        private Label lblOffset = new Label();
        private Label lblNegCoeff = new Label();
        private Label lblNegSat = new Label();
        private Label lblPosCoeff = new Label();
        private Label lblPosSat = new Label();
        private TextBox tboxDeadBand = new TextBox();
        private TextBox tboxOffset = new TextBox();
        private TextBox tboxNegCoeff = new TextBox();
        private TextBox tboxNegSat = new TextBox();
        private TextBox tboxPosCoeff = new TextBox();
        private TextBox tboxPosSat = new TextBox();
        private ToolTip tipOffset = new ToolTip();
        private ToolTip tipDeadBand = new ToolTip();
        private ToolTip tipNegCoeff = new ToolTip();
        private ToolTip tipNegSat = new ToolTip();
        private ToolTip tipPosCoeff = new ToolTip();
        private ToolTip tipPosSat = new ToolTip();

        private DataClasses.ConditionEffectTypeData efd = new DataClasses.ConditionEffectTypeData();

        public ConditionForcePanel()
        {
            lblDeadBand.Text = "Dead band:";
            lblOffset.Text = "Offset:";
            lblNegCoeff.Text = "Negative coeff:";
            lblNegSat.Text = "Negative satur:";
            lblPosCoeff.Text = "Positive coeff:";
            lblPosSat.Text = "Positive satur:";
            tboxDeadBand.Text = "0";
            tboxOffset.Text = "0";
            tboxNegCoeff.Text = "5000";
            tboxNegSat.Text = "10000";
            tboxPosCoeff.Text = "5000";
            tboxPosSat.Text = "10000";

            tipOffset.SetToolTip(lblOffset, FFBInspector.Properties.Resources.tip_conditionOffset);
            tipDeadBand.SetToolTip(lblDeadBand, FFBInspector.Properties.Resources.tip_conditionDeadband);
            tipNegCoeff.SetToolTip(lblNegCoeff, FFBInspector.Properties.Resources.tip_conditionNegCoeff);
            tipNegSat.SetToolTip(lblNegSat, FFBInspector.Properties.Resources.tip_conditionNegSat);
            tipPosCoeff.SetToolTip(lblPosCoeff, FFBInspector.Properties.Resources.tip_conditionPosCoeff);
            tipPosSat.SetToolTip(lblPosSat, FFBInspector.Properties.Resources.tip_conditionPosSat);

            this.Controls.Add(lblDeadBand, 0, 0);
            this.Controls.Add(tboxDeadBand, 1, 0);
            this.Controls.Add(lblOffset, 0, 1);
            this.Controls.Add(tboxOffset, 1, 1);
            this.Controls.Add(lblNegCoeff, 0, 2);
            this.Controls.Add(tboxNegCoeff, 1, 2);
            this.Controls.Add(lblNegSat, 0, 3);
            this.Controls.Add(tboxNegSat, 1, 3);
            this.Controls.Add(lblPosCoeff, 0, 4);
            this.Controls.Add(tboxPosCoeff, 1, 4);
            this.Controls.Add(lblPosSat, 0, 5);
            this.Controls.Add(tboxPosSat,1, 5);
        }

        public override DataClasses.EffectTypeData PassEffectData()
        {
            try
            {
                efd.deadBand = Convert.ToInt32(tboxDeadBand.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid dead band value.\n" + ex.Message, errMsgCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            try
            {
                efd.offset = Convert.ToInt32(tboxOffset.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid offset value.\n" + ex.Message, errMsgCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            try
            {
                efd.negCoeff = Convert.ToInt32(tboxNegCoeff.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid negative coefficient value.\n" + ex.Message, errMsgCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return efd;
            }
             try
            {
                efd.negSat = Convert.ToInt32(tboxNegSat.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid negative saturation value.\n" + ex.Message, errMsgCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
             try
            {
                efd.posCoeff = Convert.ToInt32(tboxPosCoeff.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid positive coefficient value.\n" + ex.Message, errMsgCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
             try
            {
                efd.posSat = Convert.ToInt32(tboxPosSat.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Invalid positive saturation value.\n" + ex.Message, errMsgCap, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            EnsureValidity();
            return efd;
        }

        private void EnsureValidity()
        {
            if (efd.deadBand > 10000)
            {
                efd.deadBand = 10000;
                tboxDeadBand.Text = "10000";
            }
            else if (efd.deadBand < 0)
            {
                efd.deadBand = 0;
                tboxDeadBand.Text = "0";
            }

            if (efd.offset > 10000)
            {
                efd.offset = 10000;
                tboxOffset.Text = "10000";
            }
            else if (efd.offset < -10000)
            {
                efd.offset = -10000;
                tboxOffset.Text = "-10000";
            }

            if (efd.negCoeff > 10000)
            {
                efd.negCoeff = 10000;
                tboxNegCoeff.Text = "10000";
            }
            else if (efd.negCoeff < -10000)
            {
                efd.negCoeff = -10000;
                tboxNegCoeff.Text = "-10000";
            }

            if (efd.negSat > 10000)
            {
                efd.negSat = 10000;
                tboxNegSat.Text = "10000";
            }
            else if (efd.negSat < 0)
            {
                efd.negSat = 0;
                tboxNegSat.Text = "0";
            }

            if (efd.posCoeff > 10000)
            {
                efd.posCoeff = 10000;
                tboxPosCoeff.Text = "10000";
            }
            else if (efd.posCoeff < -10000)
            {
                efd.posCoeff = -10000;
                tboxPosCoeff.Text = "-1000";
            }

            if (efd.posSat > 10000)
            {
                efd.posSat = 10000;
                tboxPosCoeff.Text = "10000";
            }
            else if (efd.posSat < 0)
            {
                efd.posSat = 0;
                tboxPosSat.Text = "0";
            }
        }
    }
}
