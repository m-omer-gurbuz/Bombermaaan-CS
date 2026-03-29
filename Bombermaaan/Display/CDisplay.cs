/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008 Bernd Arnold
    Copyright (C) 2008 Jerome Bigot
    Copyright (C) 2008 Markus Drescher
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
 *  \file CDisplay.cs
 *  \brief Load and display sprites, change screen mode (C# port)
 */

using System;

namespace Bombermaaan
{
    // -----------------------------------------------------------------------

    // -----------------------------------------------------------------------
    // BMP resource IDs  (mirrors trunk/res/res.h)
    // -----------------------------------------------------------------------

    /// <summary>Bitmap resource identifiers used when loading sprite tables.</summary>
    public static class BmpId
    {
        public const int BMP_GREEN_BACKGROUND_SOLID   = 1001;
        public const int BMP_BLUE_BACKGROUND_SOLID    = 1002;
        public const int BMP_PURPLE_BACKGROUND_SOLID  = 1003;
        public const int BMP_RED_BACKGROUND_SOLID     = 1004;
        public const int BMP_GREEN_BACKGROUND_BOMB    = 1005;
        public const int BMP_BLUE_BACKGROUND_BOMB     = 1006;
        public const int BMP_PURPLE_BACKGROUND_BOMB   = 1007;
        public const int BMP_RED_BACKGROUND_BOMB      = 1008;
        public const int BMP_GREEN_BACKGROUND_CHAR    = 1068;
        public const int BMP_BLUE_BACKGROUND_CHAR     = 1069;
        public const int BMP_PURPLE_BACKGROUND_CHAR   = 1070;
        public const int BMP_RED_BACKGROUND_CHAR      = 1071;
        public const int BMP_GREEN_BACKGROUND_FLAME   = 1072;
        public const int BMP_BLUE_BACKGROUND_FLAME    = 1073;
        public const int BMP_PURPLE_BACKGROUND_FLAME  = 1074;
        public const int BMP_RED_BACKGROUND_FLAME     = 1075;

        public const int BMP_ARENA_FIRE               = 1010;
        public const int BMP_ARENA_FLAME              = 1011;
        public const int BMP_ARENA_FLOOR              = 1012;
        public const int BMP_ARENA_FLY                = 1013;
        public const int BMP_ARENA_ITEM               = 1014;
        public const int BMP_ARENA_BOMB               = 1015;
        public const int BMP_DRAWGAME_FLAG            = 1016;
        public const int BMP_BOARD_BACKGROUND         = 1017;
        public const int BMP_BOARD_CLOCK_BOTTOM       = 1018;
        public const int BMP_BOARD_CLOCK_TOP          = 1019;
        public const int BMP_BOARD_HEADS              = 1020;
        public const int BMP_BOARD_SCORE              = 1021;
        public const int BMP_BOARD_TIME               = 1022;
        public const int BMP_ARENA_WALL               = 1023;
        public const int BMP_DRAWGAME_MAIN            = 1024;
        public const int BMP_GLOBAL_FONT              = 1025;
        public const int BMP_VICTORY_BOMBER           = 1026;
        public const int BMP_VICTORY_CROWD            = 1027;
        public const int BMP_VICTORY_TITLE            = 1028;
        public const int BMP_VICTORY_WALL             = 1029;
        public const int BMP_DRAWGAME_FUMES           = 1030;
        public const int BMP_WINNER_TITLE             = 1031;
        public const int BMP_WINNER_COIN              = 1032;
        public const int BMP_WINNER_LIGHTS            = 1033;
        public const int BMP_WINNER_SPARKS            = 1034;
        public const int BMP_WINNER_BOMBER            = 1035;
        public const int BMP_MENU_FRAME_1             = 1036;
        public const int BMP_MENU_BOMBER              = 1037;
        public const int BMP_MENU_HAND                = 1038;
        public const int BMP_WINNER_CROSS             = 1039;
        public const int BMP_VICTORY_CONFETTIS_LARGE  = 1040;
        public const int BMP_VICTORY_CONFETTIS_MEDIUM = 1041;
        public const int BMP_VICTORY_CONFETTIS_SMALL  = 1042;
        public const int BMP_PAUSE                    = 1043;
        public const int BMP_HURRY                    = 1044;
        public const int BMP_MENU_FRAME_2             = 1045;
        public const int BMP_ARENA_FUMES              = 1046;
        public const int BMP_BOARD_DRAWGAME           = 1047;
        public const int BMP_TITLE_BACKGROUND         = 1048;
        public const int BMP_TITLE_BOMBERS            = 1049;
        public const int BMP_TITLE_MENU_ITEMS         = 1050;
        public const int BMP_TITLE_TITLE              = 1051;
        public const int BMP_TITLE_CLOUD_1            = 1052;
        public const int BMP_TITLE_CLOUD_2            = 1053;
        public const int BMP_TITLE_CLOUD_3            = 1054;
        public const int BMP_LEVEL_MINI_BOMBERS       = 1055;
        public const int BMP_LEVEL_MINI_TILES         = 1056;
        public const int BMP_ARENA_BOMBER_DEATH       = 1057;
        public const int BMP_ARENA_BOMBER_LIFT        = 1058;
        public const int BMP_ARENA_BOMBER_THROW       = 1059;
        public const int BMP_ARENA_BOMBER_WALK        = 1060;
        public const int BMP_ARENA_BOMBER_WALK_HOLD   = 1061;
        public const int BMP_ARENA_BOMBER_PUNCH       = 1062;
        public const int BMP_ARENA_BOMBER_STUNT       = 1063;
        public const int BMP_ARENA_ARROWS             = 1064;
        public const int BMP_MENU_HAND_TITLE          = 1065;
        public const int BMP_ARENA_REMOTE_BOMB        = 1066;
        public const int BMP_TITLE_SNOWFLAKE          = 1067;
    }

    // -----------------------------------------------------------------------
    // Game-layout constants  (mirrors StdAfx.h)
    // -----------------------------------------------------------------------

    internal static class GameConst
    {
        public const int ARENA_WIDTH  = 15;
        public const int ARENA_HEIGHT = 13;
        public const int BLOCK_SIZE   = 32;
        public const int VIEW_WIDTH   = ARENA_WIDTH  * BLOCK_SIZE;           // 480
        public const int VIEW_HEIGHT  = 26 + ARENA_HEIGHT * BLOCK_SIZE;      // 442
        public const int MAX_PLAYERS  = 5;
    }

    // -----------------------------------------------------------------------
    // CDisplay
    // -----------------------------------------------------------------------

    /// <summary>
    /// High-level display manager.  Wraps CVideoSDL and owns the sprite tables.
    /// The DIRECTX code path has been removed; only the SDL path is kept.
    /// </summary>
    public class CDisplay
    {
        // ---- constants forwarded from other classes for convenience --------

        public const int PRIORITY_UNUSED = -1;
        public const int VIEW_WIDTH      = GameConst.VIEW_WIDTH;
        public const int VIEW_HEIGHT     = GameConst.VIEW_HEIGHT;
        public const int BMP_PAUSE       = BmpId.BMP_PAUSE;
        public const int BMP_HURRY       = BmpId.BMP_HURRY;

        // ---- private state ------------------------------------------------

        private IntPtr    m_hModule;       ///< Module handle (reserved for future resource loading)
        private CVideoSDL m_VideoSDL;      ///< SDL video back-end
        private int       m_ViewOriginX;   ///< Top-left offset of the game view within the window
        private int       m_ViewOriginY;

        // ---- construction -------------------------------------------------

        public CDisplay()
        {
            m_hModule     = IntPtr.Zero;
            m_VideoSDL    = new CVideoSDL();
            m_ViewOriginX = 0;
            m_ViewOriginY = 0;
        }

        // ---- property accessors -------------------------------------------

        public void SetWindowHandle(IntPtr hWnd)
        {
            m_VideoSDL.SetWindowHandle(hWnd);
        }

        public void SetModuleHandle(IntPtr hModule)
        {
            m_hModule = hModule;
        }

        public CVideoSDL GetSDLVideo()
        {
            return m_VideoSDL;
        }

        // ---- Origin -------------------------------------------------------

        public void SetOrigin(int OriginX, int OriginY)
        {
            m_VideoSDL.SetOrigin(m_ViewOriginX + OriginX, m_ViewOriginY + OriginY);
        }

        // ---- Clear / Update -----------------------------------------------

        public void Clear()
        {
            m_VideoSDL.Clear();
        }

        public void Update()
        {
            m_VideoSDL.UpdateAll();
        }

        // ---- Window messages ----------------------------------------------

        public void OnWindowMove()
        {
            m_VideoSDL.OnWindowMove();
        }

        public void SetWindowTitle(string title)
        {
            m_VideoSDL.SetWindowTitle(title);
        }

        public void SetWindowIcon(string icoPath)
        {
            m_VideoSDL.SetWindowIcon(icoPath);
        }

        public void OnPaint()
        {
            m_VideoSDL.UpdateScreen();
        }

        // ---- Draw ---------------------------------------------------------

        public int GetSpriteWidth(int SpriteTable, int Sprite)  => m_VideoSDL.GetSpriteWidth(SpriteTable, Sprite);
        public int GetSpriteHeight(int SpriteTable, int Sprite) => m_VideoSDL.GetSpriteHeight(SpriteTable, Sprite);

        public void DrawSprite(int PositionX, int PositionY,
                               RECT? pZone, RECT? pClip,
                               int SpriteTable, int Sprite,
                               int SpriteLayer, int PriorityInLayer)
        {
            m_VideoSDL.DrawSprite(PositionX, PositionY,
                                  pZone, pClip,
                                  SpriteTable, Sprite,
                                  SpriteLayer, PriorityInLayer);
        }

        public void DrawDebugRectangle(int PositionX, int PositionY,
                                       int w, int h,
                                       byte r, byte g, byte b,
                                       int SpriteLayer, int PriorityInLayer)
        {
            m_VideoSDL.DrawDebugRectangle(PositionX, PositionY,
                                          w, h, r, g, b,
                                          SpriteLayer, PriorityInLayer);
        }

        public void RemoveAllDebugRectangles()
        {
            m_VideoSDL.RemoveAllDebugRectangles();
        }

        // ---- Create (by EDisplayMode) -------------------------------------

        /// <summary>
        /// Creates (or re-creates) the display for the specified mode.
        /// Calls the internal Create(width, height, fullscreen) overload.
        /// </summary>
        public bool Create(EDisplayMode DisplayMode)
        {
            System.Diagnostics.Debug.Assert(DisplayMode != EDisplayMode.DISPLAYMODE_NONE);

            switch (DisplayMode)
            {
                case EDisplayMode.DISPLAYMODE_FULL1:    return Create(320, 240, true);
                case EDisplayMode.DISPLAYMODE_FULL2:    return Create(512, 384, true);
                case EDisplayMode.DISPLAYMODE_FULL3:    return Create(640, 480, true);
                case EDisplayMode.DISPLAYMODE_WINDOWED: return Create(GameConst.VIEW_WIDTH, GameConst.VIEW_HEIGHT, false);
                default:                                return false;
            }
        }

        // ---- Create (by dimensions) ---------------------------------------

        /// <summary>
        /// (Re)creates the SDL interface and loads all sprite tables for the
        /// requested resolution.
        /// </summary>
        private bool Create(int Width, int Height, bool FullScreen)
        {
            int Depth = 32;

            if (!m_VideoSDL.IsModeSet(Width, Height, Depth, FullScreen))
            {
                Destroy();

                if (!m_VideoSDL.Create(Width, Height, Depth, FullScreen))
                    return false;

                // Load every sprite table from files (SDL path, no embedded resources)
                if (!LoadBackgroundSprites(              BmpId.BMP_GREEN_BACKGROUND_SOLID,   "green_background_solid.bmp")   ||
                    !LoadBackgroundSprites(              BmpId.BMP_BLUE_BACKGROUND_SOLID,    "blue_background_solid.bmp")    ||
                    !LoadBackgroundSprites(              BmpId.BMP_PURPLE_BACKGROUND_SOLID,  "purple_background_solid.bmp")  ||
                    !LoadBackgroundSprites(              BmpId.BMP_RED_BACKGROUND_SOLID,     "red_background_solid.bmp")     ||
                    !LoadBackgroundSprites(              BmpId.BMP_GREEN_BACKGROUND_BOMB,    "green_background_bomb.bmp")    ||
                    !LoadBackgroundSprites(              BmpId.BMP_BLUE_BACKGROUND_BOMB,     "blue_background_bomb.bmp")     ||
                    !LoadBackgroundSprites(              BmpId.BMP_PURPLE_BACKGROUND_BOMB,   "purple_background_bomb.bmp")   ||
                    !LoadBackgroundSprites(              BmpId.BMP_RED_BACKGROUND_BOMB,      "red_background_bomb.bmp")      ||
                    !LoadBackgroundSprites(              BmpId.BMP_GREEN_BACKGROUND_CHAR,    "green_background_char.bmp")    ||
                    !LoadBackgroundSprites(              BmpId.BMP_BLUE_BACKGROUND_CHAR,     "blue_background_char.bmp")     ||
                    !LoadBackgroundSprites(              BmpId.BMP_PURPLE_BACKGROUND_CHAR,   "purple_background_char.bmp")   ||
                    !LoadBackgroundSprites(              BmpId.BMP_RED_BACKGROUND_CHAR,      "red_background_char.bmp")      ||
                    !LoadBackgroundSprites(              BmpId.BMP_GREEN_BACKGROUND_FLAME,   "green_background_flame.bmp")   ||
                    !LoadBackgroundSprites(              BmpId.BMP_BLUE_BACKGROUND_FLAME,    "blue_background_flame.bmp")    ||
                    !LoadBackgroundSprites(              BmpId.BMP_PURPLE_BACKGROUND_FLAME,  "purple_background_flame.bmp")  ||
                    !LoadBackgroundSprites(              BmpId.BMP_RED_BACKGROUND_FLAME,     "red_background_flame.bmp")     ||
                    !LoadSprites( 2, 1,  32,  32, false, BmpId.BMP_ARENA_FLOOR,              "arena_floor.bmp")              ||
                    !LoadSprites( 7, 1,  32,  32,  true, BmpId.BMP_ARENA_WALL,               "arena_wall.bmp")               ||
                    !LoadSprites(28, 1,  32,  32,  true, BmpId.BMP_ARENA_FLAME,              "arena_flame.bmp")              ||
                    !LoadSprites(20, 1,  32,  32, false, BmpId.BMP_ARENA_ITEM,               "arena_item.bmp")               ||
                    !LoadSprites( 3, 1,  32,  32,  true, BmpId.BMP_ARENA_BOMB,               "arena_bomb.bmp")               ||
                    !LoadSprites(12, 8,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_WALK,        "arena_bomber_walk.bmp")        ||
                    !LoadSprites( 7, 1,  52,  54,  true, BmpId.BMP_ARENA_FIRE,               "arena_fire.bmp")               ||
                    !LoadSprites(12, 8,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_WALK_HOLD,   "arena_bomber_walk_hold.bmp")   ||
                    !LoadSprites( 4, 1,  32,  32,  true, BmpId.BMP_ARENA_FLY,                "arena_fly.bmp")                ||
                    !LoadSprites( 1, 1, 480,  26, false, BmpId.BMP_BOARD_BACKGROUND,         "board_background.bmp")         ||
                    !LoadSprites(12, 1,   7,  10,  true, BmpId.BMP_BOARD_TIME,               "board_time.bmp")               ||
                    !LoadSprites( 2, 1,  15,   7,  true, BmpId.BMP_BOARD_CLOCK_TOP,          "board_clock_top.bmp")          ||
                    !LoadSprites( 8, 1,  15,  13,  true, BmpId.BMP_BOARD_CLOCK_BOTTOM,       "board_clock_bottom.bmp")       ||
                    !LoadSprites( 6, 1,   6,   8,  true, BmpId.BMP_BOARD_SCORE,              "board_score.bmp")              ||
                    !LoadSprites( 5, 2,  14,  14,  true, BmpId.BMP_BOARD_HEADS,              "board_heads.bmp")              ||
                    !LoadSprites( 1, 1, 480, 442, false, BmpId.BMP_DRAWGAME_MAIN,            "drawgame_main.bmp")            ||
                    !LoadSprites( 2, 1,  68,  96, false, BmpId.BMP_DRAWGAME_FLAG,            "drawgame_flag.bmp")            ||
                    !LoadSprites( 4, 1,  20,  62,  true, BmpId.BMP_DRAWGAME_FUMES,           "drawgame_fumes.bmp")           ||
                    !LoadSprites( 4, 5,  24,  32,  true, BmpId.BMP_WINNER_BOMBER,            "winner_bomber.bmp")            ||
                    !LoadSprites(16, 1,  22,  22,  true, BmpId.BMP_WINNER_COIN,              "winner_coin.bmp")              ||
                    !LoadSprites( 4, 1,   6,   6,  true, BmpId.BMP_WINNER_LIGHTS,            "winner_lights.bmp")            ||
                    !LoadSprites( 4, 2,  16,  16,  true, BmpId.BMP_WINNER_SPARKS,            "winner_sparks.bmp")            ||
                    !LoadSprites( 1, 1, 158,  16,  true, BmpId.BMP_WINNER_TITLE,             "winner_title.bmp")             ||
                    !LoadSprites( 1, 1,  32, 405, false, BmpId.BMP_VICTORY_WALL,             "victory_wall.bmp")             ||
                    !LoadSprites( 9, 1,  14,  16,  true, BmpId.BMP_VICTORY_CROWD,            "victory_crowd.bmp")            ||
                    !LoadSprites(14, 5,  36,  61,  true, BmpId.BMP_VICTORY_BOMBER,           "victory_bomber.bmp")           ||
                    !LoadSprites( 1, 1, 192,  60,  true, BmpId.BMP_VICTORY_TITLE,            "victory_title.bmp")            ||
                    !LoadSprites(46, 6,  10,  10,  true, BmpId.BMP_GLOBAL_FONT,              "global_font.bmp")              ||
                    !LoadSprites( 5, 2,  21,  19,  true, BmpId.BMP_MENU_BOMBER,              "menu_bomber.bmp")              ||
                    !LoadSprites( 1, 1, 420, 362,  true, BmpId.BMP_MENU_FRAME_1,             "menu_frame_1.bmp")             ||
                    !LoadSprites( 2, 1,  15,  16,  true, BmpId.BMP_MENU_HAND,                "menu_hand.bmp")                ||
                    !LoadSprites( 5, 1,  23,  23,  true, BmpId.BMP_WINNER_CROSS,             "winner_cross.bmp")             ||
                    !LoadSprites( 5, 5,  14,  15,  true, BmpId.BMP_VICTORY_CONFETTIS_LARGE,  "victory_confettis_large.bmp")  ||
                    !LoadSprites( 5, 5,  13,  14,  true, BmpId.BMP_VICTORY_CONFETTIS_MEDIUM, "victory_confettis_medium.bmp") ||
                    !LoadSprites( 5, 5,  10,  10,  true, BmpId.BMP_VICTORY_CONFETTIS_SMALL,  "victory_confettis_small.bmp")  ||
                    !LoadSprites( 1, 1, 200,  36,  true, BmpId.BMP_PAUSE,                    "arena_pause.bmp")              ||
                    !LoadSprites( 1, 1, 200,  36,  true, BmpId.BMP_HURRY,                    "arena_hurry.bmp")              ||
                    !LoadSprites( 1, 1, 154,  93,  true, BmpId.BMP_MENU_FRAME_2,             "menu_frame_2.bmp")             ||
                    !LoadSprites( 3, 4,  32,  32,  true, BmpId.BMP_ARENA_FUMES,              "arena_fumes.bmp")              ||
                    !LoadSprites( 1, 1,  14,  14,  true, BmpId.BMP_BOARD_DRAWGAME,           "board_drawgame.bmp")           ||
                    !LoadSprites( 1, 1, 480, 442, false, BmpId.BMP_TITLE_BACKGROUND,         "title_background.bmp")         ||
                    !LoadSprites( 1, 1, 480, 126,  true, BmpId.BMP_TITLE_BOMBERS,            "title_bombers.bmp")            ||
                    !LoadSprites( 1, 1, 298, 139,  true, BmpId.BMP_TITLE_TITLE,              "title_title.bmp")              ||
                    !LoadSprites( 2, 6, 128,  26,  true, BmpId.BMP_TITLE_MENU_ITEMS,         "title_menu_items.bmp")         ||
                    !LoadSprites( 1, 1, 138,  46,  true, BmpId.BMP_TITLE_CLOUD_1,            "title_cloud_1.bmp")            ||
                    !LoadSprites( 1, 1, 106,  46,  true, BmpId.BMP_TITLE_CLOUD_2,            "title_cloud_2.bmp")            ||
                    !LoadSprites( 1, 1,  66,  22,  true, BmpId.BMP_TITLE_CLOUD_3,            "title_cloud_3.bmp")            ||
                    !LoadSprites( 1, 1,  21,  24,  true, BmpId.BMP_TITLE_SNOWFLAKE,          "title_snowflake.bmp")          ||
                    !LoadSprites(18, 1,  16,  16,  true, BmpId.BMP_LEVEL_MINI_TILES,         "level_mini_tiles.bmp")         ||
                    !LoadSprites( 5, 1,  24,  20,  true, BmpId.BMP_LEVEL_MINI_BOMBERS,       "level_mini_bombers.bmp")       ||
                    !LoadSprites( 7, 5,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_DEATH,       "arena_bomber_death.bmp")       ||
                    !LoadSprites(12, 8,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_LIFT,        "arena_bomber_lift.bmp")        ||
                    !LoadSprites(20, 8,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_THROW,       "arena_bomber_throw.bmp")       ||
                    !LoadSprites( 8, 8,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_PUNCH,       "arena_bomber_punch.bmp")       ||
                    !LoadSprites( 4, 8,  42,  44,  true, BmpId.BMP_ARENA_BOMBER_STUNT,       "arena_bomber_stunt.bmp")       ||
                    !LoadSprites( 4, 1,  32,  32,  true, BmpId.BMP_ARENA_ARROWS,             "arena_arrows.bmp")             ||
                    !LoadSprites( 1, 1,  30,  32,  true, BmpId.BMP_MENU_HAND_TITLE,          "menu_hand_title.bmp")          ||
                    !LoadSprites( 3, 1,  32,  32,  true, BmpId.BMP_ARENA_REMOTE_BOMB,        "arena_remote_bomb.bmp")          )
                {
                    return false;
                }

                // Compute view origin so the game area is centred in the window
                m_ViewOriginX = (Width  - GameConst.VIEW_WIDTH)  / 2;
                m_ViewOriginY = (Height - GameConst.VIEW_HEIGHT) / 2;

                m_VideoSDL.SetOrigin(m_ViewOriginX, m_ViewOriginY);
            }

            return true;
        }

        // ---- Destroy ------------------------------------------------------

        public void Destroy()
        {
            m_VideoSDL.Destroy();
        }

        // ---- IsDisplayModeAvailable ---------------------------------------

        public bool IsDisplayModeAvailable(EDisplayMode DisplayMode)
        {
            System.Diagnostics.Debug.Assert(DisplayMode != EDisplayMode.DISPLAYMODE_NONE);

            switch (DisplayMode)
            {
                case EDisplayMode.DISPLAYMODE_FULL1:    return m_VideoSDL.IsModeAvailable(320, 240, 32);
                case EDisplayMode.DISPLAYMODE_FULL2:    return m_VideoSDL.IsModeAvailable(512, 384, 32);
                case EDisplayMode.DISPLAYMODE_FULL3:    return m_VideoSDL.IsModeAvailable(640, 480, 32);
                case EDisplayMode.DISPLAYMODE_WINDOWED: return true;
                default:                                return false;
            }
        }

        // ---- LoadSprites (private) ----------------------------------------

        /// <summary>
        /// Loads one sprite table from a BMP file.
        /// Uses the SDL file-based path (no Windows resource loading).
        /// </summary>
        private bool LoadSprites(int SpriteTableWidth, int SpriteTableHeight,
                                 int SpriteWidth,      int SpriteHeight,
                                 bool Transparent,     int BMP_ID,
                                 string file)
        {
            return m_VideoSDL.LoadSprites(SpriteTableWidth, SpriteTableHeight,
                                          SpriteWidth, SpriteHeight,
                                          Transparent, BMP_ID,
                                          file);
        }

        private bool LoadBackgroundSprites(int BMP_ID, string file)
        {
            return m_VideoSDL.LoadSpritesAuto(false, BMP_ID, file);
        }
    }
}
