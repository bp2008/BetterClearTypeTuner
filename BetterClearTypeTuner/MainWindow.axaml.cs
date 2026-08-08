using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using BetterClearTypeTuner.Native;

namespace BetterClearTypeTuner;

public partial class MainWindow : Window
{
	private bool _initialized;
	private bool _suppressEvents;
	private bool _setDefaults;
	private bool _registryFail;
	private string _fontFamily = "Segoe UI";

	public MainWindow()
	{
		InitializeComponent();

		var version = Assembly.GetExecutingAssembly().GetName().Version;
		Title = $"Better ClearType Tuner {version}";

		try
		{
			LegacyRegistry.EnsureLocalMachineKeys();
		}
		catch (UnauthorizedAccessException)
		{
			Title += " [NOT ADMIN]";
			AdminBanner.IsVisible = true;
		}

		bool dark = LegacyRegistry.PrefersDarkMode();
		DarkModeCheck.IsChecked = dark;
		App.SetDarkMode(dark);

		AntialiasingCheck.IsCheckedChanged += OnControlsChanged;
		GrayscaleRadio.IsCheckedChanged += OnControlsChanged;
		RgbRadio.IsCheckedChanged += OnControlsChanged;
		BgrRadio.IsCheckedChanged += OnControlsChanged;
		ContrastUpDown.ValueChanged += OnControlsChanged;
		RestoreDefaultsButton.Click += OnRestoreDefaults;
		DarkModeCheck.IsCheckedChanged += OnDarkModeChanged;
		ChangeFontButton.Click += OnChangeFont;
		Opened += (_, _) =>
		{
			UpdateStatus();
			_initialized = true;
			RefreshSamples();
		};
	}

	private void OnDarkModeChanged(object? sender, RoutedEventArgs e)
	{
		App.SetDarkMode(DarkModeCheck.IsChecked == true);
		RefreshSamples();
	}

	private async void OnChangeFont(object? sender, RoutedEventArgs e)
	{
		var box = new TextBox { Text = _fontFamily, Watermark = "Segoe UI" };
		var ok = new Button { Content = "OK", Width = 88 };
		var cancel = new Button { Content = "Cancel", Width = 88 };
		var buttons = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			Spacing = 8,
			HorizontalAlignment = HorizontalAlignment.Right,
			Children = { cancel, ok },
		};
		var root = new StackPanel
		{
			Margin = new Avalonia.Thickness(16),
			Spacing = 12,
			Children =
			{
				new TextBlock { Text = "Font family name (must be installed on this PC):" },
				box,
				buttons,
			},
		};

		var dialog = new Window
		{
			Title = "Change Font",
			Width = 380,
			Height = 160,
			CanResize = false,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			Content = root,
		};

		string? chosen = null;
		ok.Click += (_, _) =>
		{
			chosen = box.Text;
			dialog.Close();
		};
		cancel.Click += (_, _) => dialog.Close();

