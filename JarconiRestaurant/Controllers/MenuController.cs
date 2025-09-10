using JarconiRestaurant.Data;
using JarconiRestaurant.Domain.Menu;
using JarconiRestaurant.DTOs.Menu;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarconiRestaurant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase {
    private readonly AppDbContext _db;

    public MenuController(AppDbContext db) => _db = db;

    // PUBLIC: GET /api/menu
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MenuVm>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? q = null, [FromQuery] int? categoryId = null) {
        var categories = await _db.MenuCategories
            .AsNoTracking()
            .Where(c => c.IsPublished)
            .OrderBy(c => c.SortOrder)
            .Select(c => new MenuVm {
                CategoryId = c.Id,
                CategoryName = c.Name,
                SortOrder = c.SortOrder,
                Items = c.Items
                    .Where(i => i.IsPublished && i.IsAvailable &&
                                (categoryId == null || i.CategoryId == categoryId) &&
                                (q == null || (EF.Functions.ILike(i.Title, $"%{q}%") || EF.Functions.ILike(i.Description!, $"%{q}%"))))
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new MenuItemVm {
                        Id = i.Id,
                        Title = i.Title,
                        Description = i.Description,
                        Price = i.Price,
                        IsAvailable = i.IsAvailable,
                        IsPublished = i.IsPublished,
                        SortOrder = i.SortOrder
                    })
            })
            .ToListAsync();

        return Ok(categories.Where(c => c.Items.Any())); // скрываем пустые опубликованные категории
    }

    // ADMIN: категории
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateMenuCategoryDto dto) {
        var c = new MenuCategory { Name = dto.Name, SortOrder = dto.SortOrder, IsPublished = true };
        _db.MenuCategories.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, new { c.Id, c.Name, c.SortOrder, c.IsPublished });
    }

    // ADMIN: позиции
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateMenuItemDto dto) {
        var exists = await _db.MenuCategories.AnyAsync(x => x.Id == dto.CategoryId);
        if (!exists) return BadRequest("CATEGORY_NOT_FOUND");

        var item = new MenuItem {
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            IsPublished = dto.IsPublished,
            SortOrder = dto.SortOrder
        };

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpGet("items/{id:int}")]
    public async Task<IActionResult> GetItem([FromRoute] int id) {
        var item = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItem([FromRoute] int id, [FromBody] UpdateMenuItemDto dto) {
        var item = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.Title = dto.Title;
        item.Description = dto.Description;
        item.Price = dto.Price;
        item.IsAvailable = dto.IsAvailable;
        item.IsPublished = dto.IsPublished;
        item.SortOrder = dto.SortOrder;
        item.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> DeleteItem([FromRoute] int id) {
        var item = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}