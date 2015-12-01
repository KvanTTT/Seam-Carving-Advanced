##Seam-Carving-Advanced

Experiments with seam carving (contet-aware resizing), such as Seam Carving on GPU, for video and other.

Currently implemented methods:
* **CAIR**. Using wrapper for native library [CAIR](https://sites.google.com/site/brainrecall/cair)
* **Standart**. Using managed version of CAIR (Code has been translted from C++ to C#).
For performance reason code has been rewritten with **unsafe** parts.
* **GPU** (exprerimental). Using [Cudafy](https://cudafy.codeplex.com/) for interaction with GPU.
Actually is not working.

##GUI

Most part of options has been borrowed from [CAIR](https://sites.google.com/site/brainrecall/cair):
* Energy Type (Prewitt, Sobel, V1, VSquare, Laplacian).
* Forward Energy.
* HD.
* Parallelization is also available.

<img src="https://hsto.org/files/022/8d0/1fa/0228d01fa9d24fd3b421092207e2e8b0.png" alt="GUI sample screen"/>

##Licence

Seam-Carving-Advanced is licensed under the Apache 2.0 License.