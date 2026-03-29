/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008 Bernd Arnold
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
 *  \file CDemo.cs
 *  \brief The demo
 */

using System.Diagnostics;

namespace Bombermaaan
{
    /// <summary>
    /// The demo screen, showing a match between computer players.
    /// </summary>
    public class CDemo : CModeScreen
    {
        //----------------------------------------------------------------------
        // Constants
        //----------------------------------------------------------------------

        private const float BLACKSCREEN_DURATION    = 0.350f;
        private const float PAUSE_BEGIN             = 1.0f;
        private const float PAUSE_DRAWGAME          = 2.5f;
        private const float PAUSE_WINNER            = 2.5f;

        private const float DEMO_TEXT_FLASH_TIME    = 0.2f;
        private const int   DEMO_TEXT_POSITION_X    = 4;
        // DEMO_TEXT_POSITION_Y uses VIEW_HEIGHT — computed inline
        private const string DEMO_TEXT_STRING       = "DEMO";

        private const int   LEVEL_FILENAME_POSITION_X   = 30;
        // LEVEL_FILENAME_POSITION_Y uses VIEW_HEIGHT — computed inline

        //----------------------------------------------------------------------
        // Private members
        //----------------------------------------------------------------------

        private COptions        m_Options;          //!< Our own customized options based on the real options
        private CBoard          m_Board;            //!< Board object
        private CClock          m_Clock;            //!< Clock object
        private CArena          m_Arena;            //!< Arena object
        private CTeam[]         m_Teams;            //!< Teams object
        private CAiManager      m_AiManager;        //!< Computer brain
        private bool            m_MatchOver;        //!< Is match over?
        private ESong           m_CurrentSong;      //!< Current song being played
        private bool            m_IsSongPlaying;    //!< Is the match song playing?
        private bool            m_NoticedTimeUp;    //!< Did we notice that time is up?
        private CHurryMessage   m_pHurryMessage;    //!< Hurry up message object
        private CFont           m_Font;             //!< Font object needed to draw the DEMO text
        private float           m_DemoTextTime;     //!< Time we have spent drawing (or not) the demo text
        private bool            m_DrawDemoText;     //!< Do we currently need to draw the demo text?
        private float           m_ModeTime;         //!< Time (in seconds) that elapsed since the mode has started
        private float           m_ExitModeTime;     //!< Mode time when we have to start the last black screen
        private int             m_ExitGameMode;     //!< Game mode to ask for when exiting
        private bool            m_HaveToExit;       //!< Do we have to exit this mode?

        //----------------------------------------------------------------------
        // Constructor / Destructor
        //----------------------------------------------------------------------

        public CDemo() : base()
        {
            m_Options = new COptions();
            m_Board = new CBoard();
            m_Clock = new CClock();
            m_Arena = new CArena();
            m_Teams = new CTeam[Globals.MAX_TEAMS];
            for (int i = 0; i < Globals.MAX_TEAMS; i++)
                m_Teams[i] = new CTeam();
            m_AiManager = new CAiManager();
            m_Font = new CFont();

            m_Board.SetClock(m_Clock);
            m_Board.SetArena(m_Arena);
            m_AiManager.SetArena(m_Arena);

            m_pHurryMessage = null;
            m_MatchOver = false;
            m_CurrentSong = ESong.SONG_NONE;
            m_IsSongPlaying = false;
            m_NoticedTimeUp = false;
            m_DemoTextTime = 0.0f;
            m_DrawDemoText = false;
            m_ModeTime = 0.0f;
            m_ExitModeTime = 0.0f;
            m_ExitGameMode = 0;
            m_HaveToExit = false;
        }

        //----------------------------------------------------------------------
        // SetDisplay
        //----------------------------------------------------------------------

        public override void SetDisplay(CDisplay pDisplay)
        {
            base.SetDisplay(pDisplay);
            m_Board.SetDisplay(pDisplay);
            m_Arena.SetDisplay(pDisplay);
            m_Font.SetDisplay(pDisplay);
        }

