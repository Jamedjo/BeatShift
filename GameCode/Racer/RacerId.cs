using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace BeatShift
{
    public class RacerId
    {
        private String playerName = "";
        private String AIName = "";

        //public string Name { get{ if(XboxLiveName not null) return XboxLiveName, then same for GamerTag and AiName }; }

        RacerStatistics racerStatistics;
        public SignedInGamer gamer;

        public RacerId(SignedInGamer gamer)
        {
            this.gamer = gamer;
            this.playerName = gamer.Gamertag;
        }

        public RacerId(String AIName)
        {
            this.AIName = AIName;
        }

        public override string ToString()
        {
            if ( !playerName.Equals(""))
                return playerName;
            else
                return AIName;
        }
        public void createNewId()
        {
            PlayerIndex controllingPlayer = PlayerIndex.One;
              
            for (PlayerIndex index = PlayerIndex.One; index <= PlayerIndex.Four; index++)
            {
               if (GamePad.GetState(index).Buttons.Start == ButtonState.Pressed)
               {
                  controllingPlayer = index;
                  break;
               }
            }

            SignedInGamer gamer = Gamer.SignedInGamers[controllingPlayer];

            if (gamer != null)
            {
                playerName = gamer.Gamertag;
            }
            else
            {
                Guide.ShowSignIn(4, false);

            }
        }

    }
}
