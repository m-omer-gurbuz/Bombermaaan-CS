// SDL2# - C# Wrapper for SDL2
// Minimal SDL2 P/Invoke bindings for Bombermaaan
// Based on SDL2-CS by Ethan Lee (public domain)

using System;
using System.Runtime.InteropServices;

namespace Bombermaaan.SDL2
{
    public static class SDL
    {
        private const string nativeLibName = "SDL2";

        // SDL_init flags
        public const uint SDL_INIT_TIMER       = 0x00000001;
        public const uint SDL_INIT_AUDIO       = 0x00000010;
        public const uint SDL_INIT_VIDEO       = 0x00000020;
        public const uint SDL_INIT_JOYSTICK    = 0x00000200;
        public const uint SDL_INIT_HAPTIC      = 0x00001000;
        public const uint SDL_INIT_GAMECONTROLLER = 0x00002000;
        public const uint SDL_INIT_EVENTS      = 0x00004000;
        public const uint SDL_INIT_NOPARACHUTE = 0x00100000;
        public const uint SDL_INIT_EVERYTHING  = (
            SDL_INIT_TIMER | SDL_INIT_AUDIO | SDL_INIT_VIDEO |
            SDL_INIT_EVENTS | SDL_INIT_JOYSTICK | SDL_INIT_HAPTIC |
            SDL_INIT_GAMECONTROLLER
        );

        // SDL_WindowFlags
        public const uint SDL_WINDOW_FULLSCREEN         = 0x00000001;
        public const uint SDL_WINDOW_OPENGL             = 0x00000002;
        public const uint SDL_WINDOW_SHOWN              = 0x00000004;
        public const uint SDL_WINDOW_HIDDEN             = 0x00000008;
        public const uint SDL_WINDOW_BORDERLESS         = 0x00000010;
        public const uint SDL_WINDOW_RESIZABLE          = 0x00000020;
        public const uint SDL_WINDOW_MINIMIZED          = 0x00000040;
        public const uint SDL_WINDOW_MAXIMIZED          = 0x00000080;
        public const uint SDL_WINDOW_INPUT_GRABBED      = 0x00000100;
        public const uint SDL_WINDOW_INPUT_FOCUS        = 0x00000200;
        public const uint SDL_WINDOW_MOUSE_FOCUS        = 0x00000400;
        public const uint SDL_WINDOW_FULLSCREEN_DESKTOP = (SDL_WINDOW_FULLSCREEN | 0x00001000);
        public const uint SDL_WINDOW_FOREIGN            = 0x00000800;
        public const uint SDL_MESSAGEBOX_ERROR          = 0x00000010;

        public const int SDL_WINDOWPOS_UNDEFINED        = 0x1FFF0000;
        public const int SDL_WINDOWPOS_CENTERED         = 0x2FFF0000;

        // SDL_RendererFlags
        public const uint SDL_RENDERER_SOFTWARE      = 0x00000001;
        public const uint SDL_RENDERER_ACCELERATED   = 0x00000002;
        public const uint SDL_RENDERER_PRESENTVSYNC  = 0x00000004;
        public const uint SDL_RENDERER_TARGETTEXTURE = 0x00000008;

        // SDL_TextureAccess
        public const int SDL_TEXTUREACCESS_STATIC    = 0;
        public const int SDL_TEXTUREACCESS_STREAMING = 1;
        public const int SDL_TEXTUREACCESS_TARGET    = 2;

        // SDL_BlendMode
        public const int SDL_BLENDMODE_NONE  = 0x00000000;
        public const int SDL_BLENDMODE_BLEND = 0x00000001;
        public const int SDL_BLENDMODE_ADD   = 0x00000002;
        public const int SDL_BLENDMODE_MOD   = 0x00000004;

        // SDL_PixelFormat
        public const uint SDL_PIXELFORMAT_UNKNOWN     = 0;
        public const uint SDL_PIXELFORMAT_RGB24       = 0x17101803;
        public const uint SDL_PIXELFORMAT_BGR24       = 0x17401803;
        public const uint SDL_PIXELFORMAT_ARGB8888    = 0x16362004;
        public const uint SDL_PIXELFORMAT_RGBA8888    = 0x16462004;
        public const uint SDL_PIXELFORMAT_ABGR8888    = 0x16762004;
        public const uint SDL_PIXELFORMAT_BGRA8888    = 0x16862004;

