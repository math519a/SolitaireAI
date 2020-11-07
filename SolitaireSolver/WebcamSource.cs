/*
 If there is more names on class written by, then it means we wrote it in collaboration.
===============================
 CLASS WRITTEN BY: Rasmus Søborg (S185119)
 CREATE DATE: 30/04/2020
 PURPOSE: Get a webcam stream frame as bitmap and draw using GraphicBuffer
 SPECIAL NOTES: 
===============================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.IO;
using FastWebcam;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace SolitaireSolver
{
    public class WebcamSource : IDisposable
    {
        public readonly GraphicBuffer UpdateBuffer;
        private readonly TimeSpan InitCameraDelay = TimeSpan.FromSeconds(3);


        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        VideoCapture capture;
        Mat frame;
        Bitmap image;
        private Thread camera;
        private bool isCameraRunning = false;

        readonly BlockConfiguration BlockConfig;

        public WebcamSource(GraphicBuffer UpdateBuffer, BlockConfiguration BlockConfig)
        {
            this.BlockConfig = BlockConfig;
            this.UpdateBuffer = UpdateBuffer;
        }

        public void Start()
        {
            CaptureCamera();
            isCameraRunning = true;
        }

        private void CaptureCamera()
        {
            camera = new Thread(new ThreadStart(CaptureCameraCallback));
            camera.Start();
        }

        private void CaptureCameraCallback()
        {
            if (!isCameraRunning)
            {
                return;
            }
            frame = new Mat();
            capture = new VideoCapture(0);
            capture.Set(CaptureProperty.FrameWidth, 1920);
            capture.Set(CaptureProperty.FrameHeight, 1080);

            Thread.Sleep(InitCameraDelay);
            capture.Open(CaptureDevice.DShow, 0);
            while (!capture.IsOpened()) { }

            if (capture.IsOpened())
            {
                capture.Set(CaptureProperty.FrameWidth, 1920);
                capture.Set(CaptureProperty.FrameHeight, 1080);

                while (isCameraRunning)
                {
                    capture.Read(frame);

                    image = BitmapConverter.ToBitmap(frame);

#if DEBUG
                    if (GetAsyncKeyState(0x7B) == -32767)
                    {
                        if (!Directory.Exists("imgs"))
                            Directory.CreateDirectory("imgs");

                        var rnd = new Random();
                        var rndName = rnd.Next(0, int.MaxValue);

                        int blockIndex = 0;
                        for (int x = 0; x < image.Width; x += BlockConfig.BlockSize.Width)
                        {
                            for (int y = 0; y < image.Height; y+= BlockConfig.BlockSize.Height)
                            {
                                blockIndex++;

                                if (blockIndex == 3)
                                {
                                    using (Bitmap blockRegion = new Bitmap(BlockConfig.BlockSize.Width, BlockConfig.BlockSize.Height))
                                    {
                                        using (Graphics blockGraphics = Graphics.FromImage(blockRegion))
                                        {
                                            blockGraphics.DrawImage(image, new Rectangle(0, 0, BlockConfig.BlockSize.Width, BlockConfig.BlockSize.Height), new Rectangle(x, y, BlockConfig.BlockSize.Width, BlockConfig.BlockSize.Height), GraphicsUnit.Pixel);

                                            var name = $"solitaire_{rndName}_{x}_{y}.png";
                                            blockRegion.Save($"imgs/{name}", ImageFormat.Png);
                                        }
                                    }
                                }
                            }
                        }

                        Console.Beep();
                    }
#endif


                    UpdateBuffer.Draw(image);
                }
            }
        }

        public void Stop()
        {
            camera.Abort();
            capture.Release();
            isCameraRunning = false;
        }

        public void Dispose()
        {
            Stop();
            capture.Dispose();
            frame.Dispose();
            image.Dispose();
        }
    }
}
