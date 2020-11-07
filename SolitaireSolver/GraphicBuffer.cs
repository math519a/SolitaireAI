/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 29/04/2020
 PURPOSE: Draw an image (Typically WebcamSource) to a panel using GDI+ and allow backbuffer to be changed before presentation using OnDraw event. 
 SPECIAL NOTES: 
===============================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SolitaireSolver
{
    public class GraphicBuffer : IDisposable
    {
        public int fps { get; private set; } = 0;
        private int frameCount = 0;
        private DateTime intervalEnd = DateTime.UtcNow.AddSeconds(1);

        Graphics foregroundGraphics;
        Graphics backbufferGraphics;
        Bitmap backBuffer;
        Panel panel;
        readonly object mutex = new object();

        public delegate void DrawEventDelegate(DrawEventArgs args);
        public event DrawEventDelegate OnDraw;


        public GraphicBuffer(Panel panel)
        {
            backBuffer = new Bitmap(panel.Width, panel.Height);
            InitializeGraphics(panel);
            this.panel = panel;

            panel.Resize += (s, e) =>
            {
                InitializeGraphics(panel); 
            };
        }

        private void InitializeGraphics(Panel panel)
        {
            lock (mutex)
            {
                backBuffer.Dispose();
                backBuffer = new Bitmap(panel.Width, panel.Height);
                backbufferGraphics = Graphics.FromImage(backBuffer);
                foregroundGraphics = Graphics.FromHwnd(panel.Handle);
            }
        }

        public void Draw(Image cameraSource) 
        {
            if (DateTime.UtcNow > intervalEnd)
            {
                intervalEnd = DateTime.UtcNow.AddSeconds(1);
                fps = frameCount;
                frameCount = 0;
            }

            lock (mutex) // img disposed during resize - prevent error
            {
                backbufferGraphics.Clear(Color.Black);

                var imgSourceBmp = (Bitmap)cameraSource.Clone();
                var imgSourceGraphics = Graphics.FromImage(imgSourceBmp);

                if (OnDraw != null) OnDraw.Invoke(new DrawEventArgs(imgSourceGraphics, cameraSource));

                var newWidth = 0; 
                var newHeight = 0;

                if (panel.Width < panel.Height) {
                    var aspectRatio = (double)imgSourceBmp.Height / imgSourceBmp.Width;
                    newWidth = (int)panel.Width;
                    newHeight = (int)(newWidth * aspectRatio);
                } else
                {
                    var aspectRatio = (double)imgSourceBmp.Width / imgSourceBmp.Height;
                    newHeight = panel.Height;
                    newWidth = (int)(newHeight * aspectRatio);
                }

                backbufferGraphics.DrawImage(imgSourceBmp, new Rectangle((panel.Width - newWidth) / 2, 0, newWidth, newHeight), new Rectangle(0, 0, imgSourceBmp.Width, imgSourceBmp.Height), GraphicsUnit.Pixel);// prevent flickering
                foregroundGraphics.DrawImage(backBuffer, new Point(0, 0));
            }

            frameCount++;
        }

        public void Dispose()
        {
            backBuffer.Dispose();
        }
    }
}
