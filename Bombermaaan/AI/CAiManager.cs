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
 *  \file CAiManager.cs
 *  \brief The AI management functions
 */

using System.Diagnostics;

namespace Bombermaaan
{
    public class CAiManager
    {
        private CAiBomber[] m_pBombers = new CAiBomber[Globals.MAX_PLAYERS];
        private CAiArena m_Arena = new CAiArena();
        private CDisplay m_pDisplay;

        //**************************************************************************************

        public CAiManager()
        {
            m_pDisplay = null;
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                m_pBombers[Player] = null;
        }

        //**************************************************************************************

        public void SetArena(CArena pArena)
        {
            Debug.Assert(pArena != null);
            m_Arena.SetArena(pArena);
        }

        public void SetDisplay(CDisplay pDisplay)
        {
            // Save the display object pointer to pass to elements
            m_pDisplay = pDisplay;
        }

        //**************************************************************************************

        public void Create(COptions pOptions)
        {
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                if (pOptions.GetBomberType(Player) == EBomberType.BOMBERTYPE_COM &&
                    m_Arena.GetArena().GetBomber(Player).Exist())
                {
                    m_pBombers[Player] = new CAiBomber();
                    m_pBombers[Player].SetArena(m_Arena);
                    m_pBombers[Player].SetDisplay(m_pDisplay);
                    m_pBombers[Player].Create(Player);
                }
            }

            m_Arena.SetDisplay(m_pDisplay);
            m_Arena.Create();
        }

        //**************************************************************************************

        public void Destroy()
        {
            m_Arena.Destroy();

            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                if (m_pBombers[Player] != null)
                {
                    m_pBombers[Player] = null;
                }
            }
        }

        //**************************************************************************************

        public void Update(float DeltaTime)
        {
            m_Arena.Update(DeltaTime);

            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
                if (m_pBombers[Player] != null)
                    m_pBombers[Player].Update(DeltaTime);
        }

        //**************************************************************************************
    }
}
