using System.Runtime.InteropServices;
using BetterClearTypeTuner.Native;
using Microsoft.Win32;

namespace BetterClearTypeTuner;

internal static class LegacyRegistry
{
	public const string AvalonGraphics = @"Software\Microsoft\Avalon.Graphics";

	public static string[] GetDisplayNames()
	{
		var names = new List<string>();
		var device = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
		for (uint i = 0; User32.EnumDisplayDevices(null, i, ref device, 0); i++)
		{
			if ((device.StateFlags & (uint)DisplayDeviceStateFlags.AttachedToDesktop) == 0)
				continue;

			string name = device.DeviceName ?? "";
			int idx = name.LastIndexOf('\\');
			names.Add(idx >= 0 ? name[(idx + 1)..] : name);
			device.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
		}

		if (names.Count == 0)
			names.Add("DISPLAY1");
		return names.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public static void EnsureLocalMachineKeys()
	{
		foreach (string displayName in GetDisplayNames())
			Registry.LocalMachine.CreateSubKey(AvalonGraphics + "\\" + displayName)?.Dispose();
	}

	public static void DeleteAvalonSubkeys()
	{
		DeleteSubkeys(Registry.LocalMachine, AvalonGraphics);
		DeleteSubkeys(Registry.CurrentUser, AvalonGraphics);
	}

	public static void WriteSettings(int pixelStructure, int contrast)
	{
		int clearTypeLevel = pixelStructure == 0 ? 0 : 100;
		contrast = (int)Clamp((uint)contrast, 1000, 2200);

		foreach (string displayName in GetDisplayNames())
		{
			string path = AvalonGraphics + "\\" + displayName;
			SetDword(Registry.LocalMachine, path, "GammaLevel", contrast);
			SetDword(Registry.LocalMachine, path, "PixelStructure", pixelStructure);

			SetDword(Registry.CurrentUser, path, "ClearTypeLevel", clearTypeLevel);
			SetDword(Registry.CurrentUser, path, "EnhancedContrastLevel", 50);
			SetDword(Registry.CurrentUser, path, "GammaLevel", contrast);
			SetDword(Registry.CurrentUser, path, "GrayscaleEnhancedContrastLevel", 100);
			SetDword(Registry.CurrentUser, path, "PixelStructure", pixelStructure);
			SetDword(Registry.CurrentUser, path, "TextContrastLevel", 1);
		}
	}

	public static bool PrefersDarkMode()
	{
		try
		{
			using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
			object? value = key?.GetValue("AppsUseLightTheme");
			return value is int i && i == 0;
		}
		catch
		{
			return false;
		}
	}

	private static void SetDword(RegistryKey baseKey, string keyPath, string name, int value)
	{
		using RegistryKey key = baseKey.CreateSubKey(keyPath)
			?? throw new InvalidOperationException("Unable to open registry key: " + keyPath);
		key.SetValue(name, value, RegistryValueKind.DWord);
	}

	private static void DeleteSubkeys(RegistryKey baseKey, string keyPath)
	{
		using RegistryKey? folder = baseKey.OpenSubKey(keyPath, writable: true);
		if (folder is null)
			return;
		foreach (string subkeyName in folder.GetSubKeyNames())
			folder.DeleteSubKeyTree(subkeyName);
	}

	private static uint Clamp(uint val, uint minimum, uint maximum)
	{
		if (val > maximum) return maximum;
		if (val < minimum) return minimum;
		return val;
	}
}
