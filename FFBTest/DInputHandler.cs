using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.DirectInput;

namespace FFBTest
{
    class DInputHandler
    {
        public static DirectInput dInput;
        public enum EffectState { OFF, ACTIVE, PAUSED };

        private const Int16 FFB_EFFECT_SLOTS = 8;

        private Effect[] ffbEffects;
        private Guid devInstanceGuid;
        private Joystick ffbDevice;
        private List<int> actuatorsObjectTypes;

        #region Static methods
        /// <summary>
        /// Initializes DirectInput interface.
        /// </summary>
        /// <returns></returns>
        public static bool InitDirectInput()
        {
            /* DirectInput interface should be initialized only once during startup */
            if (dInput != null)
            {
                MessageBox.Show("DirectInput already initialized.", FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            dInput = new DirectInput();
            if (dInput == null)
            {
                MessageBox.Show("Cannot initialize DirectInput.", FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deletes DirectInput interface.
        /// </summary>
        public static void DestroyDirectInput()
        {
            dInput.Dispose();
            dInput = null;
        }
        #endregion

        public DInputHandler(Guid g)
        {
            devInstanceGuid = g;
        }

        /// <summary>
        /// Sets a device up for us to use it.
        /// </summary>
        /// <param name="handle">
        /// Handle to the application which will have access to the device.
        /// </param>
        /// <returns>
        /// Returs true if successful, otherwise returns false.
        /// </returns>
        public bool InitDevice(IntPtr handle, bool autoCentering)
        {
            SlimDX.Result res;
            ffbDevice = new Joystick(dInput, devInstanceGuid);
            if (ffbDevice == null)
            {
                MessageBox.Show("Cannot create device.", FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            ffbEffects = new Effect[FFB_EFFECT_SLOTS];

            /* Tell DirectInput we want to have exlucive access to the device when the application has focus.
             * Exlusive access is necessary for the force feedback to work, but it prevents other applications
             * from accessing the device. */
            res = ffbDevice.SetCooperativeLevel(handle, CooperativeLevel.Foreground | CooperativeLevel.Exclusive);
            if (res != ResultCode.Success)
            {
                MessageBox.Show("Cannot set cooperative level.\n" +
                                "Make sure no other application is using the device\n" +
                                res.ToString(), FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }


            /* Get and assign force feedback actuators offsets.
            *  Number of axes does not necessarily have to correspond to the number
            *  of available force feedback actuators. We will try to get IDs
            *  of all available actuators and create arrays of proper size using
            *  the count of available actuators. (arrays required by DirectInput
            *  are created later in SendFFBEffect() method)*/

            actuatorsObjectTypes = new List<int>();
            //Get all available force feedback actuators
            foreach (DeviceObjectInstance doi in ffbDevice.GetObjects())
            {
                if ((doi.ObjectType & ObjectDeviceType.ForceFeedbackActuator) != 0)
                    actuatorsObjectTypes.Add((int)doi.ObjectType);
            }

            ffbDevice.Properties.SetRange(0, 16383);
            ffbDevice.Properties.AutoCenter = autoCentering;

            return true;
        }

        /// <summary>
        /// Enables or disables autocentering force
        /// </summary>
        /// <returns></returns>
        public bool SetAutoCenter(bool enableAC)
        {
            SlimDX.Result res;

            if (ffbDevice == null)
                return false;

            ffbDevice.Unacquire();
            ffbDevice.Properties.AutoCenter = enableAC;

            res = ffbDevice.Acquire();
            if (res.IsFailure)
            {
                MessageBox.Show(FFBInspector.Properties.Resources.errMsg_dihCantAcquire, FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Polls the device and gets axes positions and buttons status.
        /// </summary>
        /// <returns>
        /// Axes positions and buttons status on success, otherwise returns null.
        /// </returns>
        public DataClasses.AxesInput GetDeviceStatus()
        {
            if (ffbDevice == null)
                return null;

            SlimDX.Result res = ffbDevice.Acquire();
            if (!(res != ResultCode.Success || res != ResultCode.Failure))
                return null;

            JoystickState js;
            try
            {
                ffbDevice.Poll();
                js = ffbDevice.GetCurrentState();
            }
            catch (Exception)
            {
                MessageBox.Show(FFBInspector.Properties.Resources.errMsg_dihGetDevStat, FFBInspector.Properties.Resources.errCap_dihError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return null;
            }

            DataClasses.AxesInput dc = new DataClasses.AxesInput();
            dc.xAxis = js.X;
            dc.yAxis = js.Y;
            dc.zAxis = js.Z;
            dc.rxAxis = js.RotationX;
            dc.ryAxis = js.RotationY;
            dc.rzAxis = js.RotationZ;
            dc.fX = js.ForceX;
            dc.fY = js.ForceY;
            dc.fZ = js.ForceZ;

            return dc;
        }

        /// <summary>
        /// Gets all available force feedback actuator
        /// and returns their count.
        /// </summary>
        /// <returns>
        /// Count of available force feedback actuators.
        /// </returns>
        public int GetFFBActuatorCount()
        {
            if (ffbDevice == null)
                return 0;

            return actuatorsObjectTypes.Count;
        }

        public EffectState GetEffectState(int idx)
        {
            if (ffbEffects[idx] == null)
                return EffectState.OFF;

            if (ffbEffects[idx].Status == EffectStatus.Playing)
                return EffectState.ACTIVE;

            return EffectState.PAUSED;
        }

        /// <summary>
        /// Returns a list of all FFB effects available for a device.
        /// </summary>
        /// <returns></returns>
        public IList<EffectInfo> GetFFBEffects()
        {
            return ffbDevice.GetEffects();
        }

        /// <summary>
        /// Gets hardware information about the FFB device as reported by DirectInput.
        /// </summary>
        /// <returns>
        /// </returns>
        public DataClasses.HardwareInfo GetHardwareInfo()
        {
            if (ffbDevice == null)
                return null;

            DataClasses.HardwareInfo dc = new DataClasses.HardwareInfo();

            //Get info about hardware
            dc.axesCount = ffbDevice.Capabilities.AxesCount;
            dc.buttonCount = ffbDevice.Capabilities.ButtonCount;
            dc.povCount = ffbDevice.Capabilities.PovCount;
            dc.driverVersion = ffbDevice.Capabilities.DriverVersion;
            dc.fwVersion = ffbDevice.Capabilities.FirmwareRevision;
            dc.hwRevision = ffbDevice.Capabilities.HardwareRevision;
            dc.ffbMinTime = ffbDevice.Capabilities.ForceFeedbackMinimumTimeResolution;
            dc.ffbSamplePer = ffbDevice.Capabilities.ForceFeedbackSamplePeriod;

            return dc;
        }

        /// <summary>
        /// Pauses playback of all currently active effects without deleting them.
        /// Effects can be resumed by calling ResumeAllEffects()
        /// </summary>
        public void PauseAllEffects()
        {
            foreach (Effect ef in ffbEffects)
            {
                if (ef == null)
                    continue;

                StopFFBEffect(ef);
            }
        }

        public bool PauseOneEffect(int idx)
        {
            if (ffbDevice == null)
                return false;

            if (ffbEffects[idx] == null)
                return false;

            return StopFFBEffect(ffbEffects[idx]);
        }

        /// <summary>
        /// Releases the FFB device and cleans up.
        /// </summary>
        public void ReleaseAll()
        {
            if (ffbDevice != null)
            {
                ffbDevice.Unacquire();
                ffbDevice.Dispose();
                ffbDevice = null;
            }
        }


        /// <summary>
        /// Resumes playback of all currently active effects.
        /// </summary>
        public void ResumeAllEffects()
        {
            foreach (Effect ef in ffbEffects)
            {
                if (ef == null)
                    continue;

                StartFFBEffect(ef);
            }
        }

        public bool ResumeOneEffect(int idx)
        {
            if (ffbDevice == null)
                return false;

            if (ffbEffects[idx] == null)
                return false;

            return StartFFBEffect(ffbEffects[idx]);
        }

        /// <summary>
        /// Sets range of axes values reported by DirectInput.
        /// </summary>
        /// <param name="s">
        /// Range to be set.
        /// </param>
        /// <returns>
        /// Returns true if successful, otherwise returns false.
        /// </returns>
        public bool SetAxisRange(DataClasses.AxesRange s)
        {
            if (ffbDevice == null)
                return false;

            PauseAllEffects();
            ffbDevice.Unacquire();
            ffbDevice.Properties.SetRange(s.minimum, s.maximum);

 
            SlimDX.Result res = ffbDevice.Acquire();
            if (!(res != ResultCode.Success || res != ResultCode.Failure))
            {
                MessageBox.Show("Cannot reacquire the device.\n" +
                                "Make sure no other application is using the device\n" +
                                res.ToString(), FFBInspector.Properties.Resources.errCap_devError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            ResumeAllEffects();
            return true;
        }

        /// <summary>
        /// Creates an FFB effect, stores it at a given slot and starts its playback.
        /// </summary>
        /// <param name="data">
        /// Parameters of the effect to be created.
        /// </param>
        /// <returns>
        /// Returns true if successful, otherwise returns false.
        /// </returns>
        public bool SendFFBEffect(DataClasses.FFBEffectData data)
        {
            //Check if the device is properly initialized
            if (ffbDevice == null)
            {
                MessageBox.Show("Device not initialized.", FFBInspector.Properties.Resources.errCap_devError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            //If there is an effect already stored in this slot, remove it
            StopOneEffect(data.slot);

            int[] axes = new Int32[actuatorsObjectTypes.Count];
            int i = 0;
            foreach (int objt in actuatorsObjectTypes)
            {
                axes[i++] = objt;
            }

            //Set effect direction
            int[] dirs = data.effect.GetDirections();

            //Set the general effect parameters up
            EffectParameters eParams = new EffectParameters();
            eParams.Duration = data.effect.duration;
            eParams.Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds;
            eParams.Gain = data.effect.gain;
            eParams.SetAxes(axes, dirs);
            eParams.StartDelay = data.effect.startDelay;
            eParams.SamplePeriod = 0; //Use the default sample period;
            eParams.TriggerButton = data.effect.trigButton;
            eParams.TriggerRepeatInterval = data.effect.trigRepInterval;

            //Set the type specific effect parameters up
            TypeSpecificParameters typeSpec = null;
            if (data.effect.effectGuid == EffectGuid.ConstantForce)
            {
                DataClasses.ConstantEffectTypeData cfEfd = (DataClasses.ConstantEffectTypeData)data.effect;
                typeSpec = new ConstantForce();
                typeSpec.AsConstantForce().Magnitude = cfEfd.magnitude;
            }
            else if (data.effect.effectGuid == EffectGuid.RampForce)
            {
                DataClasses.RampEffectTypeData rfEfd = (DataClasses.RampEffectTypeData)data.effect;
                typeSpec = new RampForce();
                typeSpec.AsRampForce().Start = rfEfd.start;
                typeSpec.AsRampForce().End = rfEfd.end;
            }
            else if (data.effect.effectGuid == EffectGuid.Sine || data.effect.effectGuid == EffectGuid.Triangle ||
                     data.effect.effectGuid == EffectGuid.Square ||
                     data.effect.effectGuid == EffectGuid.SawtoothUp ||
                     data.effect.effectGuid == EffectGuid.SawtoothDown)
            {
                DataClasses.PeriodicEffectTypeData pfEfd = (DataClasses.PeriodicEffectTypeData)data.effect;
                typeSpec = new PeriodicForce();
                typeSpec.AsPeriodicForce().Magnitude = pfEfd.magnitude;
                typeSpec.AsPeriodicForce().Offset = pfEfd.offset;
                typeSpec.AsPeriodicForce().Period = pfEfd.period;
                typeSpec.AsPeriodicForce().Phase = pfEfd.phase;
            }
            else if (data.effect.effectGuid == EffectGuid.Friction || data.effect.effectGuid == EffectGuid.Inertia ||
                     data.effect.effectGuid == EffectGuid.Damper || data.effect.effectGuid == EffectGuid.Spring)
            {
                DataClasses.ConditionEffectTypeData cdEfd = (DataClasses.ConditionEffectTypeData)data.effect;
                typeSpec = new ConditionSet();
                typeSpec.AsConditionSet().Conditions = new Condition[1];

                typeSpec.AsConditionSet().Conditions[0].DeadBand = cdEfd.deadBand;
                typeSpec.AsConditionSet().Conditions[0].Offset = cdEfd.offset;
                typeSpec.AsConditionSet().Conditions[0].NegativeCoefficient = cdEfd.negCoeff;
                typeSpec.AsConditionSet().Conditions[0].NegativeSaturation = cdEfd.negSat;
                typeSpec.AsConditionSet().Conditions[0].PositiveCoefficient = cdEfd.posCoeff;
                typeSpec.AsConditionSet().Conditions[0].PositiveSaturation = cdEfd.posSat;
            }
            eParams.Parameters = typeSpec;

            //Create an envelope
            if (data.envelope != null)
            {
                Envelope envp = new Envelope();
                envp.AttackLevel = data.envelope.attackLevel;
                envp.AttackTime = data.envelope.attackTime;
                envp.FadeLevel = data.envelope.fadeLevel;
                envp.FadeTime = data.envelope.fadeTime;

                eParams.Envelope = envp;
            }

            //Create an effect and add it to the list
            Effect ef;
            try
            {
                ef = new Effect(ffbDevice, data.effect.effectGuid, eParams);
                ffbEffects[data.slot] = ef;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cannot create effect.\n" + ex.Message + "\n" + ex.Data,
                                FFBInspector.Properties.Resources.errCap_effError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return StartFFBEffect(ef);
        }

        /// <summary>
        /// Stops playback of all currently active effects and deletes them.
        /// </summary>
        public void StopAllEffects()
        {
            for (int i = 0; i < FFB_EFFECT_SLOTS; i++)
            {
                Effect ef = ffbEffects[i];
                if (ef == null)
                    continue;

                if (StopFFBEffect(ef))
                    ffbEffects[i] = null;
            }
        }

        /// <summary>
        /// Stops and deletes one effect.
        /// </summary>
        /// <param name="slot">
        /// Slot where the effect to be stopped is stored.
        /// </param>
        /// <returns>
        /// Returns true if successful, otherwise returns false.
        /// </returns>
        public bool StopOneEffect(int slot)
        {
            if (StopFFBEffect(ffbEffects[slot]))
            {
                ffbEffects[slot] = null;
                return true;
            }
            return false;
        }

        #region Private methods

        /// <summary>
        /// Starts playback of an effect.
        /// </summary>
        /// <param name="ef">
        /// Effect which is to be played.
        /// </param>
        /// <returns>
        /// Returns true if successful, otherwise returns false.
        /// </returns>
        private bool StartFFBEffect(Effect ef)
        {
            SlimDX.Result res;
            
            if (ef == null)
            {
                MessageBox.Show("NULL pointer to effect in \"DInputHandler::StartFFBEffect()\"",
                                FFBInspector.Properties.Resources.errCap_effError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            res = ffbDevice.Acquire();
            if (!(res != ResultCode.Success || res != ResultCode.Failure))
            {
                MessageBox.Show("Cannot acquire the device.\n" +
                                "Make sure no other application is using the device\n" +
                                res.ToString(), FFBInspector.Properties.Resources.errCap_devError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
            res = ef.Start();
            if (res != ResultCode.Success)
            {
                MessageBox.Show("Cannot start the effect.\n", FFBInspector.Properties.Resources.errCap_effError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stops playback of an effect if it is playing.
        /// </summary>
        /// <param name="ef">
        /// The effect whose playback is to be stopped.
        /// </param>
        /// <returns>
        /// Returns true if successful, otherwise returns false.
        /// </returns>
        private bool StopFFBEffect(Effect ef)
        {
            SlimDX.Result res;

            if (ef == null)
            {
                return false;
            }

            res = ffbDevice.Acquire();
            if (!(res != ResultCode.Success || res != ResultCode.Failure))
            {
                MessageBox.Show("Cannot acquire the device.\n" +
                                "Make sure no other application is using the device\n" +
                                res.ToString(), FFBInspector.Properties.Resources.errCap_devError,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
           
            res = ef.Stop();
            if (res != ResultCode.Success)
            {
                MessageBox.Show("Cannot stop effect.", FFBInspector.Properties.Resources.errCap_effError,
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            return true;
        }

        #endregion
    }
}
