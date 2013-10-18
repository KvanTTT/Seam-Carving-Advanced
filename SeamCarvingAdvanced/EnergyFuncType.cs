using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeamCarvingAdvanced
{
	public enum EnergyFuncType
	{
		Diff = 0,
		Sobel = 1,
		Prewitt = 2,
		Canny = 3,
		V1 = 4,
		VSquare = 5,
		Laplacian = 6
	}
}