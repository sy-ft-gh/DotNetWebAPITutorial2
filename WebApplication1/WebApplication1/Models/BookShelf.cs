using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models {
    public class Bookshelf {
        public int BookshelfId { get; set; }
        [Required]
        public string NamePlate { get; set; }
        /// <summary>
        /// 1対多のリレーションを設定
        /// </summary>
        public virtual ICollection<Book> Books { get; set; }

    }
}