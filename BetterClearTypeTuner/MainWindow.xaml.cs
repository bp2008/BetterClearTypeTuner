using BetterClearTypeTuner.Native;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BetterClearTypeTuner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool dirty = false;
		bool initialized = false;
		bool setDefaults = false;
		Brush defaultWindowTextColor;
		public MainWindow()
		{
			InitializeComponent();

			defaultWindowTextColor = txtContrast.Foreground;
			lblNotAdmin.Visibility = Visibility.Hidden;
			this.Title += "BetterClearTypeTuner " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

			try
			{
				foreach (string displayName in GetDisplayNames())
				{
					RegistryKey key = Registry.LocalMachine.CreateSubKey("Software\\Microsoft\\Avalon.Graphics\\" + displayName);
				}
			}
			catch (UnauthorizedAccessException)
			{
				this.Title += " [NOT ADMIN]";
				lblNotAdmin.Visibility = Visibility.Visible;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			new Thread(() =>
			{
				for (int i = 0; i < 4; i++)
				{
					Thread.Sleep(500);
					this.Dispatcher.Invoke(() =>
					{
						CopyZoomedSnapshot();
					});
				}
			})
			{
				IsBackground = true
			}
			.Start();

			UpdateStatus();
			cbEnableFontAntialiasing.Focus();
			initialized = true;
		}

		private void txtContrast_Validate(object sender, TextCompositionEventArgs e)
		{
			Regex regex = new Regex("[^0-9]+");
			e.Handled = regex.IsMatch(e.Text);
		}

		private void btnSetContrast_Click(object sender, RoutedEventArgs e)
		{
			ControlsChanged();
		}

		private void btnRestoreDefaults_Click(object sender, RoutedEventArgs e)
		{
			DisableEvents();
			setDefaults = true;
			cbEnableFontAntialiasing.IsChecked = true;
			rbRGB.IsChecked = true;
			txtContrast.Text = "0";
			EnableEvents();
			ControlsChanged();
		}

		private void ControlsChanged(object sender, RoutedEventArgs e)
		{
			ControlsChanged();
		}

		private void ControlsChanged()
		{
			if (initialized)
			{
				SetLegacyKeys();
				if (rbGrayscale.IsChecked.Value)
				{
					SetFontSmoothingTypeIfNotAlready(FontSmoothingType.Standard);
				}
				else if (rbRGB.IsChecked.Value)
				{
					SetFontSmoothingTypeIfNotAlready(FontSmoothingType.ClearType);
					SetFontSmoothingIfNotAlready(FontSmoothingOrientation.RGB);
				}
				else if (rbBGR.IsChecked.Value)
				{
					SetFontSmoothingTypeIfNotAlready(FontSmoothingType.ClearType);
					SetFontSmoothingIfNotAlready(FontSmoothingOrientation.BGR);
				}
				if (FontSmoothing.GetContrast() != (uint)ParseInt(txtContrast.Text, 0))
				{
					FontSmoothing.SetContrast((uint)ParseInt(txtContrast.Text, 0));
					dirty = true;
				}
				if (FontSmoothing.GetAntialiasingEnabled() != cbEnableFontAntialiasing.IsChecked.Value)
				{
					FontSmoothing.SetAntialiasingEnabled(cbEnableFontAntialiasing.IsChecked.Value);
					dirty = true;
				}
				if (dirty)
					UpdateStatus();
			}
			setDefaults = false;
		}

		private void SetLegacyKeys()
		{
			foreach (string displayName in GetDisplayNames())
			{
				if (setDefaults)
				{
					DeleteRegistrySubkeys(Registry.LocalMachine, "Software\\Microsoft\\Avalon.Graphics");
					DeleteRegistrySubkeys(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics");
				}
				else
				{
					int pixelStructure = 0;
					if (rbGrayscale.IsChecked.Value)
						pixelStructure = 0;
					else if (rbRGB.IsChecked.Value)
						pixelStructure = 1;
					else if (rbBGR.IsChecked.Value)
						pixelStructure = 2;

					int contrast = (int)Clamp((uint)ParseInt(txtContrast.Text, 0), 1000, 2200);

					// Local Machine
					SetRegistryDWORDValue(Registry.LocalMachine, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "GammaLevel", contrast);
					SetRegistryDWORDValue(Registry.LocalMachine, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "PixelStructure", pixelStructure);

					// Current User
					SetRegistryDWORDValue(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "ClearTypeLevel", pixelStructure == 0 ? 0 : 100);
					SetRegistryDWORDValue(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "EnhancedContrastLevel", 50);
					SetRegistryDWORDValue(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "GammaLevel", contrast);
					SetRegistryDWORDValue(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "GrayscaleEnhancedContrastLevel", 100);
					SetRegistryDWORDValue(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "PixelStructure", pixelStructure);
					SetRegistryDWORDValue(Registry.CurrentUser, "Software\\Microsoft\\Avalon.Graphics\\" + displayName, "TextContrastLevel", 1);
				}
			}
		}


		private void SetFontSmoothingIfNotAlready(FontSmoothingOrientation orientation)
		{
			if (FontSmoothing.GetFontSmoothingOrientation() != orientation)
			{
				FontSmoothing.SetFontSmoothingOrientation(orientation);
				dirty = true;
			}
		}

		private void SetFontSmoothingTypeIfNotAlready(FontSmoothingType type)
		{
			if (FontSmoothing.GetFontSmoothingType() != type)
			{
				FontSmoothing.SetFontSmoothingType(type);
				dirty = true;
			}
		}
		private bool InvokeRequired
		{
			get
			{
				return !this.Dispatcher.CheckAccess();
			}
		}
		private void UpdateStatus()
		{
			dirty = false;
			if (InvokeRequired)
				Dispatcher.Invoke(UpdateStatus);
			else
			{
				// Read font settings
				bool aaEnabled = FontSmoothing.GetAntialiasingEnabled();
				FontSmoothingOrientation orientation = FontSmoothing.GetFontSmoothingOrientation();
				FontSmoothingType smoothingType = FontSmoothing.GetFontSmoothingType();
				uint contrast = FontSmoothing.GetContrast();

				// Update UI controls
				DisableEvents();

				cbEnableFontAntialiasing.IsChecked = aaEnabled;

				if (smoothingType == FontSmoothingType.Standard)
				{
					rbGrayscale.IsChecked = true;
					btnSetContrast.IsEnabled = txtContrast.IsEnabled = false;
				}
				else
				{
					if (orientation == FontSmoothingOrientation.RGB)
					{
						rbRGB.IsChecked = true;
						btnSetContrast.IsEnabled = txtContrast.IsEnabled = true;
					}
					else if (orientation == FontSmoothingOrientation.BGR)
					{
						rbBGR.IsChecked = true;
						btnSetContrast.IsEnabled = txtContrast.IsEnabled = true;
					}
					else if (orientation == FontSmoothingOrientation.Unknown)
					{
						rbGrayscale.IsChecked = rbRGB.IsChecked = rbBGR.IsChecked = false;
						btnSetContrast.IsEnabled = txtContrast.IsEnabled = false;
					}
				}

				txtContrast.Text = Clamp(contrast, 0, 2200).ToString();
				if (contrast < 1000 || contrast > 2200)
					txtContrast.Foreground = Brushes.Red;
				else
					txtContrast.Foreground = defaultWindowTextColor;

				rbGrayscale.IsEnabled = rbRGB.IsEnabled = rbBGR.IsEnabled = aaEnabled;

				if (!aaEnabled)
					txtContrast.IsEnabled = btnSetContrast.IsEnabled = false;

				EnableEvents();

				string quick = "The quick brown fox jumps over the lazy dog. ";
				// Update status text
				if (aaEnabled)
				{
					if (smoothingType == FontSmoothingType.ClearType)
						lblStatus.Content = quick + orientation + " (Contrast " + contrast + ")";
					else
						lblStatus.Content = quick + "Grayscale (Contrast " + contrast + ")";
				}
				else
				{
					lblStatus.Content = quick + "Font Antialiasing is disabled.";
				}

				if (initialized)
				{
					// Snapshot the sample text and render it zoomed-in
					CopyZoomedSnapshot();
				}
			}
		}
		#region Registry
		bool registryFail = false;
		private void SetRegistryDWORDValue(RegistryKey baseKey, string keyPath, string name, int value)
		{
			try
			{
				RegistryKey key = baseKey.CreateSubKey(keyPath);
				key.SetValue(name, value, RegistryValueKind.DWord);
			}
			catch (SecurityException ex)
			{
				HandleRegistryException(ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				HandleRegistryException(ex);
			}
		}
		private void DeleteRegistrySubkeys(RegistryKey baseKey, string keyPath)
		{
			try
			{
				RegistryKey folder = baseKey.OpenSubKey(keyPath, true);
				if (folder != null)
					foreach (string subkeyName in folder.GetSubKeyNames())
						folder.DeleteSubKeyTree(subkeyName);
				//RegistryKey key = baseKey.DeleteSubKeyTree();
				//key.SetValue(name, value, RegistryValueKind.DWord);
			}
			catch (SecurityException ex)
			{
				HandleRegistryException(ex);
			}
			catch (UnauthorizedAccessException ex)
			{
				HandleRegistryException(ex);
			}
		}
		private void HandleRegistryException(Exception ex)
		{
			if (registryFail)
				return;
			lblNotAdmin.Visibility = Visibility.Visible;
			registryFail = true;
			MessageBox.Show("Unable to set all legacy registry values. While your change may have worked, for best results you should run this application as an administrator and try making changes again.");
		}
		#endregion
		#region Helpers
		private int ParseInt(string text, int defaultValue)
		{
			if (int.TryParse(text, out int result))
				return result;
			return defaultValue;
		}
		private string[] GetDisplayNames()
		{
			return System.Windows.Forms.Screen.AllScreens
				.Select(s =>
				{
					int idxStart = s.DeviceName.LastIndexOf('\\');
					if (idxStart < 0)
						idxStart = 0;
					else
						idxStart++;
					return s.DeviceName.Substring(idxStart);
				})
				.ToArray();
		}
		private uint Clamp(uint val, uint minimum, uint maximum)
		{
			if (val > maximum)
				val = maximum;
			if (val < minimum)
				val = minimum;
			return val;
		}
		public void DisableEvents()
		{
			cbEnableFontAntialiasing.Checked -= ControlsChanged;
			cbEnableFontAntialiasing.Unchecked -= ControlsChanged;
			rbGrayscale.Checked -= ControlsChanged;
			rbGrayscale.Unchecked -= ControlsChanged;
			rbRGB.Checked -= ControlsChanged;
			rbRGB.Unchecked -= ControlsChanged;
			rbBGR.Checked -= ControlsChanged;
			rbBGR.Unchecked -= ControlsChanged;
			txtContrast.TextChanged -= ControlsChanged;
		}
		public void EnableEvents()
		{
			cbEnableFontAntialiasing.Checked += ControlsChanged;
			cbEnableFontAntialiasing.Unchecked += ControlsChanged;
			rbGrayscale.Checked += ControlsChanged;
			rbGrayscale.Unchecked += ControlsChanged;
			rbRGB.Checked += ControlsChanged;
			rbRGB.Unchecked += ControlsChanged;
			rbBGR.Checked += ControlsChanged;
			rbBGR.Unchecked += ControlsChanged;
			txtContrast.TextChanged += ControlsChanged;
		}
		#endregion
		#region Screen Capture
		private void CopyZoomedSnapshot()
		{
			this.InvalidateArrange();
			this.InvalidateMeasure();
			this.InvalidateVisual();

			Point dpi = GetDPI();

			imgZoomed.Source = CaptureScreen(sampleNormal, dpi.X, dpi.Y);

			//RenderTargetBitmap bmp = new RenderTargetBitmap(160, 120, 96, 96, PixelFormats.Pbgra32);
			//bmp.Render(sampleNormal);
			//imgZoomed.Source = BitmapFrame.Create(bmp);

			//PngBitmapEncoder encoder = new PngBitmapEncoder();

			//encoder.Frames.Add(BitmapFrame.Create(bmp));
			//using (MemoryStream ms = new MemoryStream())
			//{
			//	encoder.Save(ms);
			//	File.WriteAllBytes("test.png", ms.ToArray());
			//}
		}
		private static BitmapSource CaptureScreen(Panel target, double dpiX, double dpiY)
		{
			if (target == null)
			{
				return null;
			}
			RenderOptions.SetEdgeMode(target, EdgeMode.Aliased);
			Point position = target.PointToScreen(new Point(0, 0));

			Rect screenBounds = new Rect((int)(position.X * dpiX / 96.0),
							(int)(position.Y * dpiY / 96.0),
							(int)(target.Width * dpiX / 96.0),
							(int)(target.Height * dpiY / 96.0));
			return CopyScreen(screenBounds);

			// The following method doesn't capture subpixel antialiasing....
			//RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0),
			//												(int)(bounds.Height * dpiY / 96.0),
			//												dpiX,
			//												dpiY,
			//												PixelFormats.Pbgra32);
			//DrawingVisual dv = new DrawingVisual();
			//using (DrawingContext ctx = dv.RenderOpen())
			//{
			//	VisualBrush vb = new VisualBrush(target);
			//	ctx.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
			//}
			//rtb.Render(dv);
			//return rtb;
		}
		private static BitmapSource CopyScreen(Rect bounds)
		{
			using (var screenBmp = new System.Drawing.Bitmap(
				(int)bounds.Width,
				(int)bounds.Height,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb))
			{
				using (var bmpGraphics = System.Drawing.Graphics.FromImage(screenBmp))
				{
					bmpGraphics.CopyFromScreen((int)bounds.Left, (int)bounds.Top, 0, 0, screenBmp.Size);
					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
						screenBmp.GetHbitmap(),
						IntPtr.Zero,
						Int32Rect.Empty,
						BitmapSizeOptions.FromEmptyOptions());
				}
			}
		}
		private Point GetDPI()
		{
			PresentationSource source = PresentationSource.FromVisual(this);

			double dpiX = 96;
			double dpiY = 96;
			if (source != null)
			{
				dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
				dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
			}
			return new Point(dpiX, dpiY);
		}
		#endregion

		private void ControlsChanged(object sender, TextChangedEventArgs e)
		{
			ControlsChanged();
		}
	}
}
