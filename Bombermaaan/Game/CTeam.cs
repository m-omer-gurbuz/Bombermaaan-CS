/*************************************************************************************

    Copyright (C) 2016 Billy Araujo
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

*************************************************************************************/

/// <summary>
/// CTeam.cs - The team (bombers are playing)
/// </summary>

namespace Bombermaaan
{

    public class CTeam
    {
        private int  m_TeamId;
        private bool m_Victorious; // Has the team won the current match?

        /// <summary>Constructor. Initialize some members.</summary>
        public CTeam()
        {
            m_TeamId = -1;
            m_Victorious = false;
        }

        public void SetTeamId(int TeamId)
        {
            m_TeamId = TeamId;
        }

        public int GetTeamId()
        {
            return m_TeamId;
        }

        public void SetVictorious(bool Victorious)
        {
            m_Victorious = Victorious;
        }

        public bool IsVictorious()
        {
            return m_Victorious;
        }
    }

} // namespace Bombermaaan
