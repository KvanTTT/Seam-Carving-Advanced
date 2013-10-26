using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarvingAdvanced
{
	public unsafe class SeamCarverGPU : SeamCarverStandart
	{
		#region Fields

		static GPGPU _gpu = CudafyHost.GetDevice(eGPUType.Cuda);
		private int[] _energyDiffGPU;
		private byte[] _energyGPU;

		#endregion

		public static void InitGPU()
		{
			string moduleName = typeof(SeamCarverGPUKernel).Name;
			if (!_gpu.IsModuleLoaded(moduleName))
			{
				var mod = CudafyModule.TryDeserialize(moduleName);
				if (mod == null || !mod.TryVerifyChecksums())
				{
					mod = CudafyTranslator.Cudafy(ePlatform.Auto, eArchitecture.sm_12, typeof(SeamCarverGPUKernel));
					mod.Serialize(moduleName);
				}
				_gpu.LoadModule(mod);
			}
		}

		public SeamCarverGPU(EnergyFuncType energyFuncType, bool hd, bool forwardEnergy, bool parallelization, int blockSize)
			: base(energyFuncType, hd, forwardEnergy, parallelization, 0, blockSize)
		{
		}

		#region Overrides

		public override Bitmap Generate(Bitmap bitmap, int newWidth, int newHeight)
		{
			Bitmap result;
			BitmapData bitmapData = null;
			int length = 0;
			byte[] coloredArray = null;
			try
			{
				_initWidth = bitmap.Width;
				_initHeight = bitmap.Height;
				length = _initWidth * _initHeight;
				coloredArray = new byte[length * ColorOffset];
				bitmapData = bitmap.LockBits(
					new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
				byte* bitmapPtr = (byte*)bitmapData.Scan0.ToPointer();

				fixed (byte* colored = coloredArray)
				{
					BitmapToColored(bitmapPtr, colored, length);
				}
			}
			catch
			{
				result = null;
			}
			finally
			{
				if (bitmapData != null)
					bitmap.UnlockBits(bitmapData);
			}

			byte[] grayscaleArray = new byte[length];
			energyArray = new byte[length];
			energyDiffArray = new int[length];
			int[] shrinkedArray = new int[Math.Max(_initWidth, _initHeight)];

			int width = _initWidth;
			int height = _initHeight;

			fixed (byte* colored = coloredArray, grayscaled = grayscaleArray, energy = energyArray)
			{
				_energyGPU = _gpu.CopyToDevice<byte>(energyArray);
				_energyDiffGPU = _gpu.Allocate<int>(length);

				ColoredToGrayscaled(colored, grayscaled, length);
				CalculateEnergy(grayscaled, energy, width, height);

				fixed (int* energyDiff = energyDiffArray, shrinked = shrinkedArray)
				{
					if (newWidth < width)
					{
						shrinked[0] = int.MinValue;
						do
						{
							int xMin;
							CalculateEnergyDiffParallel(energy, energyDiff, width, height, true, out xMin);
							FindShrinkedPixels(energyDiff, shrinked, width, height, true, xMin);
							DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, true);
							width--;
							_gpu.Free(_energyGPU);
							_energyGPU = _gpu.CopyToDevice(energyArray);
						}
						while (width > newWidth);
					}
				}
				result = ColoredToBitmap(colored, width, height);
			}

			_gpu.Free(_energyDiffGPU);

			return result;
		}

		protected override int CalculateEnergyDiffParallel(byte* energy, int* energyDiff, int width, int height, bool xDir, out int sMin)
		{
			if (xDir)
			{
				int blockSize2 = BlockSize / 2;
				int blockCountX1 = (width + BlockSize - 1) / BlockSize;
				int blockCountX2 = width / BlockSize == blockSize2 ? blockCountX1 : blockCountX1 + 1;
				int blockCountY = (height + blockSize2 - 1) / blockSize2;
				
				int gpuBlockCount = 256;
				int gpuThreadCount = 1;

				for (int i = 0; i < blockCountY; i++)
				{
					int yStart = i * blockSize2;
					int yStop = Math.Min(yStart + blockSize2, height);

					_gpu.Launch(blockCountX1, 1).DecTriangle(_energyGPU, _energyDiffGPU,
						BlockSize, blockCountX1, width, height, _initWidth, yStart, yStop);

					yStart = Math.Min(yStart + 1, height);

					_gpu.Launch(blockCountX2, 1).IncTriangle(_energyGPU, _energyDiffGPU,
						BlockSize, blockCountX2, width, height, _initWidth, yStart, yStop);
				}
			}

			_gpu.CopyFromDevice(_energyDiffGPU, energyDiffArray);

			return GetMinIndAndValue(energyDiff, width, height, xDir, out sMin);
		}

		#endregion
	}

	public class SeamCarverGPUKernel
	{
		[Cudafy]
		public static void DecTriangle(GThread thread, int[] energy, int[] energyDiff,
			int blockSizeX, int blockCount, int width, int height, int initWidth, int yStart, int yStop)
		{
			int xBlock = Gettid(thread);
			if (xBlock < blockCount)
			{
				int xStart = xBlock * blockSizeX;
				int xStop = xStart + blockSizeX;

				for (int y = yStart; y < yStop; y++)
				{
					for (int x = xStart; x <= xStop; x++)
					{
						if (x < width)
						{
							if (y == 0)
								energyDiff[x] = energy[x];
							else
							{
								int y_x_ind = y * initWidth + x;
								int ym1_x_ind = y_x_ind - initWidth;

								int leftUpperVal = energyDiff[ym1_x_ind - 1];
								int rightUpperVal = x < width - 1 ? energyDiff[ym1_x_ind + 1] : SeamCarverStandart.MaxEnergyDiff;
								int upperVal = energyDiff[ym1_x_ind];

								energyDiff[y_x_ind] = GpuMin(leftUpperVal, upperVal, rightUpperVal) + energy[y_x_ind];
							}
						}
					}
					xStart++;
					xStop--;
				}
			}
		}

		[Cudafy]
		public static void IncTriangle(GThread thread, int[] energy, int[] energyDiff,
			int blockSizeX, int blockCount, int width, int height, int initWidth, int yStart, int yStop)
		{
			int xBlock = Gettid(thread);
			if (xBlock < blockCount)
			{
				int xStart = xBlock * blockSizeX;
				int xStop = xStart;
				for (int y = yStart; y < yStop; y++)
				{
					for (int x = xStart; x <= xStop; x++)
					{
						if (x >= 0 || x < width)
						{
							int y_x_ind = y * initWidth + x;
							int ym1_x_ind = y_x_ind - initWidth;

							int leftUpperVal = x > 0 ? energyDiff[ym1_x_ind - 1] : SeamCarverStandart.MaxEnergyDiff;
							int rightUpperVal = x < width - 1 ? energyDiff[ym1_x_ind + 1] : SeamCarverStandart.MaxEnergyDiff;
							int upperVal = energyDiff[ym1_x_ind];

							energyDiff[y_x_ind] = GpuMin(leftUpperVal, upperVal, rightUpperVal) + energy[y_x_ind];
						}
					}
					xStart--;
					xStop++;
				}
			}
		}

		[Cudafy]
		public static int GpuMin(int a, int b, int c)
		{
			int result = a;
			if (b < result)
				result = b;
			if (c < result)
				result = c;
			return result;
		}

		[Cudafy]
		public static int Gettid(GThread thread)
		{
			int tid = thread.blockIdx.x * thread.blockDim.x + thread.threadIdx.x;
			return tid;
		}
	}
}
