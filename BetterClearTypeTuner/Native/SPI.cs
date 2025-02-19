
namespace BetterClearTypeTuner
{
	#region SPI
	/// <summary>
	/// SPI_ System-wide parameter - Used in SystemParametersInfo function 
	/// </summary>
	public enum SPI : uint
	{
        /// <summary>
        /// Determines whether the font smoothing feature is enabled. This feature uses font antialiasing to make font curves appear smoother 
        /// by painting pixels at different gray levels. 
        /// The pvParam parameter must point to a BOOL variable that receives TRUE if the feature is enabled, or FALSE if it is not.
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
        /// </summary>
        SPI_GETFONTSMOOTHING = 0x004A,

        /// <summary>
        /// Enables or disables the font smoothing feature, which uses font antialiasing to make font curves appear smoother 
        /// by painting pixels at different gray levels. 
        /// To enable the feature, set the uiParam parameter to TRUE. To disable the feature, set uiParam to FALSE.
        /// Windows 95:  This flag is supported only if Windows Plus! is installed. See SPI_GETWINDOWSEXTENSION.
        /// </summary>
        SPI_SETFONTSMOOTHING = 0x004B,
        /// <summary>
        /// Retrieves the type of font smoothing. The pvParam parameter must point to a UINT that receives the information.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFONTSMOOTHINGTYPE = 0x200A,

        /// <summary>
        /// Sets the font smoothing type. The pvParam parameter points to a UINT that contains either FE_FONTSMOOTHINGSTANDARD, 
        /// if standard anti-aliasing is used, or FE_FONTSMOOTHINGCLEARTYPE, if ClearType is used. The default is FE_FONTSMOOTHINGSTANDARD. 
        /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise, 
        /// SystemParametersInfo fails.
        /// </summary>
        SPI_SETFONTSMOOTHINGTYPE = 0x200B,

        /// <summary>
        /// Not implemented.
        /// </summary>
        SPI_GETFONTSMOOTHINGORIENTATION = 0x2012,

        /// <summary>
        /// Not implemented.
        /// </summary>
        SPI_SETFONTSMOOTHINGORIENTATION = 0x2013,

        /// <summary>
        /// Retrieves a contrast value that is used in ClearType™ smoothing. The pvParam parameter must point to a UINT 
        /// that receives the information.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_GETFONTSMOOTHINGCONTRAST = 0x200C,

        /// <summary>
        /// Sets the contrast value used in ClearType smoothing. The pvParam parameter points to a UINT that holds the contrast value. 
        /// Valid contrast values are from 1000 to 2200. The default value is 1400.
        /// When using this option, the fWinIni parameter must be set to SPIF_SENDWININICHANGE | SPIF_UPDATEINIFILE; otherwise, 
        /// SystemParametersInfo fails.
        /// SPI_SETFONTSMOOTHINGTYPE must also be set to FE_FONTSMOOTHINGCLEARTYPE.
        /// Windows 2000/NT, Windows Me/98/95:  This value is not supported.
        /// </summary>
        SPI_SETFONTSMOOTHINGCONTRAST = 0x200D,
    }
	#endregion // SPI
}
