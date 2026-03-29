/*************************************************************************************

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

*************************************************************************************/

/// <summary>
/// CScores.cs - The scores during a game
/// </summary>

using System.Linq;
namespace Bombermaaan
{

    public class CScores
    {
        private COptions m_pOptions;                     ///< Link to the options object to use
        private int[]    m_Scores = new int[Globals.MAX_PLAYERS]; ///< Score of each player
        private int      m_DrawGamesCount;               ///< Number of draw games

        public CScores()
        {
            m_pOptions = null;
            Reset();
        }

        /// <summary>Set link to the options object to use.</summary>
        public void SetOptions(COptions pOptions)
        {
            m_pOptions = pOptions;
        }

        /// <summary>Get the score of a player.</summary>
        public int GetPlayerScore(int Player)
        {
            return m_Scores[Player];
        }

        /// <summary>Get how many draw games there were.</summary>
        public int GetDrawGamesCount()
        {
            return m_DrawGamesCount;
        }

        /// <summary>Determines whether this is the very first score in the match.</summary>
        public bool IsFirstScore()
        {
            return m_Scores.Sum() == 1;
        }

        /// <summary>Determines whether the match is in its final round.</summary>
        public bool IsFinalRound()
        {
            return m_Scores.Any(score => score == m_pOptions.GetBattleCount() - 1);
        }

        /// <summary>Reset the scores to zero.</summary>
        public void Reset()
        {
            for (int Player = 0; Player < Globals.MAX_PLAYERS; Player++)
            {
                m_Scores[Player] = 0;
            }
            m_DrawGamesCount = 0;
        }

        /// <summary>Add one to the score of the specified player.</summary>
        public void RaisePlayerScore(int Player)
        {
            if (m_Scores[Player] < m_pOptions.GetBattleCount())
            {
                m_Scores[Player]++;
            }
        }

        /// <summary>Add one to the draw games count.</summary>
        public void RaiseDrawGamesCount()
        {
            if (m_DrawGamesCount < Globals.MAX_DRAWGAME_SCORE)
            {
                m_DrawGamesCount++;
            }
        }
    }

} // namespace Bombermaaan
