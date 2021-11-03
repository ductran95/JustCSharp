using System.Linq;

namespace JustCSharp.Utility.Helpers
{
    public static class OperatorHelper
    {
        public static bool OnlyOneTrue(params bool[] values)
        {
            return values.Count(x => x) == 1;
        }
        
        public static bool OnlyOneFalse(params bool[] values)
        {
            return values.Count(x => !x) == 1;
        }
    }
}