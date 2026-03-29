/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
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
 *  \file CVideoSDL.cs
 *  \brief SDL video (C# port)
 */

using Bombermaaan.SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Bombermaaan
{
    // -----------------------------------------------------------------------
    // Shared data structures
    // -----------------------------------------------------------------------

    /// <summary>Describes a single sprite within a sprite table.</summary>
    public struct SSprite
    {
        public int SurfaceNumber;   ///< Index into CVideoSDL surface list
        public int ZoneX1;          // Top-left corner in the source surface
        public int ZoneY1;
        public int ZoneX2;          // Bottom-right corner in the source surface
        public int ZoneY2;
    }

    /// <summary>Describes an available display resolution.</summary>
    public struct SDisplayMode
    {
        public int Width;
        public int Height;
        public int Depth;
    }

    /// <summary>
    /// A queued request to blit a sprite to the screen at the next UpdateAll().
    /// The C++ std::priority_queue popped the item with the *largest* layer/priority
    /// first (drawn last = on top).  We replicate that by sorting the List ascending
    /// (lowest layer first) so sprites with higher layers are drawn later, ending up
    /// on top, which matches the original rendering order.
    /// </summary>
    public struct SDrawingRequest : IComparable<SDrawingRequest>
    {
        public const int PRIORITY_UNUSED = -1;

        public int PositionX;
        public int PositionY;
        public int ZoneX1;
        public int ZoneY1;
        public int ZoneX2;
        public int ZoneY2;
        public int SpriteTable;
        public int Sprite;
        public int SpriteLayer;
        public int PriorityInLayer;

        public int CompareTo(SDrawingRequest other)
        {
            if (SpriteLayer != other.SpriteLayer)
                return SpriteLayer.CompareTo(other.SpriteLayer);
            return PriorityInLayer.CompareTo(other.PriorityInLayer);
        }
    }

    /// <summary>A debug-only coloured rectangle drawing request.</summary>
    public struct SDebugDrawingRequest : IComparable<SDebugDrawingRequest>
    {
        public const int PRIORITY_UNUSED = -1;

        public int PositionX;
        public int PositionY;
        public int ZoneX1;
        public int ZoneY1;
        public int ZoneX2;
        public int ZoneY2;

        public byte R;
        public byte G;
        public byte B;

        public int SpriteLayer;
        public int PriorityInLayer;

        public int CompareTo(SDebugDrawingRequest other)
        {
            if (SpriteLayer != other.SpriteLayer)
                return SpriteLayer.CompareTo(other.SpriteLayer);
            return PriorityInLayer.CompareTo(other.PriorityInLayer);
        }
    }

    /// <summary>Wraps an SDL surface together with its blit parameters.</summary>
    public struct SSurface
    {
        public IntPtr pSurface;        ///< SDL_Surface* (opaque IntPtr)
        public uint   BlitParameters;  ///< Blit flags (transparency etc.)
    }

    // -----------------------------------------------------------------------
    // CVideoSDL
    // -----------------------------------------------------------------------

    /// <summary>SDL-based video back-end for Bombermaaan.</summary>
    public class CVideoSDL
    {
        // ---- private state ------------------------------------------------

        private IntPtr                          m_hWnd;
        private SDL.SDL_Rect                    m_rcScreen;
        private SDL.SDL_Rect                    m_rcViewport;
        private int                             m_Width;
        private int                             m_Height;
        private int                             m_Depth;
        private bool                            m_FullScreen;
        private IntPtr                          m_pBackBuffer;   ///< SDL_Surface* back buffer (reserved)
        private IntPtr                          m_pPrimary;      ///< SDL_Surface* primary (window surface)
        private IntPtr                          m_pWindow;       ///< SDL_Window* owning the primary surface
        private List<SSurface>                  m_Surfaces;
        private uint                            m_ColorKey;
        private List<SDrawingRequest>           m_DrawingRequests;
        private List<SDebugDrawingRequest>      m_DebugDrawingRequests;
        private Dictionary<int, List<SSprite>>  m_SpriteTables;
        private int                             m_OriginX;
        private int                             m_OriginY;
        private List<SDisplayMode>              m_AvailableDisplayModes;

        // ---- Additional SDL2 P/Invoke that are not yet in SDL2.cs ---------
        // These are declared here as private statics so we don't modify the
        // shared SDL2 wrapper.

        private const string SDL2Lib = "SDL2";

        [DllImport(SDL2Lib, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint SDL_MapRGBA(IntPtr format, byte r, byte g, byte b, byte a);

        [DllImport(SDL2Lib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr SDL_GetWindowFromID(uint id);

        [DllImport(SDL2Lib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetSurfaceAlphaMod(IntPtr surface, byte alpha);

        [DllImport(SDL2Lib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_SetSurfaceBlendMode(IntPtr surface, int blendMode);

        [DllImport(SDL2Lib, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SDL_FreeRW(IntPtr area);

        // Helper: extract SDL_Surface.format (IntPtr) from an SDL_Surface* (IntPtr).
        // Uses Marshal.PtrToStructure to respect platform pointer alignment.
        private static IntPtr GetSurfaceFormat(IntPtr surface)
        {
            SDL.SDL_Surface s = Marshal.PtrToStructure<SDL.SDL_Surface>(surface);
            return s.format;
        }

        // ---- construction -------------------------------------------------

        public CVideoSDL()
        {
            m_hWnd          = IntPtr.Zero;
            m_pBackBuffer   = IntPtr.Zero;
            m_pPrimary      = IntPtr.Zero;
            m_pWindow       = IntPtr.Zero;
            m_Width         = 0;
            m_Height        = 0;
            m_Depth         = 0;
            m_FullScreen    = false;
            m_ColorKey      = 0;
            m_OriginX       = 0;
            m_OriginY       = 0;
            m_rcScreen      = new SDL.SDL_Rect();
            m_rcViewport    = new SDL.SDL_Rect();

            m_Surfaces              = new List<SSurface>();
            m_DrawingRequests       = new List<SDrawingRequest>();
            m_DebugDrawingRequests  = new List<SDebugDrawingRequest>();
            m_SpriteTables          = new Dictionary<int, List<SSprite>>();
            m_AvailableDisplayModes = new List<SDisplayMode>();
        }

        // ---- inline / property accessors ----------------------------------

        public void SetWindowHandle(IntPtr hWnd)
        {
            m_hWnd = hWnd;
        }

        public bool IsModeSet(int Width, int Height, int Depth, bool FullScreen)
        {
            return m_Width      == Width      &&
                   m_Height     == Height     &&
                   m_Depth      == Depth      &&
                   m_FullScreen == FullScreen;
        }

        public void OnPaint()
        {
            UpdateScreen();
        }

        public void SetOrigin(int OriginX, int OriginY)
        {
            m_OriginX = OriginX;
            m_OriginY = OriginY;
        }

        public void SetNewPrimary(IntPtr pSurface)
        {
            m_pPrimary = pSurface;
        }

        public void SetWindowTitle(string title)
        {
            if (m_pWindow != IntPtr.Zero)
                SDL.SDL_SetWindowTitle(m_pWindow, title);
        }

        public void SetWindowIcon(string icoPath)
        {
            // The executable icon is embedded via ApplicationIcon.
            // Avoid pulling in System.Drawing just to decode .ico files at runtime.
            _ = icoPath;
        }

        // ---- helpers ------------------------------------------------------

        private static void AddDisplayMode(int width, int height, int depth,
                                           List<SDisplayMode> displayModes)
        {
            SDisplayMode mode;
            mode.Width  = width;
            mode.Height = height;
            mode.Depth  = depth;
            displayModes.Add(mode);
        }

        private static string GetSDLVideoError()
        {
            return SDL.SDL_GetErrorString();
        }

        /// <summary>Counts the set bits in a bitmask.</summary>
        private ushort GetNumberOfBits(uint dwMask)
        {
            ushort wBits = 0;
            while (dwMask != 0)
            {
                dwMask = dwMask & (dwMask - 1);
                wBits++;
            }
            return wBits;
        }

        // ---- Create / Destroy ---------------------------------------------

        /// <summary>
        /// Initialises the SDL window for the requested resolution.
        /// Equivalent to CVideoSDL::Create in the C++ source.
        /// </summary>
        public bool Create(int Width, int Height, int Depth, bool FullScreen)
        {
            m_Width      = Width;
            m_Height     = Height;
            m_Depth      = Depth;
            m_FullScreen = FullScreen;

            m_pBackBuffer = IntPtr.Zero;
            m_pPrimary    = IntPtr.Zero;
            m_ColorKey    = 0;

            // Populate the known-good display modes list.
            m_AvailableDisplayModes.Clear();
            AddDisplayMode(240, 234, 32, m_AvailableDisplayModes);
            AddDisplayMode(320, 240, 32, m_AvailableDisplayModes);
            AddDisplayMode(480, 442, 32, m_AvailableDisplayModes);
            AddDisplayMode(512, 384, 32, m_AvailableDisplayModes);
            AddDisplayMode(640, 480, 32, m_AvailableDisplayModes);

            bool validMode = (Width == 240 && Height == 234) ||
                             (Width == 320 && Height == 240) ||
                             (Width == 480 && Height == 442) ||
                             (Width == 512 && Height == 384) ||
                             (Width == 640 && Height == 480);

            if (!validMode)
            {
                CLog.GetLog().WriteLine("SDLVideo        => !!! Requested video mode {0}x{1} not found.", Width, Height);
                return false;
            }

            if (!FullScreen)
                CLog.GetLog().WriteLine("SDLVideo        => Initializing SDLVideo interface for windowed mode {0}x{1}.", Width, Height);
            else
                CLog.GetLog().WriteLine("SDLVideo        => Initializing SDLVideo interface for fullscreen mode {0}x{1}x{2}.", Width, Height, Depth);

            uint flags = SDL.SDL_WINDOW_SHOWN | SDL.SDL_WINDOW_OPENGL;
            if (FullScreen)
                flags |= SDL.SDL_WINDOW_FULLSCREEN;

            m_pWindow = SDL.SDL_CreateWindow(
                "Bombermaaan",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                Width, Height,
                flags);

            if (m_pWindow == IntPtr.Zero)
            {
                CLog.GetLog().WriteLine("SDLVideo        => !!! Requested video mode could not be set. (window)");
                return false;
            }

            // Obtain the window surface (SDL2 software-blit path)
            m_pPrimary = SDL.SDL_GetWindowSurface(m_pWindow);
            if (m_pPrimary == IntPtr.Zero)
            {
                CLog.GetLog().WriteLine("SDLVideo        => !!! Requested video mode could not be set. (primary surface)");
                return false;
            }

            // Viewport / screen rects
            m_rcViewport.x = 0;
            m_rcViewport.y = 0;
            m_rcViewport.w = Width;
            m_rcViewport.h = Height;
            m_rcScreen = m_rcViewport;

            SDL.SDL_ShowCursor(FullScreen ? 0 : 1);

            Clear();
            m_OriginX = 0;
            m_OriginY = 0;

            return true;
        }

        /// <summary>
        /// Releases all SDL resources.
        /// Equivalent to CVideoSDL::Destroy in the C++ source.
        /// </summary>
        public void Destroy()
        {
            FreeSprites();

            if (m_pPrimary != IntPtr.Zero)
            {
                if (m_FullScreen)
                    SDL.SDL_ShowCursor(1);

                if (m_pBackBuffer != IntPtr.Zero)
                {
                    SDL.SDL_FreeSurface(m_pBackBuffer);
                    m_pBackBuffer = IntPtr.Zero;
                    CLog.GetLog().WriteLine("SDLVideo        => Backbuffer surface was released.");
                }

                // The primary surface is owned by the window; do not free it directly.
                m_pPrimary = IntPtr.Zero;
                CLog.GetLog().WriteLine("SDLVideo        => Primary surface was released.");

                if (m_pWindow != IntPtr.Zero)
                {
                    SDL.SDL_DestroyWindow(m_pWindow);
                    m_pWindow = IntPtr.Zero;
                }

                CLog.GetLog().WriteLine("SDLVideo        => SDLVideo object was released.");
            }
        }

        // ---- Transparent colour -------------------------------------------

        /// <summary>Records the colour that will be treated as transparent on new surfaces.</summary>
        public bool SetTransparentColor(int Red, int Green, int Blue)
        {
            m_ColorKey = SDL.SDL_MapRGB(GetSurfaceFormat(m_pPrimary),
                                        (byte)Red, (byte)Green, (byte)Blue);
            return true;
        }

        // ---- LoadSprites (from byte array / embedded resource) ------------

        /// <summary>
        /// Loads sprites from a raw BMP byte array (e.g. an embedded resource).
        /// Replaces the HBITMAP overload from the C++ code.
        /// </summary>
        public bool LoadSprites(int SpriteTableWidth, int SpriteTableHeight,
                                int SpriteWidth,      int SpriteHeight,
                                bool Transparent,     int BMP_ID,
                                byte[] bmpData)
        {
            System.Diagnostics.Debug.Assert(bmpData != null && bmpData.Length > 0);

            GCHandle handle = GCHandle.Alloc(bmpData, GCHandleType.Pinned);
            IntPtr ddsd = IntPtr.Zero;
            try
            {
                IntPtr rw = SDL.SDL_RWFromMem(handle.AddrOfPinnedObject(), bmpData.Length);
                if (rw == IntPtr.Zero)
                {
                    CLog.GetLog().WriteLine("SDLVideo        => !!! Could not create RWops from byte array.");
                    return false;
                }
                ddsd = SDL.SDL_LoadBMP_RW(rw, 0);
                SDL_FreeRW(rw);
            }
            finally
            {
                handle.Free();
            }

            return FinishLoadSprites(ddsd, Transparent, BMP_ID,
                                     SpriteTableWidth, SpriteTableHeight,
                                     SpriteWidth, SpriteHeight);
        }

        // ---- LoadSprites (from file path) ---------------------------------

        /// <summary>
        /// Loads sprites from a BMP file on disk.
        /// Equivalent to the const char* overload in the C++ source.
        /// </summary>
        public bool LoadSprites(int SpriteTableWidth, int SpriteTableHeight,
                                int SpriteWidth,      int SpriteHeight,
                                bool Transparent,     int BMP_ID,
                                string file)
        {
            string path = Path.Combine("images", file);
            IntPtr ddsd = SDL.SDL_LoadBMP(path);

            return FinishLoadSprites(ddsd, Transparent, BMP_ID,
                                     SpriteTableWidth, SpriteTableHeight,
                                     SpriteWidth, SpriteHeight);
        }

        /// <summary>
        /// Loads a single-sprite BMP, auto-detecting sprite size from the actual image dimensions.
        /// Sprite size = (bmpWidth - 2) x (bmpHeight - 2) to account for the 1px border.
        /// </summary>
        public bool LoadSpritesAuto(bool Transparent, int BMP_ID, string file)
        {
            string path = Path.Combine("images", file);
            IntPtr ddsd = SDL.SDL_LoadBMP(path);
            if (ddsd == IntPtr.Zero) return false;

            SDL.SDL_Surface surf = System.Runtime.InteropServices.Marshal.PtrToStructure<SDL.SDL_Surface>(ddsd);
            int spriteW = surf.w - 2;
            int spriteH = surf.h - 2;

            return FinishLoadSprites(ddsd, Transparent, BMP_ID, 1, 1, spriteW, spriteH);
        }

        // ---- shared LoadSprites tail --------------------------------------

        private bool FinishLoadSprites(IntPtr ddsd, bool Transparent, int BMP_ID,
                                       int SpriteTableWidth, int SpriteTableHeight,
                                       int SpriteWidth,      int SpriteHeight)
        {
            if (ddsd == IntPtr.Zero)
            {
                CLog.GetLog().WriteLine("SDLVideo        => !!! Could not create surface.");
                CLog.GetLog().WriteLine("SDLVideo        => !!! SDLVideo error is : {0}.", GetSDLVideoError());
                return false;
            }

            SSurface surface = new SSurface();
            surface.pSurface       = ddsd;
            surface.BlitParameters = 0;

            if (Transparent)
            {
                // Colour key: pure green (0x00, 0xFF, 0x00) with full alpha
                uint key = SDL_MapRGBA(GetSurfaceFormat(ddsd), 0x00, 0xFF, 0x00, 0xFF);
                int hRet = SDL.SDL_SetColorKey(ddsd, 1 /* SDL_TRUE */, key);

                if (hRet != 0)
                {
                    CLog.GetLog().WriteLine("SDLVideo        => !!! Could not set colorkey.");
                    CLog.GetLog().WriteLine("SDLVideo        => !!! SDLVideo error is : {0}.", GetSDLVideoError());
                    return false;
                }
            }

            m_Surfaces.Add(surface);

            // Build the sprite table
            List<SSprite> spriteTable = new List<SSprite>();

            int ZoneX1 = 1;
            int ZoneY1 = 1;
            int ZoneX2 = 1 + SpriteWidth;
            int ZoneY2 = 1 + SpriteHeight;

            for (int Y = 0; Y < SpriteTableHeight; Y++)
            {
                for (int X = 0; X < SpriteTableWidth; X++)
                {
                    SSprite sprite = new SSprite();
                    sprite.SurfaceNumber = m_Surfaces.Count - 1;
                    sprite.ZoneX1 = ZoneX1;
                    sprite.ZoneY1 = ZoneY1;
                    sprite.ZoneX2 = ZoneX2;
                    sprite.ZoneY2 = ZoneY2;

                    ZoneX1 += SpriteWidth + 1;
                    ZoneX2 += SpriteWidth + 1;

                    spriteTable.Add(sprite);
                }

                ZoneX1 = 1;
                ZoneX2 = 1 + SpriteWidth;
                ZoneY1 += SpriteHeight + 1;
                ZoneY2 += SpriteHeight + 1;
            }

            m_SpriteTables[BMP_ID] = spriteTable;
            return true;
        }

        // ---- FreeSprites --------------------------------------------------

        /// <summary>Releases all sprite tables and SDL surfaces.</summary>
        public void FreeSprites()
        {
            m_DrawingRequests.Clear();
            m_SpriteTables.Clear();

            foreach (SSurface surf in m_Surfaces)
            {
                if (surf.pSurface != IntPtr.Zero)
                    SDL.SDL_FreeSurface(surf.pSurface);
            }

            m_Surfaces.Clear();
        }

        // ---- Window events ------------------------------------------------

        public void OnWindowMove()
        {
            // Nothing to do for SDL2
        }

        // ---- Clear --------------------------------------------------------

        /// <summary>Fills the primary surface with black.</summary>
        public void Clear()
        {
            SDL.SDL_FillRect(m_pPrimary, ref m_rcViewport, 0);
        }

        // ---- DrawSprite ---------------------------------------------------

        /// <summary>
        public int GetSpriteWidth(int SpriteTable, int Sprite)
        {
            SSprite s = m_SpriteTables[SpriteTable][Sprite];
            return s.ZoneX2 - s.ZoneX1;
        }

        public int GetSpriteHeight(int SpriteTable, int Sprite)
        {
            SSprite s = m_SpriteTables[SpriteTable][Sprite];
            return s.ZoneY2 - s.ZoneY1;
        }

        /// Queues a sprite blit request.  Executed in the correct layer/priority
        /// order during UpdateAll().
        /// </summary>
        public void DrawSprite(int PositionX, int PositionY,
                               RECT? pZone, RECT? pClip,
                               int SpriteTable, int Sprite,
                               int SpriteLayer, int PriorityInLayer)
        {
            SDrawingRequest dr = new SDrawingRequest();
            SSprite pSprite = m_SpriteTables[SpriteTable][Sprite];

            if (pClip.HasValue)
            {
                RECT clip = pClip.Value;
                int SpriteSizeX = pSprite.ZoneX2 - pSprite.ZoneX1;
                int SpriteSizeY = pSprite.ZoneY2 - pSprite.ZoneY1;

                // Completely outside?
                if (PositionX >= clip.right  ||
                    PositionY >= clip.bottom  ||
                    PositionX + SpriteSizeX < clip.left ||
                    PositionY + SpriteSizeY < clip.top)
                {
                    return;
                }

                // Left clip
                if (PositionX < clip.left)
                {
                    dr.PositionX = clip.left;
                    dr.ZoneX1    = pSprite.ZoneX1 + clip.left - PositionX;
                }
                else
                {
                    dr.PositionX = PositionX;
                    dr.ZoneX1    = pSprite.ZoneX1;
                }

                // Top clip
                if (PositionY < clip.top)
                {
                    dr.PositionY = clip.top;
                    dr.ZoneY1    = pSprite.ZoneY1 + clip.top - PositionY;
                }
                else
                {
                    dr.PositionY = PositionY;
                    dr.ZoneY1    = pSprite.ZoneY1;
                }

                // Right clip
                dr.ZoneX2 = (PositionX + SpriteSizeX >= clip.right)
                    ? pSprite.ZoneX1 + clip.right  - PositionX
                    : pSprite.ZoneX2;

                // Bottom clip
                dr.ZoneY2 = (PositionY + SpriteSizeY >= clip.bottom)
                    ? pSprite.ZoneY1 + clip.bottom - PositionY
                    : pSprite.ZoneY2;
            }
            else
            {
                dr.PositionX = PositionX;
                dr.PositionY = PositionY;
                dr.ZoneX1    = pSprite.ZoneX1;
                dr.ZoneY1    = pSprite.ZoneY1;
                dr.ZoneX2    = pSprite.ZoneX2;
                dr.ZoneY2    = pSprite.ZoneY2;
            }

            dr.PositionX      += m_OriginX;
            dr.PositionY      += m_OriginY;
            dr.SpriteTable     = SpriteTable;
            dr.Sprite          = Sprite;
            dr.SpriteLayer     = SpriteLayer;
            dr.PriorityInLayer = PriorityInLayer;

            m_DrawingRequests.Add(dr);
        }

        // ---- Debug rectangles ---------------------------------------------

        public void DrawDebugRectangle(int PositionX, int PositionY,
                                       int w, int h,
                                       byte r, byte g, byte b,
                                       int SpriteLayer, int PriorityInLayer)
        {
            SDebugDrawingRequest dr = new SDebugDrawingRequest();
            dr.PositionX      = PositionX + m_OriginX;
            dr.PositionY      = PositionY + m_OriginY;
            dr.ZoneX1         = 0;
            dr.ZoneY1         = 0;
            dr.ZoneX2         = w;
            dr.ZoneY2         = h;
            dr.R              = r;
            dr.G              = g;
            dr.B              = b;
            dr.SpriteLayer    = SpriteLayer;
            dr.PriorityInLayer = PriorityInLayer;

            m_DebugDrawingRequests.Add(dr);
        }

        public void RemoveAllDebugRectangles()
        {
            m_DebugDrawingRequests.Clear();
        }

        // ---- UpdateAll ----------------------------------------------------

        /// <summary>
        /// Executes all queued drawing requests in layer/priority order,
        /// then calls UpdateScreen().
        /// </summary>
        public void UpdateAll()
        {
            // Sort ascending: lowest layer/priority drawn first (behind).
            // Items with higher layer/priority are drawn last, ending up on top.
            m_DrawingRequests.Sort();

            foreach (SDrawingRequest dr in m_DrawingRequests)
            {
                SSprite pSprite = m_SpriteTables[dr.SpriteTable][dr.Sprite];

                SDL.SDL_Rect srcRect = new SDL.SDL_Rect();
                srcRect.x = dr.ZoneX1;
                srcRect.y = dr.ZoneY1;
                srcRect.w = dr.ZoneX2 - dr.ZoneX1;
                srcRect.h = dr.ZoneY2 - dr.ZoneY1;

                SDL.SDL_Rect dstRect = new SDL.SDL_Rect();
                dstRect.x = dr.PositionX;
                dstRect.y = dr.PositionY;
                dstRect.w = 0;
                dstRect.h = 0;

                IntPtr srcSurface = m_Surfaces[pSprite.SurfaceNumber].pSurface;
                if (SDL.SDL_BlitSurface(srcSurface, ref srcRect, m_pPrimary, ref dstRect) < 0)
                {
                    CLog.GetLog().WriteLine("SDLVideo        => !!! SDLVideo error is : {0}.", GetSDLVideoError());
                }
            }

            m_DrawingRequests.Clear();

            // Debug rectangles — not cleared here; call RemoveAllDebugRectangles explicitly.
            m_DebugDrawingRequests.Sort();

            foreach (SDebugDrawingRequest dr in m_DebugDrawingRequests)
            {
                SDL.SDL_Rect srcRect = new SDL.SDL_Rect();
                srcRect.x = dr.ZoneX1;
                srcRect.y = dr.ZoneY1;
                srcRect.w = dr.ZoneX2 - dr.ZoneX1;
                srcRect.h = dr.ZoneY2 - dr.ZoneY1;

                SDL.SDL_Rect dstRect = new SDL.SDL_Rect();
                dstRect.x = dr.PositionX;
                dstRect.y = dr.PositionY;
                dstRect.w = 0;
                dstRect.h = 0;

                // Create a temporary 32-bit surface for the semi-transparent debug rect.
                // Little-endian masks (SDL2 default on x86/x64).
                IntPtr rectangle = SDL.SDL_CreateRGBSurface(
                    0,
                    srcRect.w, srcRect.h,
                    32,
                    0x000000ffu, 0x0000ff00u, 0x00ff0000u, 0xff000000u);

                if (rectangle != IntPtr.Zero)
                {
                    SDL_SetSurfaceAlphaMod(rectangle, 128);
                    SDL_SetSurfaceBlendMode(rectangle, SDL.SDL_BLENDMODE_BLEND);

                    uint colour = SDL_MapRGBA(GetSurfaceFormat(rectangle),
                                             dr.R, dr.G, dr.B, 128);

                    SDL.SDL_Rect fillRect = srcRect;
                    SDL.SDL_FillRect(rectangle, ref fillRect, colour);

                    if (SDL.SDL_BlitSurface(rectangle, ref srcRect, m_pPrimary, ref dstRect) < 0)
                    {
                        CLog.GetLog().WriteLine("SDLVideo        => !!! SDLVideo error is : {0}.", GetSDLVideoError());
                    }

                    SDL.SDL_FreeSurface(rectangle);
                }
            }

            UpdateScreen();
        }

        // ---- UpdateScreen -------------------------------------------------

        /// <summary>Presents the primary surface to the screen.</summary>
        public void UpdateScreen()
        {
            while (true)
            {
                int hRet = SDL.SDL_UpdateWindowSurface(m_pWindow);
                SDL.SDL_Delay(15);

                if (hRet == 0)
                    break;

                CLog.GetLog().WriteLine("SDLVideo        => !!! Updating failed (switching primary/backbuffer).");
                CLog.GetLog().WriteLine("SDLVideo        => !!! SDLVideo error is : {0}.", GetSDLVideoError());
                // Avoid an infinite loop on persistent errors.
                break;
            }
        }

        // ---- IsModeAvailable ----------------------------------------------

        public bool IsModeAvailable(int Width, int Height, int Depth)
        {
            foreach (SDisplayMode mode in m_AvailableDisplayModes)
            {
                if (mode.Width == Width && mode.Height == Height && mode.Depth == Depth)
                    return true;
            }
            return false;
        }
    }
}
