namespace ACE.Server.WorldObjects
{
    partial class Player
    {
        /// <summary>
        /// This variable is set to true when a change is made, and set to false before a save is requested.<para />
        /// The primary use for this is to trigger save on add/modify/remove of properties.
        /// </summary>
        public bool CharacterChangesDetected { get; set; }
    }
}
