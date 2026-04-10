namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// Base class for all WorkOS API entities.
    /// Provides access to the raw HTTP response and unmapped JSON fields.
    /// </summary>
    /// <typeparam name="T">The concrete entity type.</typeparam>
    public abstract class WorkOSEntity<T>
        where T : WorkOSEntity<T>
    {
    }
}
