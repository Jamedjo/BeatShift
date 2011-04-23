using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BeatShift
{
    class InputCombo
    {
        PlayerIndex playerIndex;//No Longer Used
        BeatShift game;
        List<String> combo;
        int listCounter = 0;
        public InputCombo(BeatShift maingame, PlayerIndex index)
            : this(maingame, index, 4)        
        {

        }

        public InputCombo(BeatShift maingame, PlayerIndex index, int comboLength)
        {
            playerIndex = index;
            game = maingame;
            combo = new List<String>();
            for (int i = 0; i < comboLength; i++)
            {
                Random random = new Random();
                int comboNumber = random.Next(4);
                switch (comboNumber)
                {
                    case 0:
                        combo.Add("X");
                        break;
                    case 1:
                        combo.Add("Y");
                        break;
                    case 2:
                        combo.Add("A");
                        break;
                    case 3:
                        combo.Add("B");
                        break;
                }                
            }
        }

        int buttonHit(String button)
        {
            if (combo.ElementAt(listCounter) == button)
            {
                listCounter++;
                if (listCounter == combo.Count)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                listCounter = 0;
                return 0;
            }
        }
    }
}
