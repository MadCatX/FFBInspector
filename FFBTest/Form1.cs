using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace FFBTest
{
    public partial class Form1 : Form
    {
        private Dictionary<int, DInputHandler> diHandlers;
        private EffectPanel efPanel;
        private IList<EffectInfo> availableFFBEffects;
        private List<TextBox> axesControls;
        private List<ToolTip> directionEffParamsTooltips;
        private List<ToolTip> generalEffParamsTooltips;
        private List<Label> effectSlotIndicators;
        private Timer updateDeviceStatus;

        public Form1()
        {
            InitializeComponent();
            lblVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            effectSlotIndicators = new List<Label>();
            effectSlotIndicators.Add(labelSlot1);
            effectSlotIndicators.Add(labelSlot2);
            effectSlotIndicators.Add(labelSlot3);
            effectSlotIndicators.Add(labelSlot4);
            effectSlotIndicators.Add(labelSlot5);
            effectSlotIndicators.Add(labelSlot6);
            effectSlotIndicators.Add(labelSlot7);
            effectSlotIndicators.Add(labelSlot8);
            CreateTooltips();

            //Initialize DirectInput and enumerate all available force feedback devices
            if (!DInputHandler.InitDirectInput())
            {
                //Close application if DirectInput initialization fails
                Close();
            }
            
            IEnumerable<DeviceInstance> deviceList = DInputHandler.dInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.ForceFeedback);
            if (deviceList.Count() == 0)
            {
                MessageBox.Show("No force feedback devices found", "No devices",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                diHandlers = new Dictionary<int, DInputHandler>(deviceList.Count());
                //Add all force feedback devices to cboxDevices and create DInputHandlers for them
                int devIdx = 0;
                foreach (DeviceInstance ffbDev in deviceList)
                    HandleFFBDevice(ffbDev, devIdx++);


                cboxDevices.SelectedIndex = 0; //Select first device in the list
            }

            //Set effect slot "1" as default
            cboxEffSlots.SelectedIndex = 0;

            this.Activated += new EventHandler(Form1_onActivated);
            this.Deactivate += new EventHandler(Form1_onDeactivate);
        }

        /// <summary>
        /// Creates an instance of FFBEffectData and fills it with FFB effect
        /// parameters entered by the user in the control form.
        /// </summary>
        /// <returns>
        /// Reference to FFBEffectData if successful, otherwise returns null.
        /// </returns>
        private DataClasses.FFBEffectData CreateFFBEffectData()
        {
            /* Tells currently active effect panel to pass the
             * effect parameters. */
            DataClasses.EffectTypeData efd = efPanel.PassEffectData();

            //Parse general effect parameters
            #region General
            try
            {
                efd.gain = Convert.ToInt32(tboxGain.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid gain value.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            try
            {
                efd.duration = Convert.ToInt32(tboxDuration.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid duration value.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            try
            {
                efd.startDelay = Convert.ToInt32(tboxStartDelay.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid start delay value.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            try
            {
                efd.trigRepInterval = Convert.ToInt32(tboxRepInt.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid repeat interval value.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            try
            {
                efd.trigButton = Convert.ToInt32(nboxTrigButton.Value);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid trigger button.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return null;
            }
            #endregion

            #region Directions
            //Parse directions
            foreach (TextBox tbox in axesControls)
                efd.AddDirection(Convert.ToInt32(tbox.Text));
            #endregion

            #region Envelope
            DataClasses.EnvelopeData evd = null;
            //Parse envelope
            if (cbUseEnvelope.Checked)
            {
                evd = new DataClasses.EnvelopeData();
                try
                {
                    evd.attackLevel = Convert.ToInt32(tboxAttackLevel.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid attack time.", FFBInspector.Properties.Resources.errCap_invalData,
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }
                try
                {
                    evd.attackTime = Convert.ToInt32(tboxAttackTime.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid attack level.", FFBInspector.Properties.Resources.errCap_invalData,
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }
                try
                {
                    evd.fadeLevel = Convert.ToInt32(tboxFadeLevel.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid fade level.", FFBInspector.Properties.Resources.errCap_invalData,
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }
                try
                {
                    evd.fadeTime = Convert.ToInt32(tboxFadeTime.Text);
                }
                catch (FormatException)
                {
                    MessageBox.Show("Invalid fade time.", FFBInspector.Properties.Resources.errCap_invalData,
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                }
            }
            #endregion

            #region Effect Slot
            int slot = GetSelectedEffectSlot();
            if (slot == -1)
            {
                MessageBox.Show("Invalid effect slot number.\n This should never ever happen!", FFBInspector.Properties.Resources.errCap_impossible,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return null;
            }
            else
            #endregion

            return new DataClasses.FFBEffectData(efd, evd, slot);
        }

        private void CreateTooltips()
        {
            generalEffParamsTooltips = new List<ToolTip>();
            directionEffParamsTooltips = new List<ToolTip>();

            #region General parameters tooltips
            //Gain
            ToolTip t = new ToolTip();
            t.SetToolTip(label1, FFBInspector.Properties.Resources.tip_generalGain);
            generalEffParamsTooltips.Add(t);
            
            //Duration
            t = new ToolTip();
            t.SetToolTip(label2, FFBInspector.Properties.Resources.tip_generalDuration);
            generalEffParamsTooltips.Add(t);

            //Delay
            t = new ToolTip();
            t.SetToolTip(label3, FFBInspector.Properties.Resources.tip_generalDelay);
            generalEffParamsTooltips.Add(t);

            //Trigger repeat interval
            t = new ToolTip();
            t.SetToolTip(label4, FFBInspector.Properties.Resources.tip_generalTrigRep);
            generalEffParamsTooltips.Add(t);

            //Trigger
            t = new ToolTip();
            t.SetToolTip(label5, FFBInspector.Properties.Resources.tip_generalTrig);
            generalEffParamsTooltips.Add(t);
            #endregion

            #region Direction tooltips
            t = new ToolTip();
            t.SetToolTip(gbDirections, FFBInspector.Properties.Resources.tip_directions);
            directionEffParamsTooltips.Add(t);
            #endregion

            #region Envelope tooltips
            //Envelope
            t = new ToolTip();
            t.AutoPopDelay = 15000;
            t.SetToolTip(gbEnvelope, FFBInspector.Properties.Resources.tip_envelope);
            generalEffParamsTooltips.Add(t);
            
            //Attack level
            t = new ToolTip();
            t.SetToolTip(label6, FFBInspector.Properties.Resources.tip_envelopeAttackLevel);
            generalEffParamsTooltips.Add(t);

            //Attack time
            t = new ToolTip();
            t.SetToolTip(label7, FFBInspector.Properties.Resources.tip_envelopeAttackTime);
            generalEffParamsTooltips.Add(t);

            //Fade level
            t = new ToolTip();
            t.SetToolTip(label8, FFBInspector.Properties.Resources.tip_envelopeFadeLevel);
            generalEffParamsTooltips.Add(t);

            //Fade time
            t = new ToolTip();
            t.SetToolTip(label9, FFBInspector.Properties.Resources.tip_envelopeFadeTime);
            generalEffParamsTooltips.Add(t);

            #endregion

        }
            

        /// <summary>
        /// Returns DInputHandler for a FFB device with given index.
        /// </summary>
        /// <param name="devIdx">
        /// Index of the FFB device under which the device is stored in diHandlers dictionary.
        /// </param>
        /// <returns>
        /// Reference to a DInputHandler if index is valid, otherwise returns null.
        /// </returns>
        private DInputHandler GetDInputHandlerFromDict(int devIdx)
        {
            if (diHandlers == null)
                return null;

            DInputHandler dih;
            if (diHandlers.TryGetValue(devIdx, out dih))
            {
                return dih;
            }
            else
            {
                MessageBox.Show("Cannot get DIHandler for the selected device.", FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return null;
            }
        }

        /// <summary>
        /// Returns DInputHandler of the device currently selected in the "Devices" dropdown menu
        /// </summary>
        /// <returns>
        /// Reference to DInputHandler if the selected device has been added to diHandlers dictionary,
        /// otherwise returns null.
        /// </returns>
        private DInputHandler GetDInputHandler()
        {
            if (diHandlers == null)
                return null;

            int devIdx = cboxDevices.SelectedIndex;
            DInputHandler dih = GetDInputHandlerFromDict(devIdx);
            if (dih == null)
            {
                MessageBox.Show("Got null DIHandler, this is a bug.", FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return null;
            }
            return dih;
        }

        /// <summary>
        /// Returns the effect slot currently selected in the "Slots" dropdown menu.
        /// </summary>
        /// <returns>
        /// Returns index of the slot &lt; 0; 7 &gt;. Returns -1 if the slot selected in the menu
        /// is invalid (shoud never happen).
        /// </returns>
        private int GetSelectedEffectSlot()
        {
            if (cboxEffSlots.SelectedIndex > -1 && cboxEffSlots.SelectedIndex < 8)
                return cboxEffSlots.SelectedIndex;
            else
                return -1;
        }

       
        /// <summary>
        /// Creates an instance of DInputHandler object for a FFB device and adds it to diHandlers dictionary under
        /// "devIdx" index.
        /// </summary>
        /// <param name="ffbDev">
        /// DeviceInstance returned by DirectInput::GetDevices.
        /// </param>
        /// <param name="devIdx">
        /// Index of the device in the enumeration of FFB devices.
        /// </param>
        private void HandleFFBDevice(DeviceInstance ffbDev, int devIdx)
        {
            DInputHandler dih = new DInputHandler(ffbDev.InstanceGuid);

            if (!dih.InitDevice(this.Handle, cbAutoCentering.Checked))
                return;

            diHandlers.Add(devIdx, dih);
            cboxDevices.Items.Add(ffbDev.ProductName + " (" + ffbDev.InstanceGuid.ToString() + ")");
        }

        /// <summary>
        /// Marks the effect state indicator green, showing that the effect is playing
        /// </summary>
        private void MarkActiveSlot()
        {
            effectSlotIndicators[cboxEffSlots.SelectedIndex].BackColor = Color.Green;
            effectSlotIndicators[cboxEffSlots.SelectedIndex].ForeColor = Color.White;
            btnPauseEffect.Text = "Pause effect";
        }

        /// <summary>
        /// Marks the effect indicators accordingly to the states of the effects
        /// </summary>
        /// <param name="dih"></param>
        private void MarkAllSlots(DInputHandler dih)
        {
            for (int i = 0; i < 8; i++)
            {
                if (dih.GetEffectState(i) == DInputHandler.EffectState.OFF)
                {
                    effectSlotIndicators[i].BackColor = Control.DefaultBackColor;
                    effectSlotIndicators[i].ForeColor = Control.DefaultForeColor;
                }
                else if (dih.GetEffectState(i) == DInputHandler.EffectState.PAUSED)
                {
                    effectSlotIndicators[i].BackColor = Color.Yellow;
                    effectSlotIndicators[i].ForeColor = Color.Black;
                }
                else
                {
                    effectSlotIndicators[i].BackColor = Color.Green;
                    effectSlotIndicators[i].ForeColor = Color.White;
                }
            }
        }

        /// <summary>
        /// Marks the effect state indicator yellow, showing that the effect is paused
        /// </summary>
        private void MarkPausedSlot()
        {
            effectSlotIndicators[cboxEffSlots.SelectedIndex].BackColor = Color.Yellow;
            effectSlotIndicators[cboxEffSlots.SelectedIndex].ForeColor = Color.Black;
            btnPauseEffect.Text = "Resume effect";
        }

        /// <summary>
        /// Unmarks the effect state indicator, showing that there is no effect stored in the respective slot
        /// </summary>
        private void UnmarkActiveSlot()
        {
            effectSlotIndicators[cboxEffSlots.SelectedIndex].BackColor = Control.DefaultBackColor;
            effectSlotIndicators[cboxEffSlots.SelectedIndex].ForeColor = Control.DefaultForeColor;
            btnPauseEffect.Text = "(no effect)";
        }

        #region Event handlers' callbacks

        private void btnPauseEffect_Click(object sender, EventArgs e)
        {
            DInputHandler dih = GetDInputHandler();
            if (dih == null)
                return;

            int idx = cboxEffSlots.SelectedIndex;

            if (dih.GetEffectState(idx) == DInputHandler.EffectState.PAUSED)
            {
                if (dih.ResumeOneEffect(idx))
                    MarkActiveSlot();
            }
            else if (dih.GetEffectState(idx) == DInputHandler.EffectState.ACTIVE)
            {
                if (dih.PauseOneEffect(idx))
                    MarkPausedSlot();
            }
        }

        /// <summary>
        /// Event handler:
        /// Attempts to parse the user data, create an FFB effect and send it to the currently
        /// selected device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSendEffect_Click(object sender, EventArgs e)
        {
            Guid ffbG;
            try
            {
                ffbG = availableFFBEffects.ElementAt(cboxEffects.SelectedIndex).Guid;
            }
            catch (ArgumentNullException)
            {
                return;
            }
            if (ffbG == null)
            {
                MessageBox.Show("No effect selected.", FFBInspector.Properties.Resources.errCap_noEffect,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DataClasses.FFBEffectData efd = CreateFFBEffectData();
            if (efd == null)
                return;

            efd.effect.effectGuid = ffbG;

            updateDeviceStatus.Enabled = false;
            if (GetDInputHandler().SendFFBEffect(efd))
                MarkActiveSlot();
            updateDeviceStatus.Enabled = true;
        }

        /// <summary>
        /// Event handler:
        /// Attempts to set a new range 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetRanges_Click(object sender, EventArgs e)
        {
            DataClasses.AxesRange dc = new DataClasses.AxesRange();
            try
            {
                dc.minimum = Convert.ToInt32(tboxAxisMin.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid minimum axes range.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                dc.maximum = Convert.ToInt32(tboxAxisMax.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid maximum axes range.", FFBInspector.Properties.Resources.errCap_invalData,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                GetDInputHandler().SetAxisRange(dc);
            }
            catch (NullReferenceException)
            {
                return;
            }
        }

        /** Issues a command to stop the effect at the selected slot
         *  if there is one currently being played. */
        private void btnStopEffect_Click(object sender, EventArgs e)
        {
            int slot = GetSelectedEffectSlot();
            if (slot == -1)
            {
                MessageBox.Show("Invalid effect slot number.\nThis should never ever happen!", FFBInspector.Properties.Resources.errCap_impossible,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            try
            {
                if (GetDInputHandler().StopOneEffect(slot))
                    UnmarkActiveSlot();
            }
            catch (NullReferenceException)
            {
                return;
            }
        }

        /// <summary>
        /// Event handler:
        /// Toggles envelope parameters input fields.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbUseEnvelope_CheckedChanged(object sender, EventArgs e)
        {
            if (cbUseEnvelope.Checked)
            {
                tboxAttackLevel.Enabled = true;
                tboxAttackTime.Enabled = true;
                tboxFadeLevel.Enabled = true;
                tboxFadeTime.Enabled = true;
            }
            else
            {
                tboxAttackLevel.Enabled = false;
                tboxAttackTime.Enabled = false;
                tboxFadeLevel.Enabled = false;
                tboxFadeTime.Enabled = false;
            }
        }

        /** Probes the selected FFB device and creates a list
         *  of available FFB effects if successful. Also starts a timer
         *  which periodically reads the status of device's axes and buttons */
        private void cboxDevices_onDeviceSelected(object sender, EventArgs e)
        {
            ComboBox cbox = (ComboBox)sender;
            int selectedIdx = cbox.SelectedIndex;

            DInputHandler dih = GetDInputHandlerFromDict(selectedIdx);
            if (dih == null)
                return;

            //Get available effects for the currently selected device
            availableFFBEffects = dih.GetFFBEffects();
            if(availableFFBEffects.Count == 0)
                MessageBox.Show("No force feedback effects available for this device.", FFBInspector.Properties.Resources.errCap_noEffect,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
            else
            {
                cboxEffects.Items.Clear();
                foreach (EffectInfo ei in availableFFBEffects)
                    cboxEffects.Items.Add(ei.Name + " (" + ei.Guid.ToString() + ")");
                    
                cboxEffects.SelectedIndex = 0;   //Select first effect in the list
            }

            //Get available force feedback actuators and create respective UI elements
            int actuators = dih.GetFFBActuatorCount();
            if (actuators == 0) {
                MessageBox.Show("No force feedback actuators found on this device.",
                                FFBInspector.Properties.Resources.errCap_devError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            /* Create textboxes in Directions groupbox to control
             * direction of the effect on each actuator */
            if (axesControls != null)
                panelAxes.Controls.Clear(); //Remove all existing textboxes

            axesControls = new List<TextBox>();
            for (int i = 0; i < actuators; i++)
            {
                axesControls.Add(new TextBox());
                if (i == 0)
                    axesControls[i].Text = "1";
                else
                {
                    axesControls[i].Text = "0";
                    axesControls[i].Top = axesControls[i - 1].Top + 25;
                }
                panelAxes.Controls.Add(axesControls[i]);
            }

            DataClasses.HardwareInfo dc = dih.GetHardwareInfo();
            if (dc != null)
            {
                //Display hardware info on the "Info" tab
                axesCount.Text = dc.axesCount.ToString();
                buttonCount.Text = dc.buttonCount.ToString();
                povCount.Text = dc.povCount.ToString();
                driverVersion.Text = dc.driverVersion.ToString();
                fwVersion.Text = dc.fwVersion.ToString();
                hwRevision.Text = dc.hwRevision.ToString();
                ffbMinTime.Text = dc.ffbMinTime.ToString();
                ffbSamplePeriod.Text = dc.ffbSamplePer.ToString();
            }

            MarkAllSlots(dih);

            //Start the controls status updating timer
            if (updateDeviceStatus == null)
            {
                updateDeviceStatus = new Timer();
                updateDeviceStatus.Interval = 75;
                updateDeviceStatus.Tick += new EventHandler(updateDeviceStatus_onTick);
                updateDeviceStatus.Enabled = true;
            }
        }

        /** Creates a panel with textboxes to adjust effect specific parameters */
        private void cboxEffects_onEffectSelected(object sender, EventArgs e)
        {
            Guid effG = availableFFBEffects.ElementAt(cboxEffects.SelectedIndex).Guid;

            if (effG == EffectGuid.ConstantForce)
            {
                gbEffectSpec.Controls.Clear();
                efPanel = new ConstantForcePanel();
                gbEffectSpec.Controls.Add(efPanel);
            }
            else if (effG == EffectGuid.RampForce)
            {
                gbEffectSpec.Controls.Clear();
                efPanel = new RampForcePanel();
                gbEffectSpec.Controls.Add(efPanel);
            }
            else if (effG == EffectGuid.Sine || effG == EffectGuid.Triangle || effG == EffectGuid.Square ||
                     effG == EffectGuid.SawtoothUp || effG == EffectGuid.SawtoothDown)
            {
                gbEffectSpec.Controls.Clear();
                efPanel = new PeriodicForcePanel();
                gbEffectSpec.Controls.Add(efPanel);
            }
            else if (effG == EffectGuid.Friction || effG == EffectGuid.Inertia || effG == EffectGuid.Damper ||
                     effG == EffectGuid.Spring)
            {
                gbEffectSpec.Controls.Clear();
                efPanel = new ConditionForcePanel();
                gbEffectSpec.Controls.Add(efPanel);
            }
            else if (effG == EffectGuid.CustomForce)
            {
                gbEffectSpec.Controls.Clear();
                efPanel = null;
                return;
            }
            //Make sure all controls are visible
            efPanel.Size = gbEffectSpec.Size;
            efPanel.Width -= 10;
            efPanel.Height -= 20;
            efPanel.Top += 15;
            efPanel.Left += 5;
        }

        private void cboxEffSlots_SelectedIndexChanged(object sender, EventArgs e)
        {
            DInputHandler dih = GetDInputHandler();
            if (dih == null)
                return;

            if (dih.GetEffectState(cboxEffSlots.SelectedIndex) == DInputHandler.EffectState.ACTIVE)
                btnPauseEffect.Text = "Pause effect";
            else if (dih.GetEffectState(cboxEffSlots.SelectedIndex) == DInputHandler.EffectState.PAUSED)
                btnPauseEffect.Text = "Resume effect";
            else
                btnPauseEffect.Text = "(no effect)";
        }

        /// <summary>
        /// Event handler:
        /// Toggles autocentering force
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbAutoCentering_CheckedChanged(object sender, EventArgs ea)
        {
            DInputHandler dih = GetDInputHandler();
            if (dih == null)
                return;

            dih.SetAutoCenter(cbAutoCentering.Checked);
        }

        private void Form1_onActivated(object sender, EventArgs ea)
        {
            if (diHandlers == null)
                return;

            foreach (DInputHandler dih in diHandlers.Values)
            {
                dih.ResumeAllEffects();
            }
            updateDeviceStatus.Enabled = true;
        }

        private void Form1_onClosing(object sender, FormClosingEventArgs e)
        {
            if (diHandlers == null)
                return;

            //Don't forget to stop any effect that might be still playing
            foreach (DInputHandler dih in diHandlers.Values)
                dih.StopAllEffects();

            DInputHandler.DestroyDirectInput();
        }

        private void Form1_onDeactivate(object sender, EventArgs ea)
        {
            if (diHandlers == null)
                return;

            updateDeviceStatus.Enabled = false;
        }

        /// <summary>
        /// Event handler:
        /// Updates device input controls status.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ea"></param>
        private void updateDeviceStatus_onTick(object sender, EventArgs ea)
        {
            DataClasses.AxesInput dc;
            try
            {
                dc = GetDInputHandler().GetDeviceStatus();
            }
            catch (NullReferenceException)
            {
                updateDeviceStatus.Enabled = false;
                return;
            }

            if (dc != null)
            {
                lblXAxis.Text = dc.xAxis.ToString();
                lblYAxis.Text = dc.yAxis.ToString();
                lblZAxis.Text = dc.zAxis.ToString();
                lblRxAxis.Text = dc.rxAxis.ToString();
                lblRyAxis.Text = dc.ryAxis.ToString();
                lblRzAxis.Text = dc.rzAxis.ToString();
                lblFX.Text = dc.fX.ToString();
                lblFY.Text = dc.fY.ToString();
                lblFZ.Text = dc.fZ.ToString();
            }
            else
            {
                updateDeviceStatus.Enabled = false;
            }
        }

        #endregion
    }
}
