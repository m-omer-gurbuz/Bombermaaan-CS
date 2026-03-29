// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

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
 *  \file CDebug.cs
 *  \brief Utilities for debugging
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Platform-agnostic virtual key codes used by CDebug.HandleKey.
    /// These names mirror the Windows VK_* / SDL SDLK_* constants so that
    /// call-sites compile without change.
    /// </summary>
    public enum DebugVirtualKey : uint
    {
        VK_MULTIPLY  = 0x6A,   // Numpad *
        VK_DIVIDE    = 0x6F,   // Numpad /
        VK_RETURN    = 0x0D,   // Enter (also numpad enter on Windows)
        VK_ADD       = 0x6B,   // Numpad +
        VK_SUBTRACT  = 0x6D,   // Numpad -
        VK_F1        = 0x70,
        VK_F2        = 0x71,
        VK_F5        = 0x74,
        VK_NUMPAD0   = 0x60,
        VK_NUMPAD1   = 0x61,
        VK_NUMPAD2   = 0x62,
        VK_NUMPAD3   = 0x63,
        VK_NUMPAD4   = 0x64
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>This class is for debugging purposes</summary>
    public class CDebug
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private CTimer  m_pTimer;
        private CGame   m_pGame;
        private CMatch  m_pMatch;
        private float   m_GameSpeed;
        private bool    m_CanBombersDie;
        private bool    m_CanBombersBeSick;
        private bool    m_CanBombersKick;
        private bool[]  m_IsComputerConsoleActive = new bool[5];

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CDebug()
        {
            m_pTimer = null;
            m_pGame  = null;

            m_pMatch            = null;
            m_GameSpeed         = 0.0f;
            m_CanBombersDie     = true;
            m_CanBombersBeSick  = true;
            m_CanBombersKick    = true;

            for (int i = 0; i < 5; i++)
                m_IsComputerConsoleActive[i] = false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        ~CDebug()
        {
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Singleton instance
        private static readonly CDebug s_Debug = new CDebug();

        public static CDebug GetInstance()
        {
            return s_Debug;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetTimer(CTimer pTimer)
        {
            m_pTimer = pTimer;
        }

        public void SetGame(CGame pGame)
        {
            m_pGame = pGame;
        }

        public void SetMatch(CMatch pMatch)
        {
            m_pMatch = pMatch;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Create()
        {
            Debug.Assert(m_pTimer != null);
            Debug.Assert(m_pGame  != null);
            Debug.Assert(m_pMatch != null);

            // Set initial game debug variables
            m_GameSpeed        = 1.0f;
            m_CanBombersDie    = true;
            m_CanBombersBeSick = true;
            m_CanBombersKick   = true;

            for (int i = 0; i < 5; i++)
                m_IsComputerConsoleActive[i] = false;

            // Actually set the speed of the game
            m_pTimer.SetSpeed(m_GameSpeed);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Destroy()
        {
            // Reset the game speed to normal
            m_pTimer.SetSpeed(1.0f);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Handles a key code for debug purposes.
        ///
        /// While holding the CTRL key:
        ///
        /// CONTROL GAME SPEED
        /// The Enter key on the numeric pad sets the game speed to normal
        /// The * on the numeric pad makes the game very fast
        /// The / on the numeric pad makes the game very slow
        /// The + on the numeric pad increases the game speed
        /// The - on the numeric pad decreases the game speed
        ///
        /// START OR RESTART A MATCH
        /// Press the F1 key
        /// You may run out of bombs if you do this too often. Example: one bomber is holding a bomb
        /// while restarting the match by pressing Ctrl+F1. This is because m_BombsInUse of CArena is
        /// not reduced.
        /// I guess it can't happen in a 'normal' game because the bomber throws his bomb when he dies or
        /// he is the winner of the match. At least I couldn't recreate this issue in a 'normal' game.
        ///
        /// TOGGLE THE BOMBERS' INVULNERABILITY
        /// Press the F2 key
        ///
        /// TOGGLE THE DEBUGGING INFORMATION OF EACH COMPUTER PLAYER
        /// Press the number of the player (0-4)
        /// </summary>
        /// <param name="VirtualKeyCode">Key to handle (use values from <see cref="DebugVirtualKey"/> or equivalent uint)</param>
        /// <param name="Modifier">Modifier flags; bit 0x40 (KMOD_CTRL equivalent) means CTRL is held</param>
        public void HandleKey(uint VirtualKeyCode, uint Modifier)
        {
            // Check if CTRL is held.
            // Modifier bit 0x40 corresponds to KMOD_CTRL (SDL) / equivalent platform flag.
            if ((Modifier & 0x40) != 0)
            {
                switch (VirtualKeyCode)
                {
                    case (uint)DebugVirtualKey.VK_MULTIPLY:
                        {
                            m_GameSpeed = 5.0f;

                            // Set the new game speed
                            m_pTimer.SetSpeed(m_GameSpeed);

                            break;
                        }

                    case (uint)DebugVirtualKey.VK_DIVIDE:
                        {
                            m_GameSpeed = 0.2f;

                            // Set the timer speed
                            m_pTimer.SetSpeed(m_GameSpeed);

                            break;
                        }

                    case (uint)DebugVirtualKey.VK_RETURN:
                        {
                            m_GameSpeed = 1.0f;

                            // Set the timer speed
                            m_pTimer.SetSpeed(m_GameSpeed);

                            break;
                        }

                    case (uint)DebugVirtualKey.VK_ADD:
                        {
                            m_GameSpeed += 0.2f;

                            if (m_GameSpeed > 5.0f)
                            {
                                m_GameSpeed = 5.0f;
                            }

                            // Set the timer speed
                            m_pTimer.SetSpeed(m_GameSpeed);

                            break;
                        }

                    case (uint)DebugVirtualKey.VK_SUBTRACT:
                        {
                            m_GameSpeed -= 0.2f;

                            if (m_GameSpeed < 0.0f)
                            {
                                m_GameSpeed = 0.0f;
                            }

                            // Set the timer speed
                            m_pTimer.SetSpeed(m_GameSpeed);

                            break;
                        }

                    case (uint)DebugVirtualKey.VK_F1:
                        {
                            m_pGame.SwitchToGameMode(EGameMode.GAMEMODE_MATCH);

                            break;
                        }

                    case (uint)DebugVirtualKey.VK_F2:
                        {
                            // Make the bombers invulnerable or not
                            m_CanBombersDie = !m_CanBombersDie;

                            break;
                        }

#if DEBUG_FLAG_1
                    case (uint)DebugVirtualKey.VK_F5:
                        CConsole.GetConsole().Write("CDebug::HandleKey(...): Ctrl+F5 was pressed. Writing bombs to log...\n");
                        m_pMatch._Debug_WriteBombsToLog();
                        break;
#endif

                    case (uint)DebugVirtualKey.VK_NUMPAD0: m_IsComputerConsoleActive[0] = !m_IsComputerConsoleActive[0]; break;
                    case (uint)DebugVirtualKey.VK_NUMPAD1: m_IsComputerConsoleActive[1] = !m_IsComputerConsoleActive[1]; break;
                    case (uint)DebugVirtualKey.VK_NUMPAD2: m_IsComputerConsoleActive[2] = !m_IsComputerConsoleActive[2]; break;
                    case (uint)DebugVirtualKey.VK_NUMPAD3: m_IsComputerConsoleActive[3] = !m_IsComputerConsoleActive[3]; break;
                    case (uint)DebugVirtualKey.VK_NUMPAD4: m_IsComputerConsoleActive[4] = !m_IsComputerConsoleActive[4]; break;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public bool CanBombersDie()
        {
            return m_CanBombersDie;
        }

        public bool CanBombersBeSick()
        {
            return m_CanBombersBeSick;
        }

        public bool CanBombersKick()
        {
            return m_CanBombersKick;
        }

        public bool IsComputerConsoleActive(int Player)
        {
            Debug.Assert(Player >= 0);
            Debug.Assert(Player < 5);

            return m_IsComputerConsoleActive[Player];
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
