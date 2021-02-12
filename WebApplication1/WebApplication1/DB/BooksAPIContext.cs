using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication1.DB
{
    public class BooksAPIContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public BooksAPIContext() : base("name=BooksAPIContext") {

            // SQLをログ出力する設定
            this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }
        public System.Data.Entity.DbSet<WebApplication1.Models.Bookshelf> Bookshelfs { get; set; }

        public System.Data.Entity.DbSet<WebApplication1.Models.Book> Books { get; set; }

        public System.Data.Entity.DbSet<WebApplication1.Models.Author> Authors { get; set; }
    }
}
