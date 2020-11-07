/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 06/06/2020
 PURPOSE: Interface for finding objects based on a source image.
 SPECIAL NOTES:
===============================
*/

using System.Drawing;

namespace ComputerVision
{
    interface ICvDetector
    {
        CvModel[] DetectObjects(Bitmap Source, Rectangle LookBounds);
    }
}