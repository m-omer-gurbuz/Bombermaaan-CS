// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

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
 *  \file CMenuBomber.cs
 *  \brief The menu where you can choose the bombers
 */

using System;
namespace Bombermaaan
{

    /// <summary>Handles the menu where bombers can be set to manual/computer/off</summary>
    public class CMenuBomber : CMenuBase
    {
        private const int   MENUBOMBER_SPRITELAYER                      = 1;       // Sprite layer where to draw sprites

        private const int   TITLE_TEXT_POSITION_Y                       = 90;      // Position Y of the title text that is centered on the X axis

        private const int   INITIAL_TEXT_POSITION_X                     = 191;     // Initial position of the text "BOMBER"
        private const int   INITIAL_TEXT_POSITION_Y                     = 77 + 90;
        private const int   TYPE_TEXT_SPACE_X                           = 75;      // X Space in pixels between the "BOMBER" text X position and the type's text X position
        private const int   TEXT_SPACE_Y                                = 21;      // Y Space in pixels between each "BOMBER" text Y position

        private const int   BOMBER_HEAD_SPACE_X                         = -29;     // Space in pixels between the "BOMBER" text position
        private const int   BOMBER_HEAD_SPACE_Y                         = -7;      // and the corresponding bomber head
        private const int   BOMBER_HEAD_PRIORITY                        = 0;       // Priority to use when drawing the menu's bomber head sprites

        private const int   CURSOR_HAND_SPACE_X                         = -54;     // Space in pixels between the "BOMBER" text position
        private const int   CURSOR_HAND_SPACE_Y                         = -2;      // and the cursor hand pointing to the corresponding bomber head
        private const int   CURSOR_HAND_SPRITE_TABLE                    = 32;      // Sprite table where the menu's cursor hand sprites are contained
        private const int   CURSOR_HAND_SPRITE                          = 0;       // Sprite number of the cursor hand in the sprite table
        private const int   CURSOR_HAND_PRIORITY                        = 0;       // Priority to use when drawing the menu's bomber hand sprites

        private const string TITLE_STRING                               = "BOMBER TYPE";  // String of the menu's title centered on the X axis
        private const string BOMBER_STRING                              = "BOMBER";       // String of the text between the bomber head and the bomber type text
        private const string BOMBERTYPE_OFF_STRING                      = "OFF";          // String for the OFF bomber type
        private const string BOMBERTYPE_MAN_STRING                      = "MAN";          // String for the MAN bomber type
        private const string BOMBERTYPE_COM_STRING                      = "COM";          // String for the COM bomber type

        private const float BLINKING_TIME                               = 0.100f;   // Time (in seconds) the bomber head has to spend blinking
        private const float NOT_BLINKING_MINIMUM_TIME                   = 3.0f;     // Minimum time (in seconds) the bomber head has to spend without blinking
        private const int   NOT_BLINKING_MAXIMUM_ADDITIONAL_TIME        = 5000;     // Maximum additional time (in milliseconds)

        private static readonly Random _rng = new Random();

        // Cursor used to remember on what player it's pointing to
        private int     m_CursorPlayer;
        // Is the bomber head (given its index) currently blinking?
        private bool[]  m_Blinking  = new bool[COptions.MAX_PLAYERS];
        // Time left to wait before toggling the corresponding blink state
        private float[] m_BlinkTimer = new float[COptions.MAX_PLAYERS];

        public CMenuBomber() : base()
        {
            // Initialize the blink values
            for (int i = 0; i < COptions.MAX_PLAYERS; i++)
            {
                m_Blinking[i] = false;
                m_BlinkTimer[i] = 0.0f;
            }

            m_CursorPlayer = 0;
        }

        protected override void OnCreate()
        {
            // Make the hand cursor point to the first player
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
            // Set the previous bomber type (and wrap if necessary)
            switch (m_pOptions.GetBomberType(m_CursorPlayer))
            {
                case EBomberType.BOMBERTYPE_OFF: m_pOptions.SetBomberType(m_CursorPlayer, EBomberType.BOMBERTYPE_COM); break;
                case EBomberType.BOMBERTYPE_MAN: m_pOptions.SetBomberType(m_CursorPlayer, EBomberType.BOMBERTYPE_OFF); break;
                case EBomberType.BOMBERTYPE_COM: m_pOptions.SetBomberType(m_CursorPlayer, EBomberType.BOMBERTYPE_MAN); break;
                default:                                                                                                break;
            }
        }

        protected override void OnRight()
        {
            // Set the next bomber type (and wrap if necessary)
            switch (m_pOptions.GetBomberType(m_CursorPlayer))
            {
                case EBomberType.BOMBERTYPE_OFF: m_pOptions.SetBomberType(m_CursorPlayer, EBomberType.BOMBERTYPE_MAN); break;
                case EBomberType.BOMBERTYPE_MAN: m_pOptions.SetBomberType(m_CursorPlayer, EBomberType.BOMBERTYPE_COM); break;
                case EBomberType.BOMBERTYPE_COM: m_pOptions.SetBomberType(m_CursorPlayer, EBomberType.BOMBERTYPE_OFF); break;
                default:                                                                                                break;
            }
        }

