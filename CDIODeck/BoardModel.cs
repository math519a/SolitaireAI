/*
If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 10/06/2020
 PURPOSE: This class is a model over the state of the current board.
 SPECIAL NOTES: 
 MODIFIED BY: Nicklas Beyer Lydersen (S185105)
 LAST MODIFIED DATE: 21/06/2020
===============================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deck
{
    public class BoardModel
    {
        public CardModel[] Top = new CardModel[4];
        public CardModel[] Bottom = new CardModel[7];

        public CardModel DeckCard;

        public int CardsOnBoard()
        {
            int Count = 0;
            if (DeckCard != default) Count++;
            foreach (var Card in Top) if (Card != default) Count++;
            foreach (var Card in Bottom) if (Card != default) Count++;
            return Count;
        }

        public override bool Equals(object obj)
        {
            return obj is BoardModel model &&
                   model.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            int hc = Top.Length + Bottom.Length + 1;
            for (int i = 0; i < Top.Length; ++i)
            {
                if (Top[i] == default)
                    hc = unchecked(hc * 314159);
                else
                    hc = unchecked(hc * 314159 + (int)Top[i].Type);
            }
            for (int i = 0; i < Bottom.Length; ++i)
            {
                if (Bottom[i] == default)
                    hc = unchecked(hc * 314159);
                else
                    hc = unchecked(hc * 314159 + (int)Bottom[i].Type);
            }

            if (DeckCard == default)
                hc = unchecked(hc * 314159);
            else
                hc = unchecked(hc * 314159 + (int)DeckCard.Type);
            return hc;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
