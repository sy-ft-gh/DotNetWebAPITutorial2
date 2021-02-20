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
        private static readonly Expression<Func<Book, BookInfo>> AsBookInfo =
            x => new BookInfo {
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

            // Include＝結合条件
            BookInfo book = await db.Books.Include(b => b.Author)
                .Where(b => b.BookId == id)
                // Selctorとして式木を指定する事でBook→BookDtoへのマッピングを行う
                .Select(AsBookInfo)
                .FirstOrDefaultAsync();
            var result = new BookDto();
            if (book == null) {
                // 結果にステータスコードとメッセージを設定(NotFound)
                result.Status = ApiStatusCode.StatusNotFound.GetStatusCode();
                result.Message = ApiStatusCode.StatusNotFound.GetMessage();
            } else {
                // 結果にステータスコードとメッセージを設定(OK)
                result.Status = ApiStatusCode.StatusOK.GetStatusCode();
                result.Message = ApiStatusCode.StatusOK.GetMessage();
                // 取得データを格納
                result.Data = book;
            }

            return Ok(result);
        }

        /// <summary>
        /// タイトルでBook情報を検索する
        /// </summary>
        /// <param name="">対象ジャンル</param>
        /// <returns>ジャンル名にマッチしたBookDto</returns>
        [Route("search")]
        [ResponseType(typeof(BooksDto))]
        [HttpGet]
        public async Task<IHttpActionResult> SearchBook([FromUri] SearchParam param) { 
            if (!ModelState.IsValid) {
                DtoBase NG_result = new DtoBase();
                NG_result.Status = ApiStatusCode.StatusIllegalArg.GetStatusCode();
                NG_result.Message = ApiStatusCode.StatusIllegalArg.GetMessage();
                NG_result.Data = null;
                return Ok(NG_result);
            }
            // bookとauthorを結合し引数のtitleを含むbookを検索

            var qbooks = from x in db.Books
                         join y in db.Authors on x.AuthorId equals y.AuthorId
                         select new BookInfo() { Author = y.Name, Genre = x.Genre, Title = x.Title };
            List<BookInfo> books = null;
            if (param != null) {
                if (!String.IsNullOrEmpty(param.title)) {
                    qbooks = qbooks.Where(s => s.Title.Contains(param.title));
                }
            } else {
                books = await qbooks.ToListAsync();
            }
            var OK_result = new BooksDto();
            if (books == null || books.Count == 0) {
                // 結果にステータスコードとメッセージを設定(NotFound)
                OK_result.Status = ApiStatusCode.StatusNotFound.GetStatusCode();
                OK_result.Message = ApiStatusCode.StatusNotFound.GetMessage() + string.Join(",", Request.GetQueryNameValuePairs().Select(e => e.Key + ":" + e.Value).ToArray());
            } else {
                // 結果にステータスコードとメッセージを設定(OK)
                OK_result.Status = ApiStatusCode.StatusOK.GetStatusCode();
                OK_result.Message = ApiStatusCode.StatusOK.GetMessage();
                // 取得データを格納
                OK_result.Data = books;
            }
            return Ok(OK_result);
        }

        /// <summary>
        /// ジャンルでBook情報を取得する
        /// </summary>
        /// <param name="genre">対象ジャンル</param>
        /// <returns>ジャンル名にマッチしたBookDto</returns>
        [Route("genre/{genre}")]
        public IQueryable<BookInfo> GetBooksByGenre(string genre) {
            // ジャンル名で検索
            // StringComparison.OrdinalIgnoreCase=小文字大文字を区別なくマッチ
            return db.Books.Include(b => b.Author)
                .Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
                .Select(AsBookInfo);
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
                              select new BookDetail{
                                Title = b.Title,
                                Genre = b.Genre,
                                PublishDate = b.PublishDate,
                                Price = b.Price,
                                Description = b.Description,
                                Author = b.Author.Name
                              }).FirstOrDefaultAsync();
            var result = new BookDetailDto();
            if (book == null) {
                // 結果にステータスコードとメッセージを設定(NotFound)
                result.Status = ApiStatusCode.StatusNotFound.GetStatusCode();
                result.Message = ApiStatusCode.StatusNotFound.GetMessage();
            } else {
                // 結果にステータスコードとメッセージを設定(OK)
                result.Status = ApiStatusCode.StatusOK.GetStatusCode();
                result.Message = ApiStatusCode.StatusOK.GetMessage();
                // 取得データを格納
                result.Data = book;
            }
            return Ok(result);
        }
        /// <summary>
        /// Authorによる検索
        /// </summary>
        /// <param name="authorId">対象のAuthorId</param>
        /// <returns>BookDtoのQueryableオブジェクト</returns>
        [Route("~/api/authors/{authorId:int}/books")]
        public IQueryable<BookInfo> GetBooksByAuthor(int authorId) {
            // Queryrableを返却する事でフレームワークからデータを抜き出してマッピングする
            return db.Books.Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .Select(AsBookInfo);
        }

        /// <summary>
        /// 出版日による検索
        /// </summary>
        /// <param name="pubdate">対象の出版日</param>
        /// <returns></returns>
        [Route("date/{pubdate:datetime}")]
        public IQueryable<BookInfo> GetBooks(DateTime pubdate) {
            // DbFunctions.TruncateTime=時間の切り捨て
            // SQLServerではDateTime型は時間も含まれる
            // この時間部分での評価をせず年月日で評価をするため付加する
            // ※内部的にはVARCHAR変換→DATETIME変換(年月日スタイル)になる
            return db.Books.Include(b => b.Author)
                .Where(b => DbFunctions.TruncateTime(b.PublishDate)
                    == DbFunctions.TruncateTime(pubdate))
                .Select(AsBookInfo);
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
    public class SearchParam { 
        public string title { get; set; }
        public string genre { get; set; }
    }
}