        //----------------------------------------------------------------------
        // SetOptions
        //----------------------------------------------------------------------

        public override void SetOptions(COptions pOptions)
        {
            Debug.Assert(pOptions != null);
            base.SetOptions(pOptions);

            // Make a copy of the options object — customized to suit demo needs.
            m_Options = pOptions.Clone();

            m_Board.SetOptions(m_Options);
            m_Arena.SetOptions(m_Options);
        }

        //----------------------------------------------------------------------
        // SetScores
        //----------------------------------------------------------------------

        public void SetScores(CScores pScores)
        {
            m_Board.SetScores(pScores);
        }

        //----------------------------------------------------------------------
        // SetTimer
        //----------------------------------------------------------------------

        public override void SetTimer(CTimer pTimer)
        {
            base.SetTimer(pTimer);
            m_Board.SetTimer(pTimer);
        }

        //----------------------------------------------------------------------
        // SetSound
        //----------------------------------------------------------------------

        public override void SetSound(CSound pSound)
        {
            base.SetSound(pSound);
            m_Arena.SetSound(pSound);
        }

        //----------------------------------------------------------------------
        // Create
        //----------------------------------------------------------------------

        public override void Create()
        {
            base.Create();

            m_MatchOver = false;
            m_IsSongPlaying = false;
            m_NoticedTimeUp = false;
            m_ModeTime = 0.0f;
            m_HaveToExit = false;
            m_DemoTextTime = 0.0f;
            m_DrawDemoText = false;

            SetupOptions();
            CreateMainComponents();
            CreateFont();

            m_AiManager.SetDisplay(m_pDisplay);
            m_AiManager.Create(m_Options);

            for (int i = 0; i < Globals.MAX_TEAMS; i++)
            {
                m_Teams[i].SetTeamId(i);
                m_Teams[i].SetVictorious(false);
            }

            for (int i = 0; i < Globals.MAX_BOMBERS; i++)
            {
                m_Arena.GetBomber(i).SetTeam(m_Teams[i]);
            }
        }

        //----------------------------------------------------------------------
        // Destroy
        //----------------------------------------------------------------------

        public override void Destroy()
        {
            base.Destroy();
            m_Font.Destroy();
            m_AiManager.Destroy();
            StopSong();
            DestroyHurryUpMessage();
            DestroyMainComponents();
        }

        //----------------------------------------------------------------------
        // Reset
        //----------------------------------------------------------------------

        public void Reset()
        {
            // Nothing specific beyond re-create
        }

        //----------------------------------------------------------------------
        // OpenInput / CloseInput
        //----------------------------------------------------------------------

        public override void OpenInput()
        {
            m_pInput.GetMainInput().Open();
        }

        public override void CloseInput()
        {
            m_pInput.GetMainInput().Close();
        }

        //----------------------------------------------------------------------
        // Update
        //----------------------------------------------------------------------

