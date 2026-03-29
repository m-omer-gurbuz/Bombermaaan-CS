/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
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
 *  \file CInput.cs
 *  \brief Input devices
 */

using System;
using System.Diagnostics;

namespace Bombermaaan
{
    /// <summary>
    /// Number of keyboard configurations available for player input assignment.
    /// </summary>
    public static class InputConfig
    {
        public const int NUMBER_OF_KEYBOARD_CONFIGURATIONS = 5;
    }

    //*****************************************************************************

    /// <summary>
    /// Manages all input for the game: keyboard, joysticks, main menu input,
    /// and per-player input objects.
    /// </summary>
    public class CInput
    {
        public const int NO_ACTIVATED_CONTROL = -1;

        private COptions       m_pOptions;
        private CTimer         m_pTimer;
        private CMainInput     m_MainInput;
        private CPlayerInput[] m_PlayerInput;

        // The underlying SDL input object (InputClass in C++)
        private CInputSDL      m_input;

        //**********************************************************************

        public CInput()
        {
            m_pOptions    = null;
            m_pTimer      = null;
            m_PlayerInput = null;

            m_input       = new CInputSDL();
            m_MainInput   = new CMainInput();

            // Set the input object used by the main input
            m_MainInput.SetInput(m_input);
        }

        //**********************************************************************

        public void SetWindowHandle(IntPtr hWnd)
        {
            m_input.SetWindowHandle(hWnd);
        }

        public void SetInstanceHandle(IntPtr hInstance)
        {
            m_input.SetInstanceHandle(hInstance);
        }

        public void SetOptions(COptions pOptions)
        {
            m_pOptions = pOptions;
        }

        public void SetTimer(CTimer pTimer)
        {
            m_pTimer = pTimer;
            m_MainInput.SetTimer(pTimer);
        }

        //**********************************************************************

        public bool Create()
        {
            Debug.Assert(m_pOptions != null);
            Debug.Assert(m_pTimer   != null);

            // Initialize the SDL input interface
            if (!m_input.Create())
                return false;

            // Initialize the main input
            m_MainInput.Create();

            // Allocate a player input object for every available input
            int count = GetPlayerInputCount();
            m_PlayerInput = new CPlayerInput[count];

            for (int i = 0; i < count; i++)
            {
                m_PlayerInput[i] = new CPlayerInput();
                m_PlayerInput[i].SetDirectInput(m_input);
                m_PlayerInput[i].SetOptions(m_pOptions);
                m_PlayerInput[i].Create(i);
            }

            return true;
        }

        //**********************************************************************

        public void Destroy()
        {
            int count = GetPlayerInputCount();
            for (int i = 0; i < count; i++)
                m_PlayerInput[i].Destroy();

            m_PlayerInput = null;

            m_MainInput.Destroy();

            m_input.Destroy();
        }

        //**********************************************************************

        public CMainInput GetMainInput()
        {
            return m_MainInput;
        }

        /// <summary>
        /// Returns the underlying SDL input object (equivalent to GetDirectInput in C++).
        /// </summary>
        public CInputSDL GetDirectInput()
        {
            return m_input;
        }

        public CPlayerInput GetPlayerInput(int playerInput)
        {
            Debug.Assert(playerInput >= 0);
            Debug.Assert(playerInput < GetPlayerInputCount());
            return m_PlayerInput[playerInput];
        }

        public int GetPlayerInputCount()
        {
            return InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS + m_input.GetJoystickCount();
        }
    }
}
