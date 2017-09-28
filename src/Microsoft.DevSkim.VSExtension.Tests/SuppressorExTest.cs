using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.DevSkim.VSExtension.Tests
{
    [TestClass]
    public class SuppressorExTest
    {
        [TestMethod]
        public void Suppress_Test()
        {
            // Is supressed test
            string testString = "md5.new()";
            SuppressionEx sup = new SuppressionEx(testString, "python");
            Assert.IsTrue(sup.Index < 0, "Suppression should not be flagged");

            // Suppress Rule test
            string ruleId = "DS196098";
            string suppressedString = sup.SuppressIssue(ruleId);
            string expected = "md5.new() #DevSkim: ignore DS196098\n";
            Assert.AreEqual(expected, suppressedString, "Supress Rule failed ");

            // Suppress Rule Until test
            DateTime expirationDate = DateTime.Now.AddDays(5);
            suppressedString = sup.SuppressIssue(ruleId, expirationDate);
            expected = string.Format("md5.new() #DevSkim: ignore DS196098 until {0:yyyy}-{0:MM}-{0:dd}\n", expirationDate);
            Assert.AreEqual(expected, suppressedString, "Supress Rule Until failed ");

            // Suppress All test
            suppressedString = sup.SuppressAll();
            expected = "md5.new() #DevSkim: ignore all\n";
            Assert.AreEqual(expected, suppressedString, "Supress All failed");

            // Suppress All Until test            
            suppressedString = sup.SuppressAll(expirationDate);
            expected = string.Format("md5.new() #DevSkim: ignore all until {0:yyyy}-{0:MM}-{0:dd}\n", expirationDate);
            Assert.AreEqual(expected, suppressedString, "Supress All Until failed ");
        }

        [TestMethod]
        public void SuppressMultiline_Test()
        {
            // Is supressed test
            string testString = "var hash=MD5.Create();";
            SuppressionEx sup = new SuppressionEx(testString, "csharp");
            Assert.IsTrue(sup.Index < 0, "Suppression should not be flagged");

            // Suppress Rule test
            string ruleId = "DS126858";
            string suppressedString = sup.SuppressIssue(ruleId);
            string expected = "var hash=MD5.Create(); /*DevSkim: ignore DS126858*/";
            Assert.AreEqual(expected, suppressedString, "Supress Rule failed ");

            // Suppress Rule Until test
            DateTime expirationDate = DateTime.Now.AddDays(5);
            suppressedString = sup.SuppressIssue(ruleId, expirationDate);
            expected = string.Format("var hash=MD5.Create(); /*DevSkim: ignore DS126858 until {0:yyyy}-{0:MM}-{0:dd}*/", expirationDate);
            Assert.AreEqual(expected, suppressedString, "Supress Rule Until failed ");

            // Suppress All test
            suppressedString = sup.SuppressAll();
            expected = "var hash=MD5.Create(); /*DevSkim: ignore all*/";
            Assert.AreEqual(expected, suppressedString, "Supress All failed");

            // Suppress All Until test            
            suppressedString = sup.SuppressAll(expirationDate);
            expected = string.Format("var hash=MD5.Create(); /*DevSkim: ignore all until {0:yyyy}-{0:MM}-{0:dd}*/", expirationDate);
            Assert.AreEqual(expected, suppressedString, "Supress All Until failed ");
        }

        [TestMethod]
        public void SuppressExisting_Test()
        {
            string testString = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore DS126858,DS168931 until {0:yyyy}-{0:MM}-{0:dd}";
            DateTime expirationDate = DateTime.Now.AddDays(5);

            SuppressionEx sup = new SuppressionEx(string.Format(testString, expirationDate), "csharp");
            Assert.IsTrue(sup.IsIssueSuppressed("DS126858"), "Is suppressed DS126858 should be True");
            Assert.IsTrue(sup.IsIssueSuppressed("DS168931"), "Is suppressed DS168931 should be True");

            // Suppress multiple
            string suppressedString = sup.SuppressIssue("DS196098");
            string expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore DS126858,DS168931,DS196098 until {0:yyyy}-{0:MM}-{0:dd}";
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple failed");

            // Suppress multiple to all
            suppressedString = sup.SuppressAll();
            expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore all until {0:yyyy}-{0:MM}-{0:dd}";
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple to all failed");

            // Suppress multiple new date            
            expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore DS126858,DS168931,DS196098 until {0:yyyy}-{0:MM}-{0:dd}";
            suppressedString = sup.SuppressIssue("DS196098", DateTime.Now.AddDays(100));            
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple new date failed");

            // Suppress multiple to all new date
            suppressedString = sup.SuppressAll(DateTime.Now.AddDays(10));
            expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore all until {0:yyyy}-{0:MM}-{0:dd}";
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple to all new date failed");
        }

        [TestMethod]
        public void UseCase_SuppressExistingPast_Test()
        {
            string testString = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore DS126858,DS168931 until 1980-07-15";
            SuppressionEx sup = new SuppressionEx(testString, "csharp");
            Assert.IsFalse(sup.IsIssueSuppressed("DS126858"), "Is suppressed DS126858 should be True");
            Assert.IsFalse(sup.IsIssueSuppressed("DS168931"), "Is suppressed DS168931 should be True");

            // Suppress multiple
            string suppressedString = sup.SuppressIssue("DS196098");
            string expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore DS126858,DS168931,DS196098 until 1980-07-15";
            Assert.AreEqual(expected, suppressedString, "Suppress multiple failed");

            // Suppress multiple new date            
            DateTime expirationDate = DateTime.Now.AddDays(10);
            suppressedString = sup.SuppressIssue("DS196098", expirationDate);
            expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore DS126858,DS168931,DS196098 until 1980-07-15";
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple new date failed");

            // Suppress multiple to all
            suppressedString = sup.SuppressAll();
            expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore all until 1980-07-15";
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple to all failed");

            // Suppress multiple to all new date
            suppressedString = sup.SuppressAll(expirationDate);
            expected = "MD5 hash = new MD5CryptoServiceProvider(); //DevSkim: ignore all until 1980-07-15";
            Assert.AreEqual(string.Format(expected, expirationDate), suppressedString, "Suppress multiple to all new date failed");
        }       
    }
}
