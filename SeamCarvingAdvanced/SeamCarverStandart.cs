using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace SeamCarvingAdvanced
{
	public unsafe class SeamCarverStandart : SeamCarver
	{
		#region Constants

		protected const int ColorOffset = 4;
		protected const int RedInc = 2;
		protected const int GreenInc = 1;
		protected const int BlueInc = 0;

		private const int EnergyDiffPathDec = 3;
		private const int EnergyDiffPathInc = 2;

		protected const int Offset = 1;
		public const int MaxEnergyDiff = 16777216;

		#endregion

		#region Public

		public double NeighbourCountRatio
		{
			get;
			set;
		}

		public int BlockSize
		{
			get;
			set;
		}

		public SeamCarverStandart(EnergyFuncType energyFuncType, bool hd, bool forwardEnergy, bool parallelization,
			double neighbourCountRatio, int blockSize)
			: base(energyFuncType, hd, forwardEnergy, parallelization)
		{
			NeighbourCountRatio = neighbourCountRatio;
			BlockSize = blockSize;
		}

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
			//if (!Parallelization)
			{
				energyArray = new byte[length];
				energyDiffArray = new int[length];
			}
			/*else
			{
				_initExtWidth = (_initWidth + (BlockSize - 1)) / BlockSize * BlockSize + 2 * (BlockSize + Offset);
				_initExtHeight = (_initHeight + (BlockSize - 1)) / BlockSize * BlockSize + 2 * (BlockSize + Offset);
				int energyArrayLength = _initExtWidth * _initExtHeight;
				energyArray = new byte[length]; // TODO: Fix it with extendedLength: //energyArray = new byte[energyArrayLength];
				energyDiffArray = new int[energyArrayLength];
			}*/

			int[] shrinkedArray = new int[Math.Max(_initWidth, _initHeight)];

			int width = _initWidth;
			int height = _initHeight;

			fixed (byte* colored = coloredArray, grayscaled = grayscaleArray, energy = energyArray)
			{
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
								int xMin;
								//CalculateEnergyDiff(energy, energyDiff, width, height, true, out xMin);
								CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, true, out xMin);
								//CalculateEnergyDiffParallel(energy, energyDiff, width, height, true, out xMin);
								FindShrinkedPixels(energyDiff, shrinked, width, height, true, xMin);
								DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, true);
								width--;
							}
							while (width > newWidth);
						}
						if (newHeight < height)
						{
							shrinked[0] = int.MinValue;
							do
							{
								int yMin;
								//CalculateEnergyDiff(energy, energyDiff, width, height, false, out yMin);
								CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, false, out yMin);
								FindShrinkedPixels(energyDiff, shrinked, width, height, false, yMin);
								DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, false);
								height--;
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
									shrinked[0] = int.MinValue;
									int xMin;
									xSum = CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, true, out xMin);
									FindShrinkedPixels(energyDiff, shrinked, width, height, true, xMin);
								}
								else
									xSum = int.MaxValue;

								if (height != newHeight)
								{
									shrinked[0] = int.MinValue;
									int yMin;
									ySum = CalculateEnergyDiff2(energy, energyDiff, shrinked, width, height, false, out yMin);
									FindShrinkedPixels(energyDiff, shrinked, width, height, false, yMin);
								}
								else
									ySum = int.MaxValue;

								if (xSum <= ySum)
								{
									DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, true);
									width--;
								}
								else
								{
									DecreaseBitmap(colored, grayscaled, energy, shrinked, width, height, false);
									height--;
								}
							}
							while (width > newWidth || height > newHeight);
						}
					}
				}
				result = ColoredToBitmap(colored, width, height);
			}

			return result;
		}

		#endregion

		#region Protected

		protected int _threadCount = Environment.ProcessorCount;
		protected object _lockObj = new object();
		protected int _initWidth;
		protected int _initHeight;
		protected int _initExtWidth;
		protected int _initExtHeight;
		protected byte[] energyArray;
		protected int[] energyDiffArray;

		protected void CalculateEnergy(byte* grayscaled, byte* energy, int width, int height)
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

			byte max = 0;
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

			byte g;
			for (int x = 0; x < width; x++)
			{
				g = ConvolvePixelSafe(grayscaled, width, height, x, 0);
				if (g > max)
					max = g;
				energyPtr[0 * width + x] = g;

				g = ConvolvePixelSafe(grayscaled, width, height, x, height - 1);
				if (g > max)
					max = g;
				energyPtr[(height - 1) * width + x] = g;
			}

			for (int y = startY; y < stopY; y++)
			{
				g = ConvolvePixelSafe(grayscaled, width, height, 0, y);
				if (g > max)
					max = g;
				energyPtr[y * width + 0] = g;

				g = ConvolvePixelSafe(grayscaled, width, height, width - 1, y);
				if (g > max)
					max = g;
				energyPtr[y * width + (width - 1)] = g;
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

		protected void CalculateEnergyPart(byte* grayscaled, byte* energy, int width, int height,
			int startX, int stopX, int startY, int stopY, ref byte max)
		{
			int srcOffset = 2 + (_initWidth - width);
			int dstOffset = 2 + (_initWidth - width);

			int grayscaledInd = width * startY + startX;
			byte* energyPtr = energy + width * startY + startX;

			byte g = 0;
			byte localMax = max;
			for (int y = startY; y < stopY; y++)
			{
				for (int x = startX; x < stopX; x++, grayscaledInd++, energyPtr++)
				{
					g = ConvolvePixel(grayscaled, grayscaledInd);

					*energyPtr = g;
					if (g > localMax)
						localMax = g;
				}

				grayscaledInd += srcOffset;
				energyPtr += dstOffset;
			}

			lock (_lockObj)
			{
				if (localMax > max)
					max = localMax;
			}
		}

		protected virtual int CalculateEnergyDiff(byte* energy, int* energyDiff, int width, int height, bool xDir, out int sMin)
		{
			if (!Parallelization)
			{
				if (xDir)
				{
					for (int x = 0; x < width; x++)
						energyDiff[x] = energy[x];

					for (int y = 1; y < height; y++)
						CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, 0, width, y);
				}
				else
				{
					for (int y = 0; y < height; y++)
						energyDiff[y * _initWidth] = energy[y * _initWidth];

					for (int x = 1; x < width; x++)
						CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, 0, height, x);
				}
			}
			else
			{
				if (xDir)
				{
					int horizBlockCount = (width + (BlockSize - 1)) / BlockSize;
					int vertBlockCount = (height + (BlockSize - 1)) / BlockSize;
					int extendedWidth = horizBlockCount * BlockSize + 2 * (BlockSize + Offset);
					int extendedHeight = vertBlockCount * BlockSize + 2 * (BlockSize + Offset);

					int* energyDiffExtPtr = energyDiff;
					byte* energyPtr = energy;

					energyDiffExtPtr = energyDiff + BlockSize * _initExtWidth;
					for (int x = 0; x < extendedWidth; x++, energyDiffExtPtr++)
						*energyDiffExtPtr = 0;

					energyDiffExtPtr = energyDiff + (BlockSize + Offset) * _initExtWidth + BlockSize + Offset;
					for (int x = 0; x < extendedWidth; x++, energyDiffExtPtr++)
					{
						int x_energy = x - BlockSize - Offset;
						if (x_energy >= 0 || x_energy < width)
							*energyDiffExtPtr = energy[x_energy];
					}
					energyDiffExtPtr = energyDiff + (BlockSize + Offset) * _initExtWidth;
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < extendedWidth; x++, energyDiffExtPtr++)
						{
							int x_energy = x - BlockSize - Offset;
							if (x_energy < 0 || x_energy >= width)
								*energyDiffExtPtr = MaxEnergyDiff;
						}
						energyDiffExtPtr += (_initExtWidth - extendedWidth);
					}

					energyDiffExtPtr = energyDiff;
					for (int i = 0; i < vertBlockCount; i++)
					{
						int startY = i * BlockSize + BlockSize + Offset;
						int stopY = startY + BlockSize;

						//for (int j = 0; j < horizBlockCount; j++)
						Parallel.For(0, horizBlockCount, j =>
						{
							int startX = j * BlockSize + Offset;
							int stopX = startX + BlockSize * 3;

							for (int y = startY; y < stopY; y++)
							{
								int y_energy = y - BlockSize - Offset;
								for (int x = startX; x < stopX; x++)
								{
									int x_energy = x - BlockSize - Offset;

									int ind_ym1 = (y - 1) * _initExtWidth + x;
									int result = energyDiffExtPtr[ind_ym1];

									int elem;
									elem = energyDiffExtPtr[ind_ym1 - 1];
									if (elem < result)
										result = elem;

									elem = energyDiffExtPtr[ind_ym1 + 1];
									if (elem < result)
										result = elem;

									if (x_energy >= 0 && x_energy < width && y_energy >= 0 && y_energy < height)
										energyDiffExtPtr[y * _initExtWidth + x] = result + energy[y_energy * _initWidth + x_energy];
									else
										energyDiffExtPtr[y * _initExtWidth + x] = result + MaxEnergyDiff;
								}
								startX++;
								stopX--;
							}
						});
					}
				}
			}

			return GetMinIndAndValue(energyDiff, width, height, xDir, out sMin);
		}

		protected int CalculateEnergyDiff2(byte* energy, int* energyDiff, int* shrinked, int width, int height, bool xDir, out int sMin)
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

					CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, xMin + boundaryMinX, xMax - boundaryMaxX + 1, y);

					if (shrinked[0] != int.MinValue)
					{
						if (shrinked[y] > xMin + EnergyDiffPathDec && energyDiff[y * _initWidth + xMin] == minXEnergy)
							xMin++;
						if (shrinked[y] < xMax - EnergyDiffPathInc && energyDiff[y * _initWidth + xMax] == maxXEnergy)
							xMax--;
					}
				}
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

					CalculateEnergyDiffPart(energy, energyDiff, width, height, xDir, yMin + boundaryMinY, yMax - boundaryMaxY + 1, x);

					if (shrinked[0] != int.MinValue)
					{
						if (shrinked[x] > yMin + EnergyDiffPathDec && energyDiff[yMin * _initWidth + x] == minYEnergy)
							yMin++;
						if (shrinked[x] < yMax - EnergyDiffPathInc && energyDiff[yMax * _initWidth + x] == maxYEnergy)
							yMax--;
					}
				}
			}

			return GetMinIndAndValue(energyDiff, width, height, xDir, out sMin);
		}

		protected virtual int CalculateEnergyDiffParallel(byte* energy, int* energyDiff, int width, int height, bool xDir, out int sMin)
		{
			if (xDir)
			{
				int blockSize2 = BlockSize / 2;
				int blockCountX1 = (width + BlockSize - 1) / BlockSize;
				int blockCountX2 = blockCountX1 + 1;
				int blockCountY = (height + blockSize2 - 1) / blockSize2;

				for (int i = 0; i < blockCountY; i++)
				{
					int yStart = i * blockSize2;
					int yStop = Math.Min(yStart + blockSize2, height);

					for (int j1 = 0; j1 < blockCountX1; j1++)
					{
						int xStart = j1 * BlockSize;
						int xStop = xStart + BlockSize;

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
										int y_x_ind = y * _initWidth + x;
										int ym1_x_ind = y_x_ind - _initWidth;

										int leftUpperVal = energyDiff[ym1_x_ind - 1];
										int rightUpperVal = x < width - 1 ? energyDiff[ym1_x_ind + 1] : MaxEnergyDiff;
										int upperVal = energyDiff[ym1_x_ind];

										energyDiff[y_x_ind] = Min(leftUpperVal, upperVal, rightUpperVal) + energy[y_x_ind];
									}
								}
							}
							xStart++;
							xStop--;
						}
					}

					yStart = Math.Min(yStart + 1, height);
					for (int j2 = 0; j2 < blockCountX2; j2++)
					{
						int xStart = j2 * BlockSize;
						int xStop = xStart;

						for (int y = yStart; y < yStop; y++)
						{
							for (int x = xStart; x <= xStop; x++)
							{
								if (x >= 0 || x < width)
								{
									int y_x_ind = y * _initWidth + x;
									int ym1_x_ind = y_x_ind - _initWidth;

									int leftUpperVal = x > 0 ? energyDiff[ym1_x_ind - 1] : MaxEnergyDiff;
									int rightUpperVal = x < width - 1 ? energyDiff[ym1_x_ind + 1] : MaxEnergyDiff;
									int upperVal = energyDiff[ym1_x_ind];

									energyDiff[y_x_ind] = Min(leftUpperVal, upperVal, rightUpperVal) + energy[y_x_ind];
								}
							}
							xStart--;
							xStop++;
						}
					}
				}
			}

			return GetMinIndAndValue(energyDiff, width, height, xDir, out sMin);
		}

		protected int GetMinIndAndValue(int* energyDiff, int width, int height, bool xDir, out int sMin)
		{
			int scale;
			int offset;
			if (!Parallelization)
			{
				scale = _initWidth;
				offset = 0;
			}
			else
			{
				scale = _initExtWidth;
				offset = (BlockSize + Offset) * _initExtWidth + BlockSize + Offset;
			}

			if (xDir)
			{
				int h1 = height - 1;
				sMin = 0;
				int sMinValue = energyDiff[h1 * scale + sMin + offset];
				for (int x = 1; x < width; x++)
					if (energyDiff[h1 * scale + x + offset] < sMinValue)
					{
						sMin = x;
						sMinValue = energyDiff[h1 * scale + sMin + offset];
					}

				return sMinValue;
			}
			else
			{
				int w1 = width - 1;
				sMin = 0;
				int sMinValue = energyDiff[sMin * scale + w1 + offset];
				for (int y = 1; y < height; y++)
					if (energyDiff[y * scale + w1 + offset] < sMinValue)
					{
						sMin = y;
						sMinValue = energyDiff[sMin * scale + w1 + offset];
					}

				return sMinValue;
			}
		}

		protected void CalculateEnergyDiffPart(byte* energy, int* energyDiff, int width, int height, bool xDir, int start, int stop, int s)
		{
			if (xDir)
			{
				int ind_y = s * _initWidth;
				for (int x = start; x < stop; x++)
				{
					int ind = ind_y + x; // x; y
					int ind_ym1 = ind - _initWidth; // x; y - 1
					int result = energyDiff[ind_ym1];
					int energyDiffElem;

					if (!ForwardEnergy)
					{
						if (x >= 1)
						{
							energyDiffElem = energyDiff[ind_ym1 - 1];
							if (energyDiffElem < result)
								result = energyDiffElem;
						}

						if (x <= width - 2)
						{
							energyDiffElem = energyDiff[ind_ym1 + 1];
							if (energyDiffElem < result)
								result = energyDiffElem;
						}

						energyDiff[ind] = result + energy[ind];
					}
					else
					{
						int val_ym1 = energy[ind_ym1];
						int costUx = Abs(energy[ind_y + (x + 1)] - energy[ind_y + (x - 1)]);
						result += energyDiff[ind_ym1] + costUx;

						energyDiffElem = energyDiff[ind_ym1 - 1] + costUx + Abs(val_ym1 - energy[ind_y + (x - 1)]);
						if (energyDiffElem < result)
							result = energyDiffElem;

						energyDiffElem = energyDiff[ind_ym1 + 1] + costUx + Abs(val_ym1 - energy[ind_y + (x + 1)]);
						if (energyDiffElem < result)
							result = energyDiffElem;

						energyDiff[ind] = result;
					}
				}
			}
			else
			{
				for (int y = start; y < stop; y++)
				{
					int ind = y * _initWidth + s;
					int ind_xm1 = ind - 1;
					int result = energyDiff[ind_xm1];
					int energyDiffElem;

					if (!ForwardEnergy)
					{
						energyDiffElem = energyDiff[ind_xm1 - _initWidth];
						if (energyDiffElem < result)
							result = energyDiffElem;

						energyDiffElem = energyDiff[ind_xm1 + _initWidth];
						if (energyDiffElem < result)
							result = energyDiffElem;

						energyDiff[ind] = result + energy[ind];
					}
					else
					{
						int val_xm1 = energy[ind_xm1];
						int costUy = Abs(energy[(y + 1) * _initWidth + s] - energy[(y - 1) * _initWidth + s]);
						result += costUy;

						energyDiffElem = energyDiff[ind_xm1 - _initWidth] + costUy + Abs(val_xm1 - energy[(y - 1) * _initWidth + s]);
						if (energyDiffElem < result)
							result = energyDiffElem;

						energyDiffElem = energyDiff[ind_xm1 + _initWidth] + costUy + Abs(val_xm1 - energy[(y + 1) * _initWidth + s]);
						if (energyDiffElem < result)
							result = energyDiffElem;

						energyDiff[ind] = result;
					}
				}
			}
		}

		protected void FindShrinkedPixels(int* energyDiff, int* shrinked, int width, int height, bool xDir, int sMin)
		{
			int scale;
			int offset;
			if (!Parallelization)
			{
				scale = _initWidth;
				offset = 0;
			}
			else
			{
				scale = _initExtWidth;
				offset = (BlockSize + Offset) * _initExtWidth + BlockSize + Offset;
			}

			if (xDir)
			{
				int h1 = height - 1;
				shrinked[h1] = sMin;
				int prev = shrinked[h1];
				for (int y = h1 - 1; y >= 0; y--)
				{
					int resultValue = prev;
					int iw = y * scale;
					if (prev > 0 && energyDiff[iw + resultValue + offset] > energyDiff[iw + prev - 1 + offset])
						resultValue = prev - 1;
					if (prev < width - 1 && energyDiff[iw + resultValue + offset] > energyDiff[iw + prev + 1 + offset])
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
					if (prev > 0 && energyDiff[resultValue * scale + x + offset] > energyDiff[(prev - 1) * scale + x + offset])
						resultValue = prev - 1;
					if (prev < height - 1 && energyDiff[resultValue * scale + x + offset] > energyDiff[(prev + 1) * scale + x + offset])
						resultValue = prev + 1;
					shrinked[x] = resultValue;
					prev = resultValue;
				}
			}
		}

		protected void DecreaseBitmap(byte* colored, byte* grayscaled, byte* energy, int* cropPixels, int width, int height, bool xDir)
		{
			int constSize = xDir ? width : height;
			int second = xDir ? height : width;
			if (Parallelization)
			{
				Parallel.For(0, _threadCount, new ParallelOptions { MaxDegreeOfParallelism = _threadCount }, i =>
							DecreaseBitmapPart(colored, grayscaled, energy, cropPixels, xDir, constSize, i * second / _threadCount, (i + 1) * second / _threadCount));
			}
			else
				DecreaseBitmapPart(colored, grayscaled, energy, cropPixels, xDir, constSize, 0, second);

			if (xDir)
			{
				int newWidth = width - 1;
				int newHeight = height;

				for (int y = 0; y < height; y++)
				{
					int cropPixelX = cropPixels[y];
					bool safety = false;

					if (y <= 4 || y >= newHeight - 5 || cropPixelX <= 4 || cropPixelX >= newWidth - 5)
						safety = true;

					for (int x = cropPixelX - 3; x < cropPixelX + 3; x++)
					{
						int xLimited = Limit(x, 0, newWidth - 1);
						int ind = y * _initWidth + xLimited;
						if (!safety)
							energy[ind] = ConvolvePixel(grayscaled, ind);
						else
							energy[ind] = ConvolvePixelSafe(grayscaled, newWidth, newHeight, xLimited, y);
					}
				}
			}
			else
			{
				int newWidth = width;
				int newHeight = height - 1;

				for (int x = 0; x < width; x++)
				{
					int cropPixelY = cropPixels[x];
					bool safety = false;

					if (x <= 4 || x >= newWidth - 5 || cropPixelY <= 4 || cropPixelY >= newHeight - 5)
						safety = true;

					for (int y = cropPixelY - 3; y < cropPixelY + 3; y++)
					{
						int yLimited = Limit(y, 0, newHeight - 1);
						int ind = yLimited * _initWidth + x;
						if (!safety)
							energy[ind] = ConvolvePixel(grayscaled, ind);
						else
							energy[ind] = ConvolvePixelSafe(grayscaled, newWidth, newHeight, x, yLimited);
					}
				}
			}
		}

		protected void DecreaseBitmapPart(byte* colored, byte* grayscaled, byte* energy, int* cropPixels, bool xDir, int size, int start, int stop)
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

					byte r = coloredPtr[RedInc];
					byte g = coloredPtr[GreenInc];
					byte b = coloredPtr[BlueInc];

					if (cropPixelY != size - 1)
					{
						coloredPtr[RedInc + ColorOffset] = (byte)((r + coloredPtr[RedInc + ColorOffset]) / 2);
						coloredPtr[GreenInc + ColorOffset] = (byte)((g + coloredPtr[GreenInc + ColorOffset]) / 2);
						coloredPtr[BlueInc + ColorOffset] = (byte)((b + coloredPtr[BlueInc + ColorOffset]) / 2);
						grayscaledPtr[1] = ColoredToGrayscaled(coloredPtr + ColorOffset);
					}

					if (cropPixelY != 0)
					{
						coloredPtr[RedInc - ColorOffset] = (byte)((r + coloredPtr[RedInc - ColorOffset]) / 2);
						coloredPtr[GreenInc - ColorOffset] = (byte)((g + coloredPtr[GreenInc - ColorOffset]) / 2);
						coloredPtr[BlueInc - ColorOffset] = (byte)((b + coloredPtr[BlueInc - ColorOffset]) / 2);
						grayscaledPtr[-1] = ColoredToGrayscaled(coloredPtr - ColorOffset);
					}

					for (int x = cropPixelY; x < size - 1; x++)
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
				int offset = ColorOffset * _initWidth;
				for (int x = start; x < stop; x++)
				{
					int cropPixelX = cropPixels[x];
					int t = cropPixelX * _initWidth + x;
					byte* coloredPtr = colored + t * ColorOffset;
					byte* energyPtr = energy + t;
					byte* grayscaledPtr = grayscaled + t;

					byte r = coloredPtr[RedInc];
					byte g = coloredPtr[GreenInc];
					byte b = coloredPtr[BlueInc];

					if (cropPixelX != size - 1)
					{
						coloredPtr[RedInc + offset] = (byte)((r + coloredPtr[RedInc + offset]) / 2);
						coloredPtr[GreenInc + offset] = (byte)((g + coloredPtr[GreenInc + offset]) / 2);
						coloredPtr[BlueInc + offset] = (byte)((b + coloredPtr[BlueInc + offset]) / 2);
						grayscaledPtr[1] = ColoredToGrayscaled(coloredPtr + offset);
					}

					if (cropPixelX != 0)
					{
						coloredPtr[RedInc - offset] = (byte)((r + coloredPtr[RedInc - offset]) / 2);
						coloredPtr[GreenInc - offset] = (byte)((g + coloredPtr[GreenInc - offset]) / 2);
						coloredPtr[BlueInc - offset] = (byte)((b + coloredPtr[BlueInc - offset]) / 2);
						grayscaledPtr[-1] = ColoredToGrayscaled(coloredPtr - offset);
					}

					for (int j = cropPixelX; j < size - 1; j++)
					{
						coloredPtr[RedInc] = coloredPtr[offset + RedInc];
						coloredPtr[GreenInc] = coloredPtr[offset + GreenInc];
						coloredPtr[BlueInc] = coloredPtr[offset + BlueInc];
						*grayscaledPtr = *(grayscaledPtr + _initWidth);
						*energyPtr = *(energyPtr + _initWidth);

						coloredPtr += offset;
						energyPtr += _initWidth;
						grayscaledPtr += _initWidth;
					}
				}
			}
		}

		#region Misc

		protected byte ConvolvePixel(byte* grayscaled, int ind)
		{
			byte* grayscaledPtr = grayscaled + ind;
			int result = 0;
			if (EnergyFuncType == EnergyFuncType.Prewitt || EnergyFuncType == EnergyFuncType.Sobel)
			{
				int coef = 1;
				if (EnergyFuncType == EnergyFuncType.Sobel)
					coef = 2;

				result = Math.Abs(grayscaledPtr[-_initWidth + 1] + grayscaledPtr[_initWidth + 1]
								- grayscaledPtr[-_initWidth - 1] - grayscaledPtr[_initWidth - 1]
								+ coef * (grayscaledPtr[1] - grayscaledPtr[-1])) +
						 Math.Abs(grayscaledPtr[-_initWidth - 1] + grayscaledPtr[-_initWidth + 1]
								- grayscaledPtr[_initWidth - 1] - grayscaledPtr[_initWidth + 1]
								+ coef * (grayscaledPtr[-_initWidth] - grayscaledPtr[_initWidth]));
			}
			else if (EnergyFuncType == EnergyFuncType.VSquare || EnergyFuncType == EnergyFuncType.V1)
			{
				result = grayscaledPtr[-_initWidth + 1] + grayscaledPtr[1] + grayscaledPtr[+_initWidth + 1] -
					grayscaledPtr[-_initWidth - 1] - grayscaledPtr[-1] - grayscaledPtr[-_initWidth - 1];
				if (EnergyFuncType == EnergyFuncType.VSquare)
					result *= result;
				else
					result = Abs(result);
			}
			else if (EnergyFuncType == EnergyFuncType.Laplacian)
			{
				result = Abs(grayscaledPtr[1] + grayscaledPtr[-1] + grayscaledPtr[_initWidth] + grayscaledPtr[-_initWidth] -
					4 * *grayscaledPtr);
			}

			return (byte)Limit(result, 0, 255);
		}

		protected byte ConvolvePixelSafe(byte* grayscaled, int width, int height, int x, int y)
		{
			int result = 0;
			if (EnergyFuncType == EnergyFuncType.Prewitt || EnergyFuncType == EnergyFuncType.Sobel)
			{
				int coef = 1;
				if (EnergyFuncType == EnergyFuncType.Sobel)
					coef = 2;

				result = Math.Min(255,
						Math.Abs(GetGrayscaledAt(grayscaled, width, height, x - 1, y - 1) +
								 GetGrayscaledAt(grayscaled, width, height, x + 1, y - 1) -
								 GetGrayscaledAt(grayscaled, width, height, x - 1, y + 1) -
								 GetGrayscaledAt(grayscaled, width, height, x + 1, y + 1)
								+ coef * (
								GetGrayscaledAt(grayscaled, width, height, x, y - 1) -
								GetGrayscaledAt(grayscaled, width, height, x, y + 1)))

					  + Math.Abs(GetGrayscaledAt(grayscaled, width, height, x + 1, y - 1) +
								 GetGrayscaledAt(grayscaled, width, height, x + 1, y + 1) -
								 GetGrayscaledAt(grayscaled, width, height, x - 1, y - 1) -
								 GetGrayscaledAt(grayscaled, width, height, x - 1, y + 1)
								+ coef * (
								GetGrayscaledAt(grayscaled, width, height, x + 1, y) -
								GetGrayscaledAt(grayscaled, width, height, x - 1, y))));
			}
			else if (EnergyFuncType == EnergyFuncType.VSquare || EnergyFuncType == EnergyFuncType.V1)
			{
				result =
					GetGrayscaledAt(grayscaled, width, height, x + 1, y - 1) +
					GetGrayscaledAt(grayscaled, width, height, x + 1, y) +
					GetGrayscaledAt(grayscaled, width, height, x + 1, y + 1) -
					GetGrayscaledAt(grayscaled, width, height, x - 1, y - 1) -
					GetGrayscaledAt(grayscaled, width, height, x - 1, y) -
					GetGrayscaledAt(grayscaled, width, height, x - 1, y + 1);
				if (EnergyFuncType == EnergyFuncType.VSquare)
					result *= result;
				else
					result = Abs(result);
			}
			else if (EnergyFuncType == EnergyFuncType.Laplacian)
			{
				result = Abs(GetGrayscaledAt(grayscaled, width, height, x + 1, y) + GetGrayscaledAt(grayscaled, width, height, x - 1, y) +
						 GetGrayscaledAt(grayscaled, width, height, x, y + 1) + GetGrayscaledAt(grayscaled, width, height, x, y - 1) - 4 * *grayscaled);
			}

			return (byte)Limit(result, 0, 255);
		}

		protected byte GetGrayscaledAt(byte* grayscaled, int width, int height, int x, int y)
		{
			if (x < 0 || x >= width || y < 0 || y >= height)
				return 0;
			else
				return grayscaled[y * _initWidth + x];
		}

		protected static int Limit(int a, int min, int max)
		{
			if (a < min)
				return min;
			else if (a > max)
				return max;
			return a;
		}

		protected static int Abs(int a)
		{
			return a >= 0 ? a : -a;
		}

		public static int Min(int a, int b, int c)
		{
			int result = a;
			if (b < result)
				result = b;
			if (c < result)
				result = c;
			return result;
		}

		#endregion

		#endregion

		#region Image Conversation

		protected void BitmapToColored(byte* bitmap, byte* colored, int length)
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

		protected void ColoredToGrayscaled(byte* colored, byte* grayscaled, int length)
		{
			byte* source = colored;
			byte* dest = grayscaled;
			for (int i = 0; i < length; i++)
			{
				//*dest = (byte)Math.Round(source[RedInc] * 0.2125 + source[GreenInc] * 0.7154 + source[BlueInc] * 0.0721);
				*dest = ColoredToGrayscaled(source);
				source += ColorOffset;
				dest++;
			}
		}

		protected byte ColoredToGrayscaled(byte* colored)
		{
			return (byte)((299 * colored[RedInc] + 587 * colored[GreenInc] + 114 * colored[BlueInc]) / 1000);
		}

		protected Bitmap GrayscaledToBitmap(byte* grayscaled, int width, int height)
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

		protected Bitmap ColoredToBitmap(byte* colored, int width, int height)
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
