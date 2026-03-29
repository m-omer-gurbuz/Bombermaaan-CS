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
 *  \file CMenu.cs
 *  \brief Menu
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>The different screens that form the game menu.</summary>
    public enum EMenuMode
    {
        MENUMODE_BOMBER,    //!< Which bombers are playing or not?
        MENUMODE_INPUT,     //!< What player input configuration to use for each human player?
        MENUMODE_MATCH,     //!< Match setup : arena times, win matches...
        MENUMODE_TEAM,      //!< Team setup : set teams...
        MENUMODE_LEVEL      //!< Choose level layout to use
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>The top-level menu screen that manages all sub-menu screens.</summary>
    public class CMenu : CModeScreen
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private const float MENU_BLACKSCREEN_DURATION         = 0.350f;    //!< Duration (in seconds) of the two black screens

        private const int   MOSAIC_SPRITE_LAYER               = 0;         //!< Sprite layer where to draw the mosaic tiles
        private const int   MOSAIC_SPRITE_PRIORITY_IN_LAYER   = 0;         //!< Priority to use in the sprite layer where to draw the mosaic tiles
        private const float MOSAIC_SPEED_X                    = 50.0f;     //!< Speed of the mosaic background horizontally
        private const float MOSAIC_SPEED_Y                    = -50.0f;    //!< Speed of the mosaic background vertically

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private CScores     m_pScores;              //!< Link to the scores object to use
        private CFont       m_Font;                 //!< Font object
        private EMenuMode   m_MenuMode;             //!< Current menu mode (= current menu screen)
        private CMenuBomber m_MenuBomber;           //!< Menu screen object corresponding to MENUMODE_BOMBER
        private CMenuInput  m_MenuInput;            //!< Menu screen object corresponding to MENUMODE_INPUT
        private CMenuMatch  m_MenuMatch;            //!< Menu screen object corresponding to MENUMODE_MATCH
        private CMenuTeam   m_MenuTeam;             //!< Menu screen object corresponding to MENUMODE_TEAM
        private CMenuLevel  m_MenuLevel;            //!< Menu screen object corresponding to MENUMODE_LEVEL
        private float       m_GameModeTime;         //!< Time (in seconds) that elapsed since this game mode has started
        private bool        m_HaveToExit;           //!< Do we have to exit this game mode?
        private EGameMode   m_ExitGameMode;         //!< Game mode to ask for when exiting (after black screen)
        private float       m_ExitGameModeTime;     //!< Game mode time when we realized we have to exit (used for blackscreen)
        private bool        m_SongStarted;          //!< Did we start playing the song after the black screen?
        private CMosaic     m_pMosaic;              //!< Mosaic object used for the animated mosaic background of the menu screen

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CMenu() : base()
        {
            m_pScores = null;

            m_Font        = new CFont();
            m_MenuBomber  = new CMenuBomber();
            m_MenuInput   = new CMenuInput();
            m_MenuMatch   = new CMenuMatch();
            m_MenuTeam    = new CMenuTeam();
            m_MenuLevel   = new CMenuLevel();

            // Set the font object the menu screen objects have to communicate with
            m_MenuBomber.SetFont(m_Font);
            m_MenuInput.SetFont(m_Font);
            m_MenuMatch.SetFont(m_Font);
            m_MenuTeam.SetFont(m_Font);
            m_MenuLevel.SetFont(m_Font);

            // The menu mode to start with will be set ONCE here
            // (not on menu creation with Create() method). The
            // reason is we have to keep the menu mode in memory
            // even when we finish or start the game menu mode.
            // This allows to get back to the last menu mode when
            // a battle is over for example.
            m_MenuMode = EMenuMode.MENUMODE_BOMBER;

            m_GameModeTime    = 0.0f;
            m_HaveToExit      = false;
            m_ExitGameMode    = EGameMode.GAMEMODE_NONE;
            m_ExitGameModeTime = 0.0f;
            m_SongStarted     = false;
            m_pMosaic         = null;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public override void SetDisplay(CDisplay pDisplay)
        {
            base.SetDisplay(pDisplay);
            m_Font.SetDisplay(pDisplay);
            m_MenuBomber.SetDisplay(pDisplay);
            m_MenuInput.SetDisplay(pDisplay);
            m_MenuMatch.SetDisplay(pDisplay);
            m_MenuTeam.SetDisplay(pDisplay);
            m_MenuLevel.SetDisplay(pDisplay);
        }

        public override void SetInput(CInput pInput)
        {
            base.SetInput(pInput);
            m_MenuBomber.SetInput(pInput);
            m_MenuInput.SetInput(pInput);
            m_MenuMatch.SetInput(pInput);
            m_MenuTeam.SetInput(pInput);
            m_MenuLevel.SetInput(pInput);
        }

        public override void SetOptions(COptions pOptions)
        {
            base.SetOptions(pOptions);
            m_MenuBomber.SetOptions(pOptions);
            m_MenuInput.SetOptions(pOptions);
            m_MenuMatch.SetOptions(pOptions);
            m_MenuTeam.SetOptions(pOptions);
            m_MenuLevel.SetOptions(pOptions);
        }

        public override void SetTimer(CTimer pTimer)
        {
            base.SetTimer(pTimer);
            m_MenuBomber.SetTimer(pTimer);
            m_MenuInput.SetTimer(pTimer);
            m_MenuMatch.SetTimer(pTimer);
            m_MenuTeam.SetTimer(pTimer);
            m_MenuLevel.SetTimer(pTimer);
        }

        public override void SetSound(CSound pSound)
        {
            base.SetSound(pSound);
            m_MenuBomber.SetSound(pSound);
            m_MenuInput.SetSound(pSound);
            m_MenuMatch.SetSound(pSound);
            m_MenuTeam.SetSound(pSound);
            m_MenuLevel.SetSound(pSound);
        }

        public void SetScores(CScores pScores)
        {
            m_pScores = pScores;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public override void Create()
        {
            base.Create();

            // Check if all the objects to communicate with are set
            Debug.Assert(m_pScores != null);

            // Reset game mode time (no time has been elapsed in this game mode yet)
            m_GameModeTime = 0.0f;

            // Don't have to exit this game mode yet
            m_HaveToExit = false;

            // We didn't start playing the song yet
            m_SongStarted = false;

            // Make a random blue mosaic object
            m_pMosaic = CRandomMosaic.CreateRandomMosaic(m_pDisplay,
                MOSAIC_SPRITE_LAYER,
                MOSAIC_SPRITE_PRIORITY_IN_LAYER,
                MOSAIC_SPEED_X,
                MOSAIC_SPEED_Y,
                EMosaicColor.MOSAICCOLOR_BLUE,
                EMosaicType.MOSAICTYPE_BOMB);

            // Assure all scores are set to zero
            m_pScores.Reset();

            // Start the current menu mode!
            StartMenuMode(m_MenuMode);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public override void Destroy()
        {
            base.Destroy();

            // Delete the scrolling mosaic background
            m_pMosaic.Destroy();
            m_pMosaic = null;

            // If the song is playing
            if (m_SongStarted)
            {
                // Stop playing the menu song
                m_pSound.StopSong(ESong.SONG_MENU_MUSIC);
            }

            // Terminate menu mode. The current menu mode remains
            // the same so that it can be used in the next call to
            // the Create() method.
            FinishMenuMode();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public override void OpenInput()
        {
            m_pInput.GetMainInput().Open();
        }

        public override void CloseInput()
        {
            m_pInput.GetMainInput().Close();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public override EGameMode Update()
        {
            // Increase elapsed time since this game mode has started
            m_GameModeTime += m_pTimer.GetDeltaTime();

            // If we have to make the first black screen
            if (m_GameModeTime <= MENU_BLACKSCREEN_DURATION)
            {
                // nothing
            }
            // If minimum duration of the mode has not elapsed OR we don't have to exit yet
            else if (!m_HaveToExit)
            {
                // If we didn't start playing the song yet
                if (!m_SongStarted)
                {
                    // Start playing the menu song
                    m_pSound.PlaySong(ESong.SONG_MENU_MUSIC);

                    // We started playing the song
                    m_SongStarted = true;
                }

                // Update the scrolling mosaic background
                m_pMosaic.Update(m_pTimer.GetDeltaTime());

                //-------------------------------------
                // Update current menu object
                //-------------------------------------

                // We have to save the menu action to perform
                EMenuAction MenuAction = EMenuAction.MENUACTION_NONE;

                // Update the object corresponding to the current menu mode
                switch (m_MenuMode)
                {
                    case EMenuMode.MENUMODE_BOMBER: MenuAction = m_MenuBomber.Update(); break;
                    case EMenuMode.MENUMODE_INPUT:  MenuAction = m_MenuInput.Update();  break;
                    case EMenuMode.MENUMODE_MATCH:  MenuAction = m_MenuMatch.Update();  break;
                    case EMenuMode.MENUMODE_TEAM:   MenuAction = m_MenuTeam.Update();   break;
                    case EMenuMode.MENUMODE_LEVEL:  MenuAction = m_MenuLevel.Update();  break;
                }

                //-------------------------------------
                // Perform menu action if needed
                //-------------------------------------

                // If there is a menu action to perform
                if (MenuAction != EMenuAction.MENUACTION_NONE)
                {
                    switch (MenuAction)
                    {
                        // We have to go back
                        case EMenuAction.MENUACTION_PREVIOUS:
                        {
                            switch (m_MenuMode)
                            {
                                case EMenuMode.MENUMODE_BOMBER:
                                {
                                    // There is no previous menu mode. Switch to the game title screen.
                                    m_HaveToExit   = true;
                                    m_pSound.StopSong(ESong.SONG_MENU_MUSIC);
                                    m_ExitGameMode = EGameMode.GAMEMODE_TITLE;
                                    break;
                                }

                                case EMenuMode.MENUMODE_INPUT:
                                {
                                    EMenuMode MenuMode = EMenuMode.MENUMODE_BOMBER;
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_MATCH:
                                {
                                    // Number of human players
                                    int ManCount = 0;

                                    // Scan the players
                                    for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                                    {
                                        if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN)
                                        {
                                            ManCount++;
                                        }

                                        int PlayerInput = m_pOptions.GetPlayerInput(Player);
                                        if (PlayerInput >= m_pInput.GetPlayerInputCount())
                                        {
                                            PlayerInput = 0;
                                            m_pOptions.SetPlayerInput(Player, PlayerInput);
                                        }
                                    }

                                    // Skip the input menu if there is no human player
                                    EMenuMode MenuMode = (ManCount > 0 ? EMenuMode.MENUMODE_INPUT : EMenuMode.MENUMODE_BOMBER);
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_TEAM:
                                {
                                    EMenuMode MenuMode = EMenuMode.MENUMODE_MATCH;
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_LEVEL:
                                {
                                    EMenuMode MenuMode = (m_pOptions.GetBattleMode() == EBattleMode.BATTLEMODE_TEAM
                                        ? EMenuMode.MENUMODE_TEAM
                                        : EMenuMode.MENUMODE_MATCH);
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }
                            }
                            break;
                        }

                        // We have to go forward
                        case EMenuAction.MENUACTION_NEXT:
                        {
                            switch (m_MenuMode)
                            {
                                case EMenuMode.MENUMODE_BOMBER:
                                {
                                    int ManCount = 0;

                                    for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                                    {
                                        if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN)
                                        {
                                            ManCount++;
                                        }

                                        int PlayerInput = m_pOptions.GetPlayerInput(Player);
                                        if (PlayerInput >= m_pInput.GetPlayerInputCount())
                                        {
                                            PlayerInput = 0;
                                            m_pOptions.SetPlayerInput(Player, PlayerInput);
                                        }
                                    }

                                    // Skip the input menu if there is no human player
                                    EMenuMode MenuMode = (ManCount > 0 ? EMenuMode.MENUMODE_INPUT : EMenuMode.MENUMODE_MATCH);
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_INPUT:
                                {
                                    EMenuMode MenuMode = EMenuMode.MENUMODE_MATCH;
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_MATCH:
                                {
                                    EMenuMode MenuMode = (m_pOptions.GetBattleMode() == EBattleMode.BATTLEMODE_TEAM
                                        ? EMenuMode.MENUMODE_TEAM
                                        : EMenuMode.MENUMODE_LEVEL);
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_TEAM:
                                {
                                    EMenuMode MenuMode = EMenuMode.MENUMODE_LEVEL;
                                    FinishMenuMode();
                                    StartMenuMode(MenuMode);
                                    break;
                                }

                                case EMenuMode.MENUMODE_LEVEL:
                                {
                                    // There is no next menu mode. Switch to the game match screen.
                                    m_HaveToExit        = true;
                                    m_pSound.StopSong(ESong.SONG_MENU_MUSIC);
                                    m_ExitGameMode      = EGameMode.GAMEMODE_MATCH;
                                    m_ExitGameModeTime  = m_GameModeTime;
                                    break;
                                }
                            }
                            break;
                        }

                        default:
                            break;
                    }
                }
            }
            // The minimum mode duration has elapsed AND we have to exit,
            // so we have to make the last black screen
            else if (m_GameModeTime - m_ExitGameModeTime <= MENU_BLACKSCREEN_DURATION)
            {
                // nothing
            }
            // Last black screen is complete! Get out of here!
            else
            {
                // Ask for the game mode that was saved when performing the menu action
                return m_ExitGameMode;
            }

            // Stay in this game mode
            return EGameMode.GAMEMODE_MENU;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set a new menu mode. Creates the object corresponding to the new menu mode.</summary>
        private void StartMenuMode(EMenuMode MenuMode)
        {
            // Set the new menu mode
            m_MenuMode = MenuMode;

            // Create the object corresponding to the new menu mode
            switch (m_MenuMode)
            {
                case EMenuMode.MENUMODE_BOMBER: m_MenuBomber.Create(); break;
                case EMenuMode.MENUMODE_INPUT:  m_MenuInput.Create();  break;
                case EMenuMode.MENUMODE_MATCH:  m_MenuMatch.Create();  break;
                case EMenuMode.MENUMODE_TEAM:   m_MenuTeam.Create();   break;
                case EMenuMode.MENUMODE_LEVEL:  m_MenuLevel.Create();  break;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set no menu mode. Destroys the object corresponding to the current menu mode.</summary>
        private void FinishMenuMode()
        {
            // Destroy the object corresponding to the current menu mode
            switch (m_MenuMode)
            {
                case EMenuMode.MENUMODE_BOMBER: m_MenuBomber.Destroy(); break;
                case EMenuMode.MENUMODE_INPUT:  m_MenuInput.Destroy();  break;
                case EMenuMode.MENUMODE_MATCH:  m_MenuMatch.Destroy();  break;
                case EMenuMode.MENUMODE_TEAM:   m_MenuTeam.Destroy();   break;
                case EMenuMode.MENUMODE_LEVEL:  m_MenuLevel.Destroy();  break;
            }

            // Don't modify current menu mode! Leave it as it is.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetMenuMode(EMenuMode MenuMode)
        {
            // If this menu mode is not already set
            if (m_MenuMode != MenuMode)
            {
                // Set this menu mode
                FinishMenuMode();
                StartMenuMode(MenuMode);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public override void Display()
        {
            // If we have to make the first black screen
            if (m_GameModeTime <= MENU_BLACKSCREEN_DURATION)
            {
                // nothing
            }
            // If minimum duration of the mode has not elapsed OR we don't have to exit yet
            else if (!m_HaveToExit)
            {
                // Draw the scrolling tiled background
                m_pMosaic.Display();

                // Display the object corresponding to the current menu mode
                switch (m_MenuMode)
                {
                    case EMenuMode.MENUMODE_BOMBER: m_MenuBomber.Display(); break;
                    case EMenuMode.MENUMODE_INPUT:  m_MenuInput.Display();  break;
                    case EMenuMode.MENUMODE_MATCH:  m_MenuMatch.Display();  break;
                    case EMenuMode.MENUMODE_TEAM:   m_MenuTeam.Display();   break;
                    case EMenuMode.MENUMODE_LEVEL:  m_MenuLevel.Display();  break;
                }
            }
            // The minimum mode duration has elapsed AND we have to exit,
            // so we have to make the last black screen
            else if (m_GameModeTime - m_ExitGameModeTime <= MENU_BLACKSCREEN_DURATION)
            {
                // nothing
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
