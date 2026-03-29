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
 *  \file CHelp.cs
 *  \brief The help screen
 */

namespace Bombermaaan 
{

    public class CHelp : CModeScreen
    {
        // Duration (in seconds) of the two black screens
        private const float HELP_BLACKSCREEN_DURATION          = 0.750f;

        // Display origin of the screen
        private const int HELP_DISPLAY_ORIGIN_X                = 0;
        private const int HELP_DISPLAY_ORIGIN_Y                = 0;

        // Sprite layers
        private const int HELP_TEXT_SPRITE_LAYER               = 1;
        private const int HELP_CURSOR_HAND_SPRITE_LAYER        = 1;

        private const int FIRST_MENU_ITEM                      = 0;

        private const int SCREEN_TITLE_POSITION_Y              = 20 + 80;
        private const int SCREEN_TEXT_POSITION_X               = 80;

        private const string SCREEN_HELP_TITLE_STRING          = "HELP";

        private const int ITEM_BOMB                            = 8;
        private const int ITEM_FLAME                           = 9;
        private const int ITEM_KICK                            = 10;
        private const int ITEM_ROLLER                          = 11;
        private const int ITEM_SKULL                           = 12;
        private const int ITEM_THROW                           = 13;
        private const int ITEM_PUNCH                           = 14;
        private const int ITEM_REMOTES                         = 15;
        private const int ITEM_SHIELD                          = 16;
        private const int ITEM_STRONGWEAK                      = 17;

        private const int FRAME_POSITION_X                     = 30;
        private const int FRAME_POSITION_Y                     = 52;
        private const int FRAME_SPRITE                         = 0;
        private const int FRAME_PRIORITY                       = 1;
        private const int FRAME_SPRITELAYER                    = 0;

        private const int   MOSAIC_SPRITE_LAYER                = 0;
        private const int   MOSAIC_SPRITE_PRIORITY_IN_LAYER    = 0;
        private const float MOSAIC_SPEED_X                     = 50.0f;
        private const float MOSAIC_SPEED_Y                     = -50.0f;

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

        public CHelp() : base()
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

        // Before using a CHelp, you must create it.
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
            m_Font.SetSpriteLayer(HELP_TEXT_SPRITE_LAYER);

            // The cursor points to the first menu item
            m_Cursor = FIRST_MENU_ITEM;

            // Make a random green mosaic object
            m_pMosaic = CRandomMosaic.CreateRandomMosaic(m_pDisplay,
                                                          MOSAIC_SPRITE_LAYER,
                                                          MOSAIC_SPRITE_PRIORITY_IN_LAYER,
                                                          MOSAIC_SPEED_X,
                                                          MOSAIC_SPEED_Y,
                                                          EMosaicColor.MOSAICCOLOR_GREEN,
                                                          EMosaicType.MOSAICTYPE_BOMB);
        }

        // When a CHelp is not needed anymore, you should destroy it
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

        // This updates the help screen.
        // There are 3 parts in this screen :
        // - a black screen
        // - an update of the winner screen (animations, display...)
        //   until an action control is pressed (input is only checked after a minimum duration of the mode)
        // - a black screen
        // It finally returns the game mode that should be set in the parent CGame object.
        // When the screen should continue, it returns GAMEMODE_HELP to keep this mode.
        // When the screen has ended, it returns GAMEMODE_TITLE to start the title screen.
        public override EGameMode Update()
        {
            // Increase elapsed time since mode has started
            m_ModeTime += m_pTimer.GetDeltaTime();

            // If we have to make the first black screen
            if (m_ModeTime <= HELP_BLACKSCREEN_DURATION)
            {
            }
            // If we don't have to exit yet
            else if (!m_HaveToExit)
            {
                // If we didn't start playing the song yet
                if (!m_SongStarted)
                {
                    // Start playing the help song
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
            else if (m_ModeTime - m_ExitModeTime <= HELP_BLACKSCREEN_DURATION) { }
            // Last black screen is complete! Get out of here!
            else
            {
                return EGameMode.GAMEMODE_TITLE;
            }

            // Stay in this game mode
            return EGameMode.GAMEMODE_HELP;
        }

        public override void Display()
        {
            // If we have to make the first black screen
            if (m_ModeTime <= HELP_BLACKSCREEN_DURATION) { }
            // If we don't have to exit yet
            else if (!m_HaveToExit)
            {
                // Set the position from which to display sprites
                m_pDisplay.SetOrigin(HELP_DISPLAY_ORIGIN_X, HELP_DISPLAY_ORIGIN_Y);

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
                m_Font.DrawCenteredX(0, CDisplay.VIEW_WIDTH, SCREEN_TITLE_POSITION_Y, SCREEN_HELP_TITLE_STRING);

                // Bomb help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 60,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_BOMB,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 60, "Increments number of bombs");

                // Flame help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 80,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_FLAME,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 80, "Increments flame size");

                // Roller help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 100,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_ROLLER,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 100, "Increses walk speed");

                // Kick help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 120,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_KICK,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 120, "Ability to kick bombs");

                // Throw help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 140,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_THROW,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 140, "Ability to throw bombs");

                // Punch help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 160,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_PUNCH,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 160, "Ability to punch bombs");

                // Remote help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 180,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_REMOTES,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 180, "Remote detonate");

                // Shield help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 200,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_SHIELD,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 200, "Shields one bomb blast");

                // Strong/weak help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 220,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_STRONGWEAK,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 220, "Strong/weak item");

                // Skull help
                m_pDisplay.DrawSprite(SCREEN_TEXT_POSITION_X,
                    SCREEN_TITLE_POSITION_Y + 240,
                    null, null,
                    BmpId.BMP_LEVEL_MINI_TILES,
                    ITEM_SKULL,
                    2, 0);

                m_Font.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                m_Font.Draw(SCREEN_TEXT_POSITION_X + 30, SCREEN_TITLE_POSITION_Y + 240, "Sickness");
            }
            // We have to exit, so we have to make the last black screen
            else if (m_ModeTime - m_ExitModeTime <= HELP_BLACKSCREEN_DURATION)
            {
            }
        }
    }

} // namespace Bombermaaan
