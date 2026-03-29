/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008-2010 Markus Drescher
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
 *  \file CInputSDL.cs
 *  \brief SDL input devices
 */

using System;
using System.Collections.Generic;
using Bombermaaan.SDL2;

namespace Bombermaaan
{
    // Maximum number of buttons a joystick can have
    // Maximum number of keys (SDL2 scancodes)
    // KEYBOARD_* constants map to SDL2 scancode values

    public static class InputConstants
    {
        public const int MAX_JOYSTICK_BUTTONS = 32;
        public const int MAX_KEYS             = SDL.SDL_NUM_SCANCODES; // 512

        // Keyboard key IDs (SDL2 scancodes)
        public const int KEYBOARD_ESCAPE       = SDL.SDL_SCANCODE_ESCAPE;
        public const int KEYBOARD_1            = SDL.SDL_SCANCODE_1;
        public const int KEYBOARD_2            = SDL.SDL_SCANCODE_2;
        public const int KEYBOARD_3            = SDL.SDL_SCANCODE_3;
        public const int KEYBOARD_4            = SDL.SDL_SCANCODE_4;
        public const int KEYBOARD_5            = SDL.SDL_SCANCODE_5;
        public const int KEYBOARD_6            = SDL.SDL_SCANCODE_6;
        public const int KEYBOARD_7            = SDL.SDL_SCANCODE_7;
        public const int KEYBOARD_8            = SDL.SDL_SCANCODE_8;
        public const int KEYBOARD_9            = SDL.SDL_SCANCODE_9;
        public const int KEYBOARD_0            = SDL.SDL_SCANCODE_0;
        public const int KEYBOARD_MINUS        = 45;   // SDL_SCANCODE_MINUS
        public const int KEYBOARD_EQUALS       = 46;   // SDL_SCANCODE_EQUALS
        public const int KEYBOARD_BACK         = SDL.SDL_SCANCODE_BACKSPACE;
        public const int KEYBOARD_TAB          = SDL.SDL_SCANCODE_TAB;
        public const int KEYBOARD_Q            = SDL.SDL_SCANCODE_Q;
        public const int KEYBOARD_W            = SDL.SDL_SCANCODE_W;
        public const int KEYBOARD_E            = SDL.SDL_SCANCODE_E;
        public const int KEYBOARD_R            = SDL.SDL_SCANCODE_R;
        public const int KEYBOARD_T            = SDL.SDL_SCANCODE_T;
        public const int KEYBOARD_Y            = SDL.SDL_SCANCODE_Y;
        public const int KEYBOARD_U            = SDL.SDL_SCANCODE_U;
        public const int KEYBOARD_I            = SDL.SDL_SCANCODE_I;
        public const int KEYBOARD_O            = SDL.SDL_SCANCODE_O;
        public const int KEYBOARD_P            = SDL.SDL_SCANCODE_P;
        public const int KEYBOARD_LBRACKET     = 47;   // SDL_SCANCODE_LEFTBRACKET
        public const int KEYBOARD_RBRACKET     = 48;   // SDL_SCANCODE_RIGHTBRACKET
        public const int KEYBOARD_RETURN       = SDL.SDL_SCANCODE_RETURN;
        public const int KEYBOARD_LCONTROL     = SDL.SDL_SCANCODE_LCTRL;
        public const int KEYBOARD_A            = SDL.SDL_SCANCODE_A;
        public const int KEYBOARD_S            = SDL.SDL_SCANCODE_S;
        public const int KEYBOARD_D            = SDL.SDL_SCANCODE_D;
        public const int KEYBOARD_F            = SDL.SDL_SCANCODE_F;
        public const int KEYBOARD_G            = SDL.SDL_SCANCODE_G;
        public const int KEYBOARD_H            = SDL.SDL_SCANCODE_H;
        public const int KEYBOARD_J            = SDL.SDL_SCANCODE_J;
        public const int KEYBOARD_K            = SDL.SDL_SCANCODE_K;
        public const int KEYBOARD_L            = SDL.SDL_SCANCODE_L;
        public const int KEYBOARD_SEMICOLON    = 51;   // SDL_SCANCODE_SEMICOLON
        public const int KEYBOARD_APOSTROPHE   = 52;   // SDL_SCANCODE_APOSTROPHE
        public const int KEYBOARD_GRAVE        = 53;   // SDL_SCANCODE_GRAVE
        public const int KEYBOARD_LSHIFT       = SDL.SDL_SCANCODE_LSHIFT;
        public const int KEYBOARD_BACKSLASH    = 49;   // SDL_SCANCODE_BACKSLASH
        public const int KEYBOARD_Z            = SDL.SDL_SCANCODE_Z;
        public const int KEYBOARD_X            = SDL.SDL_SCANCODE_X;
        public const int KEYBOARD_C            = SDL.SDL_SCANCODE_C;
        public const int KEYBOARD_V            = SDL.SDL_SCANCODE_V;
        public const int KEYBOARD_B            = SDL.SDL_SCANCODE_B;
        public const int KEYBOARD_N            = SDL.SDL_SCANCODE_N;
        public const int KEYBOARD_M            = SDL.SDL_SCANCODE_M;
        public const int KEYBOARD_COMMA        = 54;   // SDL_SCANCODE_COMMA
        public const int KEYBOARD_PERIOD       = 55;   // SDL_SCANCODE_PERIOD
        public const int KEYBOARD_SLASH        = 56;   // SDL_SCANCODE_SLASH
        public const int KEYBOARD_RSHIFT       = SDL.SDL_SCANCODE_RSHIFT;
        public const int KEYBOARD_MULTIPLY     = 85;   // SDL_SCANCODE_KP_MULTIPLY
        public const int KEYBOARD_LMENU        = SDL.SDL_SCANCODE_LALT;
        public const int KEYBOARD_SPACE        = SDL.SDL_SCANCODE_SPACE;
        public const int KEYBOARD_CAPITAL      = 57;   // SDL_SCANCODE_CAPSLOCK
        public const int KEYBOARD_F1           = SDL.SDL_SCANCODE_F1;
        public const int KEYBOARD_F2           = SDL.SDL_SCANCODE_F2;
        public const int KEYBOARD_F3           = SDL.SDL_SCANCODE_F3;
        public const int KEYBOARD_F4           = SDL.SDL_SCANCODE_F4;
        public const int KEYBOARD_F5           = SDL.SDL_SCANCODE_F5;
        public const int KEYBOARD_F6           = SDL.SDL_SCANCODE_F6;
        public const int KEYBOARD_F7           = SDL.SDL_SCANCODE_F7;
        public const int KEYBOARD_F8           = SDL.SDL_SCANCODE_F8;
        public const int KEYBOARD_F9           = SDL.SDL_SCANCODE_F9;
        public const int KEYBOARD_F10          = SDL.SDL_SCANCODE_F10;
        public const int KEYBOARD_NUMLOCK      = 83;   // SDL_SCANCODE_NUMLOCKCLEAR
        public const int KEYBOARD_SCROLL       = 71;   // SDL_SCANCODE_SCROLLLOCK
        public const int KEYBOARD_NUMPAD7      = 95;   // SDL_SCANCODE_KP_7
        public const int KEYBOARD_NUMPAD8      = 96;   // SDL_SCANCODE_KP_8
        public const int KEYBOARD_NUMPAD9      = 97;   // SDL_SCANCODE_KP_9
        public const int KEYBOARD_SUBTRACT     = 86;   // SDL_SCANCODE_KP_MINUS
        public const int KEYBOARD_NUMPAD4      = 92;   // SDL_SCANCODE_KP_4
        public const int KEYBOARD_NUMPAD5      = 93;   // SDL_SCANCODE_KP_5
        public const int KEYBOARD_NUMPAD6      = 94;   // SDL_SCANCODE_KP_6
        public const int KEYBOARD_ADD          = 87;   // SDL_SCANCODE_KP_PLUS
        public const int KEYBOARD_NUMPAD1      = 89;   // SDL_SCANCODE_KP_1
        public const int KEYBOARD_NUMPAD2      = 90;   // SDL_SCANCODE_KP_2
        public const int KEYBOARD_NUMPAD3      = 91;   // SDL_SCANCODE_KP_3
        public const int KEYBOARD_NUMPAD0      = 98;   // SDL_SCANCODE_KP_0
        public const int KEYBOARD_DECIMAL      = 99;   // SDL_SCANCODE_KP_PERIOD
        public const int KEYBOARD_F11          = SDL.SDL_SCANCODE_F11;
        public const int KEYBOARD_F12          = SDL.SDL_SCANCODE_F12;
        public const int KEYBOARD_NUMPADENTER  = 88;   // SDL_SCANCODE_KP_ENTER
        public const int KEYBOARD_RCONTROL     = SDL.SDL_SCANCODE_RCTRL;
        public const int KEYBOARD_DIVIDE       = 84;   // SDL_SCANCODE_KP_DIVIDE
        public const int KEYBOARD_SYSRQ        = 70;   // SDL_SCANCODE_SYSREQ
        public const int KEYBOARD_RMENU        = SDL.SDL_SCANCODE_RALT;
        public const int KEYBOARD_PAUSE        = 72;   // SDL_SCANCODE_PAUSE
        public const int KEYBOARD_HOME         = 74;   // SDL_SCANCODE_HOME
        public const int KEYBOARD_UP           = SDL.SDL_SCANCODE_UP;
        public const int KEYBOARD_PRIOR        = 75;   // SDL_SCANCODE_PAGEUP
        public const int KEYBOARD_LEFT         = SDL.SDL_SCANCODE_LEFT;
        public const int KEYBOARD_RIGHT        = SDL.SDL_SCANCODE_RIGHT;
        public const int KEYBOARD_END          = 77;   // SDL_SCANCODE_END
        public const int KEYBOARD_DOWN         = SDL.SDL_SCANCODE_DOWN;
        public const int KEYBOARD_NEXT         = 78;   // SDL_SCANCODE_PAGEDOWN
        public const int KEYBOARD_INSERT       = 73;   // SDL_SCANCODE_INSERT
        public const int KEYBOARD_DELETE       = 76;   // SDL_SCANCODE_DELETE
        public const int KEYBOARD_LWIN         = 227;  // SDL_SCANCODE_LGUI
        public const int KEYBOARD_RWIN         = 231;  // SDL_SCANCODE_RGUI
        public const int KEYBOARD_APPS         = 101;  // SDL_SCANCODE_APPLICATION

