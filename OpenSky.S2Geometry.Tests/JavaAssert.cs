namespace OpenSky.S2Geometry.Tests
{
    using System.Diagnostics;

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