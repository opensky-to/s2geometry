using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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