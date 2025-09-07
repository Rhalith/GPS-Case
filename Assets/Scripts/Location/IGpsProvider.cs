using System.Collections;

namespace Location
{
    /// <summary>Abstracts how GPS is obtained (Unity LocationService, WebGL bridge, or editor-fake).</summary>
    public interface IGpsProvider
    {
        /// <summary>Call from a MonoBehaviour as a Coroutine. Yields until service is ready (or fails).</summary>
        IEnumerator Initialize();

        /// <summary>Try to read the latest sample. Returns false if not available yet.</summary>
        bool TryGetLatest(out LocationData data);

        /// <summary>Free resources.</summary>
        void Shutdown();
    }
}