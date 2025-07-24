using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.XuGu.IntegrationTests.Models
{
    public class User
    {
        public int UserId { get; set; }  // 主键
        [Column(TypeName = "varchar")]
        public string UserName { get; set; }  // 用户名
        public string PasswordHash { get; set; }  // 密码的哈希值
        [Column(TypeName = "varchar")]
        public string FullName { get; set; }  // 用户的全名
        [Column(TypeName = "varchar")]
        public string Email { get; set; }  // 用户的邮箱
        public string Phone { get; set; }  // 用户的电话
        public UserType UserType { get; set; }  // 用户类型 (管理员/普通用户)

        // 外键
        public ICollection<BorrowRecord> BorrowRecords { get; set; }  // 借阅记录
    }

    public enum UserType
    {
        Admin,  // 管理员
        Normal  // 普通用户
    }
}
