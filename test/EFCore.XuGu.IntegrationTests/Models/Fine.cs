using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class Fine
    {
        public int FineId { get; set; }  // 主键
        public decimal Amount { get; set; }  // 罚款金额
        public DateTime DateIssued { get; set; }  // 罚款日期
        public bool IsPaid { get; set; }  // 是否已支付
        public int BorrowRecordId { get; set; }  // 关联的借阅记录

        public virtual BorrowRecord BorrowRecord { get; set; }  // 借阅记录
    }

}
