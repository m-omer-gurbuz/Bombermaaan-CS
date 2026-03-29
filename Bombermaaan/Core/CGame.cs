/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007-2010 Bernd Arnold
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
 *  \file CGame.cs
 *  \brief The core of the program, handling sub-components, program control (C# / SDL2 port)
 */

using System;
using Bombermaaan.SDL2;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// The main game controller of Bombermaaan.
    ///
    /// CGame extends CWindow. It initializes SDL, creates the window, initialises
    /// the timer / display / input / sound / options objects, and manages switches
    /// between game modes (see <see cref="EGameMode"/>).
    /// </summary>
    public class CGame : CWindow
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        //! Define to enable the main log file
        // private const bool ENABLE_LOG = true;

        //! Define to always treat the window as active (update even when unfocused)
        private const bool ENABLE_UPDATE_WHEN_WINDOW_IS_INACTIVE = true;

        //! Define to enable sound and music
        private const bool ENABLE_SOUND = true;

        private const int SDL_JOYSTICK_AXIS_MIN = -32768;
        private const int SDL_JOYSTICK_AXIS_MAX =  32767;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private EGameMode   m_GameMode;     //!< Current game mode defining what to update
        private IntPtr      m_hModule;      //!< Connection to the resources (SDL: always IntPtr.Zero)
        private IntPtr      m_hInstance;    //!< Application instance handle (SDL: always IntPtr.Zero)

        private CTimer      m_Timer;        //!< Timer object for movement, animation, synchronization
        private CDisplay    m_Display;      //!< Needed to draw sprites and manage display
        private CInput      m_Input;        //!< Needed to read the players choices in menus, match, etc.
        private CSound      m_Sound;        //!< Needed to play sounds and musics
        private COptions    m_Options;      //!< Options chosen by the players
        private CScores     m_Scores;       //!< Scores object where we keep the player scores and the draw games count

        private CDrawGame   m_DrawGame;     //!< Draw game screen object
        private CWinner     m_Winner;       //!< Winner screen object
        private CVictory    m_Victory;      //!< Victory screen object
        private CMatch      m_Match;        //!< Match screen object
        private CMenu       m_Menu;         //!< Menu screen object
        private CTitle      m_Title;        //!< Title screen object
        private CControls   m_Controls;     //!< Controls screen object
        private CDemo       m_Demo;         //!< Demo screen object
        private CMenuYesNo  m_MenuYesNo;    //!< Yes/No message box object
        private CCredits    m_Credits;      //!< Credits screen object
        private CHelp       m_Help;         //!< Help screen object

        private string      m_WindowTitle;  //!< Window title string (SDL path)

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Constructor. Sets the initial game mode, seeds the random number generator
        /// and builds the window title string.
        /// </summary>
        /// <param name="hInstance">Application instance handle (IntPtr.Zero on SDL).</param>
        /// <param name="args">Command-line arguments array.</param>
        public CGame(IntPtr hInstance, string[] args)
            : base(hInstance, "Bombermaaan")
        {
            m_GameMode  = EGameMode.GAMEMODE_NONE;
            m_hModule   = IntPtr.Zero;
            m_hInstance = IntPtr.Zero;

            // Seed the RNG with the current time (mirrors SEED_RANDOM((unsigned)time(NULL)))
            CRandom.Seed((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            // Build window title: "Bombermaaan <version> - Compiled YYYY-MM-DD"
            m_WindowTitle = $"Bombermaaan {Globals.APP_VERSION_INFO} - Compiled {DateTime.Now:yyyy-MM-dd}";

            // Initialise all game-object members to null-equivalent stubs;
            // Create() will instantiate the real objects once SDL is ready.
            m_Timer    = new CTimer();
            m_Display  = new CDisplay();
            m_Input    = new CInput();
            m_Sound    = new CSound();
            m_Options  = new COptions();
            m_Scores   = new CScores();
            m_DrawGame = new CDrawGame();
            m_Winner   = new CWinner();
            m_Victory  = new CVictory();
            m_Match    = new CMatch();
            m_Menu     = new CMenu();
            m_Title    = new CTitle();
            m_Controls = new CControls();
            m_Demo     = new CDemo();
            m_MenuYesNo = new CMenuYesNo();
            m_Credits  = new CCredits();
            m_Help     = new CHelp();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Creates the main parts of the game and establishes relationships between members.
        /// </summary>
        /// <param name="args">Command-line arguments (used for --help, --use-appdata-dir, etc.).</param>
        /// <returns>True if everything was successfully initialised; false on any error.</returns>
        public bool Create(string[] args)
        {
            // ------------------------------------------------------------------
            // Handle --help / --license / -? / /? flags
            // ------------------------------------------------------------------
            bool helpRequested = false;
            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg == "-h"      || arg == "--help"         ||
                        arg == "--license" || arg == "--show-license" ||
                        arg == "-?"      || arg == "/?")
                    {
                        helpRequested = true;
                        break;
                    }
                }
            }

            if (helpRequested)
            {
                Console.WriteLine(
                    "Bombermaaan\n"
                    + "Copyright (C) 2000-2002, 2007 Thibaut Tollemer\n"
                    + "Copyright (C) 2007, 2008 Bernd Arnold\n"
                    + "Copyright (C) 2008 Jerome Bigot\n"
                    + "Copyright (C) 2008 Markus Drescher\n"
                    + "Copyright (C) 2016 Billy Araujo\n"
                    + "Copyright (C) 2026 Ömer Gürbüz\n"
                    + "\n"
                    + "Bombermaaan is free software: you can redistribute it and/or modify\n"
                    + "it under the terms of the GNU General Public License as published by\n"
                    + "the Free Software Foundation, either version 3 of the License, or\n"
                    + "(at your option) any later version.\n"
                    + "\n"
                    + "Bombermaaan is distributed in the hope that it will be useful,\n"
                    + "but WITHOUT ANY WARRANTY; without even the implied warranty of\n"
                    + "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the\n"
                    + "GNU General Public License for more details.\n"
                    + "\n"
                    + "You should have received a copy of the GNU General Public License\n"
                    + "along with Bombermaaan.  If not, see <http://www.gnu.org/licenses/>.\n");

                // Return false so the program will terminate
                return false;
            }

            // ------------------------------------------------------------------
            // Determine where config/log files live
            // ------------------------------------------------------------------

            // On SDL the default is to store data next to the executable.
            // --use-appdata-dir moves config/log to the user's home folder.
            bool useAppDataFolder = false;
            if (args != null)
            {
                foreach (string arg in args)
                {
                    if (arg == "--use-appdata-dir")
                    {
                        useAppDataFolder = true;
                        break;
                    }
                }
            }

            string dynamicDataFolder;

            if (useAppDataFolder)
            {
                // Use %APPDATA% / $HOME
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (string.IsNullOrEmpty(appDataPath))
                {
                    Console.Error.WriteLine("Could not determine user application data folder.\nBombermaaan terminates.");
                    return false;
                }

                dynamicDataFolder = System.IO.Path.Combine(appDataPath, "Bombermaaan") + System.IO.Path.DirectorySeparatorChar;

                try
                {
                    System.IO.Directory.CreateDirectory(dynamicDataFolder);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Could not create folder '{dynamicDataFolder}'.\nBombermaaan cannot run without this folder.\n{ex.Message}");
                    return false;
                }
            }
            else
            {
                // Use the current directory
                dynamicDataFolder = "." + System.IO.Path.DirectorySeparatorChar;
            }

            // ------------------------------------------------------------------
            // Open log file
            // ------------------------------------------------------------------

            string logFileName = System.IO.Path.Combine(dynamicDataFolder, "log.txt");
            CLog.GetLog().Open(logFileName);

            // ------------------------------------------------------------------
            // Log startup information
            // ------------------------------------------------------------------

            CLog.GetLog().WriteLine("Game            => Bombermaaan {0}", Globals.APP_VERSION_INFO);
            CLog.GetLog().WriteLine("Game            => Built at {0}.", DateTime.Now.ToString("HH:mm:ss 'on' yyyy-MM-dd"));
            CLog.GetLog().WriteLine("Game            => Program name: '{0}'.",
                System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "<unknown>");

            // ------------------------------------------------------------------
            // Set up the debug helper
            // ------------------------------------------------------------------

            CDebug.GetInstance().SetGame(this);
            CDebug.GetInstance().SetTimer(m_Timer);
            CDebug.GetInstance().SetMatch(m_Match);
            CDebug.GetInstance().Create();

            // ------------------------------------------------------------------
            // Initialise SDL (video + audio + joystick)
            // ------------------------------------------------------------------

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO | SDL.SDL_INIT_JOYSTICK) != 0)
            {
                CLog.GetLog().WriteLine("Game            => !!! Could not initialise SDL library: {0}", SDL.SDL_GetError());
                return false;
            }

            // Update the window caption now that SDL is running
            if (m_hWnd != IntPtr.Zero)
            {
                SDL.SDL_SetWindowTitle(m_hWnd, m_WindowTitle);
            }

            // ------------------------------------------------------------------
            // Initialise options (must come before display/input/sound Create())
            // ------------------------------------------------------------------

            if (!m_Options.Create(useAppDataFolder, dynamicDataFolder, AppDomain.CurrentDomain.BaseDirectory))
            {
                return false;
            }

            // ------------------------------------------------------------------
            // Wire up cross-object references
            // ------------------------------------------------------------------

            // Input
            m_Input.SetOptions(m_Options);
            m_Input.SetTimer(m_Timer);

            // Display
            m_Display.SetModuleHandle(IntPtr.Zero);

            // Match
            m_Match.SetDisplay(m_Display);
            m_Match.SetInput(m_Input);
            m_Match.SetOptions(m_Options);
            m_Match.SetTimer(m_Timer);
            m_Match.SetScores(m_Scores);
            m_Match.SetSound(m_Sound);

            // Demo
            m_Demo.SetDisplay(m_Display);
            m_Demo.SetInput(m_Input);
            m_Demo.SetOptions(m_Options);
            m_Demo.SetTimer(m_Timer);
            m_Demo.SetScores(m_Scores);
            m_Demo.SetSound(m_Sound);

            // DrawGame
            m_DrawGame.SetDisplay(m_Display);
            m_DrawGame.SetInput(m_Input);
            m_DrawGame.SetTimer(m_Timer);
            m_DrawGame.SetScores(m_Scores);
            m_DrawGame.SetOptions(m_Options);
            m_DrawGame.SetSound(m_Sound);

            // Winner
            m_Winner.SetDisplay(m_Display);
            m_Winner.SetInput(m_Input);
            m_Winner.SetOptions(m_Options);
            m_Winner.SetTimer(m_Timer);
            m_Winner.SetScores(m_Scores);
            m_Winner.SetMatch(m_Match);
            m_Winner.SetSound(m_Sound);

            // Victory
            m_Victory.SetDisplay(m_Display);
            m_Victory.SetInput(m_Input);
            m_Victory.SetOptions(m_Options);
            m_Victory.SetTimer(m_Timer);
            m_Victory.SetScores(m_Scores);
            m_Victory.SetSound(m_Sound);

            // Scores
            m_Scores.SetOptions(m_Options);

            // Menu
            m_Menu.SetDisplay(m_Display);
            m_Menu.SetInput(m_Input);
            m_Menu.SetOptions(m_Options);
            m_Menu.SetTimer(m_Timer);
            m_Menu.SetSound(m_Sound);
            m_Menu.SetScores(m_Scores);

            // Title
            m_Title.SetDisplay(m_Display);
            m_Title.SetInput(m_Input);
            m_Title.SetOptions(m_Options);
            m_Title.SetTimer(m_Timer);
            m_Title.SetSound(m_Sound);

            // Controls
            m_Controls.SetDisplay(m_Display);
            m_Controls.SetInput(m_Input);
            m_Controls.SetOptions(m_Options);
            m_Controls.SetTimer(m_Timer);
            m_Controls.SetSound(m_Sound);

            // Credits
            m_Credits.SetDisplay(m_Display);
            m_Credits.SetInput(m_Input);
            m_Credits.SetOptions(m_Options);
            m_Credits.SetTimer(m_Timer);
            m_Credits.SetSound(m_Sound);

            // Help
            m_Help.SetDisplay(m_Display);
            m_Help.SetInput(m_Input);
            m_Help.SetOptions(m_Options);
            m_Help.SetTimer(m_Timer);
            m_Help.SetSound(m_Sound);

            // MenuYesNo
            m_MenuYesNo.SetDisplay(m_Display);
            m_MenuYesNo.SetInput(m_Input);
            m_MenuYesNo.SetTimer(m_Timer);
            m_MenuYesNo.SetSound(m_Sound);

            // Sound
            m_Sound.SetModuleHandle(IntPtr.Zero);

            // ------------------------------------------------------------------
            // Create display (opens the game window at the correct size)
            // ------------------------------------------------------------------

            if (!m_Display.Create(m_Options.GetDisplayMode()))
            {
                return false;
            }

            // Set the full version title and icon on the actual SDL window created by CVideoSDL
            m_Display.SetWindowTitle(m_WindowTitle);
            m_Display.SetWindowIcon("Bombermaaan.ico");

            // ------------------------------------------------------------------
            // Create input
            // ------------------------------------------------------------------

            if (!m_Input.Create())
            {
                return false;
            }

            // ------------------------------------------------------------------
            // Create sound
            // ------------------------------------------------------------------

            if (ENABLE_SOUND)
            {
                if (!m_Sound.Create())
                {
                    return false;
                }
            }

            // ------------------------------------------------------------------
            // Create the yes/no menu overlay
            // ------------------------------------------------------------------

            m_MenuYesNo.Create();

            // ------------------------------------------------------------------
            // Enter the title screen (the initial game mode)
            // ------------------------------------------------------------------

            StartGameMode(EGameMode.GAMEMODE_TITLE);

            // Log that initialisation is complete
            CLog.GetLog().WriteLine("Game            => Game initialization is complete!");

            // Leave a blank line between initialisation and game-loop output
            CLog.GetLog().Write("\n");

            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Shuts down all subsystems and frees resources.
        /// </summary>
        public void Destroy()
        {
            // Leave a blank line between game-loop output and shutdown output
            CLog.GetLog().Write("\n");
            CLog.GetLog().WriteLine("Game            => Game shutdown will now begin.");

            // Terminate the current game mode
            FinishGameMode();

            if (ENABLE_SOUND)
            {
                m_Sound.Destroy();
            }

            m_Input.Destroy();
            m_Display.Destroy();

            m_Options.SaveBeforeExit();
            m_Options.Destroy();
            m_MenuYesNo.Destroy();

            CDebug.GetInstance().Destroy();

            // Shut down SDL
            SDL.SDL_Quit();

            m_hModule = IntPtr.Zero;

            // Close the log file
            CLog.GetLog().Close();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Returns the <see cref="CModeScreen"/> object that manages the given game mode,
        /// or null if the mode has no object (GAMEMODE_NONE / GAMEMODE_EXIT).
        /// </summary>
        private CModeScreen GetGameModeObject(EGameMode GameMode)
        {
            switch (GameMode)
            {
                case EGameMode.GAMEMODE_TITLE:    return m_Title;
                case EGameMode.GAMEMODE_DEMO:     return m_Demo;
                case EGameMode.GAMEMODE_MENU:     return m_Menu;
                case EGameMode.GAMEMODE_MATCH:    return m_Match;
                case EGameMode.GAMEMODE_WINNER:   return m_Winner;
                case EGameMode.GAMEMODE_DRAWGAME: return m_DrawGame;
                case EGameMode.GAMEMODE_VICTORY:  return m_Victory;
                case EGameMode.GAMEMODE_CONTROLS: return m_Controls;
                case EGameMode.GAMEMODE_GREETS:   return m_Credits;
                case EGameMode.GAMEMODE_HELP:     return m_Help;
                case EGameMode.GAMEMODE_EXIT:     break;
                default:                          break;
            }

            // There is no object manager for this game mode
            return null;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Finish the current game mode, then start the specified one.
        /// </summary>
        public void SwitchToGameMode(EGameMode GameMode)
        {
            FinishGameMode();
            StartGameMode(GameMode);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Set a new game mode. Creates the object corresponding to the new game mode.
        /// </summary>
        private void StartGameMode(EGameMode GameMode)
        {
            // Set the new game mode
            m_GameMode = GameMode;

            // If we must exit the game
            if (m_GameMode == EGameMode.GAMEMODE_EXIT)
            {
                // Come back to windowed mode to avoid display quirks on exit
                m_Display.Create(EDisplayMode.DISPLAYMODE_WINDOWED);

                // Push an SDL_QUIT event so the MessagePump exits its loop
                SDL.SDL_Event quitEvent = new SDL.SDL_Event();
                quitEvent.type = SDL.SDL_QUIT;
                SDL.SDL_PushEvent(ref quitEvent);
            }
            else
            {
                // Create the object corresponding to the new game mode
                CModeScreen modeScreen = GetGameModeObject(m_GameMode);
                if (modeScreen != null)
                    modeScreen.Create();
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Finish the current game mode: destroys its screen object and sets GAMEMODE_NONE.
        /// </summary>
        private void FinishGameMode()
        {
            //! Destroy the object corresponding to the current game mode
            CModeScreen modeScreen = GetGameModeObject(m_GameMode);
            if (modeScreen != null)
                modeScreen.Destroy();

            //! Set no game mode
            m_GameMode = EGameMode.GAMEMODE_NONE;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called every frame while the window is active.
        /// Updates the current game mode screen, handles the yes/no menu overlay,
        /// clears the display, renders the current screen, then flips the display.
        /// </summary>
        protected override void OnWindowActive()
        {
            // Prepare a game mode variable to hold the mode to switch to
            EGameMode NextGameMode = m_GameMode;

            m_Timer.Update();
            m_Input.GetMainInput().Update();

            //! If the yes/no menu is not active, update the current mode screen
            if (!m_MenuYesNo.IsActive())
            {
                CModeScreen modeScreen = GetGameModeObject(m_GameMode);
                if (modeScreen != null)
                    NextGameMode = modeScreen.Update();
            }

            //! If the mode screen is not requesting a mode change, let the yes/no menu decide
            if (NextGameMode == m_GameMode)
            {
                NextGameMode = m_MenuYesNo.Update(m_GameMode);

                if (NextGameMode == EGameMode.GAMEMODE_TITLE)
                    m_Menu.SetMenuMode(EMenuMode.MENUMODE_BOMBER);
            }

            //! Clear the display to black
            m_Display.Clear();

            //! Render the current mode screen
            CModeScreen currentScreen = GetGameModeObject(m_GameMode);
            if (currentScreen != null)
                currentScreen.Display();

            //! Render the yes/no menu overlay if needed
            m_MenuYesNo.Display();

            //! Flip / present the display
            m_Display.Update();

            //! Switch game mode if requested
            if (NextGameMode != m_GameMode)
            {
                FinishGameMode();
                StartGameMode(NextGameMode);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when the application gains or loses focus.
        /// Mirrors WM_ACTIVATEAPP handling in the original CGame.
        /// </summary>
        protected override void OnActivateApp(IntPtr wParam, IntPtr lParam)
        {
            bool soundWasPausedWhenLosingFocus = false;

            base.OnActivateApp(wParam, lParam);

            // When ENABLE_UPDATE_WHEN_WINDOW_IS_INACTIVE is defined the game
            // always treats the window as active.
            if (ENABLE_UPDATE_WHEN_WINDOW_IS_INACTIVE)
                m_Active = true;

            if (m_Active)
            {
                // Resume the timer
                m_Timer.Resume();

                if (ENABLE_SOUND)
                {
                    if (soundWasPausedWhenLosingFocus)
                    {
                        m_Sound.SetPause(false);
                        soundWasPausedWhenLosingFocus = false;
                    }
                }

                // Re-open input for the current mode screen
                CModeScreen modeScreen = GetGameModeObject(m_GameMode);
                if (modeScreen != null)
                    modeScreen.OpenInput();
            }
            else
            {
                // Pause the timer
                m_Timer.Pause();

                if (ENABLE_SOUND)
                {
                    if (!m_Sound.IsPaused())
                    {
                        m_Sound.SetPause(true);
                        soundWasPausedWhenLosingFocus = true;
                    }
                }

                // Close input for the current mode screen
                CModeScreen modeScreen = GetGameModeObject(m_GameMode);
                if (modeScreen != null)
                    modeScreen.CloseInput();
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when the window moves. Notifies the display object.
        /// </summary>
        protected override void OnMove(IntPtr wParam, IntPtr lParam)
        {
            base.OnMove(wParam, lParam);
            m_Display.OnWindowMove();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when a key is pressed down.
        /// </summary>
        protected override void OnKeyDown(IntPtr wParam, IntPtr lParam)
        {
            // No action needed – key-down is handled via SDL input polling
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when a key is released.
        /// Handles F3 (fullscreen), F4 (windowed) and Ctrl+F12 (quick exit).
        /// </summary>
        protected override void OnKeyUp(IntPtr wParam, IntPtr lParam)
        {
            int sym = (int)wParam;

            // If the CTRL modifier is NOT held
            if (((int)lParam & (int)SDL.SDL_Keymod.KMOD_CTRL) == 0)
            {
                EDisplayMode DisplayMode    = EDisplayMode.DISPLAYMODE_NONE;
                bool          SetDisplayMode = true;

                // F3 -> full screen mode 3, F4 -> windowed
                switch (sym)
                {
                    case (int)SDL.SDL_Keycode.SDLK_F3:
                        DisplayMode = EDisplayMode.DISPLAYMODE_FULL3;
                        break;
                    case (int)SDL.SDL_Keycode.SDLK_F4:
                        DisplayMode = EDisplayMode.DISPLAYMODE_WINDOWED;
                        break;
                    default:
                        SetDisplayMode = false;
                        break;
                }

                if (SetDisplayMode && m_Display.IsDisplayModeAvailable(DisplayMode))
                {
                    m_Display.Create(DisplayMode);
                    m_Options.SetDisplayMode(DisplayMode);
                }
            }
            else
            {
                // Ctrl+F12: quick exit
                if (sym == (int)SDL.SDL_Keycode.SDLK_F12)
                {
                    FinishGameMode();
                    StartGameMode(EGameMode.GAMEMODE_EXIT);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when the window needs to be repainted.
        /// </summary>
        protected override void OnPaint(IntPtr wParam, IntPtr lParam)
        {
            m_Display.OnPaint();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called for system commands. Returns true to let the default handler run.
        /// On SDL there are no SC_MONITORPOWER / SC_SCREENSAVE messages, so this is a no-op.
        /// </summary>
        protected override bool OnSysCommand(IntPtr wParam, IntPtr lParam)
        {
            // Make the default handler deal with it
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when the window is resized.
        /// </summary>
        protected override void OnSize(IntPtr wParam, IntPtr lParam)
        {
            // Rework necessary – left empty as per the original SDL path
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when a joystick axis changes (SDL_JOYAXISMOTION).
        /// </summary>
        protected override void OnJoystickAxis(IntPtr wParam, IntPtr lParam)
        {
            // Joystick input is now polled directly by CInput / CInputSDL each frame.
            // The SDL event-driven path forwarded raw SDL_JoyAxisEvent pointers via wParam,
            // which is not safe in managed code. Input polling is the preferred SDL2 approach.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when a joystick hat (D-pad) moves (SDL_JOYHATMOTION).
        /// </summary>
        protected override void OnJoystickHatMotion(IntPtr wParam, IntPtr lParam)
        {
            // Hat motion is handled via CInput / CInputSDL polling.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Called when a joystick button is pressed or released (SDL_JOYBUTTONDOWN / SDL_JOYBUTTONUP).
        /// </summary>
        protected override void OnJoystickButton(IntPtr wParam, IntPtr lParam)
        {
            // Button events are handled via CInput / CInputSDL polling.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
