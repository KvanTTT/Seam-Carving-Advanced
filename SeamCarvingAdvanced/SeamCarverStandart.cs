using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeamCarvingAdvanced
{
	public unsafe class SeamCarverStandart : SeamCarver
	{
		#region Constants

		private const int ColorOffset = 4;
		private const int RedInc = 2;
		private const int GreenInc = 1;
		private const int BlueInc = 0;

		#endregion

		#region Public

		public double NeighbourCountRatio
		{
			get;
			set;
		}

		public SeamCarverStandart(EnergyFuncType energyFuncType, bool hd, bool forwardEnergy, bool parallelization,
			double neighbourCountRatio)
			: base(energyFuncType, hd, forwardEnergy, parallelization)
		{
			NeighbourCountRatio = neighbourCountRatio;
		}
		
		public override Bitmap Generate(Bitmap bitmap, int newWidth, int newHeight)
		{
			Bitmap result;
			BitmapData bitmapData = null;
			try
			{
				bitmapData = bitmap.LockBits(
					new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
				byte* bitmapPtr = (byte*)bitmapData.Scan0.ToPointer();
				int initWidth = bitmap.Width;
				int initHeight = bitmap.Height;
				int length = initWidth * initHeight;
				byte[] coloredArray = new byte[length * ColorOffset];
				byte[] grayscaleArray = new byte[length];
				byte[] energyArray = new byte[length];
				int[] energyDiffArray = new int[length];
				int[] shrinkedArray = new int[Math.Max(initWidth, initHeight)];
				int width = initWidth;
				int height = initHeight;

				fixed (byte* colored = coloredArray, grayscaled = grayscaleArray, energy = energyArray)
				{
					BitmapToColored(bitmapPtr, colored, length);
					ColoredToGrayscaled(colored, grayscaled, length);
					CalculateEnergy(grayscaled, energy, width, height, initWidth, initHeight);

					fixed (int* energyDiff = energyDiffArray, shrinked = shrinkedArray)
					{
						if (!Hd || width == newWidth || height == newHeight)
						{
							if (newWidth < width)
							{
								do
								{
									CalculateEnergyDiff(energy, energyDiff, width, height, initWidth, initHeight, true);
									FindShrinkedPixels(energyDiff, shrinked, width, height, initWidth, initHeight, true);
									width--;
									DecreaseBitmap(colored, energy, shrinked, width, height, initWidth, initHeight, true);
								}
								while (width > newWidth);
							}
							if (newHeight < height)
							{
								do
								{
									CalculateEnergyDiff(energy, energyDiff, width, height, initWidth, initHeight, false);
									FindShrinkedPixels(energyDiff, shrinked, width, height, initWidth, initHeight, false);
									height--;
									DecreaseBitmap(colored, energy, shrinked, width, height, initWidth, initHeight, false);
								}
								while (height > newHeight);
							}
						}
						else
						{
							if (newWidth < width || newHeight < height)
							{
								do
								{
									int xSum, ySum;
									if (width != newWidth)
									{
										CalculateEnergyDiff(energy, energyDiff, width, height, initWidth, initHeight, true);
										FindShrinkedPixels(energyDiff, shrinked, width, height, initWidth, initHeight, true);
										xSum = CalculateShrinkedSum(shrinked, energyDiff, width, height, true);
									}
									else
										xSum = int.MaxValue;

									if (height != newHeight)
									{
										CalculateEnergyDiff(energy, energyDiff, width, height, initWidth, initHeight, false);
										FindShrinkedPixels(energyDiff, shrinked, width, height, initWidth, initHeight, false);
										ySum = CalculateShrinkedSum(shrinked, energyDiff, width, height, false);
									}
									else
										ySum = int.MaxValue;

									if (xSum <= ySum)
									{
										width--;
										DecreaseBitmap(colored, energy, shrinked, width, height, initWidth, initHeight, true);
									}
									else
									{
										height--;
										DecreaseBitmap(colored, energy, shrinked, width, height, initWidth, initHeight, false);
									}
								}
								while (width > newWidth || height > newHeight);
							}
						}
					}

					result = ColoredToBitmap(colored, width, height, initWidth, initHeight);
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
			return result;
		}

		#endregion

		#region Private

		private int _threadCount = Environment.ProcessorCount;
		private object _lockObj = new object();

		private void CalculateEnergy(byte* grayscaled, byte* energy, int width, int height, int initWidth, int initHeight)
		{
			byte* grayscaledPtr;
			byte* energyPtr;
			int length = width * height;
			int startX = 1;
			int startY = 1;
			int stopX = width - 1;
			int stopY = height - 1;

			int dstOffset = 2 + (initWidth - width);
			int srcOffset = 2 + (initWidth - width);

			grayscaledPtr = grayscaled + width * startY + startX;
			energyPtr = energy + width * startY + startX;

			int max = 0;
			if (Parallelization)
			{
				Parallel.For(0, _threadCount, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, y =>
					CalculateEnergyPart(grayscaled, energy, width, height, initWidth, initHeight,
					1, width - 1, y * (height - 1) / _threadCount + 1, (y + 1) * (height - 1) / _threadCount, ref max));
			}
			else
			{
				CalculateEnergyPart(grayscaled, energy, width, height, initWidth, initHeight, 1, width - 1, 1, height - 1, ref max);
			}

			grayscaledPtr = grayscaled;
			energyPtr = energy;

			int g;
			for (int x = 0; x < width; x++)
			{
				g = CalculateEnergyAt(grayscaled, width, height, x, 0);
				if (g > max)
					max = g;
				energyPtr[0 * width + x] = (byte)g;

				g = CalculateEnergyAt(grayscaled, width, height, x, height - 1);
				if (g > max)
					max = g;
				energyPtr[(height - 1) * width + x] = (byte)g;
			}

			for (int y = startY; y < stopY; y++)
			{
				g = CalculateEnergyAt(grayscaled, width, height, 0, y);
				if (g > max)
					max = g;
				energyPtr[y * width + 0] = (byte)g;

				g = CalculateEnergyAt(grayscaled, width, height, width - 1, y);
				if (g > max)
					max = g;
				energyPtr[y * width + (width - 1)] = (byte)g;
			}

			if (max != 255)
			{
				double factor = 255.0 / (double)max;
				energyPtr = energy + width * startY + startX;

				for (int y = startY; y < stopY; y++)
				{
					for (int x = startX; x < stopX; x++, energyPtr++)
						*energyPtr = (byte)Math.Round(factor * *energyPtr);
					energyPtr += dstOffset;
				}
			}
		}

		private void CalculateEnergyPart(byte* grayscaled, byte* energy, int width, int height, int initWidth, int initHeight,
			int startX, int stopX, int startY, int stopY, ref int max)
		{
			int srcOffset = 2 + (initWidth - width);
			int dstOffset = 2 + (initWidth - width);

			int coef1 = 1, coef2 = 1;
			if (EnergyFuncType == EnergyFuncType.Sobel)
			{
				coef1 = 2;
				coef2 = 2;
			}

			byte* grayscaledPtr = grayscaled + width * startY + startX;
			byte* energyPtr = energy + width * startY + startX;
			
			int g = 0;
			int localMax = max;
			for (int y = startY; y < stopY; y++)
			{
				for (int x = startX; x < stopX; x++, grayscaledPtr++, energyPtr++)
				{
					if (EnergyFuncType == EnergyFuncType.Prewitt || EnergyFuncType == EnergyFuncType.Sobel)
					{
						g = Math.Min(255,
								Math.Abs(grayscaledPtr[-width - 1] + grayscaledPtr[-width + 1]
										- grayscaledPtr[width - 1] - grayscaledPtr[width + 1]
										+ coef1 * (grayscaledPtr[-width] - grayscaledPtr[width]))
							  + Math.Abs(grayscaledPtr[-width + 1] + grayscaledPtr[width + 1]
										- grayscaledPtr[-width - 1] - grayscaledPtr[width - 1]
										+ coef2 * (grayscaledPtr[1] - grayscaledPtr[-1])));
					}
					else if (EnergyFuncType == EnergyFuncType.VSquare || EnergyFuncType == EnergyFuncType.V1)
					{
						g = grayscaledPtr[-width + 1] + grayscaledPtr[1] + grayscaledPtr[+width + 1] -
							grayscaledPtr[-width - 1] - grayscaledPtr[-1] - grayscaledPtr[-width - 1];
						if (EnergyFuncType == EnergyFuncType.VSquare)
							g *= g;
					}
					else if (EnergyFuncType == EnergyFuncType.Laplacian)
					{
						g = grayscaledPtr[1] + grayscaledPtr[-1] + grayscaledPtr[width] + grayscaledPtr[-width] -
							4 * *grayscaledPtr;
					}

					*energyPtr = (byte)g;
					if (g > localMax)
						localMax = g;
				}

				grayscaledPtr += srcOffset;
				energyPtr += dstOffset;
			}

			lock (_lockObj)
			{
				if (localMax > max)
					max = localMax;
			}
		}

		private int CalculateEnergyAt(byte* grayscaled, int width, int height, int x, int y)
		{
			int result = 0;
			if (EnergyFuncType == EnergyFuncType.Prewitt || EnergyFuncType == EnergyFuncType.Sobel)
			{
				int coef1 = 1;
				int coef2 = 1;
				if (EnergyFuncType == EnergyFuncType.Sobel)
				{
					coef1 = 2;
					coef2 = 2;
				}

				result = Math.Min(255,
						Math.Abs(GetGrayscaledAt(grayscaled, width, height, x - 1, y - 1) +
								 GetGrayscaledAt(grayscaled, width, height, x + 1, y - 1) -
								 GetGrayscaledAt(grayscaled, width, height, x - 1, y + 1) -
								 GetGrayscaledAt(grayscaled, width, height, x + 1, y + 1)
								+ coef1 * (
								GetGrayscaledAt(grayscaled, width, height, x, y - 1) -
								GetGrayscaledAt(grayscaled, width, height, x, y + 1)))

					  + Math.Abs(GetGrayscaledAt(grayscaled, width, height, x + 1, y - 1) +
								 GetGrayscaledAt(grayscaled, width, height, x + 1, y + 1) -
								 GetGrayscaledAt(grayscaled, width, height, x - 1, y - 1) -
								 GetGrayscaledAt(grayscaled, width, height, x - 1, y + 1)
								+ coef2 * (
								GetGrayscaledAt(grayscaled, width, height, x + 1, y) -
								GetGrayscaledAt(grayscaled, width, height, x - 1, y))));
			}
			else if (EnergyFuncType == EnergyFuncType.VSquare || EnergyFuncType == EnergyFuncType.V1)
			{
				result = 
					GetGrayscaledAt(grayscaled, width, height, x + 1, y - 1) + 
					GetGrayscaledAt(grayscaled, width, height, x + 1, y    ) +
					GetGrayscaledAt(grayscaled, width, height, x + 1, y + 1) -
					GetGrayscaledAt(grayscaled, width, height, x - 1, y - 1) -
					GetGrayscaledAt(grayscaled, width, height, x - 1, y    ) - 
					GetGrayscaledAt(grayscaled, width, height, x - 1, y + 1);
				if (EnergyFuncType == EnergyFuncType.VSquare)
					result *= result;
			}
			else if (EnergyFuncType == EnergyFuncType.Laplacian)
			{
				result = GetGrayscaledAt(grayscaled, width, height, x + 1, y) +
						 GetGrayscaledAt(grayscaled, width, height, x - 1, y) +
						 GetGrayscaledAt(grayscaled, width, height, x, y + 1) +
						 GetGrayscaledAt(grayscaled, width, height, x, y - 1) -
							4 * *grayscaled;
			}

			return result;
		}

		private byte GetGrayscaledAt(byte* grayscaled, int width, int height, int x, int y)
		{
			if (x < 0 || x >= width || y < 0 || y >= height)
				return 0;
			else
				return grayscaled[y * width + x];
		}

		private void CalculateEnergyDiff(byte* energy, int* energyDiff, int width, int height, int initWidth, int initHeight, bool xDir)
		{
			if (xDir)
			{
				for (int x = 0; x < width; x++)
					energyDiff[x] = energy[x];

				for (int y = 1; y < height; y++)
				{
					if (Parallelization)
					{
						Parallel.For(0, _threadCount, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, i =>
							CalculateEnergyDiffPart(energy, energyDiff, width, height, initWidth, initHeight, xDir,
							i * width / _threadCount, (i + 1) * width / _threadCount, y));
					}
					else
						CalculateEnergyDiffPart(energy, energyDiff, width, height, initWidth, initHeight, xDir, 0, width, y);
				}
			}
			else
			{
				for (int y = 0; y < height; y++)
					energyDiff[y * initWidth] = energy[y * initWidth];

				for (int x = 1; x < width; x++)
				{
					if (Parallelization)
					{
						Parallel.For(0, _threadCount, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, i => 
							CalculateEnergyDiffPart(energy, energyDiff, width, height, initWidth, initHeight, xDir,
							i * height / _threadCount, (i + 1) * height / _threadCount, x));
					}
					else
						CalculateEnergyDiffPart(energy, energyDiff, width, height, initWidth, initHeight, xDir,
							0, height, x);
				}
			}
		}

		private void CalculateEnergyDiffPart(byte* energy, int* energyDiff, int width, int height, int initWidth, int initHeight, bool xDir, int start, int stop, int constCoord)
		{
			if (xDir)
			{
				for (int x = start; x < stop; x++)
				{
					int ind = constCoord * initWidth + x; // x; y
					int ind1 = ind - initWidth; // x; y - 1
					int result = energyDiff[ind1];
					if (ForwardEnergy)
						result += ForwardCostUX(energy, initWidth, initHeight, x, constCoord);
					int energyDiffElem;
					if (x > 0)
					{
						energyDiffElem = energyDiff[ind1 - 1];
						if (ForwardEnergy)
							energyDiffElem += ForwardCostLX(energy, initWidth, initHeight, x, constCoord);
						if (energyDiffElem < result)
							result = energyDiffElem;
					}
					if (x < width - 1)
					{
						energyDiffElem = energyDiff[ind1 + 1];
						if (ForwardEnergy)
							energyDiffElem += ForwardCostRX(energy, initWidth, initHeight, x, constCoord);
						if (energyDiffElem < result)
							result = energyDiffElem;
					}

					if (!ForwardEnergy)
						energyDiff[ind] = result + energy[ind];
					else
						energyDiff[ind] = result;
				}
			}
			else
			{
				for (int y = start; y < stop; y++)
				{
					int ind = y * initWidth + constCoord;
					int ind1 = ind - 1;
					int result = energyDiff[ind1];
					if (ForwardEnergy)
						result += ForwardCostUY(energy, initWidth, initHeight, constCoord, y);
					int energyDiffElem;
					if (y > 0)
					{
						energyDiffElem = energyDiff[ind1 - initWidth];
						if (ForwardEnergy)
							energyDiffElem += ForwardCostLY(energy, initWidth, initHeight, constCoord, y);
						if (energyDiffElem < result)
							result = energyDiffElem;
					}
					if (y < height - 1)
					{
						energyDiffElem = energyDiff[ind1 + initWidth];
						if (ForwardEnergy)
							energyDiffElem += ForwardCostRY(energy, initWidth, initHeight, constCoord, y);
						if (energyDiffElem < result)
							result = energyDiffElem;
					}

					if (!ForwardEnergy)
						energyDiff[ind] = result + energy[ind];
					else
						energyDiff[ind] = result;
				}
			}
		}

		private void FindShrinkedPixels(int* energyDiff, int* shrinked, int width, int height, int initWidth, int initHeight, bool xDir)
		{
			if (xDir)
			{
				int last = height - 1;

				shrinked[last] = 0;
				for (int j = 1; j < width; j++)
					if (energyDiff[last * initWidth + j] < energyDiff[last * initWidth + shrinked[last]])
						shrinked[last] = j;

				for (int i = last - 1; i >= 0; i--)
				{
					int prev = shrinked[i + 1];
					int resultValue = prev;
					int iw = i * initWidth;
					if (prev > 0 && energyDiff[iw + resultValue] > energyDiff[iw + prev - 1])
						resultValue = prev - 1;
					if (prev < width - 1 && energyDiff[iw + resultValue] > energyDiff[iw + prev + 1])
						resultValue = prev + 1;
					shrinked[i] = resultValue;
				}
			}
			else
			{
				int last = width - 1;

				shrinked[last] = 0;
				for (int j = 1; j < height; j++)
					if (energyDiff[j * initWidth + last] < energyDiff[shrinked[last] * initWidth + last])
						shrinked[last] = j;

				for (int i = last - 1; i >= 0; i--)
				{
					int prev = shrinked[i + 1];
					int resultValue = prev;
					if (prev > 0 && energyDiff[resultValue * initWidth + i] > energyDiff[(prev - 1) * initWidth + i])
						resultValue = prev - 1;
					if (prev < height - 1 && energyDiff[resultValue * initWidth + i] > energyDiff[(prev + 1) * initWidth + i])
						resultValue = prev + 1;
					shrinked[i] = resultValue;
				}
			}
		}

		private int CalculateShrinkedSum(int* shrinked, int* energyDiff, int width, int height, bool xDir)
		{
			int result = 0;
			if (xDir)
			{
				for (int i = 0; i < height; i++)
					result += energyDiff[i * width + shrinked[i]];
			}
			else
			{
				for (int i = 0; i < width; i++)
					result += energyDiff[shrinked[i] * width + i];
			}
			return result;
		}

		private void DecreaseBitmap(byte* colored, byte* energy, int* cropPixels, int width, int height, int initWidth, int initHeight, bool xDir)
		{
			int s = xDir ? height : width;
			if (Parallelization)
			{
				Parallel.For(0, _threadCount, new ParallelOptions{ MaxDegreeOfParallelism = _threadCount }, i =>
							DecreaseBitmapPart(colored, energy, cropPixels, width, height, initWidth, initHeight, xDir,
							i * s / _threadCount, (i + 1) * s / _threadCount));
			}
			else
				DecreaseBitmapPart(colored, energy, cropPixels, width, height, initWidth, initHeight, xDir, 0, s);
		}

		private void DecreaseBitmapPart(byte* colored, byte* energy, int* cropPixels, int width, int height, int initWidth, int initHeight, bool xDir, int start, int stop)
		{
			if (xDir)
			{
				for (int y = start; y < stop; y++)
				{
					int t = y * initWidth + cropPixels[y];
					byte* coloredPtr = colored + t * ColorOffset;
					byte* energyPtr = energy + t;
					for (int j = cropPixels[y]; j < width; j++)
					{
						coloredPtr[RedInc] = coloredPtr[ColorOffset + RedInc];
						coloredPtr[GreenInc] = coloredPtr[ColorOffset + GreenInc];
						coloredPtr[BlueInc] = coloredPtr[ColorOffset + BlueInc];
						*energyPtr = *(energyPtr + 1);

						coloredPtr += ColorOffset;
						energyPtr++;
					}
				}
			}
			else
			{
				for (int x = start; x < stop; x++)
				{
					int t = cropPixels[x] * initWidth + x;
					byte* coloredPtr = colored + t * ColorOffset;
					byte* energyPtr = energy + t;
					int offset = ColorOffset * initWidth;
					for (int j = cropPixels[x]; j < height; j++)
					{
						coloredPtr[RedInc] = coloredPtr[offset + RedInc];
						coloredPtr[GreenInc] = coloredPtr[offset + GreenInc];
						coloredPtr[BlueInc] = coloredPtr[offset + BlueInc];
						*energyPtr = *(energyPtr + initWidth);

						coloredPtr += offset;
						energyPtr += initWidth;
					}
				}
			}
		}

		#region Forward Energy

		private int ForwardCostLX(byte* energy, int width, int height, int x, int y)
		{
			return Math.Abs(energy[y * width + (x + 1)] - energy[y * width + (x - 1)])
				+ Math.Abs(energy[(y - 1) * width + x] - energy[y * width + (x - 1)]);
		}

		private int ForwardCostUX(byte* energy, int width, int height, int x, int y)
		{
			return Math.Abs(energy[y * width + (x + 1)] - energy[y * width + (x - 1)]);
		}

		private int ForwardCostRX(byte* energy, int width, int height, int x, int y)
		{
			return Math.Abs(energy[y * width + (x + 1)] - energy[y * width + (x - 1)])
				+ Math.Abs(energy[(y - 1) * width + x] - energy[y * width + (x + 1)]);
		}

		private int ForwardCostLY(byte* energy, int width, int height, int x, int y)
		{
			return Math.Abs(energy[(y + 1) * width + x] - energy[(y - 1) * width + x])
				+ Math.Abs(energy[y * width + (x - 1)] - energy[(y - 1) * width + x]);
		}

		private int ForwardCostUY(byte* energy, int width, int height, int x, int y)
		{
			return Math.Abs(energy[(y + 1) * width + x] - energy[(y - 1) * width + x]);
		}

		private int ForwardCostRY(byte* energy, int width, int height, int x, int y)
		{
			return Math.Abs(energy[(y + 1) * width + x] - energy[(y - 1) * width + x])
				+ Math.Abs(energy[y * width + (x - 1)] - energy[(y + 1) * width + x]);
		}

		#endregion

		#endregion

		#region Image Conversation

		private void BitmapToColored(byte* bitmap, byte* colored, int length)
		{
			byte* coloredPtr = colored;
			byte* bitmapPtr = bitmap;
			for (int i = 0; i < length; i++)
			{
				coloredPtr[RedInc] = bitmapPtr[RedInc];
				coloredPtr[GreenInc] = bitmapPtr[GreenInc];
				coloredPtr[BlueInc] = bitmapPtr[BlueInc];
				coloredPtr += ColorOffset;
				bitmapPtr += ColorOffset;
			}
		}

		private void ColoredToGrayscaled(byte* colored, byte* grayscaled, int length)
		{
			byte* source = colored;
			byte* dest = grayscaled;
			for (int i = 0; i < length; i++)
			{
				*dest = (byte)Math.Round(source[RedInc] * 0.2125 + source[GreenInc] * 0.7154 + source[BlueInc] * 0.0721);
				source += ColorOffset;
				dest++;
			}
		}

		private Bitmap GrayscaledToBitmap(byte* grayscaled, int width, int height, int initWidth, int initHeight)
		{
			Bitmap result = new Bitmap(width, height);
			BitmapData bitmapData = null;
			try
			{
				bitmapData = result.LockBits(
					new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
				byte* bitmapPtr = (byte*)bitmapData.Scan0.ToPointer();

				byte* grayscaledPtr = grayscaled;
				int srcOffset = initWidth - width;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						bitmapPtr[RedInc] = *grayscaledPtr;
						bitmapPtr[GreenInc] = *grayscaledPtr;
						bitmapPtr[BlueInc] = *grayscaledPtr;
						bitmapPtr += ColorOffset;
						grayscaledPtr++;
					}
					grayscaledPtr += srcOffset;
				}
			}
			finally
			{
				if (bitmapData != null)
					result.UnlockBits(bitmapData);
			}

			return result;
		}

		private Bitmap ColoredToBitmap(byte* colored, int width, int height, int initWidth, int initHeight)
		{
			Bitmap result = new Bitmap(width, height);
			BitmapData bitmapData = null;
			try
			{
				bitmapData = result.LockBits(
					new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
				byte* bitmapPtr = (byte*)bitmapData.Scan0.ToPointer();

				int srcOffset = (initWidth - width) * ColorOffset;
				byte* coloredPtr = colored;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						bitmapPtr[RedInc] = coloredPtr[RedInc];
						bitmapPtr[GreenInc] = coloredPtr[GreenInc];
						bitmapPtr[BlueInc] = coloredPtr[BlueInc];
						bitmapPtr += ColorOffset;
						coloredPtr += ColorOffset;
					}
					coloredPtr += srcOffset;
				}
			}
			finally
			{
				if (bitmapData != null)
					result.UnlockBits(bitmapData);
			}

			return result;
		}

		#endregion
	}
}
