using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class StockAlert
    {
        public int StockAlertId { get; set; }  // 主键
        public int BookId { get; set; }  // 图书ID
        public int Threshold { get; set; }  // 库存警告阈值
        public DateTime AlertDate { get; set; }  // 警告日期

        public Book Book { get; set; }  // 关联图书
    }

}
