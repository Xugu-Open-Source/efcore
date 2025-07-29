using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TestConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BloggingContext())
            {

                // 添加数据
                //if (db.Blogs.LongCount()<=0)
                //{
                //    var blog = new Blog { Url = "FGSDFGSDFG22", Created = DateTime.Now };
                //    blog.Posts.Add(new Post { Title = "EFCore2.2 First Post", Content = "EFCore2.2 Hello World", Created = DateTime.Now });
                //    blog.Posts.Add(new Post { Title = "EFCore2.2 Second Post", Content = "EFCore2.2 EF Core 7.0 is great!", Created = DateTime.Now });

                //    db.Blogs.Add(blog);
                //    db.SaveChanges();
                //    Console.WriteLine("向数据库中添加样例数据");
                //}

                // 查询数据
                Console.WriteLine("所有博客和帖子:");
                //var blogs = db.Blogs.Include(b => b.Posts).ToList();

                //foreach (var blog in blogs)
                //{
                //    Console.WriteLine($"Blog: {blog.Url}----{blog.IsPublic}----{blog.Created}");
                //    foreach (var post in blog.Posts)
                //    {
                //        Console.WriteLine($"- {post.Title}: {post.Content}----{post.Created}");
                //    }
                //}

                var ds = db.TestDatecols.ToList();

                foreach (var d in ds)
                {
                    Console.WriteLine($"testDatecol: {d.col1}");
                }
            }
            Console.ReadLine();
        }
    }
}
