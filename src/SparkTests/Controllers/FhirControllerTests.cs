using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spark.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spark.Controllers.Tests
{
    [TestClass()]
    public class FhirControllerTests
    {
        [TestMethod()]
        public void ReadTest()
        {
            FhirController fc = new FhirController(null);
            Assert.Fail();
        }
    }
}