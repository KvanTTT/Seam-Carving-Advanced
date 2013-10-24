using Cudafy;
using Cudafy.Host;
using Cudafy.Translator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarvingAdvanced
{
	public unsafe class SeamCarvingGPU : SeamCarverStandart
	{
		public SeamCarvingGPU(EnergyFuncType energyFuncType, bool hd, bool forwardEnergy, bool parallelization)
			: base(energyFuncType, hd, forwardEnergy, parallelization, 0)
		{
		}

		#region Overrides

		protected override int CalculateEnergyDiff(byte* energy, int* energyDiff, int width, int height, bool xDir, out int sMin)
		{
			sMin = 0;
			return 0;
		}

		#endregion
	}
}
