using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class BorrowRecord
    {
        public int BorrowRecordId { get; set; }  // 主键
        public DateTime BorrowDate { get; set; }  // 借阅日期
        public DateTime DueDate { get; set; }  // 应还日期
        public DateTime? ReturnDate { get; set; }  // 归还日期（可为空）
        //public decimal? FineAmount { get; set; }  // 逾期罚款（如果有）


        public virtual ICollection<Fine> Fines { get; set; }

        // 外键
        public int UserId { get; set; }  // 用户ID
        public virtual User User { get; set; }  // 用户
        public Guid BookId { get; set; }  // 图书ID
        public virtual Book Book { get; set; }  // 图书
    }

}
