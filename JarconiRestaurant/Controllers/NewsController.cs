using JarconiRestaurant.Data;
using JarconiRestaurant.Domain.News;
using JarconiRestaurant.DTOs.News;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarconiRestaurant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase {
    private readonly AppDbContext _db;

    public NewsController(AppDbContext db) => _db = db;

    // PUBLIC: GET /api/news
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NewsVm>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20) {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.NewsPosts.AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.PublishedAtUtc);

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new NewsVm {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Excerpt = x.Excerpt,
                Content = x.Content,
                IsPublished = x.IsPublished,
                PublishedAtUtc = x.PublishedAtUtc,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToListAsync();

        return Ok(items);
    }

    // PUBLIC: GET /api/news/{slug}
    [HttpGet("{slug}")]
    public async Task<ActionResult<NewsVm>> GetBySlug([FromRoute] string slug) {
        var x = await _db.NewsPosts.AsNoTracking().FirstOrDefaultAsync(n => n.Slug == slug && n.IsPublished);
        if (x is null) return NotFound();

        return Ok(new NewsVm {
            Id = x.Id,
            Title = x.Title,
            Slug = x.Slug,
            Excerpt = x.Excerpt,
            Content = x.Content,
            IsPublished = x.IsPublished,
            PublishedAtUtc = x.PublishedAtUtc,
            CreatedAtUtc = x.CreatedAtUtc
        });
    }

    // ADMIN: POST /api/news
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNewsDto dto) {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(dto.Title)
            : dto.Slug.Trim().ToLowerInvariant();

        var exists = await _db.NewsPosts.AnyAsync(x => x.Slug == slug);
        if (exists) return Conflict("NEWS_SLUG_EXISTS");

        var post = new NewsPost {
            Title = dto.Title,
            Slug = slug,
            Excerpt = dto.Excerpt,
            Content = dto.Content,
            IsPublished = false,
            PublishedAtUtc = null
        };

        _db.NewsPosts.Add(post);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBySlug), new { slug = post.Slug }, new { post.Id, post.Slug });
    }

    // ADMIN: PUT /api/news/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateNewsDto dto) {
        var post = await _db.NewsPosts.FirstOrDefaultAsync(x => x.Id == id);
        if (post is null) return NotFound();

        post.Title = dto.Title;
        post.Excerpt = dto.Excerpt;
        post.Content = dto.Content;

        await _db.SaveChangesAsync();
        return Ok(new { post.Id, post.Title, post.Slug, post.IsPublished });
    }

    // ADMIN: PATCH /api/news/{id}/publish
    [HttpPatch("{id:int}/publish")]
    public async Task<IActionResult> Publish([FromRoute] int id) {
        var post = await _db.NewsPosts.FirstOrDefaultAsync(x => x.Id == id);
        if (post is null) return NotFound();

        post.IsPublished = true;
        post.PublishedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { post.Id, post.Slug, post.PublishedAtUtc });
    }

    // ADMIN: DELETE /api/news/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id) {
        var post = await _db.NewsPosts.FirstOrDefaultAsync(x => x.Id == id);
        if (post is null) return NotFound();

        _db.NewsPosts.Remove(post);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string GenerateSlug(string title) {
        var s = title.Trim().ToLowerInvariant();
        s = string.Concat(s.Select(c => char.IsLetterOrDigit(c) ? c : '-'));
        s = System.Text.RegularExpressions.Regex.Replace(s, "-{2,}", "-").Trim('-');
        return string.IsNullOrWhiteSpace(s) ? Guid.NewGuid().ToString("n")[..8] : s;
    }
}