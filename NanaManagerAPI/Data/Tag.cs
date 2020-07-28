namespace NanaManagerAPI.Data
{
	/// <summary>
	/// A reference to a tag's data and properties
	/// </summary>
	public class Tag {

		private readonly int[] aliases;
		/// <summary>
		/// The name of the tag
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The location of the tag
		/// </summary>
		public readonly int Index;
		/// <summary>
		/// The group this tag belongs to. -1 if Misc
		/// </summary>
		public readonly int Group;
		/// <summary>
		/// Returns the aliases of this tag
		/// </summary>
		/// <returns>An array of the tags aliased with this tag</returns>
		public int[] GetAliases() {
			return aliases;
		}

		/// <summary>
		/// Constructs a new description of a Tag
		/// </summary>
		/// <param name="Name">The name of the tag</param>
		/// <param name="Group">The group the tag belongs to. -1 if Misc</param>
		/// <param name="Aliases">The aliases of this tag</param>
		public Tag(string Name, int Index, int Group, int[] Aliases) {
			this.Name = Name;
			this.Group = Group;
			this.Index = Index;
			aliases = Aliases;
		}
	}
}
