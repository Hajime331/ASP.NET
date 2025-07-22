using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebAPI.DTOs;
using MyWebAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyWebAPI.Controllers
{
    //3.2.4 修改API介接路由為「api[controller]」
    [Route("api[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly GoodStoreContext _context;

        public ProductsController(GoodStoreContext context)
        {
            _context = context;
        }

        // GET: api/Products
        /*表示根部目錄，會覆蓋 Controller 層級的Route
        [HttpGet("/p")]
        Action 層級的Route
        [HttpGet("productList")]*/
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProduct(decimal price=0)
        {
            //4.1.2 使用Include()同時取得關聯資料
            // 表達Category與Product之間的關聯，因此不使用CateID作為查詢條件，而是直接使用Include()方法來載入Category資料。
            // money 用 decimal，price 小寫表示參數
            //4.1.3 使用Where()改變查詢的條件並測試
            // price=0 表示顯示全部資料
            //4.1.4 使用OrderBy()相關排序方法改變資料排序並測試
            // query string: api/products?price=100

            // var products = await _context.Product.Include(c => c.Cate).Where(p=>p.Price>= price).OrderBy(p=>p.Price).ToListAsync();
            //4.1.5 使用Select()抓取特定的欄位並測試，這樣做會無法 return
            /*var products = await _context.Product.Include(c => c.Cate).Where(p => p.Price >= price)
                .OrderBy(p => p.Price).Select(p => new
                {
                    p.ProductID,
                    p.ProductName,
                    p.Price,
                    p.Picture,
                    p.Description,
                    p.CreatedDate,
                    p.CateID,
                    p.Cate.CateName

                }).ToListAsync();*/
            var products = await _context.Product.Include(c => c.Cate).Where(p => p.Price >= price)
                .OrderBy(p => p.Price).Select(p => new ProductDTO
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Picture = p.Picture,
                    Description = p.Description,
                    CateID = p.CateID,
                    CateName = p.Cate.CateName

                }).ToListAsync();

            return products;
        }

        // GET: api/Products/5
        /*[HttpGet("{id}")]
        希望查詢的資訊能與 ProductDTO 相符
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _context.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }*/
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(string id)
        {
            var product = await _context.Product.Include(c => c.Cate).Where(p => p.ProductID == id)
              .OrderBy(p => p.Price).Select(p => new ProductDTO
              {
                  ProductID = p.ProductID,
                  ProductName = p.ProductName,
                  Price = p.Price,
                  Description = p.Description,
                  Picture = p.Picture,
                  CateID = p.CateID,
                  CateName = p.Cate.CateName
              }).FirstOrDefaultAsync();


            if (product == null)
            {
                return NotFound("找不到產品資料");
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, Product product)
        {
            if (id != product.ProductID)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Product.Add(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductExists(product.ProductID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProduct", new { id = product.ProductID }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(string id)
        {
            return _context.Product.Any(e => e.ProductID == id);
        }
    }
}
