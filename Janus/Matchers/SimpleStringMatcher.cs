namespace Janus.Matchers
{
    /// <summary>
    /// Does very simple * based pattern matching where * can be any block
    /// of characters.
    /// Look at MatcherTest.cs unit tests for examples.
    /// </summary>
    public class SimpleStringMatcher : IPatternMatcher<string>
    {
        public bool Matches(string text, string filter)
        {
            bool ret = false;
            int j = 0, i = 0;
            bool inStar = false;
            bool ended = false;
            while (!ended && i < filter.Length && j < text.Length)
            {
                if (filter[i] == '*')
                {
                    inStar = true;
                    i++;
                    continue;
                }

                if (filter[i] == text[j])
                {
                    if (inStar)
                    {
                        var starEnd = i;
                        while (starEnd < filter.Length && filter[starEnd] != '*') starEnd++;

                        var blockLength = starEnd - i;
                        if (blockLength == 0)
                        {
                            if (starEnd == filter.Length - 1)
                            {
                                ended = true;
                                ret = true;
                                continue;
                            }
                            else
                            {
                                j++;
                                i++;
                                continue; // double star - unsure what to do here.
                            }
                        }

                        if (blockLength > text.Length - j) // not long enough to match
                        {
                            ended = true;
                            continue;
                        }

                        var filterBlock = filter.Substring(i, blockLength);
                        var textBlock = text.Substring(j, blockLength);
                        if (filterBlock == textBlock)
                        {
                            inStar = false;
                            j += blockLength;
                            i += blockLength;
                            continue;
                        }
                    }
                    else
                    {
                        i++;
                    }
                    j++;
                }
                else
                {
                    if (!inStar)
                    {
                        ended = true;
                        continue;
                    }
                    j++;
                }
            }

            // Check we've matched all filter against all text
            if (!(i + (filter.IndexOf('*') >= 0 ? 1 : 0) < filter.Length || (!inStar && j < text.Length)))
            {
                ret = true;
            }

            return ret;
        }
    }
}
