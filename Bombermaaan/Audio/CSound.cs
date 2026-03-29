/************************************************************************************

    Copyright (C) 2000-2002, 2007 Thibaut Tollemer
    Copyright (C) 2007 Bernd Arnold
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
 *  \file CSound.cs
 *  \brief Sound
 */

using Bombermaaan.SDL2;
using System;
using System.IO;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public enum ESample
    {
        SAMPLE_BOMB_DROP = 0,
        SAMPLE_BOMBER_DEATH,
        SAMPLE_BOMB_BOUNCE,
        SAMPLE_BOMBER_PUNCH,
        SAMPLE_BOMBER_THROW,
        SAMPLE_BOMBER_LOSE_ITEM,
        SAMPLE_BREAK_1,
        SAMPLE_BREAK_2,
        SAMPLE_DRAW_GAME,
        SAMPLE_DRAW_GAME_VOICE,
        SAMPLE_EXPLOSION_01_1,
        SAMPLE_EXPLOSION_01_2,
        SAMPLE_EXPLOSION_02_1,
        SAMPLE_EXPLOSION_02_2,
        SAMPLE_EXPLOSION_03_1,
        SAMPLE_EXPLOSION_03_2,
        SAMPLE_EXPLOSION_04_1,
        SAMPLE_EXPLOSION_04_2,
        SAMPLE_EXPLOSION_05_1,
        SAMPLE_EXPLOSION_05_2,
        SAMPLE_EXPLOSION_06_1,
        SAMPLE_EXPLOSION_06_2,
        SAMPLE_EXPLOSION_07_1,
        SAMPLE_EXPLOSION_07_2,
        SAMPLE_EXPLOSION_08_1,
        SAMPLE_EXPLOSION_08_2,
        SAMPLE_EXPLOSION_09_1,
        SAMPLE_EXPLOSION_09_2,
        SAMPLE_EXPLOSION_10_1,
        SAMPLE_EXPLOSION_10_2,
        SAMPLE_HURRY,
        SAMPLE_ITEM_FUMES,
        SAMPLE_MENU_NEXT,
        SAMPLE_MENU_PREVIOUS,
        SAMPLE_MENU_BEEP,
        SAMPLE_MENU_ERROR,
        SAMPLE_PAUSE,
        SAMPLE_PICK_ITEM_1,
        SAMPLE_PICK_ITEM_2,
        SAMPLE_RING_DING,
        SAMPLE_SICK_1,
        SAMPLE_SICK_2,
        SAMPLE_SICK_3,
        SAMPLE_VICTORY,
        SAMPLE_VICTORY_VOICE,
        SAMPLE_WALL_CLAP_1,
        SAMPLE_WALL_CLAP_2,
        SAMPLE_WINNER,
        NUM_SAMPLES
    }

    public enum ESong
    {
        SONG_NONE = -1,
        SONG_MATCH_MUSIC = 0,
        SONG_MENU_MUSIC,
        SONG_CONTROLS_MUSIC = 3,
        SONG_GREET_MUSIC = SONG_CONTROLS_MUSIC,
        SONG_TITLE_MUSIC,
        NUM_SONGS
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>CSound handles the songs and samples.</summary>
    public class CSound
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        // SDL_mixer constant: maximum volume value (128)
        private const int MIX_MAX_VOLUME = 128;

        private bool      m_GlobalPause;                              //!< Is the sound paused?
        private bool      m_SoundOK;                                  //!< Could SDL_mixer be initialised? This may be false if there is no sound card
        private IntPtr[]  m_Samples = new IntPtr[(int)ESample.NUM_SAMPLES]; //!< The available samples (Mix_Chunk*)
        private IntPtr    m_CurrentSong;                              //!< The current song (Mix_Music*)
        private ESong     m_ESong;                                    //!< Current song number

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public CSound()
        {
            // Sound is unpaused
            m_GlobalPause = false;

            // Reset the sample and song pointers
            for (int i = 0; i < (int)ESample.NUM_SAMPLES; i++)
            {
                m_Samples[i] = IntPtr.Zero;
            }

            m_CurrentSong = IntPtr.Zero;
            m_ESong = ESong.SONG_NONE;

            m_SoundOK = false;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>No-op stub: Windows module handle is not used in the SDL port.</summary>
        public void SetModuleHandle(IntPtr hModule)
        {
            // Not needed in SDL port
        }

        public bool IsPaused()
        {
            return m_GlobalPause;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Initialize the object. Opens SDL_mixer and loads all samples.</summary>
        public bool Create()
        {
            // Initialise SDL_mixer with OGG support
            SDL_mixer.Mix_Init(SDL_mixer.MIX_INIT_OGG);

            // AUDIO_S16LSB = 0x8010
            if (SDL_mixer.Mix_OpenAudio(44100, 0x8010, 2, 1024) < 0)
            {
                // Failure
                CLog.GetLog().WriteLine("Sound           => !!! Could not initialise SDL_mixer Library. Error is : {0}", SDL_mixer.Mix_GetErrorString());

                // Get out
                return false;
            }

            SDL_mixer.Mix_Volume(-1, MIX_MAX_VOLUME);

            SDL_mixer.Mix_AllocateChannels(32); // this was the default in FMOD

            if (!LoadSample(ESample.SAMPLE_BOMB_DROP,           "bomb_drop.ogg")         ||
                !LoadSample(ESample.SAMPLE_BOMBER_DEATH,        "bomber_death.ogg")      ||
                !LoadSample(ESample.SAMPLE_BOMB_BOUNCE,         "bomb_bounce.ogg")       ||
                !LoadSample(ESample.SAMPLE_BOMBER_PUNCH,        "bomber_punch.ogg")      ||
                !LoadSample(ESample.SAMPLE_BOMBER_THROW,        "bomber_throw.ogg")      ||
                !LoadSample(ESample.SAMPLE_BOMBER_LOSE_ITEM,    "bomber_lose_item.ogg")  ||
                !LoadSample(ESample.SAMPLE_BREAK_1,             "break_1.ogg")           ||
                !LoadSample(ESample.SAMPLE_BREAK_2,             "break_2.ogg")           ||
                !LoadSample(ESample.SAMPLE_DRAW_GAME,           "draw_game.ogg")         ||
                !LoadSample(ESample.SAMPLE_DRAW_GAME_VOICE,     "draw_game_voice.ogg")   ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_01_1,      "explosion_01_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_01_2,      "explosion_01_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_02_1,      "explosion_02_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_02_2,      "explosion_02_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_03_1,      "explosion_03_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_03_2,      "explosion_03_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_04_1,      "explosion_04_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_04_2,      "explosion_04_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_05_1,      "explosion_05_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_05_2,      "explosion_05_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_06_1,      "explosion_06_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_06_2,      "explosion_06_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_07_1,      "explosion_07_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_07_2,      "explosion_07_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_08_1,      "explosion_08_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_08_2,      "explosion_08_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_09_1,      "explosion_09_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_09_2,      "explosion_09_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_10_1,      "explosion_10_1.ogg")    ||
                !LoadSample(ESample.SAMPLE_EXPLOSION_10_2,      "explosion_10_2.ogg")    ||
                !LoadSample(ESample.SAMPLE_HURRY,               "hurry.ogg")             ||
                !LoadSample(ESample.SAMPLE_ITEM_FUMES,          "item_fumes.ogg")        ||
                !LoadSample(ESample.SAMPLE_MENU_NEXT,           "menu_next.ogg")         ||
                !LoadSample(ESample.SAMPLE_MENU_PREVIOUS,       "menu_previous.ogg")     ||
                !LoadSample(ESample.SAMPLE_MENU_BEEP,           "menu_beep.ogg")         ||
                !LoadSample(ESample.SAMPLE_MENU_ERROR,          "menu_error.ogg")        ||
                !LoadSample(ESample.SAMPLE_PAUSE,               "pause.ogg")             ||
                !LoadSample(ESample.SAMPLE_PICK_ITEM_1,         "pick_item_1.ogg")       ||
                !LoadSample(ESample.SAMPLE_PICK_ITEM_2,         "pick_item_2.ogg")       ||
                !LoadSample(ESample.SAMPLE_RING_DING,           "ring_ding.ogg")         ||
                !LoadSample(ESample.SAMPLE_SICK_1,              "sick_1.ogg")            ||
                !LoadSample(ESample.SAMPLE_SICK_2,              "sick_2.ogg")            ||
                !LoadSample(ESample.SAMPLE_SICK_3,              "sick_3.ogg")            ||
                !LoadSample(ESample.SAMPLE_VICTORY,             "victory.ogg")           ||
                !LoadSample(ESample.SAMPLE_VICTORY_VOICE,       "victory_voice.ogg")     ||
                !LoadSample(ESample.SAMPLE_WALL_CLAP_1,         "wall_clap_1.ogg")       ||
                !LoadSample(ESample.SAMPLE_WALL_CLAP_2,         "wall_clap_2.ogg")       ||
                !LoadSample(ESample.SAMPLE_WINNER,              "winner.ogg"))
            {
                // Songs are loaded when they are needed.
                // Failure — get out (error is logged by LoadSample / LoadSong)
                return false;
            }

            m_SoundOK = true;

            // Everything went right
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Uninitialize the object. Frees all samples and the current song, then closes SDL_mixer.</summary>
        public void Destroy()
        {
            // If the sound works
            if (m_SoundOK)
            {
                // Halt playback on all channels
                SDL_mixer.Mix_HaltChannel(-1);

                // Stop and free all samples that are playing
                for (int i = 0; i < (int)ESample.NUM_SAMPLES; i++)
                {
                    if (m_Samples[i] != IntPtr.Zero)
                    {
                        SDL_mixer.Mix_FreeChunk(m_Samples[i]);

                        // Free the sample slot
                        m_Samples[i] = IntPtr.Zero;
                    }
                }

                // Free the current song
                if (m_CurrentSong != IntPtr.Zero)
                {
                    SDL_mixer.Mix_FreeMusic(m_CurrentSong);

                    // Free the song slot
                    m_CurrentSong = IntPtr.Zero;
                }
            }

            SDL_mixer.Mix_CloseAudio();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Pause or resume the sound.</summary>
        public void SetPause(bool Pause)
        {
            // If the sound works
            if (m_SoundOK)
            {
                if (Pause)
                {
                    SDL_mixer.Mix_PauseMusic();
                }
                else
                {
                    SDL_mixer.Mix_ResumeMusic();
                }

                m_GlobalPause = Pause;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Start playing a song. Loads the song file dynamically if it is not already loaded.</summary>
        public void PlaySong(ESong Song)
        {
            bool result = true;
            int VolumePerCent = 25;

            // If the sound works
            if (m_SoundOK)
            {
                // If a song exists free it unless it is the same
                if (m_CurrentSong != IntPtr.Zero && m_ESong != Song)
                {
                    FreeSong(Song);
                }

                // Note: songs are loaded dynamically because of an error in libmikmod (used by SDL_mixer).
                // Load new song (if necessary).
                if (m_ESong != Song || m_CurrentSong == IntPtr.Zero)
                {
                    switch (Song)
                    {
                        case ESong.SONG_MATCH_MUSIC:
                            result = LoadSong(ESong.SONG_MATCH_MUSIC, "match_music.ogg");
                            break;
                        case ESong.SONG_MENU_MUSIC:
                            result = LoadSong(ESong.SONG_MENU_MUSIC, "menu_music.ogg");
                            break;
                        case ESong.SONG_CONTROLS_MUSIC:
                            result = LoadSong(ESong.SONG_CONTROLS_MUSIC, "controls_music.ogg");
                            break;
                        case ESong.SONG_TITLE_MUSIC:
                            result = LoadSong(ESong.SONG_TITLE_MUSIC, "title_music.ogg");
                            break;
                        default:
                            result = false;
                            break;
                    }
                }

                if (result)
                {
                    // Start playing this song (-1 = infinite loop)
                    SDL_mixer.Mix_PlayMusic(m_CurrentSong, -1);
                    SDL_mixer.Mix_VolumeMusic(VolumePerCent * MIX_MAX_VOLUME / 100);

                    m_ESong = Song;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Stop playing a specific song.</summary>
        public void StopSong(ESong Song)
        {
            // If the sound works
            if (m_SoundOK)
            {
                // If the song exists
                if (m_CurrentSong != IntPtr.Zero)
                {
                    // Stop playing current song (we don't know which one is playing)
                    FreeSong(m_ESong);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Start playing a sample.</summary>
        public void PlaySample(ESample Sample)
        {
            // If the sound works
            if (m_SoundOK)
            {
                // If the sample exists
                if (m_Samples[(int)Sample] != IntPtr.Zero)
                {
                    // Start playing this sample
                    SDL_mixer.Mix_PlayChannel(-1, m_Samples[(int)Sample], 0);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Stops all samples by halting all mixer channels.</summary>
        public void StopAllSamples()
        {
            // If the sound works
            if (m_SoundOK)
            {
                // Halt all channels
                SDL_mixer.Mix_HaltChannel(-1);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set the volume for all samples.</summary>
        public void SetSampleVolume(int VolumePerCent)
        {
            // If the sound works
            if (m_SoundOK)
            {
                // Set the volume of all samples
                SDL_mixer.Mix_Volume(-1, VolumePerCent * MIX_MAX_VOLUME / 100);
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set the volume for a specific song.</summary>
        public void SetSongVolume(ESong Song, int VolumePerCent)
        {
            // If the sound works
            if (m_SoundOK)
            {
                // If this song exists
                if (m_CurrentSong != IntPtr.Zero)
                {
                    SDL_mixer.Mix_VolumeMusic(VolumePerCent * MIX_MAX_VOLUME / 100);
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Load a sample from the sounds folder using SDL_mixer.Mix_LoadWAV.
        /// </summary>
        private bool LoadSample(ESample Sample, string file)
        {
            // Check if the sample slot is free
            System.Diagnostics.Debug.Assert(m_Samples[(int)Sample] == IntPtr.Zero);

            string path = Path.Combine("sounds", file);

            m_Samples[(int)Sample] = SDL_mixer.Mix_LoadWAV(path);

            if (m_Samples[(int)Sample] == IntPtr.Zero)
            {
                // Log failure
                CLog.GetLog().WriteLine("Sound           => !!! Could not open sample {0} because {1}", file, SDL_mixer.Mix_GetErrorString());

                // Get out
                return false;
            }

            // Everything went right
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Free a loaded sample and halt all channels.
        /// </summary>
        private void FreeSample(ESample Sample)
        {
            // If the sample slot is not free
            if (m_Samples[(int)Sample] != IntPtr.Zero)
            {
                // Halt playback on all channels
                SDL_mixer.Mix_HaltChannel(-1);

                // Free sample
                SDL_mixer.Mix_FreeChunk(m_Samples[(int)Sample]);

                // Free the sample slot
                m_Samples[(int)Sample] = IntPtr.Zero;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Load a song from the sounds folder using SDL_mixer.Mix_LoadMUS.
        /// Songs are loaded dynamically (one at a time) into m_CurrentSong.
        /// </summary>
        private bool LoadSong(ESong Song, string file)
        {
            // Check if the song slot is free
            System.Diagnostics.Debug.Assert(m_CurrentSong == IntPtr.Zero);

            string path = Path.Combine("sounds", file);

            // Open song
            m_CurrentSong = SDL_mixer.Mix_LoadMUS(path);

            if (m_CurrentSong == IntPtr.Zero)
            {
                // Log failure
                CLog.GetLog().WriteLine("Sound           => !!! Could not load song {0} because {1}.", file, SDL_mixer.Mix_GetErrorString());

                // Get out
                return false;
            }

            // Everything went right
            return true;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>
        /// Stop and free the current song.
        /// </summary>
        private void FreeSong(ESong Song)
        {
            // If the song slot is not free
            if (m_CurrentSong != IntPtr.Zero)
            {
                SDL_mixer.Mix_HaltMusic();
                SDL_mixer.Mix_FreeMusic(m_CurrentSong);

                // Free the song slot
                m_CurrentSong = IntPtr.Zero;
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
