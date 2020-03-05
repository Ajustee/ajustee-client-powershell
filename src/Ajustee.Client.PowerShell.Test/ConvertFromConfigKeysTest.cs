using System.Management.Automation;
using Xunit;

namespace Ajustee.Client.PowerShell
{
    public class ConvertFromConfigKeysTest
    {
        [Fact]
        public void DynamicObject()
        {
            var _obj = new PSObject();
            _obj.Properties.Add(new PSNoteProperty("prop1", 12));
            var _subObj = new PSObject();
            _subObj.Properties.Add(new PSNoteProperty("prop3", "value"));
            _obj.Properties.Add(new PSNoteProperty("prop2", _subObj));

            dynamic _dyn = _obj;
            Assert.Equal(_dyn.prop1, 12);
            Assert.Equal(_dyn.prop2.prop3, "value");
        }
    }
}
