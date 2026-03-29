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
 *  \file CCredits.cs
 *  \brief The credits screen
 */

namespace Bombermaaan
{

    public class CCredits : CModeScreen
    {
        // Duration (in seconds) of the two black screens
        private const float CREDITS_BLACKSCREEN_DURATION         = 0.750f;

        // Display origin of the screen
        private const int CREDITS_DISPLAY_ORIGIN_X               = 0;
        private const int CREDITS_DISPLAY_ORIGIN_Y               = 0;

        // Sprite layers
        private const int CREDITS_TEXT_SPRITE_LAYER              = 1;
        private const int CREDITS_CURSOR_HAND_SPRITE_LAYER       = 1;

        private const int FIRST_MENU_ITEM                        = 0;

        private const int SCREEN_TITLE_POSITION_Y                = 20 + 80;

        private const string SCREEN_CREDITS_TITLE_STRING         = "CREDITS";

        private const int FRAME_POSITION_X                       = 30;
        private const int FRAME_POSITION_Y                       = 52;
        private const int FRAME_SPRITE                           = 0;
        private const int FRAME_PRIORITY                         = 1;
        private const int FRAME_SPRITELAYER                      = 0;

        private const int   MOSAIC_SPRITE_LAYER                  = 0;
        private const int   MOSAIC_SPRITE_PRIORITY_IN_LAYER      = 0;
        private const float MOSAIC_SPEED_X                       = 50.0f;
        private const float MOSAIC_SPEED_Y                       = -50.0f;

        // Time (in seconds) that elapsed since the mode has started
        private float   m_ModeTime;
        // Do we have to exit this mode?
        private bool    m_HaveToExit;
        // Mode time when we realized we have to exit (used for blackscreen)
        private float   m_ExitModeTime;
        // Font object used to draw strings
        private CFont   m_Font = new CFont();
        // Number of the menu item the cursor hand is pointing to
        private int     m_Cursor;
        // Did we start playing the song after the black screen?
        private bool    m_SongStarted;
        private CMosaic m_pMosaic;

        public CCredits() : base()
        {
            m_HaveToExit = false;
            m_ModeTime = 0.0f;
            m_ExitModeTime = 0.0f;
            m_Cursor = 0;
            m_SongStarted = false;
            m_pMosaic = null;
        }

        public new void SetDisplay(CDisplay pDisplay)
        {
            base.SetDisplay(pDisplay);
            m_Font.SetDisplay(m_pDisplay);
        }

        // Before using a CCredits, you must create it.
        public override void Create()
        {
            base.Create();

            // Reset mode time (no time has been elapsed in this mode yet)
            m_ModeTime = 0.0f;

            // Don't have to exit this mode yet
            m_HaveToExit = false;

            // We didn't start playing the song yet
            m_SongStarted = false;

            // Initialize the font
            m_Font.Create();
            m_Font.SetShadow(false);
            m_Font.SetSpriteLayer(CREDITS_TEXT_SPRITE_LAYER);

            // The cursor points to the first menu item
            m_Cursor = FIRST_MENU_ITEM;

            // Make a random red mosaic object
            m_pMosaic = CRandomMosaic.CreateRandomMosaic(m_pDisplay,
                                                          MOSAIC_SPRITE_LAYER,
                                                          MOSAIC_SPRITE_PRIORITY_IN_LAYER,
                                                          MOSAIC_SPEED_X,
                                                          MOSAIC_SPEED_Y,
                                                          EMosaicColor.MOSAICCOLOR_RED,
                                                          EMosaicType.MOSAICTYPE_BOMB);
        }

        // When a CCredits is not needed anymore, you should destroy it
        public override void Destroy()
        {
            base.Destroy();

            // Uninitialize the font
            m_Font.Destroy();

            // Delete the scrolling mosaic background
            m_pMosaic.Destroy();
            m_pMosaic = null;
        }

        public override void OpenInput() { }

        public override void CloseInput() { }

        // This updates the credits screen.
        // There are 3 parts in this screen :
        // - a black screen
        // - an update of the winner screen (animations, display...)
        //   until an action control is pressed (input is only checked after a minimum duration of the mode)
        // - a black screen
        // It finally returns the game mode that should be set in the parent CGame object.
        // When the screen should continue, it returns GAMEMODE_GREETS to keep this mode.
        // When the screen has ended, it returns GAMEMODE_TITLE to start the title screen.
        public override EGameMode Update()
        {
            // Increase elapsed time since mode has started
            m_ModeTime += m_pTimer.GetDeltaTime();

            // If we have to make the first black screen
            if (m_ModeTime <= CREDITS_BLACKSCREEN_DURATION) { }
            // If we don't have to exit yet
            else if (!m_HaveToExit)
            {
                // If we didn't start playing the song yet
                if (!m_SongStarted)
                {
                    // Start playing the credits song
                    m_pSound.PlaySong(ESong.SONG_GREET_MUSIC);

                    // We started playing the song
                    m_SongStarted = true;
                }

                // Update the scrolling mosaic background
                m_pMosaic.Update(m_pTimer.GetDeltaTime());

                // If the ESCAPE control is active
                if (m_pInput.GetMainInput().TestBreak())
                {
                    // Stop playing the song
                    m_pSound.StopSong(ESong.SONG_GREET_MUSIC);

                    // Remember we have to exit this mode
                    m_HaveToExit = true;

                    // Remember the mode time
                    m_ExitModeTime = m_ModeTime;
                }
            }
            // We have to exit, so we have to make the last black screen
            else if (m_ModeTime - m_ExitModeTime <= CREDITS_BLACKSCREEN_DURATION) { }
            // Last black screen is complete! Get out of here!
            else
            {
                return EGameMode.GAMEMODE_TITLE;
            }

            // Stay in this game mode
            return EGameMode.GAMEMODE_GREETS;
        }

        public override void Display()
        {
            // If we have to make the first black screen
            if (m_ModeTime <= CREDITS_BLACKSCREEN_DURATION) { }
            // If we don't have to exit yet
            else if (!m_HaveToExit)
            {
                // Set the position from which to display sprites
                m_pDisplay.SetOrigin(CREDITS_DISPLAY_ORIGIN_X, CREDITS_DISPLAY_ORIGIN_Y);

                // Display the scrolling mosaic background
                m_pMosaic.Display();

                // Draw the menu frame sprite
                m_pDisplay.DrawSprite(FRAME_POSITION_X,
                    FRAME_POSITION_Y,
                    null,
                    null,
                    BmpId.BMP_MENU_FRAME_1,
                    FRAME_SPRITE,
                    FRAME_SPRITELAYER,
                    FRAME_PRIORITY);

                // Draw the title of the screen
                m_Font.SetTextColor(EFontColor.FONTCOLOR_WHITE);
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y, SCREEN_CREDITS_TITLE_STRING);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y + 60,  "Thibaut Tollemer");
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y + 80,  "Bernd Arnold");
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y + 100, "Jerome Bigot");
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y + 120, "Markus Drescher");
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y + 140, "Billy Araujo");
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y + 160, "Omer Gurbuz");
            }
            // We have to exit, so we have to make the last black screen
            else if (m_ModeTime - m_ExitModeTime <= CREDITS_BLACKSCREEN_DURATION) { }
        }
    }

} // namespace Bombermaaan
