using System.Drawing;

namespace SeamCarvingAdvanced
{
	public abstract class SeamCarver
	{
		public EnergyFuncType EnergyFuncType
		{
			get;
			set;
		}

		public bool Hd
		{
			get;
			set;
		}

		public bool ForwardEnergy
		{
			get;
			set;
		}

		public bool Parallelization
		{
			get;
			set;
		}

		public SeamCarver(EnergyFuncType energyFuncType, bool hd, bool forwardEnergy, bool parallelization)
		{
			EnergyFuncType = energyFuncType;
			Hd = hd;
			ForwardEnergy = forwardEnergy;
			Parallelization = parallelization;
		}

		public abstract Bitmap Generate(Bitmap bitmap, int newWidth, int newHeight);
	}
}
