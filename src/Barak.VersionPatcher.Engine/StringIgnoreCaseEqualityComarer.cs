using System;
using System.Collections.Generic;

namespace Barak.VersionPatcher.Engine
{
    public class StringIgnoreCaseEqualityComarer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            if (obj == null)
            {
                return 0;
            }
            else
            {
                return obj.ToUpper().GetHashCode();
            }
        }
    }
}