// @oagen-ignore-file
namespace WorkOS
{
    /// <summary>
    /// A base class to represent request options to the WorkOS API.
    /// </summary>
    public class BaseOptions
    {
        /// <summary>
        /// Returns a shallow copy of this options instance, preserving the
        /// concrete subclass. Used by runtime helpers (e.g. pagination) so
        /// they can mutate the copy without affecting caller-owned state.
        /// </summary>
        public virtual BaseOptions Clone()
        {
            return (BaseOptions)this.MemberwiseClone();
        }
    }
}
