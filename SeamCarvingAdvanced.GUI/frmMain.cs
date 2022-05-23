using SeamCarvingAdvanced.GUI.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SeamCarvingAdvanced.GUI
{
    public partial class frmMain : Form
    {
		string DoubleFormatString = "0.######";
		Bitmap InputBitmap;

		public frmMain()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			InitializeComponent();

            var seamCarvingMethods = ((SeamCarvingMethod[])Enum.GetValues(typeof(SeamCarvingMethod))).ToList();
            try
            {
                SeamCarverGPU.InitGPU();
            }
            catch
            {
                seamCarvingMethods.Remove(SeamCarvingMethod.GPU);
                MessageBox.Show("Unable to detect cuda gpu.");
            }
			foreach (var enumValue in seamCarvingMethods)
				cmbSeamCarvingMethod.Items.Add(enumValue.ToString());
			var energyFuncTypes = Enum.GetValues(typeof(EnergyFuncType));
            foreach (var enumValue in energyFuncTypes)
				cmbEnergyFuncType.Items.Add(enumValue.ToString());
		}

		private void frmMain_Load(object sender, EventArgs e)
		{
			try
			{
				tbInputImagePath.Text = Settings.Default.InputImagePath;
				cmbSeamCarvingMethod.SelectedIndex = Settings.Default.SeamCarvingMethod;
				tbWidthRatio.Text = Settings.Default.WidthRatio.ToString(DoubleFormatString);
				tbHeightRatio.Text = Settings.Default.HeightRatio.ToString(DoubleFormatString);
				cmbEnergyFuncType.SelectedIndex = Settings.Default.EnergyFuncType;
				cbForwardEnergy.Checked = Settings.Default.ForwardEnergy;
				cbHd.Checked = Settings.Default.Hd;
				cbParallel.Checked = Settings.Default.Parallel;
				tbCairAppPath.Text = Settings.Default.CairAppPath;
				tbNeighbourCountRatio.Text = Settings.Default.NeighbourCountRatio.ToString(DoubleFormatString);
				tbBlockSize.Text = Settings.Default.BlockSize.ToString();

				if (!string.IsNullOrEmpty(tbInputImagePath.Text))
					InputBitmap = new Bitmap(tbInputImagePath.Text);
			}
			catch
			{
			}
		}

		private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			Settings.Default.InputImagePath = tbInputImagePath.Text;
			Settings.Default.SeamCarvingMethod = cmbSeamCarvingMethod.SelectedIndex;
			Settings.Default.WidthRatio = double.Parse(tbWidthRatio.Text);
			Settings.Default.HeightRatio = double.Parse(tbHeightRatio.Text);
			Settings.Default.EnergyFuncType = cmbEnergyFuncType.SelectedIndex;
			Settings.Default.ForwardEnergy = cbForwardEnergy.Checked;
			Settings.Default.Hd = cbHd.Checked;
			Settings.Default.Parallel = cbParallel.Checked;
			Settings.Default.CairAppPath = tbCairAppPath.Text;
			Settings.Default.NeighbourCountRatio = double.Parse(tbNeighbourCountRatio.Text);
			Settings.Default.BlockSize = int.Parse(tbBlockSize.Text);
			Settings.Default.Save();
		}

		private void btnOpenImage_Click(object sender, EventArgs e)
		{
			if (openImageDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				tbInputImagePath.Text = openImageDialog.FileName;
				InputBitmap = new Bitmap(tbInputImagePath.Text);
			}
		}

		private void btnProcess_Click(object sender, EventArgs e)
		{
			pbInput.Image = InputBitmap;

			EnergyFuncType energyFuncType = (EnergyFuncType)cmbEnergyFuncType.SelectedIndex;
			Bitmap resultBitmap = null;
			SeamCarvingMethod seamCarvingMethod = (SeamCarvingMethod)cmbSeamCarvingMethod.SelectedIndex;
			double widthRatio = double.Parse(tbWidthRatio.Text);
			double heightRatio = double.Parse(tbHeightRatio.Text);

			int newWidth = (int)Math.Round(widthRatio * InputBitmap.Width);
			int newHeight = (int)Math.Round(heightRatio * InputBitmap.Height);
			var stopwatch = new Stopwatch();
			switch (seamCarvingMethod)
			{
				case SeamCarvingMethod.CAIR:
					Thread.Sleep(100);
					stopwatch.Start();
					SeamCarverCair cairCarver = new SeamCarverCair(energyFuncType, cbHd.Checked, cbForwardEnergy.Checked, cbParallel.Checked, tbCairAppPath.Text);
					resultBitmap = cairCarver.Generate(InputBitmap,newWidth, newHeight);
					stopwatch.Stop();
					break;
				default:
				case SeamCarvingMethod.Standart:
					stopwatch.Start();
					SeamCarverStandart standrtCarver = new SeamCarverStandart(energyFuncType, cbHd.Checked, cbForwardEnergy.Checked, cbParallel.Checked,
						double.Parse(tbNeighbourCountRatio.Text), int.Parse(tbBlockSize.Text));
					resultBitmap = standrtCarver.Generate(InputBitmap, newWidth, newHeight);
					stopwatch.Stop();
					break;
				case SeamCarvingMethod.GPU:
					stopwatch.Start();
					SeamCarverGPU gpuCarver = new SeamCarverGPU(energyFuncType, cbHd.Checked, cbForwardEnergy.Checked, cbParallel.Checked,
						int.Parse(tbBlockSize.Text));
					resultBitmap = gpuCarver.Generate(InputBitmap, newWidth, newHeight);
					stopwatch.Stop();
					break;
			}
			tbCalculationTime.Text = stopwatch.Elapsed.ToString();

			pbOutput.Image = resultBitmap;
		}

		private void tbWidthRatio_TextChanged(object sender, EventArgs e)
		{
			if (InputBitmap != null)
				try
				{
					tbWidth.Text = ((int)Math.Round(double.Parse(tbWidthRatio.Text) * InputBitmap.Width)).ToString();
				}
				catch
				{
				}
			else
				tbWidth.Text = "0";
		}

		private void tbWidth_TextChanged(object sender, EventArgs e)
		{
			/*if (InputBitmap != null)
				tbWidthRatio.Text = (int.Parse(tbWidth.Text) / (double)InputBitmap.Width).ToString(DoubleFormatString);*/
		}

		private void tbHeightRatio_TextChanged(object sender, EventArgs e)
		{
			if (InputBitmap != null)
				try
				{
					tbHeight.Text = ((int)Math.Round(double.Parse(tbHeightRatio.Text) * InputBitmap.Height)).ToString();
				}
				catch
				{
				}
			else
				tbHeight.Text = "0";
		}

		private void tbHeight_TextChanged(object sender, EventArgs e)
		{
			/*if (InputBitmap != null)
				tbHeightRatio.Text = (int.Parse(tbHeight.Text) / (double)InputBitmap.Height).ToString(DoubleFormatString);*/
		}

		private void tbNeighbourCountRatio_TextChanged(object sender, EventArgs e)
		{
			if (InputBitmap != null)
			{
				try
				{
					int neighbourCount = (int)Math.Round(double.Parse(tbNeighbourCountRatio.Text) * InputBitmap.Height);
					if (neighbourCount < 3)
						neighbourCount = 3;
					tbNeighbourCount.Text = neighbourCount.ToString();
				}
				catch
				{
				}
			}
			else
				tbNeighbourCount.Text = "0";
		}

		private void tbNeighbourCount_TextChanged(object sender, EventArgs e)
		{
			if (InputBitmap != null)
			{
				int neighbourCount = int.Parse(tbNeighbourCount.Text);
				if (neighbourCount < 3)
					neighbourCount = 3;
				tbNeighbourCount.Text = neighbourCount.ToString();
				tbNeighbourCountRatio.Text = ((double)neighbourCount / InputBitmap.Width).ToString(DoubleFormatString);
			}
		}

		private void cmbSeamCarvingMethod_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
    }
}
