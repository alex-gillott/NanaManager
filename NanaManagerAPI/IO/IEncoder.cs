
namespace NanaManagerAPI.IO
{
	public interface IEncoder
	{
		/// <summary>
		/// Display of this encoder
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Internal identifier of this encoder
		/// </summary>
		string InternalName { get; }
		/// <summary>
		/// The major version. Used for legacy support
		/// </summary>
		int VersionMajor { get; }
		/// <summary>
		/// The minor version. Used for legacy support
		/// </summary>
		int VersionMinor { get; }
		/// <summary>
		/// Displayed in the plugins tab to explain what this encoder is for
		/// </summary>
		string Description { get; }
		/// <summary>
		/// Defines whether this encoder should be used to save data or encrypt it
		/// </summary>
		EncoderType Type { get; }
		/// <summary>
		/// Saves relevant data
		/// </summary>
		void SaveData();
		/// <summary>
		/// Loads relevant data
		/// </summary>
		void LoadData();
	}

	public enum EncoderType
	{
		Data,
		Encryption
	}
}
