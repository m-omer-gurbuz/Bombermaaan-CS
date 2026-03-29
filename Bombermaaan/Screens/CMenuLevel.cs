// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2008 Markus Drescher
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
 *  \file CMenuLevel.cs
 *  \brief The menu where you can choose a level
 */

namespace Bombermaaan
{

    /// <summary>Shows a level with a mini layout picture and lets the user navigate through the different levels</summary>
    public class CMenuLevel : CMenuBase
    {
        private const int MENUMATCH_SPRITELAYER                 = 1;    // Sprite layer where to draw sprites

        private const int TITLE_TEXT_POSITION_Y                 = 90;   // Position Y of the title text that is centered on the X axis
        private const int WARNING_TEXT_POSITION_Y               = 350;  // Position Y of a warning text that is centered on the X axis

        private const int MINI_ARENA_POSITION_X                 = 120;
        private const int MINI_ARENA_POSITION_Y                 = 73 + 60;
        private const int TILE_POSITION_TO_BOMBER_POSITION      = -4;
        private const int MINI_ARENA_TILE_SIZE                  = 16;

        public CMenuLevel() : base() { }

        protected override void OnCreate() { }

        protected override void OnDestroy() { }

        protected override void OnUp() { }

        protected override void OnDown() { }

        protected override void OnLeft()
        {
            // If the first level is selected
            if (m_pOptions.GetLevel() == 0)
            {
                // Select the last level
                m_pOptions.SetLevel(m_pOptions.GetNumberOfLevels() - 1);
            }
            // If the first level is not selected
            else
            {
                // Select the previous level
                m_pOptions.SetLevel(m_pOptions.GetLevel() - 1);
            }
        }

        protected override void OnRight()
        {
            // If the last level is selected
            if (m_pOptions.GetLevel() == m_pOptions.GetNumberOfLevels() - 1)
            {
                // Select the first level
                m_pOptions.SetLevel(0);
            }
            // If the last level is not selected
            else
            {
                // Select the next level
                m_pOptions.SetLevel(m_pOptions.GetLevel() + 1);
            }
        }

        protected override void OnPrevious()
        {
            // Go to the previous screen
            Exit(EMenuAction.MENUACTION_PREVIOUS);
        }

        protected override void OnNext()
        {
            // Play the menu next sound
            m_pSound.PlaySample(ESample.SAMPLE_MENU_NEXT);

            // Go to the next screen
            Exit(EMenuAction.MENUACTION_NEXT);
        }

        protected override void OnUpdate() { }

        protected override void OnDisplay()
        {
            // Set the right font text color and write the menu title string
            m_pFont.SetTextColor(EFontColor.FONTCOLOR_WHITE);
            m_pFont.DrawCenteredX(0, CDisplay.VIEW_WIDTH - 1, TITLE_TEXT_POSITION_Y, m_pOptions.GetLevelName());

            bool[] StartPointAvailable = new bool[COptions.MAX_PLAYERS];
            for (int Player = 0; Player < COptions.MAX_PLAYERS; Player++)
                StartPointAvailable[Player] = false;

            // Scan all the blocks of the arena
            for (int X = 0; X < Globals.ARENA_WIDTH; X++)
            {
                for (int Y = 0; Y < Globals.ARENA_HEIGHT; Y++)
                {
                    EBlockType BlockType = m_pOptions.GetBlockType(X, Y);

                    if (BlockType == EBlockType.BLOCKTYPE_HARDWALL)
                    {
                        m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE,
                            MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE,
                            null, null,
                            BmpId.BMP_LEVEL_MINI_TILES,
                            0, 1, 1);
                    }
                    else if (BlockType == EBlockType.BLOCKTYPE_SOFTWALL || BlockType == EBlockType.BLOCKTYPE_RANDOM)
                    {
                        m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE,
                            MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE,
                            null, null,
                            BmpId.BMP_LEVEL_MINI_TILES,
                            1, 1, 1);
                    }
                    else
                    {
                        bool Shadow = (Y - 1 >= 0 &&
                            (m_pOptions.GetBlockType(X, Y - 1) == EBlockType.BLOCKTYPE_HARDWALL ||
                             m_pOptions.GetBlockType(X, Y - 1) == EBlockType.BLOCKTYPE_SOFTWALL ||
                             m_pOptions.GetBlockType(X, Y - 1) == EBlockType.BLOCKTYPE_RANDOM));

                        m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE,
                            MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE,
                            null, null,
                            BmpId.BMP_LEVEL_MINI_TILES,
                            (Shadow ? 3 : 2),
                            1, 0);

                        int spriteNumberAction = -1;

                        switch (m_pOptions.GetBlockType(X, Y))
                        {
                            case EBlockType.BLOCKTYPE_MOVEBOMB_RIGHT:   spriteNumberAction = 4;  break;
                            case EBlockType.BLOCKTYPE_MOVEBOMB_DOWN:    spriteNumberAction = 5;  break;
                            case EBlockType.BLOCKTYPE_MOVEBOMB_LEFT:    spriteNumberAction = 6;  break;
                            case EBlockType.BLOCKTYPE_MOVEBOMB_UP:      spriteNumberAction = 7;  break;
                            case EBlockType.BLOCKTYPE_ITEM_BOMB:        spriteNumberAction = 8;  break;
                            case EBlockType.BLOCKTYPE_ITEM_FLAME:       spriteNumberAction = 9;  break;
                            case EBlockType.BLOCKTYPE_ITEM_KICK:        spriteNumberAction = 10; break;
                            case EBlockType.BLOCKTYPE_ITEM_ROLLER:      spriteNumberAction = 11; break;
                            case EBlockType.BLOCKTYPE_ITEM_SKULL:       spriteNumberAction = 12; break;
                            case EBlockType.BLOCKTYPE_ITEM_THROW:       spriteNumberAction = 13; break;
                            case EBlockType.BLOCKTYPE_ITEM_PUNCH:       spriteNumberAction = 14; break;
                            case EBlockType.BLOCKTYPE_ITEM_REMOTES:     spriteNumberAction = 15; break;
                            case EBlockType.BLOCKTYPE_ITEM_SHIELD:      spriteNumberAction = 16; break;
                            case EBlockType.BLOCKTYPE_ITEM_STRONGWEAK:  spriteNumberAction = 17; break;
                            default: break;
                        }

                        if (spriteNumberAction != -1)
                        {
                            m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE,
                                MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE,
                                null, null,
                                BmpId.BMP_LEVEL_MINI_TILES,
                                spriteNumberAction,
                                2, 0);
                        }

