using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class Configuration
    {
        public int ConfigurationId { get; set; }  // 主键
        public string Key { get; set; }  // 配置项的键（如：MaxBorrowDays）
        public string Value { get; set; }  // 配置项的值（如：30，表示最多借阅30天）
    }

}
