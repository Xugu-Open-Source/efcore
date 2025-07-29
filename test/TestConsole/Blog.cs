using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = true;

        public List<Post> Posts { get; set; } = new List<Post>();

        public DateTime Created { get; set; }
    }
}