        // Joystick direction indices
        public const int JOYSTICK_UP    = 0;
        public const int JOYSTICK_DOWN  = 1;
        public const int JOYSTICK_LEFT  = 2;
        public const int JOYSTICK_RIGHT = 3;

        // Returns the control index for joystick button x (base 4 directions + button index)
        public static int JOYSTICK_BUTTON(int x) { return 4 + x; }

        public const int NUMBER_OF_JOYSTICK_DIRECTIONS = 4;

        public const int JOYSTICK_AXIS_THRESHOLD = 3200;

        // The third joystick button (button index 1) for menu previous
        public const int JOYSTICK_BUTTON_MENU_PREVIOUS = 4 + 1; // JOYSTICK_BUTTON(1)
        // The first joystick button (button index 0) for menu next
        public const int JOYSTICK_BUTTON_MENU_NEXT     = 4 + 0; // JOYSTICK_BUTTON(0)
        // The 9th joystick button (index 8) for break/pause
        public const int JOYSTICK_BUTTON_BREAK         = 4 + 8; // JOYSTICK_BUTTON(8)
        // The 10th joystick button (index 9) for start
        public const int JOYSTICK_BUTTON_START         = 4 + 9; // JOYSTICK_BUTTON(9)

        // Dead zone constants
        public const int JOYSTICK_DEAD_ZONE    = 10;
        public const int JOYSTICK_MINIMUM_AXIS = -32768;
        public const int JOYSTICK_MAXIMUM_AXIS = +32767;
    }

