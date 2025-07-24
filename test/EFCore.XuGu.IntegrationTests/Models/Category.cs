using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class Category
    {
        public int CategoryId { get; set; }  // 主键
        public string CategoryName { get; set; }  // 类别名称 (例如：计算机、文学)
        public string Description { get; set; }  // 类别描述

        // 外键
        public ICollection<BookCategory> BookCategories { get; set; }  // 图书与类别的关系
    }
}
