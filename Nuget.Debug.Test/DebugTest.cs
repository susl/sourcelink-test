using System;
using System.Linq;

namespace Nuget.Debug.Test
{
    using System.Globalization;

    public class DebugTest
    {
        public static string SomeMethod(int i)
        {
            var s = i.ToString(CultureInfo.InvariantCulture);
            s += "$";
            return s;
        }
    }
}
