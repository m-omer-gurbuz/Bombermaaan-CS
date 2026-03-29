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
 *  \file Program.cs
 *  \brief Entry point of the program (C# port of WinMain.cpp)
 */

using Bombermaaan.SDL2;
using System;

namespace Bombermaaan
{
    internal static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                // Create the CGame instance
                CGame game = new CGame(IntPtr.Zero, args);

                // If creating the game failed
                if (!game.Create(args))
                {
                    // Get out, failure
                    return -1;
                }

                // Show the game window
                game.Show();

                // Update the game (message/event loop)
                game.MessagePump();

                // Destroy everything
                game.Destroy();

                // Everything went right
                return 0;
            }
            catch (Exception ex)
            {
                string msg = $"FATAL EXCEPTION: {ex.GetType().FullName}\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";
                try { System.IO.File.AppendAllText("crash.txt", msg + "\n"); } catch { }
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MESSAGEBOX_ERROR, "Bombermaaan Crash", msg, IntPtr.Zero);
                return -2;
            }
        }
    }
}
