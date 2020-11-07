/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 06/06/2020
 PURPOSE: Implementation of the Computer Vision detection interface used to detect objects using the Yolo Framework. 
 SPECIAL NOTES:
===============================
*/

using Alturos.Yolo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerVision
{
    public class YoloDetector : ICvDetector
    {
        private YoloWrapper Wrapper;

        public YoloDetector(YoloWrapper Wrapper)
        {
            this.Wrapper = Wrapper;
        }


        public CvModel[] DetectObjects(Bitmap Source, Rectangle Look_Bounds)
        {
            // MemoryStream is created to convert bitmap into a byte array.
            // We then use yolo to perform image detection and convert that into Computer Vision Models.

            CvModel[] @out;
            using (var ms = new MemoryStream()) 
            {
                Source.Save(ms, ImageFormat.Jpeg);
                var items = Wrapper.Detect(ms.ToArray());
                
                @out = new CvModel[items.Count()];
                int index = 0; // We can't directly access an index of an IEnumerable<T> so we declare the index here, iterate every object of our IEnumerable and then put it into our CvModel array.
                foreach (var item in items)
                {
                    @out[index++] = new CvModel { 
                        BoundingBox = new Rectangle(item.X, item.Y, item.Width, item.Height),
                        Confidence = (float)item.Confidence,
                        Look_Bounds = Look_Bounds,
                        Type = item.Type
                    };
                }
            }
            return @out;
        }
    }
}
