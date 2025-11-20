using System.Collections.Generic;

namespace ACViewer
{
    /// <summary>
    /// DID history - browser-style navigation with back/forward support
    /// </summary>
    public class History
    {
        private static readonly int MaxSize = 50;

        private readonly List<uint> DID = new List<uint>();
        private int currentPosition = -1;

        public void Add(uint did)
        {
            // don't add consecutive duplicates
            if (currentPosition >= 0 && currentPosition < DID.Count && DID[currentPosition] == did)
                return;

            // If we're in the middle of history (user went back), remove forward history
            if (currentPosition < DID.Count - 1)
            {
                DID.RemoveRange(currentPosition + 1, DID.Count - currentPosition - 1);
            }

            DID.Add(did);
            currentPosition = DID.Count - 1;

            // Trim old history if exceeds max size
            if (DID.Count > MaxSize)
            {
                DID.RemoveAt(0);
                currentPosition--;
            }
        }

        public void Clear()
        {
            DID.Clear();
            currentPosition = -1;
        }

        /// <summary>
        /// Go back in history. Returns the previous DID, or null if at the start.
        /// </summary>
        public uint? Back()
        {
            if (!CanGoBack()) return null;

            currentPosition--;
            return DID[currentPosition];
        }

        /// <summary>
        /// Go forward in history. Returns the next DID, or null if at the end.
        /// </summary>
        public uint? Forward()
        {
            if (!CanGoForward()) return null;

            currentPosition++;
            return DID[currentPosition];
        }

        /// <summary>
        /// Check if we can go back in history
        /// </summary>
        public bool CanGoBack()
        {
            return currentPosition > 0;
        }

        /// <summary>
        /// Check if we can go forward in history
        /// </summary>
        public bool CanGoForward()
        {
            return currentPosition >= 0 && currentPosition < DID.Count - 1;
        }

        /// <summary>
        /// Get list of DIDs available to go back to (most recent first)
        /// </summary>
        public List<uint> GetBackList()
        {
            var backList = new List<uint>();
            for (int i = currentPosition - 1; i >= 0; i--)
            {
                backList.Add(DID[i]);
            }
            return backList;
        }

        /// <summary>
        /// Get list of DIDs available to go forward to (nearest first)
        /// </summary>
        public List<uint> GetForwardList()
        {
            var forwardList = new List<uint>();
            for (int i = currentPosition + 1; i < DID.Count; i++)
            {
                forwardList.Add(DID[i]);
            }
            return forwardList;
        }

        /// <summary>
        /// Navigate to a specific DID in history by index offset from current position
        /// </summary>
        /// <param name="offset">Negative for back, positive for forward</param>
        public uint? NavigateByOffset(int offset)
        {
            int targetPosition = currentPosition + offset;
            if (targetPosition < 0 || targetPosition >= DID.Count)
                return null;

            currentPosition = targetPosition;
            return DID[currentPosition];
        }

        /// <summary>
        /// Legacy Pop() method for backwards compatibility with Backspace hotkey
        /// </summary>
        public uint? Pop()
        {
            return Back();
        }
    }
}
