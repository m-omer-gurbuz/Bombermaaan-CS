/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007-2010 Bernd Arnold
    Copyright (C) 2010 Markus Drescher
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
 *  \file CMatch.cs
 *  \brief The match (bombers are playing)
 */

using System;
using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>The match screen, managing the arena and the board.</summary>
    public class CMatch : CModeScreen
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private const int   NO_WINNER_TEAM    = -1;     //!< Value for a winner team number if there is no winner

        private const float BLACKSCREEN_DURATION = 0.750f; //!< Duration (in seconds) of each of the two black screens
        private const float PAUSE_BEGIN          = 1.0f;   //!< Duration (in seconds) of the pause at the beginning of a match
        private const float PAUSE_DRAWGAME       = 2.5f;   //!< Duration (in seconds) of the pause at match end when there is a draw game
        private const float PAUSE_WINNER         = 2.5f;   //!< Duration (in seconds) of the pause at match end when there is a winner

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private CBoard          m_Board;                    //!< Board object
        private CClock          m_Clock;                    //!< Clock object
        private CArena          m_Arena;                    //!< Arena object
        private CTeam[]         m_Teams;                    //!< Teams object
        private CAiManager      m_AiManager;                //!< Computer brain
        private bool            m_NoComputer;               //!< True if no computer is playing in this match
        private bool            m_MatchOver;                //!< Is match over?
        private int             m_WinnerTeam;               //!< Number of the team that won if there is a winner
        private ESong           m_CurrentSong;              //!< Current song being played
        private bool            m_IsSongPlaying;            //!< Is the match song playing?
        private bool            m_NoticedTimeUp;            //!< Did we notice that time is up and do what is necessary?
        private CPauseMessage   m_pPauseMessage;            //!< Pause message object, instantiated when the match is paused
        private CHurryMessage   m_pHurryMessage;            //!< Hurry up message object, instantiated when the arena starts to close
        private float           m_ModeTime;                 //!< Time (in seconds) that elapsed since the mode has started
        private float           m_ExitModeTime;             //!< Mode time when we have to start the last black screen
        private bool            m_HaveToExit;               //!< Do we have to exit this mode?
        private bool            m_computerPlayersPresent;   //!< True, when there are AI players
        private bool            m_ForceDrawGame;            //!< Force a draw game when only AI bombers are alive?
        private CNetwork        m_pNetwork;                           //!< Network pointer
        private CCommandChunk   m_CommandChunk;                       //!< Network command chunk
        private CArenaSnapshot  m_Snapshot;                           //!< Network arena snapshot
        private float           m_TimeElapsedSinceLastCommandChunk;   //!< Elapsed time since last network chunk send

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Constructor. Initialize some members.</summary>
        public CMatch() : base()
        {
            m_Board      = new CBoard();
            m_Clock      = new CClock();
            m_Arena      = new CArena();
            m_AiManager  = new CAiManager();
            m_Teams      = new CTeam[Globals.MAX_TEAMS];
            for (int i = 0; i < Globals.MAX_TEAMS; i++)
                m_Teams[i] = new CTeam();

            // Set the objects the board has to communicate with
            m_Board.SetClock(m_Clock);
            m_Board.SetArena(m_Arena);

            m_AiManager.SetArena(m_Arena);

            m_pPauseMessage = null;
            m_pHurryMessage = null;

            m_MatchOver   = false;
            m_WinnerTeam  = NO_WINNER_TEAM;

            m_IsSongPlaying        = false;
            m_NoticedTimeUp        = false;
            m_ModeTime             = 0.0f;
            m_HaveToExit           = false;
            m_ForceDrawGame        = false;

            m_CurrentSong          = ESong.SONG_NONE;
            m_ExitModeTime         = 0.0f;

            m_NoComputer               = false;
            m_computerPlayersPresent   = false;

            m_pNetwork = null;
            m_CommandChunk = new CCommandChunk();
            m_CommandChunk.Create();
            m_Snapshot = new CArenaSnapshot();
            m_Snapshot.Create();
            m_TimeElapsedSinceLastCommandChunk = 0.0f;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set link to the display object to use</summary>
        public override void SetDisplay(CDisplay pDisplay)
        {
            base.SetDisplay(pDisplay);
            m_Board.SetDisplay(pDisplay);
            m_Arena.SetDisplay(pDisplay);
        }

        /// <summary>Set link to the options object to use</summary>
        public override void SetOptions(COptions pOptions)
        {
            base.SetOptions(pOptions);
            m_Board.SetOptions(pOptions);
            m_Arena.SetOptions(pOptions);
        }

        /// <summary>Set link to the scores object to use</summary>
        public void SetScores(CScores pScores)
        {
            m_Board.SetScores(pScores);
        }

        /// <summary>Set link to the timer object to use</summary>
        public override void SetTimer(CTimer pTimer)
        {
            base.SetTimer(pTimer);
            m_Board.SetTimer(pTimer);
        }

        /// <summary>Set link to the sound object to use</summary>
        public override void SetSound(CSound pSound)
        {
            base.SetSound(pSound);
            m_Arena.SetSound(pSound);
        }

        /// <summary>Set link to the network object to use</summary>
        public void SetNetwork(CNetwork pNetwork)
        {
            m_pNetwork = pNetwork;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Get the number of the team that won this match</summary>
        public int GetWinnerTeam()
        {
            return m_WinnerTeam;
        }

        /// <summary>Get whether the given player has won this match</summary>
        public bool IsPlayerWinner(int Player)
        {
            if (m_Arena.GetBomber(Player).GetTeam().GetTeamId() == m_WinnerTeam)
                return true;
            else
                return false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Initialize the object</summary>
        public override void Create()
        {
            base.Create();

            // No match result for the moment
            m_MatchOver  = false;
            m_WinnerTeam = NO_WINNER_TEAM;

            m_IsSongPlaying = false;
            m_NoticedTimeUp = false;

            // Reset mode time (no time has been elapsed in this mode yet)
            m_ModeTime = 0.0f;

            // Don't have to exit this mode yet
            m_HaveToExit = false;

            // Don't force a draw game at the beginning of the match
            m_ForceDrawGame = false;

            m_TimeElapsedSinceLastCommandChunk = 0.0f;
            m_CommandChunk.Reset();

            if (m_pNetwork != null && m_pNetwork.NetworkMode() != ENetworkMode.NETWORKMODE_LOCAL)
            {
                m_pOptions.SetTimeStart(2, 35);
                m_pOptions.SetTimeUp(0, 30);

                m_pOptions.SetBomberType(2, EBomberType.BOMBERTYPE_OFF);
                m_pOptions.SetBomberType(3, EBomberType.BOMBERTYPE_OFF);
                m_pOptions.SetBomberType(4, EBomberType.BOMBERTYPE_OFF);
                m_pOptions.SetBattleCount(3);

                if (m_pNetwork.NetworkMode() == ENetworkMode.NETWORKMODE_SERVER)
                {
                    m_pOptions.SetBomberType(0, EBomberType.BOMBERTYPE_MAN);
                    m_pOptions.SetBomberType(1, EBomberType.BOMBERTYPE_NET);

                    uint TickCount = (uint)Environment.TickCount;
                    byte[] tickBytes = BitConverter.GetBytes(TickCount);
                    m_pNetwork.Send(ESocketType.SOCKET_CLIENT, tickBytes, tickBytes.Length);
                }
                else if (m_pNetwork.NetworkMode() == ENetworkMode.NETWORKMODE_CLIENT)
                {
                    m_pOptions.SetBomberType(0, EBomberType.BOMBERTYPE_NET);
                    m_pOptions.SetBomberType(1, EBomberType.BOMBERTYPE_MAN);

                    byte[] tickBytes = new byte[4];
                    m_pNetwork.Receive(ESocketType.SOCKET_SERVER, tickBytes, 4);
                }
            }

            CreateMainComponents();

            // Set m_computerPlayersPresent to true when there are AI players in this match
            m_computerPlayersPresent = false;
            for (int i = 0; i < Globals.MAX_BOMBERS; i++)
            {
                if (m_pOptions.GetBomberType(i) == EBomberType.BOMBERTYPE_COM)
                {
                    m_computerPlayersPresent = true;
                }
            }

            if (m_computerPlayersPresent)
            {
                m_AiManager.SetDisplay(m_pDisplay);
                m_AiManager.Create(m_pOptions);
            }

            for (int i = 0; i < Globals.MAX_TEAMS; i++)
            {
                m_Teams[i].SetTeamId(i);
                m_Teams[i].SetVictorious(false);
            }

            if (m_pNetwork != null && m_pNetwork.NetworkMode() != ENetworkMode.NETWORKMODE_LOCAL)
            {
                // Network mode: each bomber is its own team
                for (int i = 0; i < Globals.MAX_BOMBERS; i++)
                {
                    m_Arena.GetBomber(i).SetTeam(m_Teams[i]);
                }
            }
            else if (m_pOptions.GetBattleMode() == EBattleMode.BATTLEMODE_TEAM)
            {
                // Set in selected team
                for (int i = 0; i < Globals.MAX_BOMBERS; i++)
                {
                    if (m_pOptions.GetBomberTeam(i) == EBomberTeam.BOMBERTEAM_A)
                        m_Arena.GetBomber(i).SetTeam(m_Teams[0]);
                    else if (m_pOptions.GetBomberTeam(i) == EBomberTeam.BOMBERTEAM_B)
                        m_Arena.GetBomber(i).SetTeam(m_Teams[1]);
                }
            }
            else
            {
                // Each bomber is its own team
                for (int i = 0; i < Globals.MAX_BOMBERS; i++)
                {
                    m_Arena.GetBomber(i).SetTeam(m_Teams[i]);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void CreateMainComponents()
        {
            // Create the main components of the match
            m_Board.Create();
            m_Arena.Create();
            m_Clock.Create(EClockType.CLOCKTYPE_COUNTDOWN,  // Time decreases until zero
                           EClockMode.CLOCKMODE_MS,          // Compute minutes and seconds
                           0,                                // Start hours
                           m_pOptions.GetTimeStartMinutes(), // Start minutes
                           m_pOptions.GetTimeStartSeconds(), // Start seconds
                           0);                               // Start seconds100
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Uninitialize the object</summary>
        public override void Destroy()
        {
            base.Destroy();

            if (m_computerPlayersPresent)
            {
                m_AiManager.Destroy();
            }

            DestroyHurryUpMessage();
            DestroyPauseMessage();
            DestroyMainComponents();
            StopSong();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void DestroyPauseMessage()
        {
            // Delete the pause message object
            if (m_pPauseMessage != null)
            {
                m_pPauseMessage = null;
            }
        }

        private void DestroyHurryUpMessage()
        {
            // Delete the hurry message object
            if (m_pHurryMessage != null)
            {
                m_pHurryMessage = null;
            }
        }

        private void DestroyMainComponents()
        {
            // Destroy the main components of the match
            m_Board.Destroy();
            m_Clock.Destroy();
            m_Arena.Destroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void StopSong()
        {
            // If the song is being played
            if (m_IsSongPlaying)
            {
                // Stop playing the match song
                m_pSound.StopSong(m_CurrentSong);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Get access to the input this object needs</summary>
        public override void OpenInput()
        {
            // Scan the players
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                // If this player plays and is human
                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN
                    && m_Arena.GetBomber(Player).Exist())
                {
                    // Open its player input given current options
                    m_pInput.GetPlayerInput(m_pOptions.GetPlayerInput(Player)).Open();
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Release access to the input this object needs</summary>
        public override void CloseInput()
        {
            // Scan the players
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                // If this player plays and is human
                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN
                    && m_Arena.GetBomber(Player).Exist())
                {
                    // Close its player input given current options
                    m_pInput.GetPlayerInput(m_pOptions.GetPlayerInput(Player)).Close();
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Start playing the match song if it hasn't started.</summary>
        private void PlaySong()
        {
            // If there is no song playing right now
            if (!m_IsSongPlaying)
            {
                // Start playing the match song
                m_pSound.PlaySong(ESong.SONG_MATCH_MUSIC);

                // Save current song number
                m_CurrentSong = ESong.SONG_MATCH_MUSIC;

                // A song is currently playing
                m_IsSongPlaying = true;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Receive player commands.</summary>
        private void ProcessPlayerCommands()
        {
            // If the match is not paused
            if (m_pPauseMessage == null)
            {
                // Do the AI stuff only when there are AI players
                if (m_computerPlayersPresent)
                {
                    m_AiManager.Update(m_pTimer.GetDeltaTime());
                }

                // Scan the players
                for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                {
                    // If this player plays and is a human
                    if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN &&
                        m_Arena.GetBomber(Player).Exist())
                    {
                        // If its bomber is still alive
                        if (m_Arena.GetBomber(Player).IsAlive())
                        {
                            EBomberMove   BomberMove   = EBomberMove.BOMBERMOVE_NONE;
                            EBomberAction BomberAction = EBomberAction.BOMBERACTION_NONE;

                            // Get his player input using the options object
                            int PlayerInput = m_pOptions.GetPlayerInput(Player);

                            // If this player input is opened
                            if (m_pInput.GetPlayerInput(PlayerInput).IsOpened())
                            {
                                // Update his player input
                                m_pInput.GetPlayerInput(PlayerInput).Update();

                                // Save the player's controls state
                                bool Up      = m_pInput.GetPlayerInput(PlayerInput).TestUp();
                                bool Down    = m_pInput.GetPlayerInput(PlayerInput).TestDown();
                                bool Left    = m_pInput.GetPlayerInput(PlayerInput).TestLeft();
                                bool Right   = m_pInput.GetPlayerInput(PlayerInput).TestRight();
                                bool Action1 = m_pInput.GetPlayerInput(PlayerInput).TestAction1();
                                bool Action2 = m_pInput.GetPlayerInput(PlayerInput).TestAction2();

                                // Determine the bomber move/action according to which controls are activated
                                if      (Up   && Left)  BomberMove = EBomberMove.BOMBERMOVE_UPLEFT;
                                else if (Up   && Right) BomberMove = EBomberMove.BOMBERMOVE_UPRIGHT;
                                else if (Down && Left)  BomberMove = EBomberMove.BOMBERMOVE_DOWNLEFT;
                                else if (Down && Right) BomberMove = EBomberMove.BOMBERMOVE_DOWNRIGHT;
                                else if (Up)            BomberMove = EBomberMove.BOMBERMOVE_UP;
                                else if (Down)          BomberMove = EBomberMove.BOMBERMOVE_DOWN;
                                else if (Left)          BomberMove = EBomberMove.BOMBERMOVE_LEFT;
                                else if (Right)         BomberMove = EBomberMove.BOMBERMOVE_RIGHT;
                                else                    BomberMove = EBomberMove.BOMBERMOVE_NONE;

                                if      (Action1) BomberAction = EBomberAction.BOMBERACTION_ACTION1;
                                else if (Action2) BomberAction = EBomberAction.BOMBERACTION_ACTION2;
                                else              BomberAction = EBomberAction.BOMBERACTION_NONE;

                                // Send these bomber move and bomber action to the bomber
                                m_Arena.GetBomber(Player).Command(BomberMove, BomberAction);
                            }
                            // If the player input is not opened
                            else
                            {
                                // Try to open the current player's player input
                                m_pInput.GetPlayerInput(PlayerInput).Open();

                                // If it's still not opened...
                                if (m_pInput.GetPlayerInput(PlayerInput).IsOpened())
                                {
                                }
                            }

                            if (m_pNetwork != null && m_pNetwork.NetworkMode() == ENetworkMode.NETWORKMODE_CLIENT)
                            {
                                m_CommandChunk.Store(BomberMove, BomberAction, m_pTimer.GetDeltaTime());
                            }
                        }
                    }
                }

                m_TimeElapsedSinceLastCommandChunk += m_pTimer.GetDeltaTime();
                if (m_pNetwork != null && m_TimeElapsedSinceLastCommandChunk >= 0.050f)
                {
                    if (m_pNetwork.NetworkMode() == ENetworkMode.NETWORKMODE_SERVER)
                    {
                        if (m_pNetwork.ReceiveCommandChunk(m_CommandChunk))
                        {
                            // Scan all the players
                            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                            {
                                // If this is the client's bomber
                                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_NET)
                                {
                                    // If the client's bomber is alive
                                    if (m_Arena.GetBomber(Player).IsAlive())
                                    {
                                        // Apply the command chunk to the bomber
                                        for (int Step = 0; Step < m_CommandChunk.GetNumberOfSteps(); Step++)
                                        {
                                            m_Arena.GetBomber(Player).Command(m_CommandChunk.GetStepMove(Step), m_CommandChunk.GetStepAction(Step));
                                            m_Arena.UpdateSingleBomber(Player, m_CommandChunk.GetStepDuration(Step));
                                        }

                                        break;
                                    }
                                }
                            }

                            // Make a snapshot of the arena and send it to the client
                            m_Arena.WriteSnapshot(m_Snapshot);

                            // Send snapshot to the client
                            m_pNetwork.SendSnapshot(m_Snapshot);
                        }
                    }
                    else if (m_pNetwork.NetworkMode() == ENetworkMode.NETWORKMODE_CLIENT)
                    {
                        // Send client command chunk to the server
                        m_pNetwork.SendCommandChunk(m_CommandChunk);

                        // Command chunk was sent, reset it.
                        m_CommandChunk.Reset();

                        // If successful apply it
                        if (m_pNetwork.ReceiveSnapshot(m_Snapshot))
                            m_Arena.ReadSnapshot(m_Snapshot);
                    }

                    m_TimeElapsedSinceLastCommandChunk = 0.0f;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Manage the pause</summary>
        private void ManagePauseMessage()
        {
            // Check if a joystick pressed the "start" button
            bool joystickRequestedPause = false;

            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                // If this player plays and is a human
                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN &&
                    m_Arena.GetBomber(Player).Exist())
                {
                    // If its bomber is still alive
                    if (m_Arena.GetBomber(Player).IsAlive())
                    {
                        // Get his player input using the options object
                        int PlayerInputNr = m_pOptions.GetPlayerInput(Player);

                        // If this player input is opened
                        if (m_pInput.GetPlayerInput(PlayerInputNr).IsOpened())
                        {
                            if (m_pInput.GetPlayerInput(PlayerInputNr).TestMenuControl(EJoystickButton.JOYSTICK_BUTTON_BREAK))
                            {
                                joystickRequestedPause = true;
                            }
                        }
                    }
                }
            }

            // If the pause control is active
            if (m_pInput.GetMainInput().TestPause() || joystickRequestedPause)
            {
                // If the pause message is not created
                if (m_pPauseMessage == null)
                {
                    // Create the pause message
                    m_pPauseMessage = new CPauseMessage(m_pDisplay, m_pSound);
                }
                // If the pause message is created but is waiting for pause toggle
                else if (m_pPauseMessage.IsWaiting())
                {
                    // Tell the pause message the pause is over
                    m_pPauseMessage.GetOut();
                }
            }

            // If the pause message is created
            if (m_pPauseMessage != null)
            {
                // Update the pause message
                m_pPauseMessage.Update(m_pTimer.GetDeltaTime());

                // Update joysticks
                for (int i = 0; i < Globals.MAX_PLAYERS; i++)
                {
                    m_pInput.GetPlayerInput(m_pOptions.GetPlayerInput(i)).Update();
                }

                // If the pause message has left the screen
                if (m_pPauseMessage.IsOutOfBounds())
                {
                    // Delete the pause message object
                    m_pPauseMessage = null;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void UpdateMatch()
        {
            // If the match is not paused
            if (m_pPauseMessage == null)
            {
                int AliveCount_Human = 0;   // Number of alive human bombers
                int AliveCount_AI    = 0;   // Number of alive computer controlled bombers

                // Count human and AI bombers
                for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                {
                    // If this bomber exists
                    if (m_Arena.GetBomber(Player).Exist())
                    {
                        // If this bomber is alive
                        if (m_Arena.GetBomber(Player).IsAlive())
                        {
                            // Count number of human and AI alive bombers
                            switch (m_Arena.GetBomber(Player).GetBomberType())
                            {
                                case EBomberType.BOMBERTYPE_MAN:
                                case EBomberType.BOMBERTYPE_NET:  AliveCount_Human++; break;
                                case EBomberType.BOMBERTYPE_COM:  AliveCount_AI++;    break;
                                default: break;
                            }
                        }
                    }
                }

                bool ForceArenaClosing = false;

                if (AliveCount_Human == 0 && AliveCount_AI > 1)
                {
                    switch (m_pOptions.GetOption_ActionWhenOnlyAIPlayersLeft())
                    {
                        case EActionAIAlive.ACTIONONLYAIPLAYERSALIVE_ENDMATCHDRAWGAME: m_ForceDrawGame    = true;          break;
                        case EActionAIAlive.ACTIONONLYAIPLAYERSALIVE_STARTCLOSING:     ForceArenaClosing  = true;          break;
                        case EActionAIAlive.ACTIONONLYAIPLAYERSALIVE_SPEEDUPGAME:      m_pTimer.SetSpeed(7.0f);             break;
                        case EActionAIAlive.ACTIONONLYAIPLAYERSALIVE_CONTINUEGAME:     break;
                        default: Debug.Assert(false); break;
                    }

                    if (ForceArenaClosing)
                    {
                        // Stop the match music song
                        m_pSound.StopSong(ESong.SONG_MATCH_MUSIC);
                    }
                }

                // If the hurry up is enabled
                if (m_pOptions.GetTimeUpMinutes() != 0 || m_pOptions.GetTimeUpSeconds() != 0 || ForceArenaClosing)
                {
                    // If the arena is not closing
                    if (!m_NoticedTimeUp && !m_Arena.GetArenaCloser().IsClosing())
                    {
                        // If the clock's current time is less than (or equal to) the timeup's time
                        if (m_Clock.GetMinutes() < m_pOptions.GetTimeUpMinutes()
                            ||
                            (m_Clock.GetMinutes() == m_pOptions.GetTimeUpMinutes() &&
                             m_Clock.GetSeconds() <= m_pOptions.GetTimeUpSeconds())
                            ||
                            ForceArenaClosing)
                        {
                            // Make the arena start closing
                            m_Arena.GetArenaCloser().Start();

                            // Don't do this more than once
                            m_NoticedTimeUp = true;
                        }
                    }
                }

                // Update the match components
                m_Clock.Update(m_pTimer.GetDeltaTime());
                m_Board.Update();
                m_Arena.Update(m_pTimer.GetDeltaTime());
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Manage the hurry up message</summary>
        private void ManageHurryUpMessage()
        {
            // If the match is not paused and the hurry up is enabled
            if (m_pPauseMessage == null && (m_pOptions.GetTimeUpMinutes() != 0 || m_pOptions.GetTimeUpSeconds() != 0))
            {
                int ClockTotalSeconds  = m_Clock.GetMinutes() * 60 + m_Clock.GetSeconds();
                int TimeUpTotalSeconds = m_pOptions.GetTimeUpMinutes() * 60 + m_pOptions.GetTimeUpSeconds();

                if (ClockTotalSeconds == TimeUpTotalSeconds + 1)
                {
                    // If the hurry message doesn't exist
                    if (m_pHurryMessage == null)
                    {
                        // Create the hurry message
                        m_pHurryMessage = new CHurryMessage(m_pDisplay, m_pSound);
                    }
                }
            }

            // If the hurry message exists
            if (m_pHurryMessage != null)
            {
                // Update the hurry message. If it has finished its behaviour
                if (m_pHurryMessage.Update(m_pTimer.GetDeltaTime()))
                {
                    // Delete the hurry message object
                    m_pHurryMessage = null;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void ManageMatchOver()
        {
            // If the match is not paused
            if (m_pPauseMessage == null)
            {
                int AliveCount = 0;     // Number of alive bombers
                int DyingCount = 0;     // Number of dying bombers

                // Count alive bombers and dying bombers
                for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                {
                    // If this bomber exists
                    if (m_Arena.GetBomber(Player).Exist())
                    {
                        // If this bomber is alive
                        if (m_Arena.GetBomber(Player).IsAlive())
                        {
                            AliveCount++;
                        }
                        // If this bomber is dying
                        else if (m_Arena.GetBomber(Player).IsDying())
                        {
                            DyingCount++;
                        }
                    }
                }

                int[] TeamCountAlive = new int[Globals.MAX_TEAMS];
                int[] TeamCountDying = new int[Globals.MAX_TEAMS];

                for (int Team = 0; Team < Globals.MAX_TEAMS; Team++)
                {
                    TeamCountAlive[Team] = 0;
                    TeamCountDying[Team] = 0;
                }

                for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                {
                    // If this bomber exists
                    if (m_Arena.GetBomber(Player).Exist())
                    {
                        int Team = m_Arena.GetBomber(Player).GetTeam().GetTeamId();

                        if (m_Arena.GetBomber(Player).IsAlive())
                        {
                            TeamCountAlive[Team]++;
                        }
                        else if (m_Arena.GetBomber(Player).IsDying())
                        {
                            TeamCountDying[Team]++;
                        }
                    }
                }

                int CountTeamsAlive = 0;
                int CountTeamsDying = 0;

                for (int Team = 0; Team < Globals.MAX_TEAMS; Team++)
                {
                    if (TeamCountAlive[Team] > 0) CountTeamsAlive++;
                    if (TeamCountDying[Team] > 0) CountTeamsDying++;
                }

                // If no bomber is alive and there are only dying bombers
                if (AliveCount == 0 && DyingCount > 0)
                {
                    // Stop the match song which was playing
                    m_pSound.StopSong(m_CurrentSong);
                }
                // If no bomber is alive or dying then this is a draw game
                else if (AliveCount == 0 && DyingCount == 0)
                {
                    m_MatchOver  = true;
                    m_WinnerTeam = NO_WINNER_TEAM;
                    m_Arena.GetArenaCloser().Stop();
                    m_Board.SetClockAnimation(false);
                    m_ExitModeTime = m_ModeTime + PAUSE_DRAWGAME;
                }
                // If only AI bombers are alive then this is also a draw game
                else if (m_ForceDrawGame)
                {
                    m_pSound.StopSong(m_CurrentSong);
                    m_MatchOver  = true;
                    m_WinnerTeam = NO_WINNER_TEAM;
                    m_Arena.GetArenaCloser().Stop();
                    m_Board.SetClockAnimation(false);
                    m_ExitModeTime = m_ModeTime + PAUSE_DRAWGAME;

                    // Tell the bombers there is no command
                    for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                    {
                        if (m_Arena.GetBomber(Player).Exist() && m_Arena.GetBomber(Player).IsAlive())
                        {
                            m_Arena.GetBomber(Player).Command(EBomberMove.BOMBERMOVE_NONE, EBomberAction.BOMBERACTION_NONE);
                        }
                    }
                }
                // If one team is alive then that team has won the match
                else if (CountTeamsAlive == 1 && CountTeamsDying == 0)
                {
                    m_MatchOver = true;

                    for (int Team = 0; Team < m_Arena.MaxTeams(); Team++)
                    {
                        if (TeamCountAlive[Team] > 0)
                        {
                            m_WinnerTeam = Team;
                            m_Teams[Team].SetVictorious(true);
                            break;
                        }
                    }

                    for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                    {
                        if (m_Arena.GetBomber(Player).Exist() && m_Arena.GetBomber(Player).IsAlive())
                        {
                            m_Arena.GetBomber(Player).Command(EBomberMove.BOMBERMOVE_NONE, EBomberAction.BOMBERACTION_NONE);
                        }
                    }

                    // Play the bell sound (ding ding ding ding ding!)
                    m_pSound.PlaySample(ESample.SAMPLE_RING_DING);

                    // Stop the match song which was playing
                    m_pSound.StopSong(m_CurrentSong);

                    m_Arena.GetArenaCloser().Stop();
                    m_Board.SetClockAnimation(false);
                    m_ExitModeTime = m_ModeTime + PAUSE_WINNER;
                }
                else if (m_pOptions.GetTimeUpMinutes() == 0 && m_pOptions.GetTimeUpSeconds() == 0 &&
                    (m_pOptions.GetTimeStartMinutes() != 0 || m_pOptions.GetTimeStartSeconds() != 0) &&
                    m_Clock.GetMinutes() == 0 && m_Clock.GetSeconds() == 0)
                {
                    m_MatchOver  = true;
                    m_WinnerTeam = NO_WINNER_TEAM;

                    // Seek the alive bombers
                    for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                    {
                        if (m_Arena.GetBomber(Player).Exist() && m_Arena.GetBomber(Player).IsAlive())
                        {
                            m_Arena.GetBomber(Player).Command(EBomberMove.BOMBERMOVE_NONE, EBomberAction.BOMBERACTION_NONE);
                            m_Arena.GetBomber(Player).GetTeam().SetVictorious(true);
                        }
                    }

                    m_pSound.PlaySample(ESample.SAMPLE_RING_DING);
                    m_pSound.StopSong(m_CurrentSong);
                    m_Arena.GetArenaCloser().Stop();
                    m_Board.SetClockAnimation(false);
                    m_ExitModeTime = m_ModeTime + PAUSE_DRAWGAME;
                }

                if (m_MatchOver)
                {
                    // Set the game speed to normal
                    m_pTimer.SetSpeed(1.0f);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Update the object and return what game mode should be set</summary>
        public override EGameMode Update()
        {
            // Increase elapsed time since mode has started
            m_ModeTime += m_pTimer.GetDeltaTime();

            // If we have to make the first black screen
            if (m_ModeTime <= BLACKSCREEN_DURATION)
            {
                // nothing
            }
            // If the first black screen is done and we have to make a little
            // pause to allow the players to see the arena before playing
            else if (m_ModeTime <= BLACKSCREEN_DURATION + PAUSE_BEGIN)
            {
                // nothing
            }
            // If match is currently playing and it's not over
            else if (!m_MatchOver)
            {
                PlaySong();
                ProcessPlayerCommands();
                ManagePauseMessage();
                UpdateMatch();
                ManageHurryUpMessage();
                ManageMatchOver();
            }
            // If the match is over and we have to make a pause before the last black screen
            else if (m_ModeTime <= m_ExitModeTime)
            {
                // Update the match
                m_Board.Update();
                m_Arena.Update(m_pTimer.GetDeltaTime());
            }
            // If the pause is over and we have to make the last black screen
            else if (m_ModeTime <= m_ExitModeTime + BLACKSCREEN_DURATION)
            {
                // nothing
            }
            // If the last black screen is over then ask for another game mode
            else
            {
                // If it's a draw game
                if (m_WinnerTeam == NO_WINNER_TEAM)
                {
                    // Ask for a game mode change to draw game screen
                    return EGameMode.GAMEMODE_DRAWGAME;
                }
                // If there is a winner
                else
                {
                    // Ask for a game mode change to winner screen
                    return EGameMode.GAMEMODE_WINNER;
                }
            }

            // Stay in this game mode
            return EGameMode.GAMEMODE_MATCH;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Display on the screen</summary>
        public override void Display()
        {
            // If we have to make the first black screen
            if (m_ModeTime <= BLACKSCREEN_DURATION)
            {
                // nothing
            }
            // If first black screen is done and we have to make a little
            // pause to allow the players to see the arena before playing
            else if (m_ModeTime <= BLACKSCREEN_DURATION + PAUSE_BEGIN)
            {
                DisplayMatchScreen();
                // Reset display origin
                m_pDisplay.SetOrigin(0, 0);
            }
            // If match is currently playing and it's not over
            else if (!m_MatchOver)
            {
                // Reset display origin
                m_pDisplay.SetOrigin(0, 0);

                DisplayMatchScreen();
                DisplayHurryUpMessage();
                DisplayPauseMessage();
            }
            // If the match is over and we have to make a pause before the last black screen
            else if (m_ModeTime <= m_ExitModeTime)
            {
                DisplayMatchScreen();
            }
            // If the pause is over and we have to make the last black screen
            else if (m_ModeTime <= m_ExitModeTime + BLACKSCREEN_DURATION)
            {
                // nothing
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void DisplayMatchScreen()
        {
            m_Board.Display();
            m_Arena.Display();
        }

        private void DisplayHurryUpMessage()
        {
            // If the hurry message exists
            if (m_pHurryMessage != null)
            {
                m_pHurryMessage.Display();
            }
        }

        private void DisplayPauseMessage()
        {
            // If the match is paused
            if (m_pPauseMessage != null)
            {
                m_pPauseMessage.Display();
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
