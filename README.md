# SolitaireAI - Run on Visual Studio 2019

1. Open Visual Studio 2019 and clone "https://github.com/rasm586c/SolitaireAI"
2. Open the project solution "SolitaireSolver"
  2a. If the C# project "SolitaireSolver" isn't marked with bold text, then right click "SolitaireSolver" and press "Set as Startup Project"
3. Press F6 to build the solution.
  3a. If you get build error(s), then right click the solution, and press "Restore Nuget Packages", right click and build CDIOComputerVision, CDIODeck and then SolitaireSolver.
  3b. If it still doesn't build make sure that the projects "CDIOComputerVision" and "CDIODeck" are imported under references

If the program runs, but complains about missing dlls, you need to download CUDA and cuDNN library.
Guide: https://mc.ai/how-to-install-cuda-10-and-cudnn-for-tensorflow-gpu-on-windows-10/

Once cudnn is installed copy cudnn64_7.dll into the "Bin" folder generated when building.

Note: If you want to run this project without CUDA, then go to InitializeYoloWrapper() in SolitaireSolver/FrmSolitaire.cs and change the line:

> YoloWrapper yoloWrapper = new YoloWrapper(
>                $@"path\to\file\solitaire_images.cfg", 
>                $@"path\to\file\solitaire_images_40000.weights", 
>                $@"path\to\file\solitaire_images.names",
>                new GpuConfig() { GpuIndex = 0 });
                
into

> YoloWrapper yoloWrapper = new YoloWrapper(
>                $@"path\to\file\solitaire_images.cfg", 
>                $@"path\to\file\solitaire_images_40000.weights", 
>                $@"path\to\file\solitaire_images.names");

Note CUDNN and weight files are too big to be uploaded.
Trained Data can be found here & CUDNN can be found here:

https://mega.nz/file/nR1SSIpD#Z4zyblTcgGoYsb_Z1rthopu0tutcdxX7sE0uaFISXic
