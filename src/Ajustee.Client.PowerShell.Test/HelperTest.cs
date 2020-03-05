using System.Management.Automation;
using Xunit;

namespace Ajustee.Client.PowerShell
{
    public class HelperTest
    {
        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("/", "")]
        [InlineData("abs", "abs/")]
        [InlineData("abs/", "abs/")]
        public void NormalizeNamespace(string ns, string expected)
        {
            Assert.Equal(expected, Helper.NormalizeNamespace(ns));
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("path", "", "path")]
        [InlineData("path1", "path", "path1")]
        [InlineData("path1/path2", "path", "path1/path2")]
        [InlineData("namespace/path1", "namespace", "path1")]
        [InlineData("namespace/path1/path2", "namespace", "path1/path2")]
        public void RemoveNamespace(string path, string ns, string expected)
        {
            Helper.RemoveNamespace(ref path, Helper.NormalizeNamespace(ns));
            Assert.Equal(expected, path);
        }

        [Fact]
        public void AddToObject()
        {
            var _configKeys = new[] {
                new ConfigKey("namespace/Database/Host", ConfigKeyType.String, "host"),
                new ConfigKey("namespace/Database/Port", ConfigKeyType.Integer, "123"),
                new ConfigKey("namespace/Database/User", ConfigKeyType.String, "user"),
                new ConfigKey("namespace/Database/Trust", ConfigKeyType.Boolean, "true"),
                new ConfigKey("namespace/Api/Redis/Login", ConfigKeyType.String, "login"),
                new ConfigKey("namespace/Api/Redis/Password", ConfigKeyType.String, "password"),
                new ConfigKey("namespace/Api/Ajustee/AppUrl", ConfigKeyType.String, "url"),
                new ConfigKey("namespace/Api/Ajustee/AppId", ConfigKeyType.String, "appid"),
                new ConfigKey("namespace/Ambig/Set", ConfigKeyType.String, "set"),
                new ConfigKey("namespace/Ambig/Set/Prop1", ConfigKeyType.String, "prop1"),
                new ConfigKey("namespace/Ambig/Set/Prop2", ConfigKeyType.String, "prop2"),
                new ConfigKey("namespace/Api/Ajustee/Default/Path", ConfigKeyType.String, "path"),
                new ConfigKey("namespace/Api/Ajustee/Default/Props", ConfigKeyType.String, "props"),
            };

            dynamic _obj = _configKeys.ToPSObject("namespace");

            Assert.Equal("host", _obj.Database.Host);
            Assert.Equal(123, _obj.Database.Port);
            Assert.Equal("user", _obj.Database.User);
            Assert.Equal(true, _obj.Database.Trust);
            Assert.Equal("login", _obj.Api.Redis.Login);
            Assert.Equal("password", _obj.Api.Redis.Password);
            Assert.Equal("url", _obj.Api.Ajustee.AppUrl);
            Assert.Equal("appid", _obj.Api.Ajustee.AppId);
            Assert.Equal("prop1", _obj.Ambig.Set.Prop1);
            Assert.Equal("prop2", _obj.Ambig.Set.Prop2);
        }

        [Fact]
        public void AddToObjectNamespaceFiltering()
        {
            var _configKeys = new[] {
                new ConfigKey("namespace1/Prop1", ConfigKeyType.String, "prop1"),
                new ConfigKey("namespace2/Prop2", ConfigKeyType.String, "prop2")
            };

            dynamic _obj = _configKeys.ToPSObject("namespace2");

            Assert.Equal(null, _obj.Prop1);
            Assert.Equal("prop2", _obj.Prop2);
        }
    }
}
