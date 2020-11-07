/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 17/06/2020
 PURPOSE: This model represent a move on the board
 SPECIAL NOTES:
===============================
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deck
{
    public class CardMove
    {
        public bool MoveToTop;

        public int FromPosition;
        public int ToPosition;

        public int CardIndex;
    }
}