                        switch (BlockType)
                        {
                            case EBlockType.BLOCKTYPE_WHITEBOMBER:
                                m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    null, null, BmpId.BMP_LEVEL_MINI_BOMBERS, 0, 1, 2);
                                StartPointAvailable[0] = true;
                                break;
                            case EBlockType.BLOCKTYPE_BLACKBOMBER:
                                m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    null, null, BmpId.BMP_LEVEL_MINI_BOMBERS, 1, 1, 2);
                                StartPointAvailable[1] = true;
                                break;
                            case EBlockType.BLOCKTYPE_REDBOMBER:
                                m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    null, null, BmpId.BMP_LEVEL_MINI_BOMBERS, 2, 1, 2);
                                StartPointAvailable[2] = true;
                                break;
                            case EBlockType.BLOCKTYPE_BLUEBOMBER:
                                m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    null, null, BmpId.BMP_LEVEL_MINI_BOMBERS, 3, 1, 2);
                                StartPointAvailable[3] = true;
                                break;
                            case EBlockType.BLOCKTYPE_GREENBOMBER:
                                m_pDisplay.DrawSprite(MINI_ARENA_POSITION_X + X * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    MINI_ARENA_POSITION_Y + Y * MINI_ARENA_TILE_SIZE + TILE_POSITION_TO_BOMBER_POSITION,
                                    null, null, BmpId.BMP_LEVEL_MINI_BOMBERS, 4, 1, 2);
                                StartPointAvailable[4] = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // show warning if starting points are missing
            bool warningShown = false;
            for (int Player = 0; Player < COptions.MAX_PLAYERS; Player++)
            {
                if (!StartPointAvailable[Player] && m_pOptions.GetBomberType(Player) != EBomberType.BOMBERTYPE_OFF)
                {
                    m_pFont.SetTextColor(EFontColor.FONTCOLOR_RED);
                    if (!warningShown)
                    {
                        m_pFont.Draw(MINI_ARENA_POSITION_X / 2, WARNING_TEXT_POSITION_Y, "NO START POS:");
                        warningShown = true;
                    }
                    m_pDisplay.DrawSprite(
                        CDisplay.VIEW_WIDTH - MINI_ARENA_POSITION_X - (COptions.MAX_PLAYERS - Player) * (MINI_ARENA_TILE_SIZE - TILE_POSITION_TO_BOMBER_POSITION * 2) + TILE_POSITION_TO_BOMBER_POSITION,
                        WARNING_TEXT_POSITION_Y + TILE_POSITION_TO_BOMBER_POSITION,
                        null, null, BmpId.BMP_LEVEL_MINI_BOMBERS, Player, 1, 2);
                }
            }
        }
    }

} // namespace Bombermaaan
