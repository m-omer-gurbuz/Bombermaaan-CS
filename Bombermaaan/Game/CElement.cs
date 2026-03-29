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
 *  \file CElement.cs
 *  \brief An element in the arena
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    /// <summary>
    /// The base class for every element of the arena.
    /// </summary>
    public abstract class CElement
    {
        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected bool m_Exist;           //!< Does the element exist? (is it created?)

        protected CArena   m_pArena;    //!< Link to the parent arena in which this element is
        protected CDisplay m_pDisplay;  //!< Link to the display object to use
        protected CSound   m_pSound;    //!< Link to the sound object to use

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Base constructor. Some member initializations.</summary>
        protected CElement()
        {
            m_Exist    = false;
            m_pDisplay = null;
            m_pSound   = null;
            m_pArena   = null;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Set link to the parent arena in which this element is.</summary>
        public virtual void SetArena(CArena pArena)
        {
            m_pArena = pArena;
        }

        /// <summary>Set link to the display object to use.</summary>
        public virtual void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        /// <summary>Set link to the sound object to use.</summary>
        public virtual void SetSound(CSound pSound)
        {
            m_pSound = pSound;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Return whether the element exists (i.e. should be updated and displayed).</summary>
        public bool Exist()
        {
            return m_Exist;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Initialize the element. Call this on creation of the inherited class.</summary>
        protected void Create()
        {
            m_Exist = true;
            Debug.Assert(m_pArena   != null);
            Debug.Assert(m_pDisplay != null);
            Debug.Assert(m_pSound   != null);
        }

        /// <summary>Uninitialize the element. Call this on destruction of the inherited class.</summary>
        protected void Destroy()
        {
            m_Exist    = false;
            m_pDisplay = null;
            m_pSound   = null;
            m_pArena   = null;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        protected abstract void OnWriteSnapshot(CArenaSnapshot Snapshot);
        protected abstract void OnReadSnapshot(CArenaSnapshot Snapshot);

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void WriteSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.WriteBoolean(m_Exist);

            if (m_Exist)
                OnWriteSnapshot(Snapshot);
        }

        public void ReadSnapshot(CArenaSnapshot Snapshot)
        {
            Snapshot.ReadBoolean(out m_Exist);

            if (m_Exist)
                OnReadSnapshot(Snapshot);
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Update the element. Return whether the element should be deleted by the arena.</summary>
        public abstract bool Update(float DeltaTime);

        /// <summary>Display the element.</summary>
        public abstract void Display();

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************
    }
}
