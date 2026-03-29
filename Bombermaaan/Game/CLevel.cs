/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008 Bernd Arnold
    Copyright (C) 2008 Jerome Bigot
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
 *  \file CLevel.cs
 *  \brief Handling a level
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EBlockType
    {
        BLOCKTYPE_HARDWALL,             //!< There must be a hard wall here
        BLOCKTYPE_SOFTWALL,             //!< There must be a soft wall here
        BLOCKTYPE_RANDOM,               //!< There must be either a soft wall or a free place here (random)
        BLOCKTYPE_FREE,                 //!< There must be a free place here
        BLOCKTYPE_WHITEBOMBER,          //!< The white bomber must start here
        BLOCKTYPE_BLACKBOMBER,          //!< The black bomber must start here
        BLOCKTYPE_REDBOMBER,            //!< The red bomber must start here
        BLOCKTYPE_BLUEBOMBER,           //!< The blue bomber must start here
        BLOCKTYPE_GREENBOMBER,          //!< The green bomber must start here
        BLOCKTYPE_MOVEBOMB_RIGHT,       //!< A bomb starts moving right if placed here
        BLOCKTYPE_MOVEBOMB_DOWN,        //!< A bomb starts moving down if placed here
        BLOCKTYPE_MOVEBOMB_LEFT,        //!< A bomb starts moving left if placed here
        BLOCKTYPE_MOVEBOMB_UP,          //!< A bomb starts moving up if placed here
        BLOCKTYPE_ITEM_BOMB,            //!< A bomb item if placed here
        BLOCKTYPE_ITEM_FLAME,           //!< A flame item if placed here
        BLOCKTYPE_ITEM_ROLLER,          //!< A roller item if placed here
        BLOCKTYPE_ITEM_KICK,            //!< A kick item if placed here
        BLOCKTYPE_ITEM_THROW,           //!< A throw item if placed here
        BLOCKTYPE_ITEM_PUNCH,           //!< A punch item if placed here
        BLOCKTYPE_ITEM_SKULL,           //!< A skull item if placed here
        BLOCKTYPE_ITEM_REMOTES,         //!< A remote item if placed here
        BLOCKTYPE_ITEM_SHIELD,          //!< A shield item if placed here
        BLOCKTYPE_ITEM_STRONGWEAK       //!< A strong/weak item if placed here
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EBomberSkills
    {
        BOMBERSKILL_DUMMYFIRST,
        BOMBERSKILL_FLAME,
        BOMBERSKILL_BOMBS,
        BOMBERSKILL_BOMBITEMS,
        BOMBERSKILL_FLAMEITEMS,
        BOMBERSKILL_ROLLERITEMS,
        BOMBERSKILL_KICKITEMS,
        BOMBERSKILL_THROWITEMS,
        BOMBERSKILL_PUNCHITEMS,
        BOMBERSKILL_REMOTEITEMS,
        BOMBERSKILL_SHIELDITEMS,
        BOMBERSKILL_STRONGWEAKITEMS,
        NUMBER_OF_BOMBERSKILLS
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Contains all settings of one level.</summary>
    public class CLevel
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Initial number of items when a new arena is built
        public const int INITIAL_ITEMBOMB       = 11;
        public const int INITIAL_ITEMFLAME      = 8;
        public const int INITIAL_ITEMROLLER     = 7;
        public const int INITIAL_ITEMKICK       = 2;
        public const int INITIAL_ITEMSKULL      = 1;
        public const int INITIAL_ITEMTHROW      = 2;
        public const int INITIAL_ITEMPUNCH      = 2;
        public const int INITIAL_ITEMREMOTE     = 2;
        public const int INITIAL_ITEMSHIELD     = 1;
        public const int INITIAL_ITEMSTRONGWEAK = 1;

        // Initial flame size
        public const int INITIAL_FLAMESIZE = 2;

        // Initial number of bombs the bomber can drop
        public const int INITIAL_BOMBS = 1;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private EBlockType[,] m_ArenaData;                  //!< Arena cells
        private string        m_FilenameShort;               //!< The short level file name without path
        private string        m_FilenameFull;                //!< The full name of a level file including path
        private int[]         m_NumberOfItemsInWalls;        //!< The number of items in the soft walls
        private int[]         m_InitialBomberSkills;         //!< The initial bomber skills

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Constructor.</summary>
        public CLevel(string filename_full, string filename_short)
        {
            m_FilenameShort = filename_short;
            m_FilenameFull  = filename_full;

            m_ArenaData = new EBlockType[Globals.ARENA_WIDTH, Globals.ARENA_HEIGHT];

            for (int i = 0; i < Globals.ARENA_WIDTH; i++)
                for (int j = 0; j < Globals.ARENA_HEIGHT; j++)
                    m_ArenaData[i, j] = EBlockType.BLOCKTYPE_HARDWALL;

            m_NumberOfItemsInWalls = new int[(int)EItemType.NUMBER_OF_ITEMS];
            for (int i = 0; i < (int)EItemType.NUMBER_OF_ITEMS; i++)
                m_NumberOfItemsInWalls[i] = 0;

            m_InitialBomberSkills = new int[(int)EBomberSkills.NUMBER_OF_BOMBERSKILLS];
            for (int i = 0; i < (int)EBomberSkills.NUMBER_OF_BOMBERSKILLS; i++)
                m_InitialBomberSkills[i] = 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Load game level data from the file.</summary>
        public bool LoadFromFile()
        {
            bool ErrorOccurred = false;

            // Open the existing level file for reading
            if (!File.Exists(m_FilenameFull))
            {
                CLog.GetLog().WriteLine("Options         => Loading level file {0} failed.", m_FilenameFull);
                return false;
            }

            string[] allLines;
            try
            {
                allLines = File.ReadAllLines(m_FilenameFull);
            }
            catch
            {
                CLog.GetLog().WriteLine("Options         => Loading level file {0} failed.", m_FilenameFull);
                return false;
            }

            if (allLines.Length == 0)
            {
                CLog.GetLog().WriteLine("Options         => Loading level file {0} failed (empty).", m_FilenameFull);
                return false;
            }

            // This is the first line for level files beginning with version 2
            string headerV2plus = "; Bombermaaan level file version=";
            string firstLine = allLines[0];
            int LevelVersion;

            if (firstLine.StartsWith(headerV2plus, StringComparison.Ordinal))
            {
                string versionStr = firstLine.Substring(headerV2plus.Length);
                if (!int.TryParse(versionStr, out LevelVersion))
                    LevelVersion = 1;
            }
            else
            {
                LevelVersion = 1;
            }

            switch (LevelVersion)
            {
                case 1:
                    if (!LoadVersion1(allLines))
                        ErrorOccurred = true;
                    break;

                case 2:
                    if (!LoadVersion2(m_FilenameFull))
                        ErrorOccurred = true;
                    break;

                default:
                    CLog.GetLog().WriteLine("Options         => !!! Unsupported version of level file {0}.", m_FilenameShort);
                    ErrorOccurred = true;
                    break;
            }

            // Validate this level if no error occurred so far
            if (!ErrorOccurred)
                ErrorOccurred = !Validate();

            if (!ErrorOccurred)
            {
                CLog.GetLog().WriteLine("Options         => Level file {0} was successfully loaded (version {1}).", m_FilenameShort, LevelVersion);
            }
            else
            {
                CLog.GetLog().WriteLine("Options         => !!! Could not load level file {0} (version {1}).", m_FilenameShort, LevelVersion);
            }

            return !ErrorOccurred;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool LoadVersion1(string[] lines)
        {
            bool StopReadingFile = false;

            // For each line of characters to read
            for (int y = 0; y < Globals.ARENA_HEIGHT; y++)
            {
                if (y >= lines.Length)
                {
                    CLog.GetLog().WriteLine("Options         => !!! Level file is incorrect (Line: {0}, not enough lines).", y + 1);
                    StopReadingFile = true;
                    break;
                }

                string Line = lines[y];
                int ReadBytes = Line.Length;

                if (ReadBytes < Globals.ARENA_WIDTH)
                {
                    CLog.GetLog().WriteLine("Options         => !!! Level file is incorrect (Line: {0}, Length: {1}).", y + 1, ReadBytes);
                    StopReadingFile = true;
                    break;
                }

                for (int x = 0; x < Globals.ARENA_WIDTH; x++)
                {
                    char c = Line[x];
                    bool validChar = true;

                    switch (c)
                    {
                        case '*': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_HARDWALL;        break;
                        case '-': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_SOFTWALL;        break;
                        case '?': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_RANDOM;          break;
                        case ' ': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_FREE;            break;
                        case '1': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_WHITEBOMBER;     break;
                        case '2': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_BLACKBOMBER;     break;
                        case '3': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_REDBOMBER;       break;
                        case '4': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_BLUEBOMBER;      break;
                        case '5': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_GREENBOMBER;     break;
                        case 'R': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_RIGHT;  break;
                        case 'D': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_DOWN;   break;
                        case 'L': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_LEFT;   break;
                        case 'U': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_UP;     break;
                        case 'B': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_BOMB;       break;
                        case 'K': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_KICK;       break;
                        case 'F': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_FLAME;      break;
                        case 'S': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_ROLLER;     break;
                        case 'P': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_PUNCH;      break;
                        case 'T': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_THROW;      break;
                        case 'Z': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_REMOTES;    break;
                        case 'C': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_SKULL;      break;
                        case 'V': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_SHIELD;     break;
                        case 'I': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_STRONGWEAK; break;
                        default:
                        {
                            CLog.GetLog().WriteLine("Options         => !!! Level file is incorrect (unknown character {0}).", c);
                            StopReadingFile = true;
                            validChar = false;
                            break;
                        }
                    }

                    if (!validChar) break;
                }

                if (StopReadingFile) break;
            }

            // Set defaults for items in walls
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_BOMB]       = INITIAL_ITEMBOMB;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_FLAME]      = INITIAL_ITEMFLAME;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_KICK]       = INITIAL_ITEMKICK;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_ROLLER]     = INITIAL_ITEMROLLER;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_SKULL]      = INITIAL_ITEMSKULL;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_THROW]      = INITIAL_ITEMTHROW;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_PUNCH]      = INITIAL_ITEMPUNCH;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_REMOTE]     = INITIAL_ITEMREMOTE;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_SHIELD]     = INITIAL_ITEMSHIELD;
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_STRONGWEAK] = INITIAL_ITEMSTRONGWEAK;

            // Set defaults for bomber skills
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_FLAME]          = INITIAL_FLAMESIZE;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_BOMBS]          = INITIAL_BOMBS;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_BOMBITEMS]      = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_FLAMEITEMS]     = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_ROLLERITEMS]    = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_KICKITEMS]      = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_THROWITEMS]     = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_PUNCHITEMS]     = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_REMOTEITEMS]    = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_SHIELDITEMS]    = 0;
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_STRONGWEAKITEMS] = 0;

            return !StopReadingFile;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Load level file version 2 (INI-based format).</summary>
        private bool LoadVersion2(string filename)
        {
            // Read all lines into a dictionary-based INI parser
            Dictionary<string, Dictionary<string, string>> ini = ParseIniFile(filename);
            if (ini == null) return false;

            // Helper to get INI value
            string GetValue(string section, string key, string defaultVal)
            {
                if (ini.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val))
                    return val;
                return defaultVal;
            }

            // Read and validate Width
            int value;
            if (!int.TryParse(GetValue("General", "Width", "0"), out value) || value != Globals.ARENA_WIDTH)
            {
                CLog.GetLog().WriteLine("Options         => !!! Invalid arena width {0}. Only {1} is allowed.", value, Globals.ARENA_WIDTH);
                return false;
            }

            // Read and validate Height
            if (!int.TryParse(GetValue("General", "Height", "0"), out value) || value != Globals.ARENA_HEIGHT)
            {
                CLog.GetLog().WriteLine("Options         => !!! Invalid arena height {0}. Only {1} is allowed.", value, Globals.ARENA_HEIGHT);
                return false;
            }

            // Read and validate MaxPlayers
            if (!int.TryParse(GetValue("General", "MaxPlayers", "0"), out value) || value != 5)
            {
                CLog.GetLog().WriteLine("Options         => !!! Invalid maximum players {0}. Only {1} is allowed.", value, 5);
                return false;
            }

            // Read and validate MinPlayers
            if (!int.TryParse(GetValue("General", "MinPlayers", "0"), out value) || value != 1)
            {
                CLog.GetLog().WriteLine("Options         => !!! Invalid minimum players {0}. Only {1} is allowed.", value, 1);
                return false;
            }

            // Creator / Priority / Comment / Description (not used currently but read for completeness)
            // string creator     = GetValue("General", "Creator", "");
            // string comment     = GetValue("General", "Comment", "");
            // string description = GetValue("General", "Description", "");

            // For each line of the map
            for (int y = 0; y < Globals.ARENA_HEIGHT; y++)
            {
                string keyName    = string.Format("Line.{0:D2}", y);
                string arenaLine  = GetValue("Map", keyName, "");

                if (arenaLine.Length != Globals.ARENA_WIDTH)
                {
                    CLog.GetLog().WriteLine("Options         => !!! Level file is incorrect (Line.{0} wrong length {1}).", y, arenaLine.Length);
                    return false;
                }

                for (int x = 0; x < Globals.ARENA_WIDTH; x++)
                {
                    char c = arenaLine[x];
                    switch (c)
                    {
                        case '*': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_HARDWALL;        break;
                        case '-': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_SOFTWALL;        break;
                        case '?': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_RANDOM;          break;
                        case ' ': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_FREE;            break;
                        case '1': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_WHITEBOMBER;     break;
                        case '2': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_BLACKBOMBER;     break;
                        case '3': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_REDBOMBER;       break;
                        case '4': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_BLUEBOMBER;      break;
                        case '5': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_GREENBOMBER;     break;
                        case 'R': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_RIGHT;  break;
                        case 'D': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_DOWN;   break;
                        case 'L': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_LEFT;   break;
                        case 'U': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_MOVEBOMB_UP;     break;
                        case 'B': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_BOMB;       break;
                        case 'K': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_KICK;       break;
                        case 'F': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_FLAME;      break;
                        case 'S': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_ROLLER;     break;
                        case 'P': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_PUNCH;      break;
                        case 'T': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_THROW;      break;
                        case 'Z': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_REMOTES;    break;
                        case 'C': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_SKULL;      break;
                        case 'V': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_SHIELD;     break;
                        case 'I': m_ArenaData[x, y] = EBlockType.BLOCKTYPE_ITEM_STRONGWEAK; break;
                        default:
                        {
                            CLog.GetLog().WriteLine("Options         => !!! Level file is incorrect (unknown character {0}).", c);
                            return false;
                        }
                    }
                }
            }

            // Read ItemsInWalls values
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_BOMB]       = ParseInt(GetValue("Settings", "ItemsInWalls.Bombs",      "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_FLAME]      = ParseInt(GetValue("Settings", "ItemsInWalls.Flames",     "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_KICK]       = ParseInt(GetValue("Settings", "ItemsInWalls.Kicks",      "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_ROLLER]     = ParseInt(GetValue("Settings", "ItemsInWalls.Rollers",    "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_SKULL]      = ParseInt(GetValue("Settings", "ItemsInWalls.Skulls",     "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_THROW]      = ParseInt(GetValue("Settings", "ItemsInWalls.Throws",     "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_PUNCH]      = ParseInt(GetValue("Settings", "ItemsInWalls.Punches",    "0"));
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_REMOTE]     = ParseInt(GetValue("Settings", "ItemsInWalls.Remotes",    INITIAL_ITEMREMOTE.ToString()));
            // "Sheilds" is a historical typo in level files; accept both spellings
            string shieldVal = GetValue("Settings", "ItemsInWalls.Shields", null)
                            ?? GetValue("Settings", "ItemsInWalls.Sheilds", "0");
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_SHIELD]     = ParseInt(shieldVal);
            m_NumberOfItemsInWalls[(int)EItemType.ITEM_STRONGWEAK] = ParseInt(GetValue("Settings", "ItemsInWalls.StrongWeak", "0"));

            // Read BomberSkillsAtStart values
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_FLAME]           = ParseInt(GetValue("Settings", "BomberSkillsAtStart.FlameSize",     "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_BOMBS]           = ParseInt(GetValue("Settings", "BomberSkillsAtStart.MaxBombs",       "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_BOMBITEMS]       = ParseInt(GetValue("Settings", "BomberSkillsAtStart.BombItems",      "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_FLAMEITEMS]      = ParseInt(GetValue("Settings", "BomberSkillsAtStart.FlameItems",     "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_ROLLERITEMS]     = ParseInt(GetValue("Settings", "BomberSkillsAtStart.RollerItems",    "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_KICKITEMS]       = ParseInt(GetValue("Settings", "BomberSkillsAtStart.KickItems",      "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_THROWITEMS]      = ParseInt(GetValue("Settings", "BomberSkillsAtStart.ThrowItems",     "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_PUNCHITEMS]      = ParseInt(GetValue("Settings", "BomberSkillsAtStart.PunchItems",     "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_REMOTEITEMS]     = ParseInt(GetValue("Settings", "BomberSkillsAtStart.RemoteItems",    "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_SHIELDITEMS]     = ParseInt(GetValue("Settings", "BomberSkillsAtStart.ShieldItems",    "0"));
            m_InitialBomberSkills[(int)EBomberSkills.BOMBERSKILL_STRONGWEAKITEMS] = ParseInt(GetValue("Settings", "BomberSkillsAtStart.StrongWeakItems","0"));

            // ContaminationsNotUsed is read but not currently stored
            // string contaminationsNotToUse = GetValue("Settings", "ContaminationsNotUsed", "");

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Minimal INI file parser: returns section -> key -> value dictionary.</summary>
        private static Dictionary<string, Dictionary<string, string>> ParseIniFile(string filename)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            string currentSection = "";

            try
            {
                foreach (string rawLine in File.ReadLines(filename))
                {
                    string line = rawLine.Trim();

                    // Skip comments and blank lines
                    if (string.IsNullOrEmpty(line) || line[0] == ';' || line[0] == '#')
                        continue;

                    // Section header
                    if (line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        currentSection = line.Substring(1, line.Length - 2).Trim();
                        if (!result.ContainsKey(currentSection))
                            result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        continue;
                    }

                    // Key = Value
                    int eqIdx = line.IndexOf('=');
                    if (eqIdx > 0)
                    {
                        string key = line.Substring(0, eqIdx).Trim();
                        string val = line.Substring(eqIdx + 1).Trim();

                        // Strip inline comment after ';'
                        int semiIdx = val.IndexOf(';');
                        if (semiIdx >= 0)
                            val = val.Substring(0, semiIdx).Trim();

                        if (!result.ContainsKey(currentSection))
                            result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        result[currentSection][key] = val;
                    }
                }
            }
            catch
            {
                return null;
            }

            return result;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private static int ParseInt(string s)
        {
            int result;
            return int.TryParse(s, out result) ? result : 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Check if the number of max items is valid.</summary>
        private bool CheckMaxNumberOfItems(out uint sumOfMaxItems)
        {
            sumOfMaxItems = 0;

            // Count items in walls
            for (int i = (int)EItemType.ITEM_NONE + 1; i < (int)EItemType.NUMBER_OF_ITEMS; i++)
                sumOfMaxItems += (uint)m_NumberOfItemsInWalls[i];

            // Count initial bomber skills (worst case with five players)
            for (int i = (int)EBomberSkills.BOMBERSKILL_DUMMYFIRST + 1; i < (int)EBomberSkills.NUMBER_OF_BOMBERSKILLS; i++)
            {
                // Initial skills like bombs and flames will not be lost
                if (i != (int)EBomberSkills.BOMBERSKILL_FLAME && i != (int)EBomberSkills.BOMBERSKILL_BOMBS)
                    sumOfMaxItems += (uint)(m_InitialBomberSkills[i] * Globals.MAX_PLAYERS);
            }

            return sumOfMaxItems <= CArena.MAX_ITEMS;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Check if this level is valid.</summary>
        private bool Validate()
        {
            uint itemCount;

            if (!CheckMaxNumberOfItems(out itemCount))
            {
                CLog.GetLog().WriteLine("Options         => !!! Level file is incorrect (Too many items: {0} of {1} allowed).", itemCount, CArena.MAX_ITEMS);
                return false;
            }

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public EBlockType GetBlockType(int X, int Y)
        {
            Debug.Assert(X >= 0 && X < Globals.ARENA_WIDTH);
            Debug.Assert(Y >= 0 && Y < Globals.ARENA_HEIGHT);
            return m_ArenaData[X, Y];
        }

        public int GetNumberOfItemsInWalls(EItemType ItemType)
        {
            Debug.Assert(ItemType > EItemType.ITEM_NONE && ItemType < EItemType.NUMBER_OF_ITEMS);
            return m_NumberOfItemsInWalls[(int)ItemType];
        }

        public int GetInitialBomberSkills(EBomberSkills BomberSkill)
        {
            Debug.Assert(BomberSkill > EBomberSkills.BOMBERSKILL_DUMMYFIRST && BomberSkill < EBomberSkills.NUMBER_OF_BOMBERSKILLS);
            return m_InitialBomberSkills[(int)BomberSkill];
        }

        public string GetLevelName()
        {
            return m_FilenameShort;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
