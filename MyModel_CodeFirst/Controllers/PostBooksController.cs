using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyModel_CodeFirst.Models;

namespace MyModel_CodeFirst.Controllers
{
    public class PostBooksController : Controller
    {
        private readonly GuestBookContext _context;

        public PostBooksController(GuestBookContext context)
        {
            _context = context;
        }

        // GET: PostBooks
        // 非同步傳輸，先處理完的資料會先回傳給使用者
        // async/await 是 C# 中用來處理非同步程式碼的關鍵字
        // 使用 Entity Framework Core 的 ToListAsync 方法來非同步地取得 Book 資料表中的所有資料
        /*public async Task<IActionResult> Index()
        {
            return View(await _context.Book.ToListAsync());
        }*/

        // 先在伺服器端排序資料，再傳送到前端顯示，讓資料排序的動作在不同地方進行處理，增加運行效率
        public async Task<IActionResult> Index()
        {
            //2.1.6 修改Index Action的寫法
            var result = _context.Book.OrderByDescending(s => s.CreatedDate);

            return View(await result.ToListAsync());
        }


        //2.2.2 將PostBooksController中Details Action改名為Display(View也要改名字)
        public async Task<IActionResult> Display(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: PostBooks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PostBooks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        // 單獨用簡單繫節法加上 IFormFile newPhoto (接收前端上傳檔案的型別)參數，讓使用者可以上傳圖片
        // IFormFile? newPhoto 代表上傳的檔案可以是 null，這樣就不會強制要求使用者一定要上傳檔案
        public async Task<IActionResult> Create([Bind("BookID,Title,Description,Author,Photo,CreatedDate")] Book book, IFormFile? newPhoto)
        {
            book.CreatedDate = DateTime.Now; // 設定建立日期為目前時間

            //2.4.6 修改Post Create Action，加上處理上傳照片的功能
            // 複製舊檔案
            // 如果使用者沒有上傳新照片，則不處理照片上傳

            if (newPhoto != null && newPhoto.Length != 0)
            { 
                // 只允許上傳圖片
                if (newPhoto.ContentType != "image/jpeg" && newPhoto.ContentType != "image/png")
                {
                    ViewData["ErrMessage"] = "只允許上傳.jpg或.png的圖片檔案!!";
                    return View();
                }


                // 取得檔案名稱
                // 使用 BookID 作為檔案名稱，並加上原始檔案的副檔名
                string fileName = book.BookID + Path.GetExtension(newPhoto.FileName);

                //取得檔案的完整路徑
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BookPhotos", fileName);
                // /wwwroot/Photos/xxx.jpg

                //將檔案上傳並儲存於指定的路徑

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    newPhoto.CopyTo(fs);
                }

                // 將檔案名稱存入 Book 的 Photo 屬性
                book.Photo = fileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }


        // 刪除、修改、查詢前，先確認資料是否存在，避免操作不存在的資料時發生錯誤。
        // 這是一個「檢查某本書是否存在」的輔助方法，回傳 true 代表有這本書，false 代表沒有。
        private bool BookExists(string id)
        {
            return _context.Book.Any(e => e.BookID == id);
        }
    }
}
