/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2010 Bernd Arnold
    Copyright (C) 2026 Ömer Gürbüz

    This file is part of Bombermaaan.

    Bombermaaan is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Bombermaaan is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Bombermaaan.  If not, see <http://www.gnu.org/licenses/>.

************************************************************************************/

/**
 *  \file CPlayerInput.cs
 *  \brief Player's input device
 */

namespace Bombermaaan
{
    // Player-input control indices (what action each control slot represents)
    public static class PlayerControlIndex
    {
        public const int MAX_PLAYER_INPUT_NAME_LENGTH = 16;
        public const int MAX_CONTROL_NAME_LENGTH      = 20;

        public const int NO_ACTIVATED_CONTROL = -1;

        public const int CONTROL_UP      = 0;
        public const int CONTROL_DOWN    = 1;
        public const int CONTROL_LEFT    = 2;
        public const int CONTROL_RIGHT   = 3;
        public const int CONTROL_ACTION1 = 4;
        public const int CONTROL_ACTION2 = 5;
    }

    //*****************************************************************************

    /// <summary>
    /// Handles per-player input, dispatching to keyboard or joystick depending
    /// on which player input index this object represents.
    /// </summary>
    public class CPlayerInput
    {
        private CInputSDL m_pDirectInput;
        private COptions  m_pOptions;
        private int       m_PlayerInput;
        private string    m_Name;
        private string    m_ControlName;

        //**********************************************************************

        public CPlayerInput()
        {
            m_PlayerInput  = -1;
            m_pDirectInput = null;
            m_pOptions     = null;
            m_Name         = string.Empty;
            m_ControlName  = string.Empty;
        }

        //**********************************************************************

        public CInputSDL GetDirectInput()
        {
            return m_pDirectInput;
        }

        public void SetDirectInput(CInputSDL pDirectInput)
        {
            m_pDirectInput = pDirectInput;
        }

        public void SetOptions(COptions pOptions)
        {
            m_pOptions = pOptions;
        }

        //**********************************************************************

        public void Create(int playerInput)
        {
            System.Diagnostics.Debug.Assert(m_pDirectInput != null);
            System.Diagnostics.Debug.Assert(m_pOptions     != null);
            System.Diagnostics.Debug.Assert(playerInput    >= 0);

            m_PlayerInput = playerInput;
            CreateName();
        }

        //**********************************************************************

        private void CreateName()
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
                m_Name = string.Format("KEYBOARD {0}", m_PlayerInput + 1);
            else
                m_Name = string.Format("JOYSTICK {0}", m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS + 1);
        }

        //**********************************************************************

        public void Destroy()
        {
            // Nothing to do
        }

        //**********************************************************************