    //*****************************************************************************

    /// <summary>
    /// State of an SDL joystick (mirrors SDLJOYSTATE from C++ code)
    /// </summary>
    public struct SJoystickState
    {
        public int    lX;               // x-axis position
        public int    lY;               // y-axis position
        public int    lZ;               // z-axis position
        public int    lRx;              // x-axis rotation
        public int    lRy;              // y-axis rotation
        public int    lRz;              // z-axis rotation
        public int[]  rglSlider;        // extra axes positions [2]
        public uint[] rgdwPOV;          // POV directions [4]
        public byte[] rgbButtons;       // 32 buttons

        public static SJoystickState Create()
        {
            var s = new SJoystickState();
            s.rglSlider  = new int[2];
            s.rgdwPOV    = new uint[4];
            s.rgbButtons = new byte[InputConstants.MAX_JOYSTICK_BUTTONS];
            return s;
        }

        public void Reset()
        {
            lX = 0; lY = 0; lZ = 0;
            lRx = 0; lRy = 0; lRz = 0;
            if (rglSlider  == null) rglSlider  = new int[2];
            if (rgdwPOV    == null) rgdwPOV    = new uint[4];
            if (rgbButtons == null) rgbButtons = new byte[InputConstants.MAX_JOYSTICK_BUTTONS];
            Array.Clear(rglSlider,  0, rglSlider.Length);
            Array.Clear(rgdwPOV,    0, rgdwPOV.Length);
            Array.Clear(rgbButtons, 0, rgbButtons.Length);
        }
    }

