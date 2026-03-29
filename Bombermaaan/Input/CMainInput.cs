/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008 Bernd Arnold
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
 *  \file CMainInput.cs
 *  \brief The main input device for menu and system controls
 */

namespace Bombermaaan
{
    // Menu control indices
    public static class MenuControlIndex
    {
        public const int NUMBER_OF_MENU_CONTROLS     = 8;
        public const int MAX_PLAYER_INPUT_NAME_LENGTH = 16;

        public const int MENU_UP       = 0;
        public const int MENU_DOWN     = 1;
        public const int MENU_LEFT     = 2;
        public const int MENU_RIGHT    = 3;
        public const int MENU_PREVIOUS = 4;
        public const int MENU_NEXT1    = 5;
        public const int MENU_NEXT2    = 6;
        public const int MENU_NEXT3    = 7;

        public const int NUMBER_OF_SYSTEM_CONTROLS = 2;
        public const int SYSTEM_PAUSE              = 0;
        public const int SYSTEM_BREAK             = 1;
    }

    //*****************************************************************************

    /// <summary>
    /// Represents a single menu navigation control with repeat behaviour.
    /// </summary>
    public struct SMenuControl
    {
        public int   Key;        // Index of the control key on the device
        public float PressTime;  // Time (seconds) elapsed since the control was first pressed
        public float RepeatTime; // Time (seconds) elapsed since the control last repeated
        public bool  Active;     // Is the control active? (takes repetition into account)
    }

    //*****************************************************************************

    /// <summary>
    /// Represents a system-level control (pause, break).
    /// </summary>
    public struct SSystemControl
    {
        public int  Key;      // Index of the control key on the device
        public bool State;    // True only on the frame the key is first pressed
        public bool Pressing; // True while the key is held down
    }

    //*****************************************************************************

    /// <summary>
    /// Handles main menu and system input (keyboard navigation, pause, break).
    /// </summary>
    public class CMainInput
    {
        // Time (seconds) before control starts repeating
        private const float MENU_CONTROL_DELAY = 0.400f;
        // Time (seconds) between repetitions
        private const float MENU_CONTROL_RATE  = 0.180f;

        private CTimer         m_pTimer;
        private CInputSDL      m_pDirectInput;
        private SMenuControl[] m_MenuControls;
        private SSystemControl[] m_SystemControls;

        //**********************************************************************

        public CMainInput()
        {
            m_pTimer        = null;
            m_pDirectInput  = null;

            m_MenuControls   = new SMenuControl[MenuControlIndex.NUMBER_OF_MENU_CONTROLS];
            m_SystemControls = new SSystemControl[MenuControlIndex.NUMBER_OF_SYSTEM_CONTROLS];

            m_MenuControls[MenuControlIndex.MENU_UP].Key       = InputConstants.KEYBOARD_UP;
            m_MenuControls[MenuControlIndex.MENU_DOWN].Key     = InputConstants.KEYBOARD_DOWN;
            m_MenuControls[MenuControlIndex.MENU_LEFT].Key     = InputConstants.KEYBOARD_LEFT;
            m_MenuControls[MenuControlIndex.MENU_RIGHT].Key    = InputConstants.KEYBOARD_RIGHT;
            m_MenuControls[MenuControlIndex.MENU_PREVIOUS].Key = InputConstants.KEYBOARD_BACK;
            m_MenuControls[MenuControlIndex.MENU_NEXT1].Key    = InputConstants.KEYBOARD_RETURN;
            m_MenuControls[MenuControlIndex.MENU_NEXT2].Key    = InputConstants.KEYBOARD_NUMPADENTER;
            m_MenuControls[MenuControlIndex.MENU_NEXT3].Key    = InputConstants.KEYBOARD_SPACE;

            for (int c = 0; c < MenuControlIndex.NUMBER_OF_SYSTEM_CONTROLS; c++)
            {
                m_SystemControls[c].State    = false;
                m_SystemControls[c].Pressing = false;
            }
        }

        //**********************************************************************

        public CInputSDL GetInput()
        {
            return m_pDirectInput;
        }

        public void SetInput(CInputSDL pDirectInput)
        {
            m_pDirectInput = pDirectInput;
        }

        public void SetTimer(CTimer pTimer)
        {
            m_pTimer = pTimer;
        }

        //**********************************************************************

        public void Create()
        {
            System.Diagnostics.Debug.Assert(m_pTimer       != null);
            System.Diagnostics.Debug.Assert(m_pDirectInput != null);

            // Set the key index for each menu control
            m_MenuControls[MenuControlIndex.MENU_UP].Key       = InputConstants.KEYBOARD_UP;
            m_MenuControls[MenuControlIndex.MENU_DOWN].Key     = InputConstants.KEYBOARD_DOWN;
            m_MenuControls[MenuControlIndex.MENU_LEFT].Key     = InputConstants.KEYBOARD_LEFT;
            m_MenuControls[MenuControlIndex.MENU_RIGHT].Key    = InputConstants.KEYBOARD_RIGHT;
            m_MenuControls[MenuControlIndex.MENU_PREVIOUS].Key = InputConstants.KEYBOARD_BACK;
            m_MenuControls[MenuControlIndex.MENU_NEXT1].Key    = InputConstants.KEYBOARD_RETURN;
            m_MenuControls[MenuControlIndex.MENU_NEXT2].Key    = InputConstants.KEYBOARD_NUMPADENTER;
            m_MenuControls[MenuControlIndex.MENU_NEXT3].Key    = InputConstants.KEYBOARD_SPACE;

            // Zero-out each menu control state
            for (int c = 0; c < MenuControlIndex.NUMBER_OF_MENU_CONTROLS; c++)
            {
                m_MenuControls[c].PressTime  = 0.0f;
                m_MenuControls[c].RepeatTime = 0.0f;
                m_MenuControls[c].Active     = false;
            }

            m_SystemControls[MenuControlIndex.SYSTEM_PAUSE].Key = InputConstants.KEYBOARD_P;
            m_SystemControls[MenuControlIndex.SYSTEM_BREAK].Key = InputConstants.KEYBOARD_ESCAPE;

            for (int c = 0; c < MenuControlIndex.NUMBER_OF_SYSTEM_CONTROLS; c++)
            {
                m_SystemControls[c].State    = false;
                m_SystemControls[c].Pressing = false;
            }
        }