        public void Open()
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
                m_pDirectInput.OpenKeyboard();
            else
                m_pDirectInput.OpenJoystick(m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS);
        }

        public bool IsOpened()
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
                return m_pDirectInput.IsKeyboardOpened();
            else
                return m_pDirectInput.IsJoystickOpened(m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS);
        }

        public void Close()
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
                m_pDirectInput.CloseKeyboard();
            else
                m_pDirectInput.CloseJoystick(m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS);
        }

        //**********************************************************************

        public string GetName()
        {
            return m_Name;
        }

        //**********************************************************************

        public void Update()
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
                m_pDirectInput.UpdateKeyboard();
            else
                m_pDirectInput.UpdateJoystick(m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS);
        }

        //**********************************************************************
        // Convenience test methods
        //**********************************************************************

        public bool TestUp()      { return TestControl(PlayerControlIndex.CONTROL_UP);      }
        public bool TestDown()    { return TestControl(PlayerControlIndex.CONTROL_DOWN);    }
        public bool TestLeft()    { return TestControl(PlayerControlIndex.CONTROL_LEFT);    }
        public bool TestRight()   { return TestControl(PlayerControlIndex.CONTROL_RIGHT);   }
        public bool TestAction1() { return TestControl(PlayerControlIndex.CONTROL_ACTION1); }
        public bool TestAction2() { return TestControl(PlayerControlIndex.CONTROL_ACTION2); }

        /// <summary>The "menu next" button was pressed?</summary>
        public bool TestMenuNext()
        {
            return TestMenuControl(InputConstants.JOYSTICK_BUTTON_MENU_NEXT);
        }

        /// <summary>The "pause" button was pressed?</summary>
        public bool TestPause()
        {
            return TestMenuControl(InputConstants.JOYSTICK_BUTTON_BREAK);
        }

        //**********************************************************************

        /// <summary>
        /// Returns the index of the first currently activated control,
        /// or NO_ACTIVATED_CONTROL (-1) if none.
        /// </summary>
        public int GetActivatedControl()
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
            {
                for (int key = 0; key < InputConstants.MAX_KEYS; key++)
                {
                    if (m_pDirectInput.GetKey(key))
                        return key;
                }
            }
            else
            {
                int joyIndex = m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS;
                int axisX = m_pDirectInput.GetJoystickAxisX(joyIndex);
                int axisY = m_pDirectInput.GetJoystickAxisY(joyIndex);

                if (axisY < -InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    return InputConstants.JOYSTICK_UP;
                else if (axisY > +InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    return InputConstants.JOYSTICK_DOWN;
                else if (axisX < -InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    return InputConstants.JOYSTICK_LEFT;
                else if (axisX > +InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    return InputConstants.JOYSTICK_RIGHT;
                else
                {
                    for (int btn = 0; btn < InputConstants.MAX_JOYSTICK_BUTTONS; btn++)
                    {
                        if (m_pDirectInput.GetJoystickButton(joyIndex, btn))
                            return InputConstants.JOYSTICK_BUTTON(btn);
                    }
                }
            }

            return PlayerControlIndex.NO_ACTIVATED_CONTROL;
        }

        //**********************************************************************

        /// <summary>
        /// Returns a friendly display name for the given control index.
        /// </summary>
        public string GetControlName(int control)
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
            {
                m_ControlName = m_pDirectInput.GetKeyFriendlyName(control);
            }
            else
            {
                if (control < InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS)
                {
                    switch (control)
                    {
                        case InputConstants.JOYSTICK_UP:    m_ControlName = "UP";    break;
                        case InputConstants.JOYSTICK_DOWN:  m_ControlName = "DOWN";  break;
                        case InputConstants.JOYSTICK_LEFT:  m_ControlName = "LEFT";  break;
                        case InputConstants.JOYSTICK_RIGHT: m_ControlName = "RIGHT"; break;
                        default:                            m_ControlName = string.Empty; break;
                    }
                }
                else
                {
                    m_ControlName = string.Format("BUTTON {0}", control - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS + 1);
                }
            }

            return m_ControlName;
        }

        //**********************************************************************

        /// <summary>
        /// Returns true if the control bound to the given control slot is
        /// currently active on the input device.
        /// </summary>
        public bool TestControl(int control)
        {
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
            {
                // Keyboard: look up the key bound to this control in options
                return m_pDirectInput.GetKey(m_pOptions.GetControl(m_PlayerInput, control));
            }
            else
            {
                int joyIndex  = m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS;
                int optControl = m_pOptions.GetControl(m_PlayerInput, control);

                if (optControl < InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS)
                {
                    switch (optControl)
                    {
                        case InputConstants.JOYSTICK_UP:
                            return m_pDirectInput.GetJoystickAxisY(joyIndex) < -InputConstants.JOYSTICK_AXIS_THRESHOLD;
                        case InputConstants.JOYSTICK_DOWN:
                            return m_pDirectInput.GetJoystickAxisY(joyIndex) > +InputConstants.JOYSTICK_AXIS_THRESHOLD;
                        case InputConstants.JOYSTICK_LEFT:
                            return m_pDirectInput.GetJoystickAxisX(joyIndex) < -InputConstants.JOYSTICK_AXIS_THRESHOLD;
                        case InputConstants.JOYSTICK_RIGHT:
                            return m_pDirectInput.GetJoystickAxisX(joyIndex) > +InputConstants.JOYSTICK_AXIS_THRESHOLD;
                    }
                }
                else
                {
                    return m_pDirectInput.GetJoystickButton(
                        joyIndex,
                        optControl - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS);
                }
            }

            System.Diagnostics.Debug.Assert(false);
            return false;
        }

        //**********************************************************************

        /// <summary>
        /// Checks a joystick menu control (e.g. menu-next, pause).
        /// Returns false for keyboard-based player inputs — those are handled
        /// by CMainInput.
        /// </summary>
        public bool TestMenuControl(int menuControl)
        {
            // Keyboard players do not use this path
            if (m_PlayerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS)
                return false;

            return m_pDirectInput.GetJoystickButton(
                m_PlayerInput - InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS,
                menuControl - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS);
        }
    }
}
