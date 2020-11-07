/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 29/04/2020
 PURPOSE: EventArgs model raised by GraphicsBuffer's Draw event
 SPECIAL NOTES: 
===============================
*/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolitaireSolver
{
    public class DrawEventArgs : EventArgs
    {
        public readonly Graphics BackBufferGraphics;
        public readonly Image Source;

        public DrawEventArgs(Graphics BackBufferGraphics, Image Source)
        {
            this.BackBufferGraphics = BackBufferGraphics;
            this.Source = Source;

        }
    }
}
