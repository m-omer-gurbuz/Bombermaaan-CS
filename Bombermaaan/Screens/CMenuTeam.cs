// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/************************************************************************************

    Copyright (C) 2016 Billy Araujo
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
 *  \file CMenuTeam.cs
 *  \brief The menu where you can choose the team
 */

namespace Bombermaaan
{

    /// <summary>Change time options, start of arena closure and number of winning matches</summary>
    public class CMenuTeam : CMenuBase
    {
        private const int MENUTEAM_SPRITELAYER          = 1;       // Sprite layer where to draw sprites

        private const int TITLE_TEXT_POSITION_Y         = 90;      // Position Y of the title text that is centered on the X axis

        private const int INITIAL_TEXT_POSITION_X       = 191;     // Initial position of the text "BOMBER"
        private const int INITIAL_TEXT_POSITION_Y       = 77 + 90;
        private const int TYPE_TEXT_SPACE_X             = 75;      // X Space in pixels between the "BOMBER" text X position and the type's text X position
        private const int TEXT_SPACE_Y                  = 21;      // Y Space in pixels between each "BOMBER" text Y position

        private const int BOMBER_HEAD_SPACE_X           = -29;     // Space in pixels between the "BOMBER" text position
        private const int BOMBER_HEAD_SPACE_Y           = -7;      // and the corresponding bomber head
        private const int BOMBER_HEAD_PRIORITY          = 0;       // Priority to use when drawing the menu's bomber head sprites

        private const int CURSOR_HAND_SPACE_X           = -54;     // Space in pixels between the "BOMBER" text position
        private const int CURSOR_HAND_SPACE_Y           = -2;      // and the cursor hand pointing to the corresponding bomber head
        private const int CURSOR_HAND_SPRITE            = 0;       // Sprite number of the cursor hand in the sprite table
        private const int CURSOR_HAND_PRIORITY          = 0;       // Priority to use when drawing the menu's bomber hand sprites

        private const int TEAM_VS_TEXT_POSITION_Y       = 140;     // Position Y of the vs text that is centered on the X axis

        private const int BOMBER_NO_TEAM_COLX           = 67;      // No team (center)
        private const int BOMBER_TEAM_A_COLX            = 0;       // Column Team A
        private const int BOMBER_TEAM_B_COLX            = 134;     // Column Team B

        private const string TITLE_STRING               = "TEAM";  // String of the menu's title centered on the X axis
        private const string TEAM_VS_STRING             = "VS";    // String of a menu item centered on the X axis

        // Cursor used to remember on what player it's pointing to
        private int m_CursorPlayer;

        public CMenuTeam() : base()
        {
            m_CursorPlayer = 0;
        }

        protected override void OnCreate()
        {
            // Make the hand cursor point to the first option
            m_CursorPlayer = 0;
        }

        protected override void OnDestroy() { }

        protected override void OnUp()
        {
            // Make the cursor go up
            m_CursorPlayer--;

            // If it is now out of bounds
            if (m_CursorPlayer < 0)
            {
                // Wrap : make the cursor point to the last player
                m_CursorPlayer = COptions.MAX_PLAYERS - 1;
            }
        }

        protected override void OnDown()
        {
            // Make the cursor go down
            m_CursorPlayer++;

            // If it is now out of bounds
            if (m_CursorPlayer > COptions.MAX_PLAYERS - 1)
            {
                // Wrap : make the cursor point to the first player
                m_CursorPlayer = 0;
            }
        }

        protected override void OnLeft()
        {
            if (m_pOptions.GetBomberType(m_CursorPlayer) == EBomberType.BOMBERTYPE_OFF)
                m_pSound.PlaySample(ESample.SAMPLE_MENU_ERROR);

            if (m_pOptions.GetBomberTeam(m_CursorPlayer) == EBomberTeam.BOMBERTEAM_B)
                m_pOptions.SetBomberTeam(m_CursorPlayer, EBomberTeam.BOMBERTEAM_A);
        }

