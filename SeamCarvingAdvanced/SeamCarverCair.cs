using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeamCarvingAdvanced
{
	public class SeamCarverCair : SeamCarver
	{
		public string CairAppFileName
		{
			get;
			set;
		}

		public override Bitmap Generate(System.Drawing.Bitmap bitmap, int newWidth, int newHeight)
		{
			string originFileName = "seam_carving_cair_orig.bmp";
			string resultFileName = "seam_carving_cair_result.bmp";

			bitmap.Save(originFileName, ImageFormat.Bmp);
			string inputFileName = Path.GetFullPath(originFileName);
			string outputFileName = Path.GetFullPath(resultFileName);

			int energyFuncTypeInt;
			switch (EnergyFuncType)
			{
				default:
				case EnergyFuncType.Prewitt:
					energyFuncTypeInt = 0;
					break;
				case EnergyFuncType.V1:
					energyFuncTypeInt = 1;
					break;
				case EnergyFuncType.VSquare:
					energyFuncTypeInt = 2;
					break;
				case EnergyFuncType.Sobel:
					energyFuncTypeInt = 3;
					break;
				case EnergyFuncType.Laplacian:
					energyFuncTypeInt = 4;
					break;
			}

			int threadCount = Parallelization ? Math.Min(4, Environment.ProcessorCount) : 1;
			string parameters = string.Format("-I \"{0}\" -O \"{1}\" -X {2} -Y {3} -R {4} -C {5} -E {6} -T {7}",
				inputFileName, outputFileName, newWidth, newHeight, Hd ? 6 : 0, energyFuncTypeInt, EnergyBackward ? 0 : 1, threadCount);

			ProcessStartInfo startInfo = new ProcessStartInfo(CairAppFileName, parameters);
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			Process proc = Process.Start(startInfo);
			proc.Start();
			proc.WaitForExit();

			Bitmap result = null;
			using (MemoryStream stream = new MemoryStream())
			{
				var outputFileInfo = new FileInfo(outputFileName);
				while (IsFileLocked(outputFileInfo))
					Thread.Sleep(100);
				using (FileStream fs = File.OpenRead(outputFileName))
				{
					fs.CopyTo(stream);
				}
				outputFileInfo.Delete();
				result = new Bitmap(stream);
			}

			var inputFileInfo = new FileInfo(inputFileName);
			while (IsFileLocked(inputFileInfo))
				Thread.Sleep(100);
			inputFileInfo.Delete();

			return result;
		}

		private static bool IsFileLocked(FileInfo file)
		{
			FileStream stream = null;

			try
			{
				stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			}
			catch (IOException)
			{
				return true;
			}
			finally
			{
				if (stream != null)
					stream.Close();
			}

			return false;
		}
	}
}
