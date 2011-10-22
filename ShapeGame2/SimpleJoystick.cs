/*
 * Simple Joystick API
 *	Coded by Chris Seto 2010
 *	
 * This code released under the Apache 2.0 license, copyright Chris Seto 2010
 * 
 * */
using System;
using SlimDX.DirectInput;

namespace ShapeGame2
{
    class SimpleJoystick
    {
        /// <summary>
        /// Joystick handle
        /// </summary>
        private Joystick Joystick;

        /// <summary>
        /// Get the state of the joystick
        /// </summary>
        public JoystickState State
        {
            get
            {
                try
                {
                    if (Joystick.Acquire().IsFailure)
                        throw new Exception("Joystick failure");

                    if (Joystick.Poll().IsFailure)
                        throw new Exception("Joystick failure");

                    return Joystick.GetCurrentState();
                }
                catch (Exception)
                {

                    throw;
                }


            }
        }

        /// <summary>
        /// Construct, attach the joystick
        /// </summary>
        public SimpleJoystick()
        {
            try
            {
                DirectInput dinput = new DirectInput();

                // Search for device
                foreach (DeviceInstance device in dinput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
                {
                    // Create device
                    try
                    {
                        Joystick = new Joystick(dinput, device.InstanceGuid);
                        break;
                    }
                    catch (DirectInputException)
                    {
                    }
                }

                if (Joystick == null)
                    throw new Exception("No joystick found");

                foreach (DeviceObjectInstance deviceObject in Joystick.GetObjects())
                {
                    if ((deviceObject.ObjectType & ObjectDeviceType.Axis) != 0)
                        Joystick.GetObjectPropertiesById((int)deviceObject.ObjectType).SetRange(-100, 100);
                }

                // Acquire device
                Joystick.Acquire();
            }
            catch (Exception)
            {
                throw;
               
            }
            
        }

        /// <summary>
        /// Release joystick
        /// </summary>
        public void Release()
        {
            if (Joystick != null)
            {
                Joystick.Unacquire();
                Joystick.Dispose();
            }

            Joystick = null;
        }
    }
}
