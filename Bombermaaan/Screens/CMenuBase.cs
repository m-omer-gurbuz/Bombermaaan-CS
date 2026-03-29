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
 *  \file CMenuBase.cs
 *  \brief The menu base
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>Abstract base for all individual menu screens (Bomber, Match, Level, etc.).</summary>
    public abstract class CMenuBase
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private const float TRANSITION_DURATION = 0.350f;   //!< Duration of the transition that happens before exiting

        private const int FONT_SPRITELAYER          = 1;    //!< Sprite layer where to draw characters using the font
        private const int FRAME_POSITION_X          = 30;   //!< Position of the menu frame
        private const int FRAME_POSITION_Y          = 52;
        private const int FRAME_SPRITE              = 0;    //!< Sprite number of the menu frame sprite
        private const int FRAME_PRIORITY            = 1;    //!< Priority to use in the menu sprite layer when drawing the frame
        private const int FRAME_SPRITELAYER         = 0;    //!< Sprite layer where to draw the menu frame

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        private float       m_MenuModeTime;         //!< Time (in seconds) that elapsed since this menu mode has started
        private bool        m_HaveToExit;           //!< Do we have to exit this menu mode?
        private EMenuAction m_ExitMenuAction;       //!< Menu action to ask for when exiting (after transition)
        private float       m_ExitMenuModeTime;     //!< Menu mode time when we realized we have to exit (used for transition)

        protected CDisplay  m_pDisplay;
        protected CSound    m_pSound;
        protected CInput    m_pInput;
        protected COptions  m_pOptions;
        protected CTimer    m_pTimer;
        protected CFont     m_pFont;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected abstract void OnCreate();
        protected abstract void OnDestroy();
        protected abstract void OnUpdate();
        protected abstract void OnDisplay();
        protected abstract void OnUp();
        protected abstract void OnDown();
        protected abstract void OnLeft();
        protected abstract void OnRight();
        protected abstract void OnPrevious();
        protected abstract void OnNext();

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected CMenuBase()
        {
            // Initialize the pointers to null so that we
            // can easily detect the ones we forgot to set.
            m_pDisplay = null;
            m_pInput   = null;
            m_pSound   = null;
            m_pOptions = null;
            m_pTimer   = null;
            m_pFont    = null;

            m_MenuModeTime     = 0.0f;
            m_HaveToExit       = false;
            m_ExitMenuAction   = EMenuAction.MENUACTION_NONE;
            m_ExitMenuModeTime = 0.0f;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void SetDisplay(CDisplay pDisplay) { m_pDisplay = pDisplay; }
        public void SetSound(CSound pSound)       { m_pSound   = pSound;   }
        public void SetInput(CInput pInput)       { m_pInput   = pInput;   }
        public void SetOptions(COptions pOptions) { m_pOptions = pOptions; }
        public void SetTimer(CTimer pTimer)       { m_pTimer   = pTimer;   }
        public void SetFont(CFont pFont)          { m_pFont    = pFont;    }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Create()
        {
            // Check if all the objects to communicate with are set
            Debug.Assert(m_pDisplay != null);
            Debug.Assert(m_pSound   != null);
            Debug.Assert(m_pInput   != null);
            Debug.Assert(m_pOptions != null);
            Debug.Assert(m_pTimer   != null);
            Debug.Assert(m_pFont    != null);

            // Reset menu mode time (no time has been elapsed in this menu mode yet)
            m_MenuModeTime = 0.0f;

            // Don't have to exit this menu mode yet
            m_HaveToExit = false;

            // Create and initialize the font for our needs
            m_pFont.Create();
            m_pFont.SetShadow(false);
            m_pFont.SetSpriteLayer(FONT_SPRITELAYER);

            OnCreate();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Destroy()
        {
            // Destroy the font
            m_pFont.Destroy();

            OnDestroy();
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public EMenuAction Update()
        {
            // Increase time elapsed in this menu mode
            m_MenuModeTime += m_pTimer.GetDeltaTime();

            // If we don't have to exit this menu mode yet
            if (!m_HaveToExit)
            {
                // If NEXT control is pressed
                if (m_pInput.GetMainInput().TestNext())
                {
                    // Don't play menu next sound here – the choice validation
                    // may play either next or error sound in OnNext().
                    OnNext();
                }
                // If PREVIOUS control is pressed
                else if (m_pInput.GetMainInput().TestPrevious())
                {
                    // Play the menu previous sound
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_PREVIOUS);

                    OnPrevious();
                }
                // If UP control is pressed
                else if (m_pInput.GetMainInput().TestUp())
                {
                    // Play the menu beep sound
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_BEEP);

                    OnUp();
                }
                // If DOWN control is pressed
                else if (m_pInput.GetMainInput().TestDown())
                {
                    // Play the menu beep sound
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_BEEP);

                    OnDown();
                }
                // If LEFT control is pressed
                else if (m_pInput.GetMainInput().TestLeft())
                {
                    // Play the menu beep sound
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_BEEP);

                    OnLeft();
                }
                // If RIGHT control is pressed
                else if (m_pInput.GetMainInput().TestRight())
                {
                    // Play the menu beep sound
                    m_pSound.PlaySample(ESample.SAMPLE_MENU_BEEP);

                    OnRight();
                }

                // Update the menu screen
                OnUpdate();
            }
            // If the transition has been entirely done (enough time has elapsed)
            else if (m_MenuModeTime >= m_ExitMenuModeTime + TRANSITION_DURATION)
            {
                // It's OK to exit now!
                // Ask for the menu action we saved
                return m_ExitMenuAction;
            }

            // Don't have to change menu mode nor game mode
            return EMenuAction.MENUACTION_NONE;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Display()
        {
            // If we don't have to exit this menu mode yet
            if (!m_HaveToExit)
            {
                m_pDisplay.SetOrigin(0, 0);

                // Draw the menu frame sprite
                m_pDisplay.DrawSprite(FRAME_POSITION_X,
                                      FRAME_POSITION_Y,
                                      null,
                                      null,
                                      BmpId.BMP_MENU_FRAME_1,
                                      FRAME_SPRITE,
                                      FRAME_SPRITELAYER,
                                      FRAME_PRIORITY);

                // Display the menu screen
                OnDisplay();
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected void Exit(EMenuAction ExitMenuAction)
        {
            // Start exiting this menu mode
            m_HaveToExit = true;

            // Remember when we realized we had to exit this menu mode
            m_ExitMenuModeTime = m_MenuModeTime;

            // Remember what menu action to ask for when exiting
            m_ExitMenuAction = ExitMenuAction;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
