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
 *  \file CCommandChunk.cs
 *  \brief Command chunk (C# port of CCommandChunk.cpp/h)
 */

using System.Diagnostics;

namespace Bombermaaan
{
    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public struct SCommandStep
    {
        public EBomberMove   BomberMove;
        public EBomberAction BomberAction;
        public float         Duration;
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************

    public class CCommandChunk
    {
        public const int MAX_STEPS_IN_COMMAND_CHUNK = 8;

        private SCommandStep[] m_Steps;
        private int            m_NumberOfSteps;

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Create()
        {
            m_Steps = new SCommandStep[MAX_STEPS_IN_COMMAND_CHUNK];
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Destroy()
        {
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Reset()
        {
            m_NumberOfSteps = 0;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public void Store(EBomberMove BomberMove, EBomberAction BomberAction, float DeltaTime)
        {
            // If there is no step yet
            if (m_NumberOfSteps == 0)
            {
                // Create first step
                m_Steps[0].BomberMove   = BomberMove;
                m_Steps[0].BomberAction = BomberAction;
                m_Steps[0].Duration     = DeltaTime;

                m_NumberOfSteps++;
            }
            // If there is already at least one step
            else
            {
                // If the latest step has a different move or action than the ones we have to add
                if (m_Steps[m_NumberOfSteps - 1].BomberMove   != BomberMove ||
                    m_Steps[m_NumberOfSteps - 1].BomberAction != BomberAction)
                {
                    Debug.Assert(m_NumberOfSteps < MAX_STEPS_IN_COMMAND_CHUNK);

                    if (m_NumberOfSteps < MAX_STEPS_IN_COMMAND_CHUNK)
                    {
                        // This is a new step
                        m_Steps[m_NumberOfSteps].BomberMove   = BomberMove;
                        m_Steps[m_NumberOfSteps].BomberAction = BomberAction;
                        m_Steps[m_NumberOfSteps].Duration     = DeltaTime;

                        m_NumberOfSteps++;
                    }
                }
                // If the move and action did not change since latest step
                else
                {
                    // Use latest step, increase duration
                    m_Steps[m_NumberOfSteps - 1].Duration += DeltaTime;
                }
            }
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        public EBomberMove GetStepMove(int Step)
        {
            Debug.Assert(Step >= 0);
            Debug.Assert(Step < m_NumberOfSteps);
            return m_Steps[Step].BomberMove;
        }

        public EBomberAction GetStepAction(int Step)
        {
            Debug.Assert(Step >= 0);
            Debug.Assert(Step < m_NumberOfSteps);
            return m_Steps[Step].BomberAction;
        }

        public float GetStepDuration(int Step)
        {
            Debug.Assert(Step >= 0);
            Debug.Assert(Step < m_NumberOfSteps);
            return m_Steps[Step].Duration;
        }

        public int GetNumberOfSteps()
        {
            return m_NumberOfSteps;
        }

        //******************************************************************************************************************************
        //******************************************************************************************************************************
        //******************************************************************************************************************************

        /// <summary>Returns a copy of the internal steps array for serialization purposes.</summary>
        public SCommandStep[] GetSteps() => m_Steps;
    }

    //******************************************************************************************************************************
    //******************************************************************************************************************************
    //******************************************************************************************************************************
}
