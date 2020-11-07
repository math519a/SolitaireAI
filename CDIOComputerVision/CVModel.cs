/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 06/06/2020
 PURPOSE: Model for Computer Vision prediction. This model has a type, bounding box and prediction percentage. 
 SPECIAL NOTES: This model is created by a detector from an image source.
===============================
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerVision
{
    public class CvModel
    {
        public float Confidence { get; set; }
        
        public Rectangle BoundingBox { get; set; }

        public Rectangle Look_Bounds { get; set; }

        public string Type { get; set; }
    }
}
