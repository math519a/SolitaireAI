/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 AUTHOR: Rasmus Søborg (S185119)
 CREATE DATE: 06/06/2020
 PURPOSE: Model for splitting up our webcam source image into regions for better detection (typically 608x608) by YOLO
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
    public class BlockConfiguration
    {
        public Size BlockSize { get; private set; }

        public BlockConfiguration(Size BlockSize)
        {
            this.BlockSize = BlockSize;
        }
    }
}
