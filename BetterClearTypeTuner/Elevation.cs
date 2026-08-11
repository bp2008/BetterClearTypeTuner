using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace BetterClearTypeTuner
{
	/// <summary>
	/// Outcome of an attempt to restart this application with administrator rights.
	/// </summary>
	public enum ElevationResult
	{
		/// <summary>The elevated process was started.  This process should exit.</summary>
		Started,
		/// <summary>The user dismissed the User Account Control prompt.</summary>
		Refused,
		/// <summary>The process could not be started at all.</summary>
		Failed
	}

	/// <summary>
	/// Restarting this application with administrator rights.  Only the HKEY_LOCAL_MACHINE half of
	/// the settings needs them, so the prompt is raised at the moment a write is actually refused
	/// rather than being demanded up front by the manifest.
	/// </summary>
	public static class Elevation
	{
		/// <summary>ERROR_CANCELLED, which ShellExecute reports when a UAC prompt is dismissed.</summary>
		private const int ErrorCancelled = 1223;

		/// <summary>
		/// True if this process is already running with administrator rights.
		/// </summary>
		public static bool IsElevated
		{
			get
			{
				try
				{
					using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
						return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
				}
				catch
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Starts a second copy of this executable with administrator rights, which raises the UAC
		/// prompt.  A running process cannot acquire rights it was not created with, so restarting
		/// is the only way to get them.
		/// </summary>
		/// <param name="arguments">Command line for the new process, which is how it learns what
		/// this one was in the middle of doing.  See <see cref="StartupState"/>.</param>
		/// <param name="error">Message describing a <see cref="ElevationResult.Failed"/> result,
		/// otherwise null.</param>
		public static ElevationResult RestartElevated(string arguments, out string error)
		{
			error = null;
			try
			{
				ProcessStartInfo psi = new ProcessStartInfo();
				psi.FileName = Application.ExecutablePath;
				psi.Arguments = arguments;
				psi.WorkingDirectory = Environment.CurrentDirectory;
				// ShellExecute is what performs the elevation; CreateProcess cannot, and
				// UseShellExecute must therefore be on for the "runas" verb to mean anything.
				psi.UseShellExecute = true;
				psi.Verb = "runas";
				Process.Start(psi);
				return ElevationResult.Started;
			}
			catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
			{
				return ElevationResult.Refused;
			}
			catch (Exception ex)
			{
				error = ex.Message;
				return ElevationResult.Failed;
			}
		}
	}
}
