using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public bool EnergyBackward
		{
			get;
			set;
		}

		public bool Parallelization
		{
			get;
			set;
		}

		public abstract Bitmap Generate(Bitmap bitmap, int newWidth, int newHeight);
	}
}
