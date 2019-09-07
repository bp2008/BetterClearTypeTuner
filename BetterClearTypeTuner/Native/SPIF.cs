using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterClearTypeTuner.Native
{
	[Flags]
	public enum SPIF
	{
		None = 0x00,
		/// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
		SPIF_UPDATEINIFILE = 0x01,
		/// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
		SPIF_SENDCHANGE = 0x02,
		/// <summary>Same as SPIF_SENDCHANGE.</summary>
		SPIF_SENDWININICHANGE = 0x02
	}
}
