using NUnit.Framework;
using UnityEngine;

public class TimeServiceTests 
{ 
        [Test]
        public void TestServerTimeSet()
        {
			TimeService.Reset();
			TimeService.SetUtcTimeReference("2023-07-27T22:10:44.619Z");
            var currentTime = TimeService.GetUtcNow();

            Assert.AreEqual( currentTime.Year, 2023 );
            Assert.AreEqual( currentTime.Month, 7 );
            Assert.AreEqual( currentTime.Day, 27 );
            Assert.AreEqual( currentTime.Hour, 22 );
            Assert.AreEqual( currentTime.Minute, 10 );
            Assert.AreEqual( currentTime.Second, 44 );
        }
}