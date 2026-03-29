/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007 Bernd Arnold
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
 *  \file Globals.cs
 *  \brief Global constants, enumerations and small utility types (C# port of StdAfx.h)
 */

using System;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Global game constants (mirrors the #define values in StdAfx.h).
    /// </summary>
    public static class Globals
    {
        //! Player number of the white bomber
        public const int PLAYER_WHITE  = 0;
        //! Player number of the black bomber
        public const int PLAYER_BLACK  = 1;
        //! Player number of the red bomber
        public const int PLAYER_RED    = 2;
        //! Player number of the blue bomber
        public const int PLAYER_BLUE   = 3;
        //! Player number of the green bomber
        public const int PLAYER_GREEN  = 4;

        //! Maximum number of players
        public const int MAX_PLAYERS       = 5;
        //! Maximum number of bombers (same as MAX_PLAYERS)
        public const int MAX_BOMBERS       = MAX_PLAYERS;
        //! Maximum number of teams (one per player, same as MAX_PLAYERS)
        public const int MAX_TEAMS         = MAX_PLAYERS;
        //! Maximum score for a player
        public const int MAX_PLAYER_SCORE  = 5;
        //! Maximum score for draw games
        public const int MAX_DRAWGAME_SCORE = 5;

        //! Arena width in blocks
        public const int ARENA_WIDTH  = 15;
        //! Arena height in blocks
        public const int ARENA_HEIGHT = 13;
        //! Block size in pixels
        public const int BLOCK_SIZE   = 32;
        //! Shift number when translating position <-> block
        public const int BLOCK_POSITION_SHIFT = 8;

        //! Size of the game view from left to right in pixels
        public const int VIEW_WIDTH  = ARENA_WIDTH  * BLOCK_SIZE;
        //! Size of the game view from top to bottom in pixels
        public const int VIEW_HEIGHT = 26 + ARENA_HEIGHT * BLOCK_SIZE;

        //! Maximum iterations constant
        public const int MAX_ITER = 50;

        //! Folder where image resources are stored
        public const string IMAGE_FOLDER = "images";
        //! Folder where sound resources are stored
        public const string SOUND_FOLDER = "sounds";

        //! Application version info string
        public const string APP_VERSION_INFO = "3.0.0";
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Describes how the game should currently be updated.
    /// </summary>
    public enum EGameMode
    {
        GAMEMODE_NONE,          //!< No mode! Nothing to update
        GAMEMODE_TITLE,         //!< Title screen (with the main menu)
        GAMEMODE_DEMO,          //!< Demo screen, showing a match between computer players
        GAMEMODE_CONTROLS,      //!< Controls screen where controls can be customized
        GAMEMODE_MENU,          //!< Menu screen managing all the menu subscreens
        GAMEMODE_MATCH,         //!< Match screen: arena and board update, bombers are playing
        GAMEMODE_WINNER,        //!< Winner screen: display match winner & stuff about scores
        GAMEMODE_DRAWGAME,      //!< Draw game screen: simple animated screen
        GAMEMODE_VICTORY,       //!< Victory screen: display battle winner
        GAMEMODE_GREETS,        //!< Greets screen: display credits
        GAMEMODE_HELP,          //!< Help screen: display help
        GAMEMODE_EXIT           //!< In this mode the game will shutdown and exit
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Describes what action a menu should take.
    /// </summary>
    public enum EMenuAction
    {
        MENUACTION_NONE,        //!< Nothing to do, stay in current menu mode and game mode
        MENUACTION_PREVIOUS,    //!< Go to previous menu mode or to game title screen
        MENUACTION_NEXT,        //!< Go to next menu mode or to game match screen
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Win32-compatible RECT structure used throughout the codebase.
    /// </summary>
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Control indices used with COptions.GetControl / SetControl.</summary>
    public static class EControl
    {
        public const int CONTROL_UP      = 0;
        public const int CONTROL_DOWN    = 1;
        public const int CONTROL_LEFT    = 2;
        public const int CONTROL_RIGHT   = 3;
        public const int CONTROL_ACTION1 = 4;
        public const int CONTROL_ACTION2 = 5;
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Joystick button indices used with CPlayerInput.TestMenuControl.</summary>
    public static class EJoystickButton
    {
        public const int JOYSTICK_BUTTON_BREAK          = 4 + 8; // JOYSTICK_BUTTON(8)
        public const int JOYSTICK_BUTTON_MENU_PREVIOUS  = 4 + 1; // JOYSTICK_BUTTON(1)
        public const int JOYSTICK_BUTTON_MENU_NEXT      = 4 + 0; // JOYSTICK_BUTTON(0)
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// Portable random-number generator that mirrors the C SEED_RANDOM / RANDOM macros.
    /// </summary>
    public static class CRandom
    {
        private static Random _rng = new Random();

        /// <summary>Seed the random number generator (mirrors SEED_RANDOM).</summary>
        public static void Seed(int s)
        {
            _rng = new Random(s);
        }

        /// <summary>Return a non-negative random integer less than <paramref name="max"/> (mirrors RANDOM).</summary>
        public static int Random(int max)
        {
            return _rng.Next(max);
        }
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************
}
