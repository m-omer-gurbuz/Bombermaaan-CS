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
/// CCloudManager.cs - The cloud manager
/// </summary>

namespace Bombermaaan
{

    /// <summary>This class manages the clouds.</summary>
    public class CCloudManager
    {
        private const int NUMBER_OF_CLOUDS = 3;

        private CDisplay    m_pDisplay;
        private CScroller[] m_Clouds       = new CScroller[NUMBER_OF_CLOUDS];
        private int[]       m_CloudSprites = new int[NUMBER_OF_CLOUDS];

        public CCloudManager()
        {
            m_pDisplay = null;

            for (int i = 0; i < NUMBER_OF_CLOUDS; i++)
                m_Clouds[i] = new CScroller();

            m_CloudSprites[0] = BmpId.BMP_TITLE_CLOUD_1;
            m_CloudSprites[1] = BmpId.BMP_TITLE_CLOUD_2;
            m_CloudSprites[2] = BmpId.BMP_TITLE_CLOUD_3;
        }

        ~CCloudManager()
        {
            Destroy();
        }

        /// <summary>Set link to the display object.</summary>
        public void SetDisplay(CDisplay pDisplay)
        {
            m_pDisplay = pDisplay;
        }

        public void Create()
        {
            System.Diagnostics.Debug.Assert(m_pDisplay != null);

            m_CloudSprites[0] = BmpId.BMP_TITLE_CLOUD_1;
            m_CloudSprites[1] = BmpId.BMP_TITLE_CLOUD_2;
            m_CloudSprites[2] = BmpId.BMP_TITLE_CLOUD_3;

            // Set the properties of each cloud
            m_Clouds[0].Create(50,   18,  138, 46, 50.0f, 0.0f, 6.0f);
            m_Clouds[1].Create(150,  74,  106, 46, 40.0f, 0.0f, 3.0f);
            m_Clouds[2].Create(-100, 130,  66, 22, 60.0f, 0.0f, 5.0f);
        }

        public void Destroy()
        {
            for (int i = 0; i < NUMBER_OF_CLOUDS; i++)
                m_Clouds[i].Destroy();
        }

        public void Update(float DeltaTime)
        {
            for (int i = 0; i < NUMBER_OF_CLOUDS; i++)
                m_Clouds[i].Update(DeltaTime);
        }

        public void Display()
        {
            for (int i = 0; i < NUMBER_OF_CLOUDS; i++)
            {
                RECT Clip;
                Clip.left   = 0;
                Clip.top    = 0;
                Clip.right  = Globals.VIEW_WIDTH;
                Clip.bottom = Globals.VIEW_HEIGHT;

                m_pDisplay.DrawSprite(m_Clouds[i].GetPositionX(),
                                      m_Clouds[i].GetPositionY(),
                                      null,
                                      Clip,
                                      m_CloudSprites[i],
                                      0,
                                      0,
                                      1);
            }
        }
    }

} // namespace Bombermaaan