		await dialog.ShowDialog(this);
		if (!string.IsNullOrWhiteSpace(chosen))
		{
			_fontFamily = chosen.Trim();
			RefreshSamples();
		}
	}

	private void OnRestoreDefaults(object? sender, RoutedEventArgs e)
	{
		_suppressEvents = true;
		_setDefaults = true;
		AntialiasingCheck.IsChecked = true;
		RgbRadio.IsChecked = true;
		ContrastUpDown.Value = FontSmoothing.ContrastDefault;
		_suppressEvents = false;
		ApplyFromUi();
	}

	private void OnControlsChanged(object? sender, RoutedEventArgs e)
	{
		if (!_initialized || _suppressEvents)
			return;
		ApplyFromUi();
	}

	private void ApplyFromUi()
	{
		try
		{
			if (_setDefaults)
			{
				LegacyRegistry.DeleteAvalonSubkeys();
			}
			else
			{
				int pixelStructure = 0;
				if (RgbRadio.IsChecked == true) pixelStructure = 1;
				else if (BgrRadio.IsChecked == true) pixelStructure = 2;
				int contrast = (int)(ContrastUpDown.Value ?? FontSmoothing.ContrastDefault);
				LegacyRegistry.WriteSettings(pixelStructure, contrast);
			}
		}
		catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException)
		{
			HandleRegistryFailure();
		}

		if (GrayscaleRadio.IsChecked == true)
			SetFontSmoothingTypeIfNeeded(FontSmoothingType.Standard);
		else if (RgbRadio.IsChecked == true)
		{
			SetFontSmoothingTypeIfNeeded(FontSmoothingType.ClearType);
			SetOrientationIfNeeded(FontSmoothingOrientation.RGB);
		}
		else if (BgrRadio.IsChecked == true)
		{
			SetFontSmoothingTypeIfNeeded(FontSmoothingType.ClearType);
			SetOrientationIfNeeded(FontSmoothingOrientation.BGR);
		}

		uint desiredContrast = (uint)(ContrastUpDown.Value ?? FontSmoothing.ContrastDefault);
		if (FontSmoothing.GetContrast() != desiredContrast)
			FontSmoothing.SetContrast(desiredContrast);

		bool aa = AntialiasingCheck.IsChecked == true;
		if (FontSmoothing.GetAntialiasingEnabled() != aa)
			FontSmoothing.SetAntialiasingEnabled(aa);

		_setDefaults = false;
		UpdateStatus();
		RefreshSamples();
	}

	private static void SetFontSmoothingTypeIfNeeded(FontSmoothingType type)
	{
		if (FontSmoothing.GetFontSmoothingType() != type)
			FontSmoothing.SetFontSmoothingType(type);
	}

	private static void SetOrientationIfNeeded(FontSmoothingOrientation orientation)
	{
		if (FontSmoothing.GetFontSmoothingOrientation() != orientation)
			FontSmoothing.SetFontSmoothingOrientation(orientation);
	}

	private void HandleRegistryFailure()
	{
		AdminBanner.IsVisible = true;
		if (_registryFail)
			return;
		_registryFail = true;
		StatusText.Text = "Unable to set all legacy registry values. Run as administrator for best results.";
	}

	private void UpdateStatus()
	{
		_suppressEvents = true;
		try
		{
			bool aaEnabled = FontSmoothing.GetAntialiasingEnabled();
			FontSmoothingOrientation orientation = FontSmoothing.GetFontSmoothingOrientation();
			FontSmoothingType smoothingType = FontSmoothing.GetFontSmoothingType();
			uint contrast = FontSmoothing.GetContrast();

			AntialiasingCheck.IsChecked = aaEnabled;

			bool contrastEnabled = false;
			if (smoothingType == FontSmoothingType.Standard)
			{
				GrayscaleRadio.IsChecked = true;
			}
			else if (orientation == FontSmoothingOrientation.RGB)
			{
				RgbRadio.IsChecked = true;
				contrastEnabled = true;
			}
			else if (orientation == FontSmoothingOrientation.BGR)
			{
				BgrRadio.IsChecked = true;
				contrastEnabled = true;
			}

			decimal min = ContrastUpDown.Minimum;
			decimal max = ContrastUpDown.Maximum;
			ContrastUpDown.Value = Math.Clamp(contrast, (uint)min, (uint)max);
			GrayscaleRadio.IsEnabled = RgbRadio.IsEnabled = BgrRadio.IsEnabled = aaEnabled;
			ContrastUpDown.IsEnabled = aaEnabled && contrastEnabled;

			string quick = GdiSampleRenderer.DefaultSample + " ";
			if (!aaEnabled)
				StatusText.Text = quick + "Font Antialiasing is disabled.";
			else if (smoothingType == FontSmoothingType.ClearType)
				StatusText.Text = quick + orientation + " (Contrast " + contrast + ")";
			else
				StatusText.Text = quick + "Grayscale (Contrast " + contrast + ")";
		}
		finally
		{
			_suppressEvents = false;
		}
	}

	private void RefreshSamples()
	{
		bool dark = DarkModeCheck.IsChecked == true;
		ReplaceImage(NormalSampleImage, GdiSampleRenderer.RenderNormal(_fontFamily, dark));
		ReplaceImage(ZoomedSampleImage, GdiSampleRenderer.RenderZoomed(_fontFamily, dark));
	}

	private static void ReplaceImage(Image control, WriteableBitmap bitmap)
	{
		var old = control.Source as IDisposable;
		control.Source = bitmap;
		old?.Dispose();
	}
}