    //*****************************************************************************

    /// <summary>
    /// Contains information about one joystick device.
    /// </summary>
    public class SJoystick
    {
        public IntPtr         pDevice;  // SDL_Joystick* handle
        public SJoystickState State;    // Most recent state of the joystick
        public bool           Opened;   // Is the joystick supposed to be opened?
    }

    //*****************************************************************************

    /// <summary>
    /// Manages SDL keyboard and joystick input devices.
    /// </summary>
    public class CInputSDL
    {
        private bool                 m_Ready;
        private IntPtr               m_hInstance;
        private IntPtr               m_hWnd;

        private bool                 m_KeyboardOpened;
        private byte[]               m_KeyState;
        private string[]             m_KeyFriendlyName;

        private List<SJoystick>      m_pJoysticks;

        private int                  m_joystickCount;

        //**********************************************************************

        public CInputSDL()
        {
            m_hWnd       = IntPtr.Zero;
            m_hInstance  = IntPtr.Zero;
            m_Ready      = false;

            m_KeyboardOpened  = false;
            m_KeyState        = new byte[InputConstants.MAX_KEYS];
            m_KeyFriendlyName = new string[InputConstants.MAX_KEYS];

            m_pJoysticks  = new List<SJoystick>();
            m_joystickCount = 0;
        }

        //**********************************************************************

        public void SetWindowHandle(IntPtr hWnd)
        {
            m_hWnd = hWnd;
        }

        public void SetInstanceHandle(IntPtr hInstance)
        {
            m_hInstance = hInstance;
        }

        //**********************************************************************