        protected override void OnRight()
        {
            if (m_pOptions.GetBomberType(m_CursorPlayer) == EBomberType.BOMBERTYPE_OFF)
                m_pSound.PlaySample(ESample.SAMPLE_MENU_ERROR);

            if (m_pOptions.GetBomberTeam(m_CursorPlayer) == EBomberTeam.BOMBERTEAM_A)
                m_pOptions.SetBomberTeam(m_CursorPlayer, EBomberTeam.BOMBERTEAM_B);
        }

        protected override void OnPrevious()
        {
            // Go to the previous screen
            Exit(EMenuAction.MENUACTION_PREVIOUS);
        }

        protected override void OnNext()
        {
            int nbPlayersTeamA = 0;
            int nbPlayersTeamB = 0;

            for (int Player = 0; Player < COptions.MAX_PLAYERS; Player++)
            {
                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_OFF)
                    continue;

                if (m_pOptions.GetBomberTeam(Player) == EBomberTeam.BOMBERTEAM_A)
                    nbPlayersTeamA++;

                if (m_pOptions.GetBomberTeam(Player) == EBomberTeam.BOMBERTEAM_B)
                    nbPlayersTeamB++;
            }

            if (nbPlayersTeamA > 0 && nbPlayersTeamB > 0)
            {
                // Play the menu next sound
                m_pSound.PlaySample(ESample.SAMPLE_MENU_NEXT);

                // Go to the next screen
                Exit(EMenuAction.MENUACTION_NEXT);
            }
            else
            {
                m_pSound.PlaySample(ESample.SAMPLE_MENU_ERROR);
            }
        }

        protected override void OnUpdate() { }

        protected override void OnDisplay()
        {
            // Set the right font text color and write the menu title string
            m_pFont.SetTextColor(EFontColor.FONTCOLOR_WHITE);
            m_pFont.DrawCenteredX(0, CDisplay.VIEW_WIDTH - 1, TITLE_TEXT_POSITION_Y, TITLE_STRING);

            m_pFont.SetTextColor(EFontColor.FONTCOLOR_GREEN);
            m_pFont.DrawCenteredX(0, CDisplay.VIEW_WIDTH - 1, TEAM_VS_TEXT_POSITION_Y, TEAM_VS_STRING);

            // Y Position where to write the text with the font object
            int PositionY = INITIAL_TEXT_POSITION_Y;

            // Scan the players
            for (int Player = 0; Player < COptions.MAX_PLAYERS; Player++)
            {
                int PositionX = 0;

                if (m_pOptions.GetBomberTeam(Player) == EBomberTeam.BOMBERTEAM_A)
                    PositionX = BOMBER_TEAM_A_COLX;
                else if (m_pOptions.GetBomberTeam(Player) == EBomberTeam.BOMBERTEAM_B)
                    PositionX = BOMBER_TEAM_B_COLX;

                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_OFF)
                    PositionX = BOMBER_NO_TEAM_COLX;

                // Draw the bomber head corresponding to the current player
                m_pDisplay.DrawSprite(INITIAL_TEXT_POSITION_X + BOMBER_HEAD_SPACE_X + PositionX,
                    PositionY + BOMBER_HEAD_SPACE_Y,
                    null,
                    null,
                    BmpId.BMP_MENU_BOMBER,
                    Player, // Blinking bomber head sprite or not
                    MENUTEAM_SPRITELAYER,
                    BOMBER_HEAD_PRIORITY);

                // If the cursor hand is pointing to the current player
                if (m_CursorPlayer == Player)
                {
                    // Draw the cursor hand sprite in front of the corresponding bomber head
                    m_pDisplay.DrawSprite(INITIAL_TEXT_POSITION_X + CURSOR_HAND_SPACE_X,
                        PositionY + CURSOR_HAND_SPACE_Y,
                        null,
                        null,
                        BmpId.BMP_MENU_HAND,
                        CURSOR_HAND_SPRITE,
                        MENUTEAM_SPRITELAYER,
                        CURSOR_HAND_PRIORITY);
                }

                // Go down
                PositionY += TEXT_SPACE_Y;
            }
        }
    }

} // namespace Bombermaaan
