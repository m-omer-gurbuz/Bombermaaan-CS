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
 *  \file CTitle.cs
 *  \brief Title screen (main screen of Bombermaaan at program start)
 */

namespace Bombermaaan
{
    /// <summary>
    /// The title screen
    /// </summary>
    public class CTitle : CModeScreen
    {
        //----------------------------------------------------------------------
        // Constants
        //----------------------------------------------------------------------

        private const float BLACKSCREEN_DURATION                    = 0.350f;

        private const int   DISPLAY_ORIGIN_X                        = 0;
        private const int   DISPLAY_ORIGIN_Y                        = 0;

        private const int   BACKGROUND_POSITION_X                   = 0;
        private const int   BACKGROUND_POSITION_Y                   = 0;
        private const int   BACKGROUND_SPRITE                       = 0;
        private const int   BACKGROUND_SPRITE_LAYER                 = 0;
        private const int   BACKGROUND_SPRITE_PRIORITY_IN_LAYER     = 0;

        private const int   BOMBERS_POSITION_X                      = 0;
        // BOMBERS_POSITION_Y uses VIEW_HEIGHT which is a runtime value; computed inline
        private const int   BOMBERS_SPRITE                          = 0;
        private const int   BOMBERS_SPRITE_LAYER                    = 0;
        private const int   BOMBERS_SPRITE_PRIORITY_IN_LAYER        = 2;

        private const int   TITLE_POSITION_X                        = 7;
        private const int   TITLE_POSITION_Y                        = 7;
        private const int   TITLE_SPRITE                            = 0;
        private const int   TITLE_SPRITE_LAYER                      = 0;
        private const int   TITLE_SPRITE_PRIORITY_IN_LAYER          = 2;

        private const int   NUMBER_OF_MENU_ITEMS                    = 6;
        private const int   FIRST_MENU_ITEM                         = 0;
        private const int   LAST_MENU_ITEM                          = NUMBER_OF_MENU_ITEMS - 1;
        private const int   FIRST_MENU_ITEM_POSITION_Y              = 180;
        private const int   ALL_MENU_ITEMS_POSITION_X               = 195;
        private const int   MENU_ITEM_SPRITE_LAYER                  = 0;
        private const int   MENU_ITEM_SPRITE_PRIORITY_IN_LAYER      = 2;
        private const int   SPACE_X_FROM_MENU_ITEM_TO_CURSOR_HAND   = -40;
        private const int   CURSOR_HAND_SPRITE                      = 0;
        private const int   CURSOR_HAND_SPRITE_LAYER                = 0;
        private const int   CURSOR_HAND_SPRITE_PRIORITY_IN_LAYER    = 2;
        private const int   SPACE_Y_BETWEEN_MENU_ITEMS              = 30;

        private const float MOVING_HAND_TIMEPERIOD                  = 0.06f;
        private const int   MOVING_HAND_DISTANCE_1                  = -20;
        private const int   MOVING_HAND_DISTANCE_2                  = -18;
        private const int   MOVING_HAND_DISTANCE_3                  = -13;
        private const int   MOVING_HAND_DISTANCE_4                  = -6;
        private const int   MOVING_HAND_DISTANCE_5                  = -2;
        private const int   MOVING_HAND_DISTANCE_6                  = 0;

        private const int   MENU_ITEM_GAME                          = 0;
        private const int   MENU_ITEM_DEMO                          = 1;
        private const int   MENU_ITEM_OPTIONS                       = 2;
        private const int   MENU_ITEM_CREDITS                       = 3;
        private const int   MENU_ITEM_HELP                          = 4;
        private const int   MENU_ITEM_EXIT                          = 5;

        private const float MAX_IDLE_TIME                           = 30.0f;

        private const bool  ENABLE_SNOW                             = false;

        //----------------------------------------------------------------------
        // Private members
        //----------------------------------------------------------------------

        private int             m_Cursor;               //!< Number of the menu item pointed by the cursor hand
        private bool            m_SongStarted;          //!< Did we start playing the song after the black screen?
        private CCloudManager   m_CloudManager;         //!< Manages the clouds in the sky
        private CSnowManager    m_SnowManager;          //!< Manages the snowflakes in the sky
        private float           m_ModeTime;             //!< Time (in seconds) that elapsed since the mode has started
        private float           m_ExitModeTime;         //!< Mode time when we have to start the last black screen
        private EGameMode       m_ExitGameMode;         //!< Game mode to ask for when exiting
        private bool            m_HaveToExit;           //!< Do we have to exit this mode?
        private int             m_MovingHandDistance;   //!< Extra distance between hand and menu
        private float           m_MovingHandTimer;      //!< Timer used for the moving hand
        private float           m_IdleTime;             //!< Time this screen is idle

