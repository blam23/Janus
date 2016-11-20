namespace Janus.Matchers
{
    public interface IPatternMatcher<T>
    {
        /// <summary>
        /// Does this <paramref name="obj"/> match this <paramref name="pattern"/>?
        /// </summary>
        /// <param name="obj">Object being tested</param>
        /// <param name="pattern">Pattern to test against</param>
        /// <returns></returns>
        bool Matches(T obj, T pattern);
    }
}
