namespace BetterClearTypeTuner.Native
{
	[Flags]
	public enum SPIF
	{
		None = 0x00,
		SPIF_UPDATEINIFILE = 0x01,
		SPIF_SENDCHANGE = 0x02,
		SPIF_SENDWININICHANGE = 0x02
	}
}
