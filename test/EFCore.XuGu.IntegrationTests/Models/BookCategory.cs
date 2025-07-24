using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class BookCategory
    {
        public Guid BookId { get; set; }  // 图书ID
        public int CategoryId { get; set; }  // 类别ID

        public virtual Book Book { get; set; }
        public virtual Category Category { get; set; }

        public virtual Fine Fine { get; set; }
    }
}
