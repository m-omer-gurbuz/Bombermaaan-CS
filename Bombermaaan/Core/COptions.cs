/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007, 2008, 2010 Bernd Arnold
    Copyright (C) 2008 Jerome Bigot
    Copyright (C) 2010 Markus Drescher
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
 *  \file COptions.cs
 *  \brief Handling game options, saving to and reading from file
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Describes the type of a bomber</summary>
    public enum EBomberType
    {
        BOMBERTYPE_OFF,     //!< The bomber is not playing
        BOMBERTYPE_MAN,     //!< The bomber is controlled by a local human player
        BOMBERTYPE_COM,     //!< The bomber is controlled by the computer
        BOMBERTYPE_NET      //!< The bomber is controlled by a human player on the network
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EBattleMode
    {
        BATTLEMODE_SINGLE,     //!< Single battle mode
        BATTLEMODE_TEAM        //!< Team battle mode
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EBomberTeam
    {
        BOMBERTEAM_A,     //!< The bomber team A
        BOMBERTEAM_B,     //!< The bomber team B
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EActionAIAlive
    {
        ACTIONONLYAIPLAYERSALIVE_CONTINUEGAME,      //!< The game continues when only AI players are alive
        ACTIONONLYAIPLAYERSALIVE_STARTCLOSING,      //!< The arena starts closing when only AI players are alive
        ACTIONONLYAIPLAYERSALIVE_ENDMATCHDRAWGAME,  //!< The match ends and there is a draw game when only AI players are alive
        ACTIONONLYAIPLAYERSALIVE_SPEEDUPGAME        //!< The game speed is increased
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum EDisplayMode
    {
        DISPLAYMODE_NONE,
        DISPLAYMODE_FULL1,
        DISPLAYMODE_FULL2,
        DISPLAYMODE_FULL3,
        DISPLAYMODE_WINDOWED
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Contains every option in the game and manages the configuration file</summary>
    public class COptions
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public const int MAX_PLAYERS       = 5;
        private const int MAX_PLAYER_INPUT = 10;
        private const int NUM_CONTROLS     = 6;

        public const int CONTROL_UP      = 0;
        public const int CONTROL_DOWN    = 1;
        public const int CONTROL_LEFT    = 2;
        public const int CONTROL_RIGHT   = 3;
        public const int CONTROL_ACTION1 = 4;
        public const int CONTROL_ACTION2 = 5;

        // Default time values
        private const int TIMESTART_MINUTES = 1;
        private const int TIMESTART_SECONDS = 0;
        private const int TIMEUP_MINUTES    = 0;
        private const int TIMEUP_SECONDS    = 35;

        // Keyboard configuration indices
        private const int CONFIGURATION_KEYBOARD_1 = 0;
        private const int CONFIGURATION_KEYBOARD_2 = 1;
        private const int CONFIGURATION_KEYBOARD_3 = 2;
        private const int CONFIGURATION_KEYBOARD_4 = 3;
        private const int CONFIGURATION_KEYBOARD_5 = 4;
        private const int CONFIGURATION_JOYSTICK_1 = 5; // must equal NUMBER_OF_KEYBOARD_CONFIGURATIONS

        // SDL scancodes used for defaults and persisted keyboard bindings.
        private const int KEYBOARD_UP      = InputConstants.KEYBOARD_UP;
        private const int KEYBOARD_DOWN    = InputConstants.KEYBOARD_DOWN;
        private const int KEYBOARD_LEFT    = InputConstants.KEYBOARD_LEFT;
        private const int KEYBOARD_RIGHT   = InputConstants.KEYBOARD_RIGHT;
        private const int KEYBOARD_X       = InputConstants.KEYBOARD_X;
        private const int KEYBOARD_Z       = InputConstants.KEYBOARD_Z;
        private const int KEYBOARD_NUMPAD8 = InputConstants.KEYBOARD_NUMPAD8;
        private const int KEYBOARD_NUMPAD5 = InputConstants.KEYBOARD_NUMPAD5;
        private const int KEYBOARD_NUMPAD4 = InputConstants.KEYBOARD_NUMPAD4;
        private const int KEYBOARD_NUMPAD6 = InputConstants.KEYBOARD_NUMPAD6;
        private const int KEYBOARD_Y       = InputConstants.KEYBOARD_Y;
        private const int KEYBOARD_T       = InputConstants.KEYBOARD_T;
        private const int KEYBOARD_I       = InputConstants.KEYBOARD_I;
        private const int KEYBOARD_K       = InputConstants.KEYBOARD_K;
        private const int KEYBOARD_J       = InputConstants.KEYBOARD_J;
        private const int KEYBOARD_L       = InputConstants.KEYBOARD_L;
        private const int KEYBOARD_8       = InputConstants.KEYBOARD_8;
        private const int KEYBOARD_7       = InputConstants.KEYBOARD_7;
        private const int KEYBOARD_H       = InputConstants.KEYBOARD_H;
        private const int KEYBOARD_N       = InputConstants.KEYBOARD_N;
        private const int KEYBOARD_B       = InputConstants.KEYBOARD_B;
        private const int KEYBOARD_M       = InputConstants.KEYBOARD_M;
        private const int KEYBOARD_5       = InputConstants.KEYBOARD_5;
        private const int KEYBOARD_4       = InputConstants.KEYBOARD_4;
        private const int KEYBOARD_R       = InputConstants.KEYBOARD_R;
        private const int KEYBOARD_F       = InputConstants.KEYBOARD_F;
        private const int KEYBOARD_D       = InputConstants.KEYBOARD_D;
        private const int KEYBOARD_G       = InputConstants.KEYBOARD_G;
        private const int KEYBOARD_1       = InputConstants.KEYBOARD_1;
        private const int KEYBOARD_2       = InputConstants.KEYBOARD_2;

        private const int JOYSTICK_UP    = 0;
        private const int JOYSTICK_DOWN  = 1;
        private const int JOYSTICK_LEFT  = 2;
        private const int JOYSTICK_RIGHT = 3;

        private static int JOYSTICK_BUTTON(int x) { return 4 + x; }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private int          m_TimeStartMinutes;                              //!< How many minutes in the time when a battle starts?
        private int          m_TimeStartSeconds;                              //!< How many seconds in the time when a battle starts?
        private int          m_TimeUpMinutes;                                 //!< How many minutes in the time when the arena starts closing?
        private int          m_TimeUpSeconds;                                 //!< How many seconds in the time when the arena starts closing?
        private EBomberType[] m_BomberType  = new EBomberType[MAX_PLAYERS];  //!< Bomber type for each player
        private EBomberTeam[] m_BomberTeam  = new EBomberTeam[MAX_PLAYERS];  //!< Bomber team for each player
        private EBattleMode  m_BattleMode;                                   //!< Battle mode single / team
        private int          m_PlayerCount;                                   //!< Total number of players in the battle
        private int          m_BattleCount;                                   //!< How many battles to win in order to be victorious
        private int[]        m_PlayerInput  = new int[MAX_PLAYERS];           //!< Player input to use for each player
        private EDisplayMode m_DisplayMode;                                   //!< Current display mode
        private int[,]       m_Control      = new int[MAX_PLAYER_INPUT, NUM_CONTROLS]; //!< Control mapping
        private int          m_Level;
        private List<CLevel> m_Levels       = new List<CLevel>();
        private string       configFileName;
        private string       oldconfigFileName;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public COptions()
        {
            m_DisplayMode = EDisplayMode.DISPLAYMODE_NONE;
            m_BattleMode  = EBattleMode.BATTLEMODE_SINGLE;

            m_TimeStartMinutes = 0;
            m_TimeStartSeconds = 0;
            m_TimeUpMinutes    = 0;
            m_TimeUpSeconds    = 0;
            m_PlayerCount      = 0;
            m_BattleCount      = 0;
            m_Level            = 0;

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                m_BomberType[i]  = EBomberType.BOMBERTYPE_OFF;
                m_BomberTeam[i]  = EBomberTeam.BOMBERTEAM_A;
                m_PlayerInput[i] = CONFIGURATION_KEYBOARD_1 + i;
            }

            for (int i = 0; i < MAX_PLAYER_INPUT; i++)
                for (int j = 0; j < NUM_CONTROLS; j++)
                    m_Control[i, j] = 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Copy constructor</summary>
        public COptions(COptions another)
        {
            CopyFrom(another);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Copy all fields from another COptions instance.</summary>
        public void CopyFrom(COptions copy)
        {
            m_TimeStartMinutes = copy.m_TimeStartMinutes;
            m_TimeStartSeconds = copy.m_TimeStartSeconds;
            m_TimeUpMinutes    = copy.m_TimeUpMinutes;
            m_TimeUpSeconds    = copy.m_TimeUpSeconds;

            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                m_BomberType[i]  = copy.m_BomberType[i];
                m_BomberTeam[i]  = copy.m_BomberTeam[i];
                m_PlayerInput[i] = copy.m_PlayerInput[i];
            }

            m_PlayerCount = copy.m_PlayerCount;
            m_BattleCount = copy.m_BattleCount;
            m_DisplayMode = copy.m_DisplayMode;
            m_BattleMode  = copy.m_BattleMode;

            for (int i = 0; i < MAX_PLAYER_INPUT; i++)
                for (int j = 0; j < NUM_CONTROLS; j++)
                    m_Control[i, j] = copy.m_Control[i, j];

            m_Level  = copy.m_Level;
            m_Levels = new List<CLevel>(copy.m_Levels);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Load the options. Create the configuration file if it doesn't exist.</summary>
        public bool Create(bool useAppDataFolder, string dynamicDataFolder, string pgmFolder)
        {
            // Set the file name of the configuration file including full path
            configFileName    = Path.Combine(dynamicDataFolder, "config.xml");
            oldconfigFileName = Path.Combine(dynamicDataFolder, "config.dat");

            CLog.GetLog().WriteLine("Options         => Name of config file: '{0}'.", configFileName);

            // Set default configuration values before loading the configuration file
            SetDefaultValues();

            // Load configuration file and overwrite the previously set defaults
            if (!LoadConfiguration())
                return false;

            // Load game levels data and names
            if (!LoadLevels(useAppDataFolder ? dynamicDataFolder : "", pgmFolder))
                return false;

            // Everything went ok.
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Destroy()
        {
            // Nothing to do
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Returns a shallow clone of the options object.</summary>
        public COptions Clone()
        {
            COptions copy = new COptions();
            copy.m_TimeStartMinutes = this.m_TimeStartMinutes;
            copy.m_TimeStartSeconds = this.m_TimeStartSeconds;
            copy.m_TimeUpMinutes    = this.m_TimeUpMinutes;
            copy.m_TimeUpSeconds    = this.m_TimeUpSeconds;
            copy.m_BattleMode       = this.m_BattleMode;
            copy.m_PlayerCount      = this.m_PlayerCount;
            copy.m_BattleCount      = this.m_BattleCount;
            copy.m_DisplayMode      = this.m_DisplayMode;
            copy.m_Level            = this.m_Level;
            copy.m_Levels           = this.m_Levels;
            copy.configFileName     = this.configFileName;
            copy.oldconfigFileName  = this.oldconfigFileName;
            for (int i = 0; i < MAX_PLAYERS; i++)
            {
                copy.m_BomberType[i]  = this.m_BomberType[i];
                copy.m_BomberTeam[i]  = this.m_BomberTeam[i];
                copy.m_PlayerInput[i] = this.m_PlayerInput[i];
            }
            for (int i = 0; i < MAX_PLAYER_INPUT; i++)
                for (int j = 0; j < NUM_CONTROLS; j++)
                    copy.m_Control[i, j] = this.m_Control[i, j];
            return copy;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Write the options to the configuration file</summary>
        public void SaveBeforeExit()
        {
            WriteXMLData();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void SetDefaultValues()
        {
            m_TimeUpMinutes    = TIMEUP_MINUTES;
            m_TimeUpSeconds    = TIMEUP_SECONDS;
            m_TimeStartMinutes = TIMESTART_MINUTES;
            m_TimeStartSeconds = TIMESTART_SECONDS;

            m_BattleCount = 3;
            m_Level       = 0;

            // Default display mode
            m_DisplayMode = EDisplayMode.DISPLAYMODE_WINDOWED;

            // Set the bomber types
            m_BomberType[0] = EBomberType.BOMBERTYPE_MAN;
            m_BomberType[1] = EBomberType.BOMBERTYPE_MAN;
            m_BomberType[2] = EBomberType.BOMBERTYPE_OFF;
            m_BomberType[3] = EBomberType.BOMBERTYPE_OFF;
            m_BomberType[4] = EBomberType.BOMBERTYPE_OFF;

            // Set the bomber teams
            m_BomberTeam[0] = EBomberTeam.BOMBERTEAM_A;
            m_BomberTeam[1] = EBomberTeam.BOMBERTEAM_A;
            m_BomberTeam[2] = EBomberTeam.BOMBERTEAM_B;
            m_BomberTeam[3] = EBomberTeam.BOMBERTEAM_B;
            m_BomberTeam[4] = EBomberTeam.BOMBERTEAM_B;

            // Initialise player inputs
            for (int i = 0; i < MAX_PLAYERS; i++)
                m_PlayerInput[i] = CONFIGURATION_KEYBOARD_1 + i;

            // Set default keyboard keys
            m_Control[CONFIGURATION_KEYBOARD_1, CONTROL_UP]      = KEYBOARD_UP;
            m_Control[CONFIGURATION_KEYBOARD_1, CONTROL_DOWN]    = KEYBOARD_DOWN;
            m_Control[CONFIGURATION_KEYBOARD_1, CONTROL_LEFT]    = KEYBOARD_LEFT;
            m_Control[CONFIGURATION_KEYBOARD_1, CONTROL_RIGHT]   = KEYBOARD_RIGHT;
            m_Control[CONFIGURATION_KEYBOARD_1, CONTROL_ACTION1] = KEYBOARD_X;
            m_Control[CONFIGURATION_KEYBOARD_1, CONTROL_ACTION2] = KEYBOARD_Z;

            m_Control[CONFIGURATION_KEYBOARD_2, CONTROL_UP]      = KEYBOARD_NUMPAD8;
            m_Control[CONFIGURATION_KEYBOARD_2, CONTROL_DOWN]    = KEYBOARD_NUMPAD5;
            m_Control[CONFIGURATION_KEYBOARD_2, CONTROL_LEFT]    = KEYBOARD_NUMPAD4;
            m_Control[CONFIGURATION_KEYBOARD_2, CONTROL_RIGHT]   = KEYBOARD_NUMPAD6;
            m_Control[CONFIGURATION_KEYBOARD_2, CONTROL_ACTION1] = KEYBOARD_Y;
            m_Control[CONFIGURATION_KEYBOARD_2, CONTROL_ACTION2] = KEYBOARD_T;

            m_Control[CONFIGURATION_KEYBOARD_3, CONTROL_UP]      = KEYBOARD_I;
            m_Control[CONFIGURATION_KEYBOARD_3, CONTROL_DOWN]    = KEYBOARD_K;
            m_Control[CONFIGURATION_KEYBOARD_3, CONTROL_LEFT]    = KEYBOARD_J;
            m_Control[CONFIGURATION_KEYBOARD_3, CONTROL_RIGHT]   = KEYBOARD_L;
            m_Control[CONFIGURATION_KEYBOARD_3, CONTROL_ACTION1] = KEYBOARD_8;
            m_Control[CONFIGURATION_KEYBOARD_3, CONTROL_ACTION2] = KEYBOARD_7;

            m_Control[CONFIGURATION_KEYBOARD_4, CONTROL_UP]      = KEYBOARD_H;
            m_Control[CONFIGURATION_KEYBOARD_4, CONTROL_DOWN]    = KEYBOARD_N;
            m_Control[CONFIGURATION_KEYBOARD_4, CONTROL_LEFT]    = KEYBOARD_B;
            m_Control[CONFIGURATION_KEYBOARD_4, CONTROL_RIGHT]   = KEYBOARD_M;
            m_Control[CONFIGURATION_KEYBOARD_4, CONTROL_ACTION1] = KEYBOARD_5;
            m_Control[CONFIGURATION_KEYBOARD_4, CONTROL_ACTION2] = KEYBOARD_4;

            m_Control[CONFIGURATION_KEYBOARD_5, CONTROL_UP]      = KEYBOARD_R;
            m_Control[CONFIGURATION_KEYBOARD_5, CONTROL_DOWN]    = KEYBOARD_F;
            m_Control[CONFIGURATION_KEYBOARD_5, CONTROL_LEFT]    = KEYBOARD_D;
            m_Control[CONFIGURATION_KEYBOARD_5, CONTROL_RIGHT]   = KEYBOARD_G;
            m_Control[CONFIGURATION_KEYBOARD_5, CONTROL_ACTION1] = KEYBOARD_1;
            m_Control[CONFIGURATION_KEYBOARD_5, CONTROL_ACTION2] = KEYBOARD_2;

            for (int j = CONFIGURATION_JOYSTICK_1; j < MAX_PLAYER_INPUT; j++)
            {
                m_Control[j, CONTROL_UP]      = JOYSTICK_UP;
                m_Control[j, CONTROL_DOWN]    = JOYSTICK_DOWN;
                m_Control[j, CONTROL_LEFT]    = JOYSTICK_LEFT;
                m_Control[j, CONTROL_RIGHT]   = JOYSTICK_RIGHT;
                m_Control[j, CONTROL_ACTION1] = JOYSTICK_BUTTON(0);
                m_Control[j, CONTROL_ACTION2] = JOYSTICK_BUTTON(1);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private bool LoadConfiguration()
        {
            // Try to load the XML configuration file
            if (File.Exists(configFileName))
            {
                try
                {
                    XDocument configDoc = XDocument.Load(configFileName);

                    // Read the revision number for logging
                    int tempRevision = 0;
                    XElement confRevision = configDoc.Root
                        ?.Element("Configuration")
                        ?.Element("ConfigRevision");
                    if (confRevision != null)
                        int.TryParse(confRevision.Attribute("value")?.Value, out tempRevision);

                    CLog.GetLog().WriteLine("Options         => Configuration file was successfully loaded and is at revision {0}.", tempRevision);

                    ReadIntFromXML(configDoc, "TimeUp",     "minutes", ref m_TimeUpMinutes);
                    ReadIntFromXML(configDoc, "TimeUp",     "seconds", ref m_TimeUpSeconds);
                    ReadIntFromXML(configDoc, "TimeStart",  "minutes", ref m_TimeStartMinutes);
                    ReadIntFromXML(configDoc, "TimeStart",  "seconds", ref m_TimeStartSeconds);

                    int battleModeVal = (int)m_BattleMode;
                    ReadIntFromXML(configDoc, "BattleMode", "value", ref battleModeVal);
                    m_BattleMode = (EBattleMode)battleModeVal;

                    ReadIntFromXML(configDoc, "BattleCount",     "value", ref m_BattleCount);
                    ReadIntFromXML(configDoc, "LevelFileNumber", "value", ref m_Level);

                    int displayModeVal = (int)m_DisplayMode;
                    ReadIntFromXML(configDoc, "DisplayMode", "value", ref displayModeVal);
                    m_DisplayMode = (EDisplayMode)displayModeVal;

                    for (int i = 0; i < MAX_PLAYERS; i++)
                    {
                        string attrName = "bomber" + i;

                        int btVal = (int)m_BomberType[i];
                        ReadIntFromXML(configDoc, "BomberTypes", attrName, ref btVal);
                        m_BomberType[i] = (EBomberType)btVal;

                        int teamVal = (int)m_BomberTeam[i];
                        ReadIntFromXML(configDoc, "BomberTeams", attrName, ref teamVal);
                        m_BomberTeam[i] = (EBomberTeam)teamVal;

                        int piVal = m_PlayerInput[i];
                        ReadIntFromXML(configDoc, "PlayerInputs", attrName, ref piVal);
                        m_PlayerInput[i] = piVal;
                    }

                    // Read the control settings
                    XElement controlList = configDoc.Root
                        ?.Element("Configuration")
                        ?.Element("ControlList");

                    if (controlList != null)
                    {
                        foreach (XElement element in controlList.Elements("Control"))
                        {
                            int id = -1;
                            if (!int.TryParse(element.Attribute("id")?.Value, out id))
                                continue;

                            if (id < 0 || id >= MAX_PLAYER_INPUT)
                                continue;

                            for (int ctrl = 0; ctrl < NUM_CONTROLS; ctrl++)
                            {
                                string ctrlAttr = "control" + ctrl;
                                int ctrldata = -1;
                                if (int.TryParse(element.Attribute(ctrlAttr)?.Value, out ctrldata))
                                {
                                    if (ctrldata >= 0)
                                        m_Control[id, ctrl] = ctrldata;
                                }
                            }
                        }
                    }

                    if (tempRevision < 2)
                    {
                        MigrateLegacyKeyboardControls();
                    }
                }
                catch (Exception ex)
                {
                    CLog.GetLog().WriteLine("Options         => Configuration file could not be loaded. Error: {0}", ex.Message);
                }
            }
            else
            {
                CLog.GetLog().WriteLine("Options         => Configuration file could not be loaded.");
            }

            // Always return true since it doesn't matter if the configuration file could not be loaded
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private void WriteXMLData()
        {
            try
            {
                XDocument newConfig = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("Bombermaaan",
                        new XComment(" Configuration settings for the Bombermaaan game (http://bombermaaan.sf.net/) "),
                        new XElement("Configuration",
                            new XElement("ConfigRevision",     new XAttribute("value", 2)),
                            new XElement("TimeUp",             new XAttribute("minutes", m_TimeUpMinutes),    new XAttribute("seconds", m_TimeUpSeconds)),
                            new XElement("TimeStart",          new XAttribute("minutes", m_TimeStartMinutes), new XAttribute("seconds", m_TimeStartSeconds)),
                            new XElement("BattleMode",         new XAttribute("value", (int)m_BattleMode)),
                            new XElement("BattleCount",        new XAttribute("value", m_BattleCount)),
                            new XElement("LevelFileNumber",    new XAttribute("value", m_Level)),
                            new XElement("DisplayMode",        new XAttribute("value", (int)m_DisplayMode)),
                            BuildBomberTypesElement(),
                            BuildBomberTeamsElement(),
                            BuildPlayerInputsElement(),
                            BuildControlListElement()
                        )
                    )
                );

                newConfig.Save(configFileName);
                CLog.GetLog().WriteLine("Options         => Configuration file was successfully written.");
            }
            catch (Exception ex)
            {
                CLog.GetLog().WriteLine("Options         => Configuration file was not written. Error: {0}", ex.Message);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private XElement BuildBomberTypesElement()
        {
            XElement el = new XElement("BomberTypes");
            for (int i = 0; i < MAX_PLAYERS; i++)
                el.Add(new XAttribute("bomber" + i, (int)m_BomberType[i]));
            return el;
        }

        private XElement BuildBomberTeamsElement()
        {
            XElement el = new XElement("BomberTeams");
            for (int i = 0; i < MAX_PLAYERS; i++)
                el.Add(new XAttribute("bomber" + i, (int)m_BomberTeam[i]));
            return el;
        }

        private XElement BuildPlayerInputsElement()
        {
            XElement el = new XElement("PlayerInputs");
            for (int i = 0; i < MAX_PLAYERS; i++)
                el.Add(new XAttribute("bomber" + i, m_PlayerInput[i]));
            return el;
        }

        private XElement BuildControlListElement()
        {
            XElement controlList = new XElement("ControlList");
            for (int j = 0; j < MAX_PLAYER_INPUT; j++)
            {
                XElement ctrl = new XElement("Control", new XAttribute("id", j));
                for (int c = 0; c < NUM_CONTROLS; c++)
                    ctrl.Add(new XAttribute("control" + c, m_Control[j, c]));
                controlList.Add(ctrl);
            }
            return controlList;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Read an integer from the XML document structure at path /Bombermaaan/Configuration/configNode[@attrName].
        /// The value variable stays unchanged if the attribute is missing or not a valid integer.
        /// </summary>
        private void ReadIntFromXML(XDocument doc, string configNode, string attrName, ref int value)
        {
            XElement element = doc.Root
                ?.Element("Configuration")
                ?.Element(configNode);

            if (element != null)
            {
                string attrVal = element.Attribute(attrName)?.Value;
                if (attrVal != null)
                    int.TryParse(attrVal, out value);
            }
        }

        private void MigrateLegacyKeyboardControls()
        {
            for (int playerInput = 0; playerInput < InputConfig.NUMBER_OF_KEYBOARD_CONFIGURATIONS; playerInput++)
            {
                for (int control = 0; control < NUM_CONTROLS; control++)
                {
                    m_Control[playerInput, control] = MapLegacyKeyboardControlToScancode(m_Control[playerInput, control]);
                }
            }
        }

        private static int MapLegacyKeyboardControlToScancode(int key)
        {
            switch (key)
            {
                case 273: return InputConstants.KEYBOARD_UP;
                case 274: return InputConstants.KEYBOARD_DOWN;
                case 276: return InputConstants.KEYBOARD_LEFT;
                case 275: return InputConstants.KEYBOARD_RIGHT;
                case 264: return InputConstants.KEYBOARD_NUMPAD8;
                case 261: return InputConstants.KEYBOARD_NUMPAD5;
                case 260: return InputConstants.KEYBOARD_NUMPAD4;
                case 262: return InputConstants.KEYBOARD_NUMPAD6;
                case (int)'x': return InputConstants.KEYBOARD_X;
                case (int)'z': return InputConstants.KEYBOARD_Z;
                case (int)'y': return InputConstants.KEYBOARD_Y;
                case (int)'t': return InputConstants.KEYBOARD_T;
                case (int)'i': return InputConstants.KEYBOARD_I;
                case (int)'k': return InputConstants.KEYBOARD_K;
                case (int)'j': return InputConstants.KEYBOARD_J;
                case (int)'l': return InputConstants.KEYBOARD_L;
                case (int)'8': return InputConstants.KEYBOARD_8;
                case (int)'7': return InputConstants.KEYBOARD_7;
                case (int)'h': return InputConstants.KEYBOARD_H;
                case (int)'n': return InputConstants.KEYBOARD_N;
                case (int)'b': return InputConstants.KEYBOARD_B;
                case (int)'m': return InputConstants.KEYBOARD_M;
                case (int)'5': return InputConstants.KEYBOARD_5;
                case (int)'4': return InputConstants.KEYBOARD_4;
                case (int)'r': return InputConstants.KEYBOARD_R;
                case (int)'f': return InputConstants.KEYBOARD_F;
                case (int)'d': return InputConstants.KEYBOARD_D;
                case (int)'g': return InputConstants.KEYBOARD_G;
                case (int)'1': return InputConstants.KEYBOARD_1;
                case (int)'2': return InputConstants.KEYBOARD_2;
                default: return key;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Load game levels data and names from the level directory.</summary>
        private bool LoadLevels(string dynamicDataFolder, string pgmFolder)
        {
            // Collect file info records
            var files = new List<(string nameOnly, string fullPath)>();

            //-------------------------------------------
            // Load levels from the program folder
            //-------------------------------------------
            string levelDir_pgm = Path.Combine(
                string.IsNullOrEmpty(pgmFolder) ? "." : pgmFolder,
                "Levels");

            CLog.GetLog().WriteLine("Options         => Loading level files '{0}'.", Path.Combine(levelDir_pgm, "*.TXT"));

            if (Directory.Exists(levelDir_pgm))
            {
                foreach (string fullPath in Directory.GetFiles(levelDir_pgm, "*.TXT"))
                {
                    files.Add((Path.GetFileName(fullPath), fullPath));
                }
            }

            //-------------------------------------------
            // Load levels from the dynamic data (AppData) folder if set
            //-------------------------------------------
            if (!string.IsNullOrEmpty(dynamicDataFolder))
            {
                string levelDir_dyn = Path.Combine(dynamicDataFolder, "levels");

                CLog.GetLog().WriteLine("Options         => Loading level files '{0}'.", Path.Combine(levelDir_dyn, "*.TXT"));

                if (Directory.Exists(levelDir_dyn))
                {
                    foreach (string fullPath in Directory.GetFiles(levelDir_dyn, "*.TXT"))
                    {
                        files.Add((Path.GetFileName(fullPath), fullPath));
                    }
                }
            }

            // Sort by file name
            files.Sort((a, b) => string.Compare(a.nameOnly, b.nameOnly, StringComparison.OrdinalIgnoreCase));

            // Build CLevel list
            foreach (var file in files)
            {
                m_Levels.Add(new CLevel(file.fullPath, file.nameOnly));
            }

            // Must have at least one level
            if (m_Levels.Count == 0)
            {
                CLog.GetLog().WriteLine("Options         => !!! There should be at least 1 level.");
                return false;
            }

            // Clamp stored level index if out of range
            if (m_Level >= m_Levels.Count)
                m_Level = 0;

            // Load all detected level files
            bool errorOccurred = false;

            for (int currentLevel = 0; currentLevel < m_Levels.Count; currentLevel++)
            {
                if (!m_Levels[currentLevel].LoadFromFile())
                {
                    errorOccurred = true;
                    break;
                }
            }

            if (errorOccurred)
                return false;

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public int GetTimeStartMinutes() { return m_TimeStartMinutes; }
        public int GetTimeStartSeconds() { return m_TimeStartSeconds; }
        public int GetTimeUpMinutes()    { return m_TimeUpMinutes; }
        public int GetTimeUpSeconds()    { return m_TimeUpSeconds; }

        public void SetTimeStart(int TimeStartMinutes, int TimeStartSeconds)
        {
            m_TimeStartMinutes = TimeStartMinutes;
            m_TimeStartSeconds = TimeStartSeconds;
        }

        public void SetTimeUp(int TimeUpMinutes, int TimeUpSeconds)
        {
            m_TimeUpMinutes = TimeUpMinutes;
            m_TimeUpSeconds = TimeUpSeconds;
        }

        public EBomberType GetBomberType(int Player)  { return m_BomberType[Player]; }
        public void SetBomberType(int Player, EBomberType BomberType) { m_BomberType[Player] = BomberType; }

        public EBomberTeam GetBomberTeam(int Player)  { return m_BomberTeam[Player]; }
        public void SetBomberTeam(int Player, EBomberTeam BomberTeam) { m_BomberTeam[Player] = BomberTeam; }

        public EBattleMode GetBattleMode()                 { return m_BattleMode; }
        public void SetBattleMode(EBattleMode BattleMode) { m_BattleMode = BattleMode; }

        public int  GetBattleCount()              { return m_BattleCount; }
        public void SetBattleCount(int BattleCount) { m_BattleCount = BattleCount; }

        public int GetPlayerInput(int Player)
        {
            Debug.Assert(Player >= 0 && Player < MAX_PLAYERS);
            return m_PlayerInput[Player];
        }

        public void SetPlayerInput(int Player, int PlayerInput)
        {
            Debug.Assert(Player >= 0 && Player < MAX_PLAYERS);
            m_PlayerInput[Player] = PlayerInput;
        }

        public EDisplayMode GetDisplayMode()                    { return m_DisplayMode; }
        public void SetDisplayMode(EDisplayMode DisplayMode)   { m_DisplayMode = DisplayMode; }

        public int GetControl(int PlayerInput, int Control)
        {
            Debug.Assert(PlayerInput >= 0 && PlayerInput < MAX_PLAYER_INPUT);
            Debug.Assert(Control >= 0 && Control < NUM_CONTROLS);
            return m_Control[PlayerInput, Control];
        }

        public void SetControl(int PlayerInput, int Control, int Value)
        {
            Debug.Assert(PlayerInput >= 0 && PlayerInput < MAX_PLAYER_INPUT);
            Debug.Assert(Control >= 0 && Control < NUM_CONTROLS);
            m_Control[PlayerInput, Control] = Value;
        }

        public EBlockType GetBlockType(int X, int Y)
        {
            Debug.Assert(m_Level >= 0 && m_Level < m_Levels.Count);
            return m_Levels[m_Level].GetBlockType(X, Y);
        }

        public int GetNumberOfItemsInWalls(EItemType ItemType)
        {
            Debug.Assert(m_Level >= 0 && m_Level < m_Levels.Count);
            return m_Levels[m_Level].GetNumberOfItemsInWalls(ItemType);
        }

        public int GetInitialBomberSkills(EBomberSkills BomberSkill)
        {
            Debug.Assert(m_Level >= 0 && m_Level < m_Levels.Count);
            return m_Levels[m_Level].GetInitialBomberSkills(BomberSkill);
        }

        public void SetLevel(int Level)
        {
            Debug.Assert(Level >= 0 && Level < m_Levels.Count);
            m_Level = Level;
        }

        public int  GetLevel()          { return m_Level; }
        public int  GetNumberOfLevels() { return m_Levels.Count; }

        public string GetLevelName()
        {
            Debug.Assert(m_Level >= 0 && m_Level < m_Levels.Count);
            return m_Levels[m_Level].GetLevelName();
        }

        public EActionAIAlive GetOption_ActionWhenOnlyAIPlayersLeft()
        {
            // TODO: This should really be an option
            return EActionAIAlive.ACTIONONLYAIPLAYERSALIVE_CONTINUEGAME;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
