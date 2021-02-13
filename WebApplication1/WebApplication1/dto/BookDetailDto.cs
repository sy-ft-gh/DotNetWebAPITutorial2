using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.dto {

    public class BookDetail {
        public string Title { get; set; }
        public string Genre { get; set; }
        public DateTime PublishDate { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Author { get; set; }

    }
    public class BookDetailDto: DtoBase {
        // 基底クラスのDataをBookDetailで上書き
        public new BookDetail Data;
    }   
}