        //----------------------------------------------------------------------
        // Constructor / Destructor
        //----------------------------------------------------------------------

        public CTitle() : base()
        {
            m_Cursor = FIRST_MENU_ITEM;
            m_SongStarted = false;
            m_ModeTime = 0.0f;
            m_HaveToExit = false;
            m_MovingHandDistance = 0;
            m_MovingHandTimer = 0.0f;
            m_ExitModeTime = 0.0f;
            m_ExitGameMode = EGameMode.GAMEMODE_NONE;
            m_IdleTime = 0.0f;
            m_CloudManager = new CCloudManager();
            m_SnowManager  = new CSnowManager();
        }

        //----------------------------------------------------------------------
        // SetDisplay
        //----------------------------------------------------------------------

        public override void SetDisplay(CDisplay pDisplay)
        {
            base.SetDisplay(pDisplay);
            m_CloudManager.SetDisplay(pDisplay);
            m_SnowManager.SetDisplay(pDisplay);
        }

        //----------------------------------------------------------------------
        // Create
        //----------------------------------------------------------------------

        public override void Create()
        {
            base.Create();

            m_SongStarted = false;
            m_ModeTime = 0.0f;
            m_HaveToExit = false;
            m_MovingHandDistance = 0;
            m_MovingHandTimer = 0.0f;

            // Don't initialize the cursor to the first item so we keep last cursor position.

            m_CloudManager.Create();

            if (ENABLE_SNOW)
            {
                m_SnowManager.Create();
            }
        }

        //----------------------------------------------------------------------
        // Destroy
        //----------------------------------------------------------------------

