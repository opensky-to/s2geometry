using System.Diagnostics;

namespace S2Geometry.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public static class JavaAssert
    {
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static void Equal(object actual, object expected)
        {
            Assert.AreEqual(expected, actual);
        }
    }
}