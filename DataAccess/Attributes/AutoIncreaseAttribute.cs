using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    /// <summary>
    /// 添加识别自增长的字段
    /// <author>
    ///		<name>shenjun</name>
    ///		<date>2018.12.21</date>
    /// </author>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class AutoIncreaseAttribute : Attribute
    {
        public AutoIncreaseAttribute()
        {
        }

        public AutoIncreaseAttribute(string name)
        {
            _name = name;
        }
        private string _name; public virtual string Name { get { return _name; } set { _name = value; } }
    }
}