        public override void Destroy()
        {
            m_CloudManager.Destroy();

            if (ENABLE_SNOW)
            {
                m_SnowManager.Destroy();
            }

            base.Destroy();
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
            m_IdleTime += m_pTimer.GetDeltaTime();

            if (m_ModeTime <= BLACKSCREEN_DURATION)
            {
                // First black screen — do nothing
            }
            else if (!m_HaveToExit)
            {
                if (!m_SongStarted)
                {
                    m_pSound.PlaySong(ESong.SONG_TITLE_MUSIC);
                    m_SongStarted = true;
                }

                m_CloudManager.Update(m_pTimer.GetDeltaTime());

                if (ENABLE_SNOW)
                {
                    m_SnowManager.Update(m_pTimer.GetDeltaTime());
                }

                // Update the moving hand
                m_MovingHandTimer += m_pTimer.GetDeltaTime();
                if      (m_MovingHandTimer < MOVING_HAND_TIMEPERIOD)       m_MovingHandDistance = MOVING_HAND_DISTANCE_1;
                else if (m_MovingHandTimer < 2 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_2;
                else if (m_MovingHandTimer < 3 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_3;
                else if (m_MovingHandTimer < 4 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_4;
                else if (m_MovingHandTimer < 5 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_5;
                else if (m_MovingHandTimer < 6 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_6;
                else if (m_MovingHandTimer < 7 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_5;
                else if (m_MovingHandTimer < 8 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_4;
                else if (m_MovingHandTimer < 9 * MOVING_HAND_TIMEPERIOD)   m_MovingHandDistance = MOVING_HAND_DISTANCE_3;
                else if (m_MovingHandTimer < 10 * MOVING_HAND_TIMEPERIOD)  m_MovingHandDistance = MOVING_HAND_DISTANCE_2;
                else
                {
                    m_MovingHandDistance = MOVING_HAND_DISTANCE_1;
                    m_MovingHandTimer = 0.0f;
                }

                if (m_pInput.GetMainInput().TestNext())
                {
                    m_IdleTime = 0.0f;

                    switch (m_Cursor)
                    {
                        case MENU_ITEM_GAME    : m_ExitGameMode = EGameMode.GAMEMODE_MENU;     break;
                        case MENU_ITEM_DEMO    : m_ExitGameMode = EGameMode.GAMEMODE_DEMO;     break;
                        case MENU_ITEM_OPTIONS : m_ExitGameMode = EGameMode.GAMEMODE_CONTROLS; break;
                        case MENU_ITEM_CREDITS : m_ExitGameMode = EGameMode.GAMEMODE_GREETS;   break;
                        case MENU_ITEM_HELP    : m_ExitGameMode = EGameMode.GAMEMODE_HELP;     break;
                        case MENU_ITEM_EXIT    : m_ExitGameMode = EGameMode.GAMEMODE_EXIT;     break;
                    }

                    if (m_ExitGameMode != EGameMode.GAMEMODE_TITLE)
                    {
                        m_pSound.StopSong(ESong.SONG_TITLE_MUSIC);
                        m_HaveToExit = true;
                        m_ExitModeTime = m_ModeTime;
                        m_pSound.PlaySample(ESample.SAMPLE_MENU_NEXT);
                    }
                    else
                    {
                        m_pSound.PlaySample(ESample.SAMPLE_MENU_ERROR);
                    }
                }
                else if (m_pInput.GetMainInput().TestUp())
                {
                    m_IdleTime = 0.0f;
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_BEEP);
                    m_Cursor--;
                    if (m_Cursor < FIRST_MENU_ITEM)
                        m_Cursor = LAST_MENU_ITEM;
                }
                else if (m_pInput.GetMainInput().TestDown())
                {
                    m_IdleTime = 0.0f;
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_BEEP);
                    m_Cursor++;
                    if (m_Cursor > LAST_MENU_ITEM)
                        m_Cursor = FIRST_MENU_ITEM;
                }

                // If idle too long go to Demo mode automatically
                if (m_IdleTime >= MAX_IDLE_TIME)
                {
                    m_IdleTime = 0.0f;
                    m_ExitGameMode = EGameMode.GAMEMODE_DEMO;
                    m_pSound.StopSong(ESong.SONG_TITLE_MUSIC);
                    m_HaveToExit = true;
                    m_ExitModeTime = m_ModeTime;
                }
            }
            else if (m_ModeTime <= m_ExitModeTime + BLACKSCREEN_DURATION)
            {
                // Last black screen — do nothing
            }
            else
            {
                return m_ExitGameMode;
            }

            return EGameMode.GAMEMODE_TITLE;
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
            else if (!m_HaveToExit)
            {
                m_pDisplay.SetOrigin(DISPLAY_ORIGIN_X, DISPLAY_ORIGIN_Y);

                m_CloudManager.Display();

                if (ENABLE_SNOW)
                {
                    m_SnowManager.Display();
                }

                m_pDisplay.DrawSprite(BACKGROUND_POSITION_X,
                                      BACKGROUND_POSITION_Y,
                                      null, null,
                                      BmpId.BMP_TITLE_BACKGROUND,
                                      BACKGROUND_SPRITE,
                                      BACKGROUND_SPRITE_LAYER,
                                      BACKGROUND_SPRITE_PRIORITY_IN_LAYER);

                m_pDisplay.DrawSprite(BOMBERS_POSITION_X,
                                      Globals.VIEW_HEIGHT - 126,
                                      null, null,
                                      BmpId.BMP_TITLE_BOMBERS,
                                      BOMBERS_SPRITE,
                                      BOMBERS_SPRITE_LAYER,
                                      BOMBERS_SPRITE_PRIORITY_IN_LAYER);

                m_pDisplay.DrawSprite(TITLE_POSITION_X,
                                      TITLE_POSITION_Y,
                                      null, null,
                                      BmpId.BMP_TITLE_TITLE,
                                      TITLE_SPRITE,
                                      TITLE_SPRITE_LAYER,
                                      TITLE_SPRITE_PRIORITY_IN_LAYER);

                int MenuItemPositionY = FIRST_MENU_ITEM_POSITION_Y;

                for (int MenuItemIndex = 0; MenuItemIndex < NUMBER_OF_MENU_ITEMS; MenuItemIndex++)
                {
                    m_pDisplay.DrawSprite(ALL_MENU_ITEMS_POSITION_X,
                                          MenuItemPositionY,
                                          null, null,
                                          BmpId.BMP_TITLE_MENU_ITEMS,
                                          MenuItemIndex * 2 + (m_Cursor == MenuItemIndex ? 1 : 0),
                                          MENU_ITEM_SPRITE_LAYER,
                                          MENU_ITEM_SPRITE_PRIORITY_IN_LAYER);

                    if (m_Cursor == MenuItemIndex)
                    {
                        m_pDisplay.DrawSprite(ALL_MENU_ITEMS_POSITION_X + SPACE_X_FROM_MENU_ITEM_TO_CURSOR_HAND + m_MovingHandDistance,
                                              MenuItemPositionY,
                                              null, null,
                                              BmpId.BMP_MENU_HAND_TITLE,
                                              CURSOR_HAND_SPRITE,
                                              CURSOR_HAND_SPRITE_LAYER,
                                              CURSOR_HAND_SPRITE_PRIORITY_IN_LAYER);
                    }

                    MenuItemPositionY += SPACE_Y_BETWEEN_MENU_ITEMS;
                }
            }
            else if (m_ModeTime <= m_ExitModeTime + BLACKSCREEN_DURATION)
            {
                // Last black screen
            }
        }
    }
}