        public bool Create()
        {
            if (!m_Ready)
            {
                // Reset the keyboard state
                Array.Clear(m_KeyState, 0, m_KeyState.Length);

                // Prepare the friendly name for each key
                MakeKeyFriendlyNames();

                // Create all joysticks installed on the system
                int numJoysticks = SDL.SDL_NumJoysticks();
                for (int i = 0; i < numJoysticks; i++)
                {
                    SJoystick pJoystick = new SJoystick();
                    pJoystick.State = SJoystickState.Create();
                    pJoystick.State.Reset();
                    pJoystick.Opened  = false;
                    pJoystick.pDevice = IntPtr.Zero;

                    m_pJoysticks.Add(pJoystick);

                    CLog.GetLog().WriteLine("SDLInput        => A joystick was added.");
                }

                m_Ready = true;
            }

            // Enable joystick event processing
            SDL.SDL_JoystickEventState(SDL.SDL_ENABLE);

            return true;
        }

        //**********************************************************************

        public void Destroy()
        {
            if (m_Ready)
            {
                for (int index = 0; index < m_pJoysticks.Count; index++)
                {
                    SDL.SDL_JoystickClose(m_pJoysticks[index].pDevice);
                    m_pJoysticks[index].pDevice = IntPtr.Zero;

                    CLog.GetLog().WriteLine("SDLInput        => A joystick was released.");
                }

                m_pJoysticks.Clear();

                CLog.GetLog().WriteLine("SDLInput        => SDLInput object was released.");
            }
        }

        //**********************************************************************
        // Keyboard
        //**********************************************************************

        public void OpenKeyboard()
        {
            m_KeyboardOpened = true;
        }

        public bool IsKeyboardOpened()
        {
            return m_KeyboardOpened;
        }

        public void CloseKeyboard()
        {
            m_KeyboardOpened = false;
        }

        public void UpdateKeyboard()
        {
            UpdateDeviceKeyboard(m_KeyState, InputConstants.MAX_KEYS);
        }

        private bool UpdateDeviceKeyboard(byte[] pState, int stateSize)
        {
            byte[] keyState = SDL.SDL_GetKeyboardStateArray();
            int count = Math.Min(stateSize, keyState.Length);
            for (int i = 0; i < count; i++)
                SetKey(i, keyState[i] == 1);
            return true;
        }

        public bool GetKey(int key)
        {
            System.Diagnostics.Debug.Assert(key >= 0 && key < InputConstants.MAX_KEYS);
            return (m_KeyState[key] & 0x80) != 0;
        }

        public void SetKey(int key, bool keySet)
        {
            System.Diagnostics.Debug.Assert(key >= 0 && key < InputConstants.MAX_KEYS);
            if (keySet)
                m_KeyState[key] |= 0x80;
            else
                m_KeyState[key] = (byte)(m_KeyState[key] & ~0x80);
        }

        public string GetKeyFriendlyName(int key)
        {
            System.Diagnostics.Debug.Assert(key >= 0 && key < InputConstants.MAX_KEYS);
            return m_KeyFriendlyName[key];
        }

        //**********************************************************************
        // Joystick
        //**********************************************************************

        public int GetJoystickCount()
        {
            return m_pJoysticks.Count;
        }

        public void OpenJoystick(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                if (m_pJoysticks[joystick].Opened)
                    return;

                m_pJoysticks[joystick].pDevice = SDL.SDL_JoystickOpen(joystick);
                m_pJoysticks[joystick].Opened  = (m_pJoysticks[joystick].pDevice != IntPtr.Zero);
            }
        }

