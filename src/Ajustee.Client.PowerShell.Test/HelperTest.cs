using System.Management.Automation;
using Xunit;

namespace Ajustee.Client.PowerShell
{
    public class HelperTest
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("/", "/")]
        [InlineData("abs", "abs/")]
        [InlineData("abs/", "abs/")]
        public void NormalizeNamespace(string ns, string expected)
        {
            Assert.Equal(expected, Helper.NormalizeNamespace(ns));
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(null, "", false)]
        [InlineData(null, "ns", false)]
        [InlineData("", null, false)]
        [InlineData("", "", false)]
        [InlineData("path", "", false)]
        [InlineData("path1", "path", false)]
        [InlineData("namespace/path1", "namespace", true)]
        public void IsNamespaceMatch(string path, string ns, bool expected)
        {
            Assert.Equal(expected, Helper.IsNamespaceMatch(new ConfigKey(path, ConfigKeyType.Undefined, null), ns));
        }

        [Theory]
        [InlineData(null, null, "")]
        [InlineData(null, "", "")]
        [InlineData(null, "ns", "")]
        [InlineData("", null, "")]
        [InlineData("", "", "")]
        [InlineData("path", "", "path")]
        [InlineData("path1", "path", "path1")]
        [InlineData("path1/path2", "path", "path1;path2")]
        [InlineData("namespace/path1", "namespace", "path1")]
        [InlineData("namespace/path1/path2", "namespace", "path1;path2")]
        public void GetPropertyNames(string path, string ns, string expected)
        {
            Assert.Equal(expected, string.Join(";", Helper.GetPropertyNames(new ConfigKey(path, ConfigKeyType.Undefined, null), ns)));
        }
    }
}
