using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.dto {

    public class BookInfo {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }

    }
    public class BookDto: DtoBase {
        // BookInfoで基底クラスのDataを上書き
        public new BookInfo Data { get; set; }
    }
}