        public bool IsJoystickOpened(int joystick)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            return m_pJoysticks[joystick].Opened;
        }

        public void CloseJoystick(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                SDL.SDL_JoystickClose(m_pJoysticks[joystick].pDevice);
                m_pJoysticks[joystick].Opened  = false;
                m_pJoysticks[joystick].pDevice = IntPtr.Zero;
            }
        }

        public void UpdateJoystick(int joystick)
        {
            // Joystick state is updated via SDL events; no polling needed here.
        }

        public int GetJoystickAxisX(int joystick)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            return m_pJoysticks[joystick].State.lX;
        }

        public int GetJoystickAxisY(int joystick)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            return m_pJoysticks[joystick].State.lY;
        }

        public bool GetJoystickButton(int joystick, int button)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            System.Diagnostics.Debug.Assert(button >= 0 && button < InputConstants.MAX_JOYSTICK_BUTTONS);
            return (m_pJoysticks[joystick].State.rgbButtons[button] & 0x80) != 0;
        }

        public void SetJoystickAxisX(int joystick, int axisX)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            m_pJoysticks[joystick].State.lX = axisX;
        }

        public void SetJoystickAxisY(int joystick, int axisY)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            m_pJoysticks[joystick].State.lY = axisY;
        }

        public void SetJoystickButton(int joystick, int button, bool onoff)
        {
            System.Diagnostics.Debug.Assert(joystick >= 0 && joystick < m_pJoysticks.Count);
            System.Diagnostics.Debug.Assert(button >= 0 && button < InputConstants.MAX_JOYSTICK_BUTTONS);
            if (onoff)
                m_pJoysticks[joystick].State.rgbButtons[button] |= 0x80;
            else
                m_pJoysticks[joystick].State.rgbButtons[button] = (byte)(m_pJoysticks[joystick].State.rgbButtons[button] & ~0x80);
        }

        //**********************************************************************
        // Joystick directional / button test helpers
        //**********************************************************************

        public bool TestUp(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                m_joystickCount++;
                if (m_joystickCount > 200)
                {
                    if (m_pJoysticks[joystick].State.lY < -InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    {
                        m_joystickCount = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TestDown(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                m_joystickCount++;
                if (m_joystickCount > 200)
                {
                    if (m_pJoysticks[joystick].State.lY > +InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    {
                        m_joystickCount = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TestLeft(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                m_joystickCount++;
                if (m_joystickCount > 200)
                {
                    if (m_pJoysticks[joystick].State.lX < -InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    {
                        m_joystickCount = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TestRight(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                m_joystickCount++;
                if (m_joystickCount > 200)
                {
                    if (m_pJoysticks[joystick].State.lX > +InputConstants.JOYSTICK_AXIS_THRESHOLD)
                    {
                        m_joystickCount = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TestNext(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                int btnIndex = InputConstants.JOYSTICK_BUTTON_MENU_NEXT - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS;
                if ((m_pJoysticks[joystick].State.rgbButtons[btnIndex] & 0x80) != 0)
                    return true;
            }
            return false;
        }

        public bool TestPrevious(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                int btnIndex = InputConstants.JOYSTICK_BUTTON_MENU_PREVIOUS - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS;
                if ((m_pJoysticks[joystick].State.rgbButtons[btnIndex] & 0x80) != 0)
                    return true;
            }
            return false;
        }

        public bool TestBreak(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                int btnIndex = InputConstants.JOYSTICK_BUTTON_BREAK - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS;
                if ((m_pJoysticks[joystick].State.rgbButtons[btnIndex] & 0x80) != 0)
                    return true;
            }
            return false;
        }

        public bool TestStart(int joystick)
        {
            if (joystick >= 0 && joystick < m_pJoysticks.Count)
            {
                int btnIndex = InputConstants.JOYSTICK_BUTTON_START - InputConstants.NUMBER_OF_JOYSTICK_DIRECTIONS;
                if ((m_pJoysticks[joystick].State.rgbButtons[btnIndex] & 0x80) != 0)
                    return true;
            }
            return false;
        }

        //**********************************************************************
        // Key friendly names
        //**********************************************************************

        private void MakeKeyFriendlyNames()
        {
            for (int key = 0; key < InputConstants.MAX_KEYS; key++)
                m_KeyFriendlyName[key] = string.Format("UNKNOWN KEY {0}", key);

            m_KeyFriendlyName[InputConstants.KEYBOARD_ESCAPE]      = "ESCAPE";
            m_KeyFriendlyName[InputConstants.KEYBOARD_1]           = "1";
            m_KeyFriendlyName[InputConstants.KEYBOARD_2]           = "2";
            m_KeyFriendlyName[InputConstants.KEYBOARD_3]           = "3";
            m_KeyFriendlyName[InputConstants.KEYBOARD_4]           = "4";
            m_KeyFriendlyName[InputConstants.KEYBOARD_5]           = "5";
            m_KeyFriendlyName[InputConstants.KEYBOARD_6]           = "6";
            m_KeyFriendlyName[InputConstants.KEYBOARD_7]           = "7";
            m_KeyFriendlyName[InputConstants.KEYBOARD_8]           = "8";
            m_KeyFriendlyName[InputConstants.KEYBOARD_9]           = "9";
            m_KeyFriendlyName[InputConstants.KEYBOARD_0]           = "0";
            m_KeyFriendlyName[InputConstants.KEYBOARD_MINUS]       = "MINUS";
            m_KeyFriendlyName[InputConstants.KEYBOARD_EQUALS]      = "EQUALS";
            m_KeyFriendlyName[InputConstants.KEYBOARD_BACK]        = "BACKSPACE";
            m_KeyFriendlyName[InputConstants.KEYBOARD_TAB]         = "TAB";
            m_KeyFriendlyName[InputConstants.KEYBOARD_Q]           = "Q";
            m_KeyFriendlyName[InputConstants.KEYBOARD_W]           = "W";
            m_KeyFriendlyName[InputConstants.KEYBOARD_E]           = "E";
            m_KeyFriendlyName[InputConstants.KEYBOARD_R]           = "R";
            m_KeyFriendlyName[InputConstants.KEYBOARD_T]           = "T";
            m_KeyFriendlyName[InputConstants.KEYBOARD_Y]           = "Y";
            m_KeyFriendlyName[InputConstants.KEYBOARD_U]           = "U";
            m_KeyFriendlyName[InputConstants.KEYBOARD_I]           = "I";
            m_KeyFriendlyName[InputConstants.KEYBOARD_O]           = "O";
            m_KeyFriendlyName[InputConstants.KEYBOARD_P]           = "P";
            m_KeyFriendlyName[InputConstants.KEYBOARD_LBRACKET]    = "L-BRACKET";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RBRACKET]    = "R-BRACKET";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RETURN]      = "RETURN";
            m_KeyFriendlyName[InputConstants.KEYBOARD_LCONTROL]    = "L.CTRL";
            m_KeyFriendlyName[InputConstants.KEYBOARD_A]           = "A";
            m_KeyFriendlyName[InputConstants.KEYBOARD_S]           = "S";
            m_KeyFriendlyName[InputConstants.KEYBOARD_D]           = "D";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F]           = "F";
            m_KeyFriendlyName[InputConstants.KEYBOARD_G]           = "G";
            m_KeyFriendlyName[InputConstants.KEYBOARD_H]           = "H";
            m_KeyFriendlyName[InputConstants.KEYBOARD_J]           = "J";
            m_KeyFriendlyName[InputConstants.KEYBOARD_K]           = "K";
            m_KeyFriendlyName[InputConstants.KEYBOARD_L]           = "L";
            m_KeyFriendlyName[InputConstants.KEYBOARD_SEMICOLON]   = "SEMICOLON";
            m_KeyFriendlyName[InputConstants.KEYBOARD_APOSTROPHE]  = "APOSTR.";
            m_KeyFriendlyName[InputConstants.KEYBOARD_GRAVE]       = "E-GRAVE";
            m_KeyFriendlyName[InputConstants.KEYBOARD_LSHIFT]      = "L-SHIFT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_BACKSLASH]   = "BACKSLASH";
            m_KeyFriendlyName[InputConstants.KEYBOARD_Z]           = "Z";
            m_KeyFriendlyName[InputConstants.KEYBOARD_X]           = "X";
            m_KeyFriendlyName[InputConstants.KEYBOARD_C]           = "C";
            m_KeyFriendlyName[InputConstants.KEYBOARD_V]           = "V";
            m_KeyFriendlyName[InputConstants.KEYBOARD_B]           = "B";
            m_KeyFriendlyName[InputConstants.KEYBOARD_N]           = "N";
            m_KeyFriendlyName[InputConstants.KEYBOARD_M]           = "M";
            m_KeyFriendlyName[InputConstants.KEYBOARD_COMMA]       = "COMMA";
            m_KeyFriendlyName[InputConstants.KEYBOARD_PERIOD]      = "PERIOD";
            m_KeyFriendlyName[InputConstants.KEYBOARD_SLASH]       = "SLASH";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RSHIFT]      = "R-SHIFT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_MULTIPLY]    = "NUM-MUL";
            m_KeyFriendlyName[InputConstants.KEYBOARD_LMENU]       = "L-ALT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_SPACE]       = "SPACE";
            m_KeyFriendlyName[InputConstants.KEYBOARD_CAPITAL]     = "CAPITAL";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F1]          = "F1";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F2]          = "F2";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F3]          = "F3";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F4]          = "F4";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F5]          = "F5";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F6]          = "F6";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F7]          = "F7";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F8]          = "F8";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F9]          = "F9";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F10]         = "F10";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMLOCK]     = "NUMLOCK";
            m_KeyFriendlyName[InputConstants.KEYBOARD_SCROLL]      = "SCROLL";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD7]     = "NUM-7";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD8]     = "NUM-8";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD9]     = "NUM-9";
            m_KeyFriendlyName[InputConstants.KEYBOARD_SUBTRACT]    = "NUM-MINUS";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD4]     = "NUM-4";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD5]     = "NUM-5";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD6]     = "NUM-6";
            m_KeyFriendlyName[InputConstants.KEYBOARD_ADD]         = "NUM-ADD";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD1]     = "NUM-1";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD2]     = "NUM-2";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD3]     = "NUM-3";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPAD0]     = "NUM-0";
            m_KeyFriendlyName[InputConstants.KEYBOARD_DECIMAL]     = "NUM-DOT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F11]         = "F11";
            m_KeyFriendlyName[InputConstants.KEYBOARD_F12]         = "F12";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NUMPADENTER] = "NUM-ENTER";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RCONTROL]    = "R-CTRL";
            m_KeyFriendlyName[InputConstants.KEYBOARD_DIVIDE]      = "NUM-SLASH";
            m_KeyFriendlyName[InputConstants.KEYBOARD_SYSRQ]       = "SYSTEM";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RMENU]       = "R-ALT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_PAUSE]       = "PAUSE";
            m_KeyFriendlyName[InputConstants.KEYBOARD_HOME]        = "HOME";
            m_KeyFriendlyName[InputConstants.KEYBOARD_UP]          = "UP";
            m_KeyFriendlyName[InputConstants.KEYBOARD_PRIOR]       = "PAGEUP";
            m_KeyFriendlyName[InputConstants.KEYBOARD_LEFT]        = "LEFT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RIGHT]       = "RIGHT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_END]         = "END";
            m_KeyFriendlyName[InputConstants.KEYBOARD_DOWN]        = "DOWN";
            m_KeyFriendlyName[InputConstants.KEYBOARD_NEXT]        = "PAGEDOWN";
            m_KeyFriendlyName[InputConstants.KEYBOARD_INSERT]      = "INSERT";
            m_KeyFriendlyName[InputConstants.KEYBOARD_DELETE]      = "DELETE";
            m_KeyFriendlyName[InputConstants.KEYBOARD_LWIN]        = "L-WIN";
            m_KeyFriendlyName[InputConstants.KEYBOARD_RWIN]        = "R-WIN";
            m_KeyFriendlyName[InputConstants.KEYBOARD_APPS]        = "APP-MENU";
        }
    }

    //*****************************************************************************
    // Type alias: InputClass = CInputSDL (mirrors C++ typedef)
    //*****************************************************************************
    // Use CInputSDL directly; the alias below is provided for clarity.
    // In C++ code: typedef CInputSDL InputClass;
}