        // SDL_EventType
        public const uint SDL_QUIT              = 0x100;
        public const uint SDL_KEYDOWN           = 0x300;
        public const uint SDL_KEYUP             = 0x301;
        public const uint SDL_MOUSEMOTION       = 0x400;
        public const uint SDL_MOUSEBUTTONDOWN   = 0x401;
        public const uint SDL_MOUSEBUTTONUP     = 0x402;
        public const uint SDL_JOYAXISMOTION     = 0x600;
        public const uint SDL_JOYBALLMOTION     = 0x601;
        public const uint SDL_JOYHATMOTION      = 0x602;
        public const uint SDL_JOYBUTTONDOWN     = 0x603;
        public const uint SDL_JOYBUTTONUP       = 0x604;
        public const uint SDL_JOYDEVICEADDED    = 0x605;
        public const uint SDL_JOYDEVICEREMOVED  = 0x606;
        public const uint SDL_WINDOWEVENT       = 0x200;
        public const uint SDL_USEREVENT         = 0x8000;

        // SDL_WindowEventID
        public const byte SDL_WINDOWEVENT_NONE          = 0;
        public const byte SDL_WINDOWEVENT_SHOWN         = 1;
        public const byte SDL_WINDOWEVENT_HIDDEN        = 2;
        public const byte SDL_WINDOWEVENT_EXPOSED       = 3;
        public const byte SDL_WINDOWEVENT_MOVED         = 4;
        public const byte SDL_WINDOWEVENT_RESIZED       = 5;
        public const byte SDL_WINDOWEVENT_SIZE_CHANGED  = 6;
        public const byte SDL_WINDOWEVENT_MINIMIZED     = 7;
        public const byte SDL_WINDOWEVENT_MAXIMIZED     = 8;
        public const byte SDL_WINDOWEVENT_RESTORED      = 9;
        public const byte SDL_WINDOWEVENT_ENTER         = 10;
        public const byte SDL_WINDOWEVENT_LEAVE         = 11;
        public const byte SDL_WINDOWEVENT_FOCUS_GAINED  = 12;
        public const byte SDL_WINDOWEVENT_FOCUS_LOST    = 13;
        public const byte SDL_WINDOWEVENT_CLOSE         = 14;

        // Hat positions
        public const byte SDL_HAT_CENTERED  = 0x00;
        public const byte SDL_HAT_UP        = 0x01;
        public const byte SDL_HAT_RIGHT     = 0x02;
        public const byte SDL_HAT_DOWN      = 0x04;
        public const byte SDL_HAT_LEFT      = 0x08;
        public const byte SDL_HAT_RIGHTUP   = (SDL_HAT_RIGHT | SDL_HAT_UP);
        public const byte SDL_HAT_RIGHTDOWN = (SDL_HAT_RIGHT | SDL_HAT_DOWN);
        public const byte SDL_HAT_LEFTUP    = (SDL_HAT_LEFT  | SDL_HAT_UP);
        public const byte SDL_HAT_LEFTDOWN  = (SDL_HAT_LEFT  | SDL_HAT_DOWN);

