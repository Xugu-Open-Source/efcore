using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class Book
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid BookId { get; set; }  // 主键
        public string Title { get; set; }  // 书名
        public string Author { get; set; }  // 作者
        public string Publisher { get; set; }  // 出版社
        public int YearOfPublication { get; set; }  // 出版年份
        [Column(TypeName ="varchar")]
        public string ISBN { get; set; }  // ISBN号
        public decimal Price { get; set; }  // 价格
        public int Quantity { get; set; }  // 库存数量
        public int AvailableQuantity { get; set; }  // 可借数量
        public string Description { get; set; }  // 书籍简介

        // 外键
        public virtual ICollection<BorrowRecord> BorrowRecords { get; set; }  // 借阅记录
        public virtual ICollection<BookCategory> BookCategories { get; set; }  // 图书分类
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