        public override EGameMode Update()
        {
            m_ModeTime += m_pTimer.GetDeltaTime();

            if (m_ModeTime <= BLACKSCREEN_DURATION)
            {
                // First black screen
            }
            else if (m_ModeTime <= BLACKSCREEN_DURATION + PAUSE_BEGIN)
            {
                // Pause at beginning
            }
            else if (!m_MatchOver)
            {
                PlaySong();
                ProcessPlayerCommands();
                ManageExit();
                UpdateMatch();
                UpdateDemoText();
                ManageHurryUpMessage();
                ManageMatchOver();
            }
            else if (m_ModeTime <= m_ExitModeTime)
            {
                m_Board.Update();
                m_Arena.Update(m_pTimer.GetDeltaTime());
            }
            else if (m_ModeTime <= m_ExitModeTime + BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
            else
            {
                return EGameMode.GAMEMODE_TITLE;
            }

            return EGameMode.GAMEMODE_DEMO;
        }

        //----------------------------------------------------------------------
        // Display
        //----------------------------------------------------------------------

        public override void Display()
        {
            if (m_ModeTime <= BLACKSCREEN_DURATION)
            {
                // First black screen
            }
            else if (m_ModeTime <= BLACKSCREEN_DURATION + PAUSE_BEGIN)
            {
                m_Board.Display();
                m_Arena.Display();
                m_pDisplay.SetOrigin(0, 0);
            }
            else if (!m_MatchOver)
            {
                m_pDisplay.SetOrigin(0, 0);
                DisplayDemoText();
                DisplayMatchScreen();
                DisplayHurryUpMessage();
            }
            else if (m_ModeTime <= m_ExitModeTime)
            {
                DisplayMatchScreen();
            }
            else if (m_ModeTime <= m_ExitModeTime + BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
        }

        //----------------------------------------------------------------------
        // Private helpers
        //----------------------------------------------------------------------

        private void SetupOptions()
        {
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                m_Options.SetBomberType(Player, EBomberType.BOMBERTYPE_COM);
            }

            m_Options.SetLevel(CRandom.Random(m_Options.GetNumberOfLevels()));
            m_Options.SetTimeStart(1, 0);
            m_Options.SetTimeUp(0, 35);
        }

        private void CreateMainComponents()
        {
            m_Board.Create();
            m_Arena.Create();
            m_Clock.Create(EClockType.CLOCKTYPE_COUNTDOWN,
                           EClockMode.CLOCKMODE_MS,
                           0,
                           m_Options.GetTimeStartMinutes(),
                           m_Options.GetTimeStartSeconds(),
                           0);
        }

        private void CreateFont()
        {
            m_Font.Create();
            m_Font.SetShadow(true);
            m_Font.SetShadowColor(EFontColor.FONTCOLOR_BLACK);
            m_Font.SetShadowDirection(EShadowDirection.SHADOWDIRECTION_DOWNRIGHT);
            m_Font.SetSpriteLayer(800);
            m_Font.SetTextColor(EFontColor.FONTCOLOR_WHITE);
        }

        private void DestroyMainComponents()
        {
            m_Board.Destroy();
            m_Clock.Destroy();
            m_Arena.Destroy();
        }

        private void StopSong()
        {
            if (m_IsSongPlaying)
            {
                m_pSound.StopSong(m_CurrentSong);
            }
        }

        private void DestroyHurryUpMessage()
        {
            if (m_pHurryMessage != null)
            {
                m_pHurryMessage = null;
            }
        }

        private void PlaySong()
        {
            if (!m_IsSongPlaying)
            {
                m_pSound.PlaySong(ESong.SONG_MATCH_MUSIC);
                m_CurrentSong = ESong.SONG_MATCH_MUSIC;
                m_IsSongPlaying = true;
            }
        }

        private void ProcessPlayerCommands()
        {
            m_AiManager.Update(m_pTimer.GetDeltaTime());
        }

        private void ManageExit()
        {
            if (m_pInput.GetMainInput().TestBreak())
            {
                m_MatchOver = true;
                m_ExitModeTime = m_ModeTime;
            }
        }

        private void UpdateMatch()
        {
            if (m_Options.GetTimeUpMinutes() != 0 || m_Options.GetTimeUpSeconds() != 0)
            {
                if (!m_NoticedTimeUp && !m_Arena.GetArenaCloser().IsClosing())
                {
                    if (m_Clock.GetMinutes() < m_Options.GetTimeUpMinutes() ||
                        (m_Clock.GetMinutes() == m_Options.GetTimeUpMinutes() &&
                         m_Clock.GetSeconds() <= m_Options.GetTimeUpSeconds()))
                    {
                        m_Arena.GetArenaCloser().Start();
                        m_pSound.PlaySong(ESong.SONG_MATCH_MUSIC);
                        m_CurrentSong = ESong.SONG_MATCH_MUSIC;
                        m_NoticedTimeUp = true;
                    }
                }
            }

            m_Clock.Update(m_pTimer.GetDeltaTime());
            m_Board.Update();
            m_Arena.Update(m_pTimer.GetDeltaTime());
        }

        private void UpdateDemoText()
        {
            m_DemoTextTime += m_pTimer.GetDeltaTime();
            if (m_DemoTextTime >= DEMO_TEXT_FLASH_TIME)
            {
                m_DrawDemoText = !m_DrawDemoText;
                m_DemoTextTime = 0.0f;
            }
        }

        private void ManageHurryUpMessage()
        {
            if (m_Options.GetTimeUpMinutes() != 0 || m_Options.GetTimeUpSeconds() != 0)
            {
                int ClockTotalSeconds = m_Clock.GetMinutes() * 60 + m_Clock.GetSeconds();
                int TimeUpTotalSeconds = m_Options.GetTimeUpMinutes() * 60 + m_Options.GetTimeUpSeconds();

                if (ClockTotalSeconds == TimeUpTotalSeconds + 1)
                {
                    if (m_pHurryMessage == null)
                    {
                        m_pHurryMessage = new CHurryMessage(m_pDisplay, m_pSound);
                    }
                }
            }

            if (m_pHurryMessage != null)
            {
                if (m_pHurryMessage.Update(m_pTimer.GetDeltaTime()))
                {
                    m_pHurryMessage = null;
                }
            }
        }

        private void ManageMatchOver()
        {
            int AliveCount = 0;
            int DyingCount = 0;

            for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
            {
                if (m_Arena.GetBomber(Player).Exist())
                {
                    if (m_Arena.GetBomber(Player).IsAlive())
                        AliveCount++;
                    else if (m_Arena.GetBomber(Player).IsDying())
                        DyingCount++;
                }
            }

            if (AliveCount == 0 && DyingCount > 0)
            {
                m_pSound.StopSong(m_CurrentSong);
            }
            else if (AliveCount == 0 && DyingCount == 0)
            {
                m_MatchOver = true;
                m_Arena.GetArenaCloser().Stop();
                m_Board.SetClockAnimation(false);
                m_ExitModeTime = m_ModeTime + PAUSE_DRAWGAME;
            }
            else if (AliveCount == 1 && DyingCount == 0)
            {
                m_MatchOver = true;

                for (int Player = 0; Player < m_Arena.MaxBombers(); Player++)
                {
                    if (m_Arena.GetBomber(Player).Exist() && m_Arena.GetBomber(Player).IsAlive())
                    {
                        m_Arena.GetBomber(Player).Command(EBomberMove.BOMBERMOVE_NONE, EBomberAction.BOMBERACTION_NONE);
                        m_Arena.GetBomber(Player).GetTeam().SetVictorious(true);
                        break;
                    }
                }

                m_pSound.PlaySample(ESample.SAMPLE_RING_DING);
                m_pSound.StopSong(m_CurrentSong);
                m_Arena.GetArenaCloser().Stop();
                m_Board.SetClockAnimation(false);
                m_ExitModeTime = m_ModeTime + PAUSE_WINNER;
            }
            else if (m_Options.GetTimeUpMinutes() == 0 && m_Options.GetTimeUpSeconds() == 0 &&
                     (m_Options.GetTimeStartMinutes() != 0 || m_Options.GetTimeStartSeconds() != 0) &&
                     m_Clock.GetMinutes() == 0 && m_Clock.GetSeconds() == 0)
            {
                m_MatchOver = true;

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
        }

        private void DisplayDemoText()
        {
            if (m_DrawDemoText)
            {
                m_Font.Draw(DEMO_TEXT_POSITION_X, Globals.VIEW_HEIGHT - 14, DEMO_TEXT_STRING);
            }
        }

        private void DisplayMatchScreen()
        {
            m_Board.Display();
            m_Arena.Display();
        }

        private void DisplayHurryUpMessage()
        {
            if (m_pHurryMessage != null)
            {
                m_pHurryMessage.Display();
            }
        }
    }
}
