using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace WebApplication1.Models {
    public class Book {
        public int BookId { get; set; }
        // 必須属性を追加
        [Required]
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Genre { get; set; }
        public DateTime PublishDate { get; set; }
        public string Description { get; set; }
        // 参照ナビゲーションプロパティが宣言されていればこちらに外部キー指定しても可
        // [ForeignKey("AuthorId")]
        public int AuthorId { get; set; }
        // 外部キー設定のアノテーション
        // 参照ナビゲーションプロパティ（外部クラスとの関連付けを記述するプロパティ）に
        // 自クラスのAuthorIdを指定
        // Authorテーブルの参照に自クラスのAuthorIdを使う という内容
        [ForeignKey("AuthorId")]
        public Author Author { get; set; }
    
        // BookShelf：Bookは1：多となる。
        public int BookshelfId { get; set; }
        [ForeignKey("BookshelfId")]
        public virtual Bookshelf Bookshelf { get; set; }
    }
}