        protected override void OnPrevious()
        {
            // Go to the previous menu mode
            Exit(EMenuAction.MENUACTION_PREVIOUS);
        }

        protected override void OnNext()
        {
            // Variables used to count human and computer players
            int ManCount = 0;
            int ComCount = 0;

            // Scan the players
            for (int Player = 0; Player < COptions.MAX_PLAYERS; Player++)
            {
                // If this player is a human player
                if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_MAN)
                {
                    // Increase the number of human players
                    ManCount++;
                }
                // If this player is a computer player
                else if (m_pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_COM)
                {
                    // Increase the number of computer players
                    ComCount++;
                }
            }

            // If there are at least two real players
            if (ManCount + ComCount >= 2)
            {
                // Play the menu next sound
                m_pSound.PlaySample(ESample.SAMPLE_MENU_NEXT);

                // The choices in this menu are correct, we can now exit this menu mode
                Exit(EMenuAction.MENUACTION_NEXT);
            }
            // If there are not enough real players (less than 2)
            else
            {
                // Play the menu error sound
                m_pSound.PlaySample(ESample.SAMPLE_MENU_ERROR);
            }
        }

        protected override void OnUpdate()
        {
            // Scan the bomber heads
            for (int i = 0; i < COptions.MAX_PLAYERS; i++)
            {
                // Decrease the time left before the blink state for this bomber head changes
                m_BlinkTimer[i] -= m_pTimer.GetDeltaTime();

                // If the blink state for this bomber head has to change
                if (m_BlinkTimer[i] <= 0.0f)
                {
                    // Toggle the blink state
                    m_Blinking[i] = !m_Blinking[i];

                    // Set the blink time left again
                    // If the bomber head is blinking
                    if (m_Blinking[i])
                    {
                        // Set a short time
                        m_BlinkTimer[i] = BLINKING_TIME;
                    }
                    // If the bomber head is not blinking
                    else
                    {
                        // Set a long random time
                        m_BlinkTimer[i] = NOT_BLINKING_MINIMUM_TIME + (float)_rng.Next(NOT_BLINKING_MAXIMUM_ADDITIONAL_TIME) / 1000.0f;
                    }
                }
            }
        }

        protected override void OnDisplay()
        {
            // Set the right font text color and write the menu title string
            m_pFont.SetTextColor(EFontColor.FONTCOLOR_WHITE);
            m_pFont.DrawCenteredX(0, CDisplay.VIEW_WIDTH - 1, TITLE_TEXT_POSITION_Y, TITLE_STRING);

            // Y Position where to write the text with the font object
            int PositionY = INITIAL_TEXT_POSITION_Y;

            // Scan the players
            for (int Player = 0; Player < COptions.MAX_PLAYERS; Player++)
            {
                // Set the right font text color and write the bomber string
                m_pFont.SetTextColor(EFontColor.FONTCOLOR_GREEN);
                m_pFont.Draw(INITIAL_TEXT_POSITION_X, PositionY, BOMBER_STRING);

                // Write a different bomber type string according to the
                // bomber type of the current player in the options object
                switch (m_pOptions.GetBomberType(Player))
                {
                    case EBomberType.BOMBERTYPE_OFF:
                    {
                        // Set the font text color and write the bomber type string
                        m_pFont.SetTextColor(EFontColor.FONTCOLOR_YELLOW);
                        m_pFont.Draw(INITIAL_TEXT_POSITION_X + TYPE_TEXT_SPACE_X, PositionY, BOMBERTYPE_OFF_STRING);
                        break;
                    }

                    case EBomberType.BOMBERTYPE_MAN:
                    {
                        // Set the font text color and write the bomber type string
                        m_pFont.SetTextColor(EFontColor.FONTCOLOR_BLUE);
                        m_pFont.Draw(INITIAL_TEXT_POSITION_X + TYPE_TEXT_SPACE_X, PositionY, BOMBERTYPE_MAN_STRING);
                        break;
                    }

                    case EBomberType.BOMBERTYPE_COM:
                    {
                        // Set the font text color and write the bomber type string
                        m_pFont.SetTextColor(EFontColor.FONTCOLOR_RED);
                        m_pFont.Draw(INITIAL_TEXT_POSITION_X + TYPE_TEXT_SPACE_X, PositionY, BOMBERTYPE_COM_STRING);
                        break;
                    }
                    default:
                        break;
                }

                // Draw the bomber head corresponding to the current player
                m_pDisplay.DrawSprite(INITIAL_TEXT_POSITION_X + BOMBER_HEAD_SPACE_X,
                                       PositionY + BOMBER_HEAD_SPACE_Y,
                                       null,
                                       null,
                                       BmpId.BMP_MENU_BOMBER,
                                       Player + (m_Blinking[Player] ? COptions.MAX_PLAYERS : 0), // Blinking bomber head sprite or not
                                       MENUBOMBER_SPRITELAYER,
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
                                           MENUBOMBER_SPRITELAYER,
                                           CURSOR_HAND_PRIORITY);
                }

                // Go down
                PositionY += TEXT_SPACE_Y;
            }
        }
    }

} // namespace Bombermaaan
