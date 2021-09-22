namespace Ship {
	public interface IReloadBehavior {
		/// <summary>
		/// Consumes up a bullet/shell in the ReloadBehavior.
		/// </summary>
		public void ConsumeAmmunition ();
		/// <summary>
		/// Checks if the ReloadBehavior is loaded.
		/// </summary>
		public bool IsLoaded ();
	}
}
