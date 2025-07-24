using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class Log
    {
        public int LogId { get; set; }  // 主键
        public string Action { get; set; }  // 操作名称（如：添加书籍、借阅书籍等）
        public DateTime ActionDate { get; set; }  // 操作时间
        public string Actor { get; set; }  // 执行操作的用户（可以是用户名）

        // 可选字段：具体操作的数据（如新增的书籍ID、删除的书籍ID等）
        public string Details { get; set; }
    }

}