        //**********************************************************************

        public void Destroy()
        {
            // Nothing to do
        }

        //**********************************************************************

        public void Open()
        {
            m_pDirectInput.OpenKeyboard();
        }

        public bool IsOpened()
        {
            return m_pDirectInput.IsKeyboardOpened();
        }

        public void Close()
        {
            m_pDirectInput.CloseKeyboard();
        }

        //**********************************************************************

        public void Update()
        {
            // Update the keyboard state
            m_pDirectInput.UpdateKeyboard();

            // Test each menu control
            for (int control = 0; control < MenuControlIndex.NUMBER_OF_MENU_CONTROLS; control++)
            {
                bool controlActiveOnDevice = m_pDirectInput.GetKey(m_MenuControls[control].Key);

                if (controlActiveOnDevice)
                {
                    // First frame the control is pressed
                    if (m_MenuControls[control].PressTime == 0.0f)
                    {
                        m_MenuControls[control].Active     = true;
                        m_MenuControls[control].PressTime += m_pTimer.GetDeltaTime();
                    }
                    // Repeat delay has not elapsed yet
                    else if (m_MenuControls[control].PressTime < MENU_CONTROL_DELAY)
                    {
                        m_MenuControls[control].Active     = false;
                        m_MenuControls[control].PressTime += m_pTimer.GetDeltaTime();
                    }
                    // Repeat delay fully elapsed — handle repetition
                    else
                    {
                        if (MENU_CONTROL_RATE != 0.0f)
                        {
                            // Start a new repetition
                            if (m_MenuControls[control].RepeatTime == 0.0f)
                            {
                                m_MenuControls[control].Active      = true;
                                m_MenuControls[control].RepeatTime += m_pTimer.GetDeltaTime();
                            }
                            // Rate not elapsed yet for current repetition
                            else if (m_MenuControls[control].RepeatTime < MENU_CONTROL_RATE)
                            {
                                m_MenuControls[control].Active      = false;
                                m_MenuControls[control].RepeatTime += m_pTimer.GetDeltaTime();
                            }
                            // Rate fully elapsed — reset for next repetition
                            else
                            {
                                m_MenuControls[control].Active     = false;
                                m_MenuControls[control].RepeatTime = 0.0f;
                            }
                        }
                        else
                        {
                            // No repeat rate — always active once delay elapses
                            m_MenuControls[control].Active = true;
                        }
                    }
                }
                else
                {
                    // Control released — reset state
                    m_MenuControls[control].PressTime  = 0.0f;
                    m_MenuControls[control].RepeatTime = 0.0f;
                    m_MenuControls[control].Active     = false;
                }
            }

            // System controls: State is true only on the frame the key becomes active
            for (int control = 0; control < MenuControlIndex.NUMBER_OF_SYSTEM_CONTROLS; control++)
            {
                if (m_pDirectInput.GetKey(m_SystemControls[control].Key) &&
                    !m_SystemControls[control].State &&
                    !m_SystemControls[control].Pressing)
                {
                    m_SystemControls[control].State    = true;
                    m_SystemControls[control].Pressing = true;
                }
                else if (!m_pDirectInput.GetKey(m_SystemControls[control].Key) &&
                          m_SystemControls[control].Pressing)
                {
                    m_SystemControls[control].State    = false;
                    m_SystemControls[control].Pressing = false;
                }
                else
                {
                    m_SystemControls[control].State = false;
                }
            }
        }

        //**********************************************************************
        // Query methods
        //**********************************************************************

        public bool TestUp()
        {
            return m_MenuControls[MenuControlIndex.MENU_UP].Active;
        }

        public bool TestDown()
        {
            return m_MenuControls[MenuControlIndex.MENU_DOWN].Active;
        }

        public bool TestLeft()
        {
            return m_MenuControls[MenuControlIndex.MENU_LEFT].Active;
        }

        public bool TestRight()
        {
            return m_MenuControls[MenuControlIndex.MENU_RIGHT].Active;
        }

        public bool TestPrevious()
        {
            return m_MenuControls[MenuControlIndex.MENU_PREVIOUS].Active;
        }

        public bool TestNext()
        {
            return m_MenuControls[MenuControlIndex.MENU_NEXT1].Active ||
                   m_MenuControls[MenuControlIndex.MENU_NEXT2].Active ||
                   m_MenuControls[MenuControlIndex.MENU_NEXT3].Active;
        }

        public bool TestPause()
        {
            return m_SystemControls[MenuControlIndex.SYSTEM_PAUSE].State;
        }

        public bool TestBreak()
        {
            return m_SystemControls[MenuControlIndex.SYSTEM_BREAK].State;
        }
    }
}
