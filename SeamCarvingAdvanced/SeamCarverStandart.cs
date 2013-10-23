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

		private const int EnergyDiffPathDec = 3;
		private const int EnergyDiffPathInc = 2;

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
				_initWidth = bitmap.Width;
				_initHeight = bitmap.Height;
				int length = _initWidth * _initHeight;
				byte[] coloredArray = new byte[length * ColorOffset];
				byte[] grayscaleArray = new byte[length];
				byte[] energyArray = new byte[length];
				int[] energyDiffArray = new int[length];
				int[] shrinkedArray = new int[Math.Max(_initWidth, _initHeight)];
				int width = _initWidth;
				int height = _initHeight;

				fixed (byte* colored = coloredArray, grayscaled = grayscaleArray, energy = energyArray)
				{
					BitmapToColored(bitmapPtr, colored, length);
					ColoredToGrayscaled(colored, grayscaled, length);
					CalculateEnergy(grayscaled, energy, width, height);

					fixed (int* energyDiff = energyDiffArray, shrinked = shrinkedArray)
					{
						if (!Hd || width == newWidth || height == newHeight)
						{
							if (newWidth < width)
							{
								shrinked[0] = int.MinValue;
								do
								{
									//CalculateEnergyDiff(energy, energyDiff, width, height, true);
									int xMin;
									CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, true, out xMin);
									FindShrinkedPixels(energyDiff, shrinked, width, height, true, xMin);
									width--;
									DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, true);
								}
								while (width > newWidth);
							}
							if (newHeight < height)
							{
								shrinked[0] = int.MinValue;
								do
								{
									//CalculateEnergyDiff(energy, energyDiff, width, height, false);
									int yMin;
									CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, false, out yMin);
									FindShrinkedPixels(energyDiff, shrinked, width, height, false, yMin);
									height--;
									DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, false);
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
										int xMin;
										xSum = CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, true, out xMin);
										FindShrinkedPixels(energyDiff, shrinked, width, height, true, xMin);
									}
									else
										xSum = int.MaxValue;

									if (height != newHeight)
									{
										int yMin;
										ySum = CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, false, out yMin);
										FindShrinkedPixels(energyDiff, shrinked, width, height, false, yMin);
									}
									else
										ySum = int.MaxValue;

									if (xSum <= ySum)
									{
										width--;
										DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, true);
									}
									else
									{
										height--;
										DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, false);
									}
								}
								while (width > newWidth || height > newHeight);
							}
						}
					}

					result = ColoredToBitmap(colored, width, height);
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
		private int _initWidth;
		private int _initHeight;

		private void CalculateEnergy(byte* grayscaled, byte* energy, int width, int height)
		{
			byte* grayscaledPtr;
			byte* energyPtr;
			int length = width * height;
			int startX = 1;
			int startY = 1;
			int stopX = width - 1;
			int stopY = height - 1;

			int dstOffset = 2 + (_initWidth - width);
			int srcOffset = 2 + (_initWidth - width);

			grayscaledPtr = grayscaled + width * startY + startX;
			energyPtr = energy + width * startY + startX;

			int max = 0;
			if (Parallelization)
			{
				Parallel.For(0, _threadCount, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, y =>
					CalculateEnergyPart(grayscaled, energy, width, height,
					1, width - 1, y * (height - 1) / _threadCount + 1, (y + 1) * (height - 1) / _threadCount, ref max));
			}
			else
			{
				CalculateEnergyPart(grayscaled, energy, width, height, 1, width - 1, 1, height - 1, ref max);
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

		private void CalculateEnergyPart(byte* grayscaled, byte* energy, int width, int height,
			int startX, int stopX, int startY, int stopY, ref int max)
		{
			int srcOffset = 2 + (_initWidth - width);
			int dstOffset = 2 + (_initWidth - width);

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

		private void CalculateEnergyDiff(byte* energy, int* energyDiff, int width, int height, bool xDir)
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
							CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir,
							i * width / _threadCount, (i + 1) * width / _threadCount, y));
					}
					else
						CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, 0, width, y);
				}
			}
			else
			{
				for (int y = 0; y < height; y++)
					energyDiff[y * _initWidth] = energy[y * _initWidth];

				for (int x = 1; x < width; x++)
				{
					if (Parallelization)
					{
						Parallel.For(0, _threadCount, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, i => 
							CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir,
							i * height / _threadCount, (i + 1) * height / _threadCount, x));
					}
					else
						CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, 0, height, x);
				}
			}
		}
		
		private int CalculateEnergyDiff2(byte* energy, int* energyDiff, int* shrinked, int width, int height, bool xDir, out int sMin)
		{
			int w1 = width - 1;
			int h1 = height - 1;
			if (xDir)
			{
				int xMin, xMax;
				int boundaryMinX, boundaryMaxX;
				int minXEnergy, maxXEnergy;
				if (shrinked[0] == int.MinValue)
				{
					xMin = 0;
					xMax = w1;
				}
				else
				{
					xMin = Math.Max(shrinked[0] - EnergyDiffPathDec, 0);
					xMax = Math.Min(shrinked[0] + EnergyDiffPathInc, w1);
				}

				for (int x = xMin; x <= xMax; x++)
					energyDiff[x] = energy[x];

				for (int y = 1; y <= h1; y++)
				{
					xMin = Math.Max(xMin - 1, 0);
					xMax = Math.Min(xMax + 1, w1);
					boundaryMinX = 0;
					boundaryMaxX = 0;

					if (xMax == w1)
					{
						energyDiff[y * _initWidth + w1] = Math.Min(energyDiff[(y - 1) * _initWidth + (w1 - 1)],
							energyDiff[(y - 1) * _initWidth + w1]) + energy[y * _initWidth + w1];
						boundaryMaxX = 1;
					}
					if (xMin == 0)
					{
						energyDiff[y * _initWidth + 0] = Math.Min(energyDiff[(y - 1) * _initWidth + 0],
							energyDiff[(y - 1) * _initWidth + 1]) + energy[y * _initWidth + 0];
						boundaryMinX = 1;
					}

					minXEnergy = energyDiff[y * _initWidth + xMin];
					maxXEnergy = energyDiff[y * _initWidth + xMax];

					if (Parallelization)
					{
						int w2 = (xMax - boundaryMaxX + 1) - (xMin + boundaryMinX);
						Parallel.For(0, _threadCount,
							new ParallelOptions { MaxDegreeOfParallelism = _threadCount },
							i =>
							CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir,
							i * w2 / _threadCount + xMin + boundaryMinX,
							(i + 1) * w2 / _threadCount + xMin + boundaryMinX, y));
					}
					else
						CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, xMin + boundaryMinX, xMax - boundaryMaxX + 1, y);

					if (shrinked[0] != int.MinValue)
					{
						if (shrinked[y] > xMin + EnergyDiffPathDec && energyDiff[y * _initWidth + xMin] == minXEnergy)
							xMin++;
						if (shrinked[y] < xMax - EnergyDiffPathInc && energyDiff[y * _initWidth + xMax] == maxXEnergy)
							xMax--;
					}
				}

				sMin = 0;
				int sMinValue = energyDiff[h1 * _initWidth + sMin];
				for (int x = 1; x < width; x++)
					if (energyDiff[h1 * _initWidth + x] < sMinValue)
					{
						sMin = x;
						sMinValue = energyDiff[h1 * _initWidth + sMin];
					}

				return sMinValue;
			}
			else
			{
				int yMin, yMax;
				int boundaryMinY, boundaryMaxY;
				int minYEnergy, maxYEnergy;
				if (shrinked[0] == int.MinValue)
				{
					yMin = 0;
					yMax = h1;
				}
				else
				{
					yMin = Math.Max(shrinked[0] - EnergyDiffPathDec, 0);
					yMax = Math.Min(shrinked[0] + EnergyDiffPathInc, h1);
				}

				for (int y = yMin; y <= yMax; y++)
					energyDiff[y * _initWidth] = energy[y * _initWidth];

				for (int x = 1; x <= w1; x++)
				{
					yMin = Math.Max(yMin - 1, 0);
					yMax = Math.Min(yMax + 1, h1);
					boundaryMinY = 0;
					boundaryMaxY = 0;

					if (yMax == h1)
					{
						energyDiff[h1 * _initWidth + x] = Math.Min(energyDiff[(h1 - 1) * _initWidth + (x - 1)],
							energyDiff[h1 * _initWidth + (x - 1)]) + energy[h1 * _initWidth + x];
						boundaryMaxY = 1;
					}
					if (yMin == 0)
					{
						energyDiff[0 * _initWidth + x] = Math.Min(energyDiff[0 * _initWidth + (x - 1)],
							energyDiff[1 * _initWidth + (x - 1)]) + energy[0 * _initWidth + x];
						boundaryMinY = 1;
					}

					minYEnergy = energyDiff[yMin * _initWidth + x];
					maxYEnergy = energyDiff[yMax * _initWidth + x];

					if (Parallelization)
					{
						int w2 = (yMax - boundaryMaxY + 1) - (yMin + boundaryMinY);
						Parallel.For(0, _threadCount,
							new ParallelOptions { MaxDegreeOfParallelism = _threadCount },
							i =>
							CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir,
							i * w2 / _threadCount + yMin + boundaryMinY,
							(i + 1) * w2 / _threadCount + yMin + boundaryMinY, x));
					}
					else
						CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir,
							yMin + boundaryMinY, yMax - boundaryMaxY + 1, x);

					if (shrinked[0] != int.MinValue)
					{
						if (shrinked[x] > yMin + EnergyDiffPathDec && energyDiff[yMin * _initWidth + x] == minYEnergy)
							yMin++;
						if (shrinked[x] < yMax - EnergyDiffPathInc && energyDiff[yMax * _initWidth + x] == maxYEnergy)
							yMax--;
					}
				}

				sMin = 0;
				int sMinValue = energyDiff[sMin * _initWidth + w1];
				for (int y = 1; y < height; y++)
					if (energyDiff[y * _initWidth + w1] < sMinValue)
					{
						sMin = y;
						sMinValue = energyDiff[sMin * _initWidth + w1];
					}

				return sMinValue;
			}
		}

		private void CalculateEnergyDiffPart(byte* energy, int* energyDiff, int width, int height, bool xDir, int start, int stop, int s)
		{
			if (xDir)
			{
				int ind_y = s * _initWidth;
				for (int x = start; x < stop; x++)
				{
					int ind = ind_y + x; // x; y
					int ind_ym1 = ind - _initWidth; // x; y - 1
					int result = energyDiff[ind_ym1];
					int val_ym1 = 0;
					int costUx = 0;
					if (ForwardEnergy)
					{
						val_ym1 = energy[ind_ym1];
						costUx = Abs(energy[ind_y + (x + 1)] - energy[ind_y + (x - 1)]);
						result += costUx;
					}
					int energyDiffElem;
					if (x > 0)
					{
						energyDiffElem = energyDiff[ind_ym1 - 1];
						if (ForwardEnergy)
							energyDiffElem += costUx + Abs(val_ym1 - energy[ind_y + (x - 1)]);
						if (energyDiffElem < result)
							result = energyDiffElem;
					}
					if (x < width - 1)
					{
						energyDiffElem = energyDiff[ind_ym1 + 1];
						if (ForwardEnergy)
							energyDiffElem += costUx + Abs(val_ym1 - energy[ind_y + (x + 1)]);
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
					int ind = y * _initWidth + s;
					int ind_xm1 = ind - 1;
					int result = energyDiff[ind_xm1];
					int val_xm1 = 0;
					int costUy = 0;
					if (ForwardEnergy)
					{
						val_xm1 = energy[ind_xm1];
						costUy = Abs(energy[(y + 1) * _initWidth + s] - energy[(y - 1) * _initWidth + s]);
						result += costUy;
					}
					int energyDiffElem;
					if (y > 0)
					{
						energyDiffElem = energyDiff[ind_xm1 - _initWidth];
						if (ForwardEnergy)
							energyDiffElem += costUy + Abs(val_xm1 - energy[(y - 1) * _initWidth + s]);
						if (energyDiffElem < result)
							result = energyDiffElem;
					}
					if (y < height - 1)
					{
						energyDiffElem = energyDiff[ind_xm1 + _initWidth];
						if (ForwardEnergy)
							energyDiffElem += costUy + Abs(val_xm1 - energy[(y + 1) * _initWidth + s]);
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

		private void FindShrinkedPixels(int* energyDiff, int* shrinked, int width, int height, bool xDir, int sMin)
		{
			if (xDir)
			{
				int h1 = height - 1;
				shrinked[h1] = sMin;
				int prev = shrinked[h1];
				for (int y = h1 - 1; y >= 0; y--)
				{
					int resultValue = prev;
					int iw = y * _initWidth;
					if (prev > 0 && energyDiff[iw + resultValue] > energyDiff[iw + prev - 1])
						resultValue = prev - 1;
					if (prev < width - 1 && energyDiff[iw + resultValue] > energyDiff[iw + prev + 1])
						resultValue = prev + 1;
					shrinked[y] = resultValue;
					prev = resultValue;
				}
			}
			else
			{
				int w1 = width - 1;
				shrinked[w1] = sMin;
				int prev = shrinked[w1];
				for (int x = w1 - 1; x >= 0; x--)
				{
					int resultValue = prev;
					if (prev > 0 && energyDiff[resultValue * _initWidth + x] > energyDiff[(prev - 1) * _initWidth + x])
						resultValue = prev - 1;
					if (prev < height - 1 && energyDiff[resultValue * _initWidth + x] > energyDiff[(prev + 1) * _initWidth + x])
						resultValue = prev + 1;
					shrinked[x] = resultValue;
					prev = resultValue;
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

		private void DecreaseBitmap(byte* colored, byte* grayscaled, byte* energy, int* cropPixels, int width, int height, bool xDir)
		{
			int s = xDir ? height : width;
			if (Parallelization)
			{
				Parallel.For(0, _threadCount, new ParallelOptions{ MaxDegreeOfParallelism = _threadCount }, i =>
							DecreaseBitmapPart(colored, grayscaled, energy, cropPixels, width, height, xDir,
							i * s / _threadCount, (i + 1) * s / _threadCount));
			}
			else
				DecreaseBitmapPart(colored, grayscaled, energy, cropPixels, width, height, xDir, 0, s);
		}

		private void DecreaseBitmapPart(byte* colored, byte* grayscaled, byte* energy, int* cropPixels, int width, int height, bool xDir, int start, int stop)
		{
			if (xDir)
			{
				for (int y = start; y < stop; y++)
				{
					int cropPixelY = cropPixels[y];
					int t = y * _initWidth + cropPixelY;
					byte* coloredPtr = colored + t * ColorOffset;
					byte* energyPtr = energy + t;
					byte* grayscaledPtr = grayscaled + t;
					for (int j = cropPixelY; j < width; j++)
					{
						coloredPtr[RedInc] = coloredPtr[ColorOffset + RedInc];
						coloredPtr[GreenInc] = coloredPtr[ColorOffset + GreenInc];
						coloredPtr[BlueInc] = coloredPtr[ColorOffset + BlueInc];
						*grayscaledPtr = *(grayscaledPtr + 1);
						*energyPtr = *(energyPtr + 1);

						coloredPtr += ColorOffset;
						energyPtr++;
						grayscaledPtr++;
					}
				}
			}
			else
			{
				for (int x = start; x < stop; x++)
				{
					int cropPixelX = cropPixels[x];
					int t = cropPixelX * _initWidth + x;
					byte* coloredPtr = colored + t * ColorOffset;
					byte* energyPtr = energy + t;
					byte* grayscaledPtr = grayscaled + t;
					int offset = ColorOffset * _initWidth;
					for (int j = cropPixelX; j < height; j++)
					{
						coloredPtr[RedInc] = coloredPtr[offset + RedInc];
						coloredPtr[GreenInc] = coloredPtr[offset + GreenInc];
						coloredPtr[BlueInc] = coloredPtr[offset + BlueInc];
						*grayscaledPtr = *(grayscaledPtr + _initWidth);
						*energyPtr = *(energyPtr + _initWidth);

						coloredPtr += offset;
						energyPtr += _initWidth;
					}
				}
			}
		}

		private static int Abs(int a)
		{
			return a >= 0 ? a : -a;
		}

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

		private Bitmap GrayscaledToBitmap(byte* grayscaled, int width, int height)
		{
			Bitmap result = new Bitmap(width, height);
			BitmapData bitmapData = null;
			try
			{
				bitmapData = result.LockBits(
					new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
				byte* bitmapPtr = (byte*)bitmapData.Scan0.ToPointer();

				byte* grayscaledPtr = grayscaled;
				int srcOffset = _initWidth - width;
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

		private Bitmap ColoredToBitmap(byte* colored, int width, int height)
		{
			Bitmap result = new Bitmap(width, height);
			BitmapData bitmapData = null;
			try
			{
				bitmapData = result.LockBits(
					new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
				byte* bitmapPtr = (byte*)bitmapData.Scan0.ToPointer();

				int srcOffset = (_initWidth - width) * ColorOffset;
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
