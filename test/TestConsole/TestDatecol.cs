using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TestConsole
{
    public class TestDatecol
    {
        public int Id { get; set; }
        [Column(TypeName = "date")]
        public DateTime col1 {  get; set; }
    }
}
