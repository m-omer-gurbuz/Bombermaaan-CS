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
 *  \file CWindow.cs
 *  \brief Implementation of the CWindow class (C# / SDL2 port)
 */

using Bombermaaan.SDL2;
using System;

namespace Bombermaaan
{
    //! Base class for managing the main window
    public class CWindow
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // SDL2 window pointer (replaces Win32 HWND)
        protected IntPtr m_hWnd;
        protected bool m_Active;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CWindow(IntPtr hInstance, string pWindowTitle, int IconResourceID = -1)
        {
            m_hWnd = IntPtr.Zero;
            m_Active = false;

            // SDL2 port: the actual game window is created by CVideoSDL.Create().
            // CWindow does not create its own window to avoid a duplicate empty window.
            // IconResourceID is ignored on SDL2.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        ~CWindow()
        {
            // If the window exists, destroy it
            if (m_hWnd != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(m_hWnd);
                m_hWnd = IntPtr.Zero;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetClientSize(int ClientWidth, int ClientHeight)
        {
            // In SDL2 there is no need to adjust for window chrome (borders/title bar) when
            // dealing purely with client size – SDL_CreateWindow / SDL_SetWindowSize already
            // work in terms of the drawable area on most platforms.
            if (m_hWnd != IntPtr.Zero)
            {
                // SDL2 does not expose SDL_SetWindowSize as a separate binding here,
                // so we recreate the window size concept by calling the SDL function directly
                // through the existing SDL2 wrapper structure.
                // SDL_SetWindowSize is not wrapped in the minimal SDL2.cs; we call it via
                // the approach available: destroy and recreate is not desirable, so we rely
                // on subclasses (e.g. CVideoSDL) to resize as needed.
                // This base implementation simply stores nothing – subclasses override as required.
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Show()
        {
            // Replaces Win32 ShowWindow / UpdateWindow
            if (m_hWnd != IntPtr.Zero)
            {
                SDL.SDL_ShowWindow(m_hWnd);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // The Message Loop. It pumps the SDL2 events, manages them. If the window has some
        // idle time, it calls OnWindowActive().
        public void MessagePump()
        {
            SDL.SDL_Event sdlEvent;
            bool quit = false;

            // Simulate WM_ACTIVATEAPP / WA_ACTIVE to signal the app has started
            OnActivateApp((IntPtr)1 /* WA_ACTIVE */, IntPtr.Zero);

            // Start main game loop here
            while (!quit)
            {
                // Manage the events if some are waiting
                while (SDL.SDL_PollEvent(out sdlEvent) != 0)
                {
                    switch (sdlEvent.type)
                    {
                        case SDL.SDL_KEYDOWN:
                            // WM_KEYDOWN: wParam = virtual key sym, lParam = modifier
                            OnKeyDown((IntPtr)sdlEvent.key.keysym.sym, (IntPtr)sdlEvent.key.keysym.mod);
                            break;

                        case SDL.SDL_KEYUP:
                            // WM_KEYUP: wParam = virtual key sym, lParam = modifier
                            OnKeyUp((IntPtr)sdlEvent.key.keysym.sym, (IntPtr)sdlEvent.key.keysym.mod);
                            break;

                        case SDL.SDL_JOYAXISMOTION:
                            // Handle Joystick Motion
                            OnJoystickAxis(IntPtr.Zero, IntPtr.Zero);
                            break;

                        case SDL.SDL_JOYHATMOTION:
                            OnJoystickHatMotion(IntPtr.Zero, IntPtr.Zero);
                            break;

                        case SDL.SDL_JOYBUTTONDOWN:
                        case SDL.SDL_JOYBUTTONUP:
                            // Handle Joystick buttons
                            OnJoystickButton(IntPtr.Zero, IntPtr.Zero);
                            break;

                        case SDL.SDL_WINDOWEVENT:
                            // SDL2 replaced SDL1's SDL_VIDEORESIZE / SDL_ACTIVEEVENT with SDL_WINDOWEVENT
                            switch (sdlEvent.window.windowEvent)
                            {
                                case SDL.SDL_WINDOWEVENT_RESIZED:
                                case SDL.SDL_WINDOWEVENT_SIZE_CHANGED:
                                    // WM_SIZE equivalent
                                    OnSize(IntPtr.Zero, IntPtr.Zero);
                                    break;

                                case SDL.SDL_WINDOWEVENT_FOCUS_GAINED:
                                    // WM_ACTIVATEAPP with WA_ACTIVE
                                    OnActivateApp((IntPtr)1 /* WA_ACTIVE */, IntPtr.Zero);
                                    break;

                                case SDL.SDL_WINDOWEVENT_FOCUS_LOST:
                                    // WM_ACTIVATEAPP with wParam == 0 (deactivate)
                                    OnActivateApp(IntPtr.Zero, IntPtr.Zero);
                                    break;

                                case SDL.SDL_WINDOWEVENT_MOVED:
                                    // WM_MOVE equivalent
                                    OnMove(IntPtr.Zero, IntPtr.Zero);
                                    break;

                                case SDL.SDL_WINDOWEVENT_EXPOSED:
                                    // WM_PAINT equivalent
                                    OnPaint(IntPtr.Zero, IntPtr.Zero);
                                    break;

                                case SDL.SDL_WINDOWEVENT_CLOSE:
                                    // WM_CLOSE equivalent
                                    OnClose(IntPtr.Zero, IntPtr.Zero);
                                    quit = true;
                                    break;
                            }
                            break;

                        case SDL.SDL_QUIT:
                            // WM_CLOSE equivalent (application-level quit)
                            OnClose(IntPtr.Zero, IntPtr.Zero);
                            quit = true;
                            break;
                    }
                }

                if (m_Active)
                {
                    // Call the virtual activity method
                    OnWindowActive();
                    SDL.SDL_Delay(1); // rest for the cpu
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // SDL2 event dispatcher (replaces Win32 WinProc).
        // In the SDL2 port this is not a static callback registered with the OS; instead
        // MessagePump calls the On* virtual methods directly. WinProc is kept as a
        // convenience dispatcher so subclasses can forward synthetic events if needed.
        public virtual void WinProc(uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == SDL.SDL_KEYDOWN)             { OnKeyDown(wParam, lParam);        return; }
            if (msg == SDL.SDL_KEYUP)               { OnKeyUp(wParam, lParam);          return; }
            if (msg == SDL.SDL_WINDOWEVENT)         { OnSize(wParam, lParam);           return; }
            if (msg == SDL.SDL_JOYAXISMOTION)       { OnJoystickAxis(wParam, lParam);   return; }
            if (msg == SDL.SDL_JOYHATMOTION)        { OnJoystickHatMotion(wParam, lParam); return; }
            if (msg == SDL.SDL_JOYBUTTONDOWN)       { OnJoystickButton(wParam, lParam); return; }
            if (msg == SDL.SDL_JOYBUTTONUP)         { OnJoystickButton(wParam, lParam); return; }
            if (msg == SDL.SDL_QUIT)                { OnClose(wParam, lParam);          return; }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected virtual void OnWindowActive()
        {
            // Nothing by default
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_CREATE message (Sent once after window creation)
        protected virtual void OnCreate(IntPtr hwnd, IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_INITDIALOG message (Sent once before a dialog box is displayed)
        protected virtual void OnInitDialog(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_ACTIVATEAPP message  (Sent when a window belonging to a different
        // application than the active window is about to be activated)
        protected virtual void OnActivateApp(IntPtr wParam, IntPtr lParam)
        {
            // Pause if minimized or not the top window.
            // wParam == 1 (WA_ACTIVE) or 2 (WA_CLICKACTIVE) means active; 0 means deactivated.
            m_Active = (wParam != IntPtr.Zero);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_SIZE message (Sent after the window's size has changed)
        protected virtual void OnSize(IntPtr wParam, IntPtr lParam)
        {
            // Check to see if we are losing our window...
            // In SDL2 the window minimised state is tracked through SDL_WINDOWEVENT_MINIMIZED.
            // Base implementation does nothing; subclasses can override.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_MOVE message (Sent after the window has been moved)
        protected virtual void OnMove(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_PAINT message (Sent when Windows or another application makes a
        // request to paint a portion of the application's window)
        protected virtual void OnPaint(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_KEYDOWN message (Posted to the window with the keyboard focus when
        // a nonsystem key is pressed (ALT not pressed)).
        protected virtual void OnKeyDown(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_KEYUP message (Posted to the window with the keyboard focus when
        // a nonsystem key is released (ALT not pressed)).
        protected virtual void OnKeyUp(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_TIMER message (Sent after each interval specified in the SetTimer
        // function used to install a timer).
        protected virtual void OnTimer(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_COMMAND message (Sent when the user selects a command item from a
        // menu, when a control sends a notification message to its parent window, or when an
        // accelerator keystroke is translated)
        protected virtual void OnCommand(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_SYSCOMMAND message (A window receives this message when the user
        // chooses a command from the window menu (also known as the System menu or Control
        // menu) or when the user chooses the Maximize button or Minimize button.)
        // Returns whether to call the default window proc or not after handling this message
        protected virtual bool OnSysCommand(IntPtr wParam, IntPtr lParam)
        {
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_CLOSE message (Sent as a signal that a window or an application
        // should terminate)
        protected virtual void OnClose(IntPtr wParam, IntPtr lParam)
        {
            // In SDL2 there is no DestroyWindow cascade like Win32's WM_DESTROY/WM_QUIT.
            // The MessagePump will exit its loop when it sets quit = true on SDL_QUIT.
            if (m_hWnd != IntPtr.Zero)
            {
                SDL.SDL_DestroyWindow(m_hWnd);
                m_hWnd = IntPtr.Zero;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the WM_DESTROY message (Sent when a window is being destroyed. It is sent
        // to the window procedure of the window being destroyed after the window is removed
        // from the screen)
        protected virtual void OnDestroy(IntPtr wParam, IntPtr lParam)
        {
            // In SDL2 there is no PostQuitMessage equivalent; the quit flag in MessagePump
            // handles termination instead.
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the SDL_JOYAXISMOTION message (SDL only).
        protected virtual void OnJoystickAxis(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected virtual void OnJoystickHatMotion(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // Handles the SDL_JOYBUTTONDOWN/-UP message (SDL only).
        protected virtual void OnJoystickButton(IntPtr wParam, IntPtr lParam)
        {

        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