        // SDL_Scancode - commonly used keys
        public const int SDL_SCANCODE_UNKNOWN   = 0;
        public const int SDL_SCANCODE_A         = 4;
        public const int SDL_SCANCODE_B         = 5;
        public const int SDL_SCANCODE_C         = 6;
        public const int SDL_SCANCODE_D         = 7;
        public const int SDL_SCANCODE_E         = 8;
        public const int SDL_SCANCODE_F         = 9;
        public const int SDL_SCANCODE_G         = 10;
        public const int SDL_SCANCODE_H         = 11;
        public const int SDL_SCANCODE_I         = 12;
        public const int SDL_SCANCODE_J         = 13;
        public const int SDL_SCANCODE_K         = 14;
        public const int SDL_SCANCODE_L         = 15;
        public const int SDL_SCANCODE_M         = 16;
        public const int SDL_SCANCODE_N         = 17;
        public const int SDL_SCANCODE_O         = 18;
        public const int SDL_SCANCODE_P         = 19;
        public const int SDL_SCANCODE_Q         = 20;
        public const int SDL_SCANCODE_R         = 21;
        public const int SDL_SCANCODE_S         = 22;
        public const int SDL_SCANCODE_T         = 23;
        public const int SDL_SCANCODE_U         = 24;
        public const int SDL_SCANCODE_V         = 25;
        public const int SDL_SCANCODE_W         = 26;
        public const int SDL_SCANCODE_X         = 27;
        public const int SDL_SCANCODE_Y         = 28;
        public const int SDL_SCANCODE_Z         = 29;
        public const int SDL_SCANCODE_1         = 30;
        public const int SDL_SCANCODE_2         = 31;
        public const int SDL_SCANCODE_3         = 32;
        public const int SDL_SCANCODE_4         = 33;
        public const int SDL_SCANCODE_5         = 34;
        public const int SDL_SCANCODE_6         = 35;
        public const int SDL_SCANCODE_7         = 36;
        public const int SDL_SCANCODE_8         = 37;
        public const int SDL_SCANCODE_9         = 38;
        public const int SDL_SCANCODE_0         = 39;
        public const int SDL_SCANCODE_RETURN    = 40;
        public const int SDL_SCANCODE_ESCAPE    = 41;
        public const int SDL_SCANCODE_BACKSPACE = 42;
        public const int SDL_SCANCODE_TAB       = 43;
        public const int SDL_SCANCODE_SPACE     = 44;
        public const int SDL_SCANCODE_F1        = 58;
        public const int SDL_SCANCODE_F2        = 59;
        public const int SDL_SCANCODE_F3        = 60;
        public const int SDL_SCANCODE_F4        = 61;
        public const int SDL_SCANCODE_F5        = 62;
        public const int SDL_SCANCODE_F6        = 63;
        public const int SDL_SCANCODE_F7        = 64;
        public const int SDL_SCANCODE_F8        = 65;
        public const int SDL_SCANCODE_F9        = 66;
        public const int SDL_SCANCODE_F10       = 67;
        public const int SDL_SCANCODE_F11       = 68;
        public const int SDL_SCANCODE_F12       = 69;
        public const int SDL_SCANCODE_RIGHT     = 79;
        public const int SDL_SCANCODE_LEFT      = 80;
        public const int SDL_SCANCODE_DOWN      = 81;
        public const int SDL_SCANCODE_UP        = 82;
        public const int SDL_SCANCODE_LCTRL     = 224;
        public const int SDL_SCANCODE_LSHIFT    = 225;
        public const int SDL_SCANCODE_LALT      = 226;
        public const int SDL_SCANCODE_RCTRL     = 228;
        public const int SDL_SCANCODE_RSHIFT    = 229;
        public const int SDL_SCANCODE_RALT      = 230;
        public const int SDL_NUM_SCANCODES      = 512;

        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_Rect
        {
            public int x, y;
            public int w, h;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_Point
        {
            public int x, y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_Color
        {
            public byte r, g, b, a;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_Keysym
        {
            public int scancode;
            public int sym;
            public ushort mod;
            public uint scancode_unused;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_KeyboardEvent
        {
            public uint type;
            public uint timestamp;
            public uint windowID;
            public byte state;
            public byte repeat;
            public byte padding2;
            public byte padding3;
            public SDL_Keysym keysym;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_WindowEvent
        {
            public uint type;
            public uint timestamp;
            public uint windowID;
            public byte windowEvent;
            public byte padding1;
            public byte padding2;
            public byte padding3;
            public int data1;
            public int data2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_JoyAxisEvent
        {
            public uint type;
            public uint timestamp;
            public int which;
            public byte axis;
            public byte padding1;
            public byte padding2;
            public byte padding3;
            public short value;
            public ushort padding4;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_JoyHatEvent
        {
            public uint type;
            public uint timestamp;
            public int which;
            public byte hat;
            public byte value;
            public byte padding1;
            public byte padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_JoyButtonEvent
        {
            public uint type;
            public uint timestamp;
            public int which;
            public byte button;
            public byte state;
            public byte padding1;
            public byte padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_QuitEvent
        {
            public uint type;
            public uint timestamp;
        }

        [StructLayout(LayoutKind.Explicit, Size = 56)]
        public struct SDL_Event
        {
            [FieldOffset(0)] public uint type;
            [FieldOffset(0)] public SDL_KeyboardEvent key;
            [FieldOffset(0)] public SDL_WindowEvent window;
            [FieldOffset(0)] public SDL_JoyAxisEvent jaxis;
            [FieldOffset(0)] public SDL_JoyHatEvent jhat;
            [FieldOffset(0)] public SDL_JoyButtonEvent jbutton;
            [FieldOffset(0)] public SDL_QuitEvent quit;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_PixelFormat
        {
            public uint format;
            public IntPtr palette;
            public byte BitsPerPixel;
            public byte BytesPerPixel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] padding;
            public uint Rmask;
            public uint Gmask;
            public uint Bmask;
            public uint Amask;
            public byte Rloss;
            public byte Gloss;
            public byte Bloss;
            public byte Aloss;
            public byte Rshift;
            public byte Gshift;
            public byte Bshift;
            public byte Ashift;
            public int refcount;
            public IntPtr next;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_Surface
        {
            public uint flags;
            public IntPtr format;
            public int w, h;
            public int pitch;
            public IntPtr pixels;
            public IntPtr userdata;
            public int locked;
            public IntPtr lock_data;
            public SDL_Rect clip_rect;
            public IntPtr map;
            public int refcount;
        }

        // Core SDL functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_Init(uint flags);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_Quit();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_GetError();

        public static string SDL_GetErrorString()
        {
            return Marshal.PtrToStringAnsi(SDL_GetError()) ?? "";
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_ClearError();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_GetTicks();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_Delay(uint ms);

        // Window functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateWindow(
            [In()][MarshalAs(UnmanagedType.LPStr)] string title,
            int x, int y, int w, int h, uint flags
        );

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_DestroyWindow(IntPtr window);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_ShowWindow(IntPtr window);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_HideWindow(IntPtr window);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_SetWindowTitle(IntPtr window,
            [In()][MarshalAs(UnmanagedType.LPStr)] string title);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_SetWindowIcon(IntPtr window, IntPtr icon);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_ShowSimpleMessageBox(
            uint flags,
            [In()][MarshalAs(UnmanagedType.LPStr)] string title,
            [In()][MarshalAs(UnmanagedType.LPStr)] string message,
            IntPtr window
        );

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GetWindowPosition(IntPtr window, out int x, out int y);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_SetWindowPosition(IntPtr window, int x, int y);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GetWindowSize(IntPtr window, out int w, out int h);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_GetWindowID(IntPtr window);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetWindowFullscreen(IntPtr window, uint flags);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_GetWindowSurface(IntPtr window);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_UpdateWindowSurface(IntPtr window);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_WarpMouseInWindow(IntPtr window, int x, int y);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_ShowCursor(int toggle);

        // Renderer functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateRenderer(IntPtr window, int index, uint flags);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_DestroyRenderer(IntPtr renderer);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderClear(IntPtr renderer);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_RenderPresent(IntPtr renderer);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture,
            ref SDL_Rect srcrect, ref SDL_Rect dstrect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture,
            IntPtr srcrect, ref SDL_Rect dstrect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture,
            ref SDL_Rect srcrect, IntPtr dstrect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderCopy(IntPtr renderer, IntPtr texture,
            IntPtr srcrect, IntPtr dstrect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetRenderDrawColor(IntPtr renderer, byte r, byte g, byte b, byte a);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderFillRect(IntPtr renderer, ref SDL_Rect rect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderDrawRect(IntPtr renderer, ref SDL_Rect rect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetRenderTarget(IntPtr renderer, IntPtr texture);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderSetClipRect(IntPtr renderer, ref SDL_Rect rect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RenderSetClipRect(IntPtr renderer, IntPtr rect);

        // Texture functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateTexture(IntPtr renderer, uint format,
            int access, int w, int h);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateTextureFromSurface(IntPtr renderer, IntPtr surface);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_DestroyTexture(IntPtr texture);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetTextureBlendMode(IntPtr texture, int blendMode);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetTextureColorMod(IntPtr texture, byte r, byte g, byte b);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetTextureAlphaMod(IntPtr texture, byte alpha);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_QueryTexture(IntPtr texture, out uint format,
            out int access, out int w, out int h);

        // Surface functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateRGBSurface(uint flags, int width, int height,
            int depth, uint Rmask, uint Gmask, uint Bmask, uint Amask);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_CreateRGBSurfaceFrom(IntPtr pixels, int width,
            int height, int depth, int pitch, uint Rmask, uint Gmask, uint Bmask, uint Amask);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_FreeSurface(IntPtr surface);

        // SDL_BlitSurface is a macro for SDL_UpperBlit since SDL2 2.0.x
        [DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_BlitSurface(IntPtr src, ref SDL_Rect srcrect,
            IntPtr dst, ref SDL_Rect dstrect);

        [DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_BlitSurface(IntPtr src, IntPtr srcrect,
            IntPtr dst, ref SDL_Rect dstrect);

        [DllImport(nativeLibName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_BlitSurface(IntPtr src, ref SDL_Rect srcrect,
            IntPtr dst, IntPtr dstrect);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_FillRect(IntPtr dst, ref SDL_Rect rect, uint color);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_FillRect(IntPtr dst, IntPtr rect, uint color);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetColorKey(IntPtr surface, int flag, uint key);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_MapRGB(IntPtr format, byte r, byte g, byte b);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_LockSurface(IntPtr surface);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_UnlockSurface(IntPtr surface);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_LoadBMP_RW(IntPtr src, int freesrc);

        public static IntPtr SDL_LoadBMP(string file)
        {
            return SDL_LoadBMP_RW(SDL_RWFromFile(file, "rb"), 1);
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_RWFromFile(
            [In()][MarshalAs(UnmanagedType.LPStr)] string file,
            [In()][MarshalAs(UnmanagedType.LPStr)] string mode
        );

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_RWFromMem(IntPtr mem, int size);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_RWclose(IntPtr context);

        // SDL_ENABLE / SDL_DISABLE for joystick event state
        public const int SDL_ENABLE = 1;
        public const int SDL_DISABLE = 0;

        // SDL_Keymod flags (modifier key bitmask)
        public enum SDL_Keymod : int
        {
            KMOD_NONE   = 0x0000,
            KMOD_LSHIFT = 0x0001,
            KMOD_RSHIFT = 0x0002,
            KMOD_LCTRL  = 0x0040,
            KMOD_RCTRL  = 0x0080,
            KMOD_LALT   = 0x0100,
            KMOD_RALT   = 0x0200,
            KMOD_LGUI   = 0x0400,
            KMOD_RGUI   = 0x0800,
            KMOD_NUM    = 0x1000,
            KMOD_CAPS   = 0x2000,
            KMOD_MODE   = 0x4000,
            KMOD_CTRL   = KMOD_LCTRL  | KMOD_RCTRL,
            KMOD_SHIFT  = KMOD_LSHIFT | KMOD_RSHIFT,
            KMOD_ALT    = KMOD_LALT   | KMOD_RALT,
            KMOD_GUI    = KMOD_LGUI   | KMOD_RGUI,
        }

        // SDL_Keycode — virtual key codes (subset of SDLK_* values)
        public enum SDL_Keycode : int
        {
            SDLK_UNKNOWN = 0,
            SDLK_RETURN  = '\r',
            SDLK_ESCAPE  = 27,
            SDLK_BACKSPACE = '\b',
            SDLK_TAB     = '\t',
            SDLK_SPACE   = ' ',
            SDLK_F1      = (1 << 30) | SDL_SCANCODE_F1,
            SDLK_F2      = (1 << 30) | SDL_SCANCODE_F2,
            SDLK_F3      = (1 << 30) | SDL_SCANCODE_F3,
            SDLK_F4      = (1 << 30) | SDL_SCANCODE_F4,
            SDLK_F5      = (1 << 30) | SDL_SCANCODE_F5,
            SDLK_F6      = (1 << 30) | SDL_SCANCODE_F6,
            SDLK_F7      = (1 << 30) | SDL_SCANCODE_F7,
            SDLK_F8      = (1 << 30) | SDL_SCANCODE_F8,
            SDLK_F9      = (1 << 30) | SDL_SCANCODE_F9,
            SDLK_F10     = (1 << 30) | SDL_SCANCODE_F10,
            SDLK_F11     = (1 << 30) | SDL_SCANCODE_F11,
            SDLK_F12     = (1 << 30) | SDL_SCANCODE_F12,
            SDLK_RIGHT   = (1 << 30) | SDL_SCANCODE_RIGHT,
            SDLK_LEFT    = (1 << 30) | SDL_SCANCODE_LEFT,
            SDLK_DOWN    = (1 << 30) | SDL_SCANCODE_DOWN,
            SDLK_UP      = (1 << 30) | SDL_SCANCODE_UP,
        }

        // Event functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_PollEvent(out SDL_Event sdlEvent);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_WaitEvent(out SDL_Event sdlEvent);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_PumpEvents();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_PushEvent(ref SDL_Event sdlEvent);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_JoystickEventState(int state);

        // Keyboard functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_GetKeyboardState(out int numkeys);

        public static unsafe byte[] SDL_GetKeyboardStateArray()
        {
            int numkeys;
            IntPtr ptr = SDL_GetKeyboardState(out numkeys);
            byte[] state = new byte[numkeys];
            Marshal.Copy(ptr, state, 0, numkeys);
            return state;
        }

        // Joystick functions
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_NumJoysticks();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_JoystickOpen(int device_index);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_JoystickClose(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_JoystickNumAxes(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_JoystickNumButtons(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_JoystickNumHats(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern short SDL_JoystickGetAxis(IntPtr joystick, int axis);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte SDL_JoystickGetButton(IntPtr joystick, int button);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte SDL_JoystickGetHat(IntPtr joystick, int hat);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_JoystickInstanceID(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_JoystickUpdate();

        // Display mode
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GetNumDisplayModes(int displayIndex);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GetDisplayMode(int displayIndex, int modeIndex,
            out SDL_DisplayMode mode);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GetCurrentDisplayMode(int displayIndex,
            out SDL_DisplayMode mode);

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_DisplayMode
        {
            public uint format;
            public int w;
            public int h;
            public int refresh_rate;
            public IntPtr driverdata;
        }

        // Clipboard
        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_SetClipboardText(
            [In()][MarshalAs(UnmanagedType.LPStr)] string text);
    }

    public static class SDL_mixer
    {
        private const string nativeLibName = "SDL2_mixer";

        public const int MIX_DEFAULT_FREQUENCY = 44100;
        public const ushort MIX_DEFAULT_FORMAT = 0x8010; // AUDIO_S16LSB
        public const int MIX_DEFAULT_CHANNELS = 2;
        public const int MIX_MAX_VOLUME = 128;

        public const int MIX_INIT_FLAC = 0x00000001;
        public const int MIX_INIT_MOD  = 0x00000002;
        public const int MIX_INIT_MP3  = 0x00000008;
        public const int MIX_INIT_OGG  = 0x00000010;
        public const int MIX_INIT_MID  = 0x00000020;

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_Init(int flags);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_Quit();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_OpenAudio(int frequency, ushort format, int channels, int chunksize);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_CloseAudio();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Mix_LoadWAV_RW(IntPtr src, int freesrc);

        public static IntPtr Mix_LoadWAV(string file)
        {
            return Mix_LoadWAV_RW(SDL.SDL_RWFromFile(file, "rb"), 1);
        }

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_FreeChunk(IntPtr chunk);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_PlayChannel(int channel, IntPtr chunk, int loops);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_HaltChannel(int channel);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_HaltGroup(int tag);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_HaltMusic();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_Volume(int channel, int volume);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_VolumeChunk(IntPtr chunk, int volume);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_VolumeMusic(int volume);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Mix_LoadMUS(
            [In()][MarshalAs(UnmanagedType.LPStr)] string file);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Mix_LoadMUS_RW(IntPtr src, int freesrc);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_FreeMusic(IntPtr music);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_PlayMusic(IntPtr music, int loops);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_FadeInMusic(IntPtr music, int loops, int ms);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_FadeOutMusic(int ms);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_PauseMusic();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_ResumeMusic();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_PlayingMusic();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_Playing(int channel);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_Pause(int channel);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Mix_Resume(int channel);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int Mix_AllocateChannels(int numchans);

        // Mix_GetError is a macro: #define Mix_GetError SDL_GetError
        public static IntPtr Mix_GetError()
        {
            return SDL.SDL_GetError();
        }

        public static string Mix_GetErrorString()
        {
            return Marshal.PtrToStringAnsi(Mix_GetError()) ?? "";
        }
    }

    // SDL_image subset (for loading BMP/PNG images)
    public static class SDL_image
    {
        private const string nativeLibName = "SDL2_image";

        public const int IMG_INIT_JPG = 0x00000001;
        public const int IMG_INIT_PNG = 0x00000002;
        public const int IMG_INIT_TIF = 0x00000004;

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int IMG_Init(int flags);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void IMG_Quit();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IMG_Load(
            [In()][MarshalAs(UnmanagedType.LPStr)] string file);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr IMG_Load_RW(IntPtr src, int freesrc);
    }
}
