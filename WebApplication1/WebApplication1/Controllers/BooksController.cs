using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using WebApplication1.DB;
using WebApplication1.Models;
using WebApplication1.dto;


namespace WebApplication1.Controllers {

    [RoutePrefix("api/books")]
    public class BooksController : ApiController {
        private BooksAPIContext db = new BooksAPIContext();

        // ラムダ式を式木にしている
        // 引数のBookインスタンスをBookDtoインスタンスに代入するラムダ式

        // Typed lambda expression for Select() method. 
        private static readonly Expression<Func<Book, BookDto>> AsBookDto =
            x => new BookDto {
                Title = x.Title,
                Author = x.Author.Name,
                Genre = x.Genre
            };


        // IQueryable = Linqで用いるQuery式 サーバへの問い合わせと結果のマッピングを行う。
        // 
        // GET: api/Books
        [Route("")]
        public IQueryable<Book> GetBooks() {
            // 結果はIQueryableで戻る。フレームワークによりコード外で内部のデータが取り出される。
            return db.Books;
        }

        // GET: api/Books/5
        /// <summary>
        /// Book情報としてTitle,Author,Genreを返却する
        /// </summary>
        /// <param name="id">対象のBookId</param>
        /// <returns>IDにマッチしたBookDto</returns>
        // レスポンスタイプ＝レスポンスボディのタイプ。（XMLかJSONで表現される構造体形式がクラスで表現される）
        // GET api/Books/5
        [Route("{id:int}")]
        [ResponseType(typeof(BookDto))]
        // IHttpActionResultを結果として返すTaskを返す。
        public async Task<IHttpActionResult> GetBook(int id) {
            // Book検索処理を実行
            // ※※ awaitが付加されている
            //      →このメソッドの終了を待たずして、呼び出し元のフレームワーク（ASP.net）側に処理が戻る
            //      →フレームワーク側は別の処理を同時に実行し当メソッドが終わり次第結果をレスポンスとして返す
            // →つまり、このステップ以下は全て子スレッドで実行される。

            // Include＝
            BookDto book = await db.Books.Include(b => b.Author)
                .Where(b => b.BookId == id)
                // Selctorとして式木を指定する事でBook→BookDtoへのマッピングを行う
                .Select(AsBookDto)
                .FirstOrDefaultAsync();
            if (book == null) {
                return NotFound();
            }

            return Ok(book);
        }

        /// <summary>
        /// ジャンルでBook情報を取得する
        /// </summary>
        /// <param name="genre">対象ジャンル</param>
        /// <returns>IDにマッチしたBookDto</returns>
        [Route("{genre}")]
        public IQueryable<BookDto> GetBooksByGenre(string genre) {
            // ジャンル名で検索
            // StringComparison.OrdinalIgnoreCase=小文字大文字を区別なくマッチ
            return db.Books.Include(b => b.Author)
                .Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .Select(AsBookDto);
        }
        /// <summary>
        /// Title,Author,Genre以外の項目を返却する
        /// </summary>
        /// <param name="id">対象のBookId</param>
        /// <returns>IDにマッチしたBookDetailDto</returns>
        [Route("{id:int}/details")]
        [ResponseType(typeof(BookDetailDto))]
        public async Task<IHttpActionResult> GetBookDetail(int id) {
            // include 参照先のオブジェクト（=DBのテーブル）を結合して取得する。
            var book = await (from b in db.Books.Include(b => b.Author)
                              where b.BookId == id
                              select new BookDetailDto {
                                  Title = b.Title,
                                  Genre = b.Genre,
                                  PublishDate = b.PublishDate,
                                  Price = b.Price,
                                  Description = b.Description,
                                  Author = b.Author.Name
                              }).FirstOrDefaultAsync();

            if (book == null) {
                return NotFound();
            }
            return Ok(book);
        }
        /// <summary>
        /// Authorによる検索
        /// </summary>
        /// <param name="authorId">対象のAuthorId</param>
        /// <returns>BookDtoのQueryableオブジェクト</returns>
        [Route("~/api/authors/{authorId:int}/books")]
        public IQueryable<BookDto> GetBooksByAuthor(int authorId) {
            // Queryrableを返却する事でフレームワークからデータを抜き出してマッピングする
            return db.Books.Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .Select(AsBookDto);
        }

        /// <summary>
        /// 出版日による検索
        /// </summary>
        /// <param name="pubdate">対象の出版日</param>
        /// <returns></returns>
        [Route("date/{pubdate:datetime}")]
        public IQueryable<BookDto> GetBooks(DateTime pubdate) {
            // DbFunctions.TruncateTime=時間の切り捨て
            // SQLServerではDateTime型は時間も含まれる
            // この時間部分での評価をせず年月日で評価をするため付加する
            // ※内部的にはVARCHAR変換→DATETIME変換(年月日スタイル)になる
            return db.Books.Include(b => b.Author)
                .Where(b => DbFunctions.TruncateTime(b.PublishDate)
                    == DbFunctions.TruncateTime(pubdate))
                .Select(AsBookDto);
        }
        /// <summary>
        /// Bookshelfによる検索
        /// </summary>
        /// <param name="id">対象のBookshelfId</param>
        /// <returns>IDにマッチしたBookshelf</returns>
        [Route("{id:int}/bookshelf")]
        [ResponseType(typeof(Bookshelf))]
        public async Task<IHttpActionResult> GetBooksInBookShelf(int id) {
            /*  Eager loading = 1度のSQLでBookshelfとBookテーブルを結合して取得
            var bookshelf = await (from b in db.Bookshelfs.Include(b => b.Books)
                                    where b.BookshelfId == id
                                    select b).FirstOrDefaultAsync();
            if (bookshelf == null) {
                return NotFound();
            }
            */
            /* Lazy loading = 遅延読み込み。対象の情報が必要になった時に情報を取得 */
            // ここではBookshelfテーブルのレコードを1件取得
            var bookshelf = await (from b in db.Bookshelfs
                                   where b.BookshelfId == id
                                   select b).FirstOrDefaultAsync();
            if (bookshelf == null) {
                return NotFound();
            }
            // ここではBooksテーブルを参照。BookshelfId=1のレコードが検索される
            var book = bookshelf.Books;


            return Ok(bookshelf);
        }

        protected override void Dispose(bool disposing) {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}