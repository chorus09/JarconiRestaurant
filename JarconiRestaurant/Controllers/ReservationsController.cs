using System.ComponentModel.DataAnnotations;
using JarconiRestaurant.Data;
using JarconiRestaurant.Domain.Enums;
using JarconiRestaurant.Domain.Reservations;
using JarconiRestaurant.DTOs.Reservations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JarconiRestaurant.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase {
    private readonly AppDbContext _db;

    public ReservationsController(AppDbContext db) => _db = db;

    // Вспомогалка: пересечение интервалов
    private static bool Overlaps(DateTime aStart, int aDur, DateTime bStart, int bDur) {
        var aEnd = aStart.AddMinutes(aDur);
        var bEnd = bStart.AddMinutes(bDur);
        return aStart < bEnd && bStart < aEnd;
    }

    // POST /api/reservations
    [HttpPost]
    public async Task<ActionResult<ReservationVm>> Create([FromBody] CreateReservationDto dto) {
        if (dto.DateTimeStartUtc <= DateTime.UtcNow)
            return BadRequest("RESERVATION_PAST_DATETIME");

        if (dto.DurationMin is not (60 or 90 or 120))
            return BadRequest("RESERVATION_DURATION_INVALID");

        // конфликты по столику
        var conflicts = await _db.Reservations
            .Where(r => r.TableNumber == dto.TableNumber &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
            .ToListAsync();

        if (conflicts.Any(r => Overlaps(dto.DateTimeStartUtc, dto.DurationMin, r.DateTimeStartUtc, r.DurationMin)))
            return Conflict("RESERVATION_CONFLICT");

        var entity = new Reservation {
            UserId = dto.UserId,             // NOTE: потом возьмём из JWT
            TableNumber = dto.TableNumber,
            DateTimeStartUtc = dto.DateTimeStartUtc,
            DurationMin = dto.DurationMin,
            PartySize = dto.PartySize,
            Comment = dto.Comment,
            Status = ReservationStatus.Pending
        };

        _db.Reservations.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToVm(entity));
    }

    // GET /api/reservations/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationVm>> GetById([FromRoute] int id) {
        var r = await _db.Reservations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return r is null ? NotFound() : Ok(ToVm(r));
    }

    // GET /api/reservations/by-user/{userId}
    // временно; после JWT будет /my
    [HttpGet("by-user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<ReservationVm>>> ListByUser([FromRoute] int userId) {
        var list = await _db.Reservations.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.DateTimeStartUtc)
            .ToListAsync();

        return Ok(list.Select(ToVm));
    }

    // PUT /api/reservations/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReservationVm>> Update([FromRoute] int id, [FromBody] UpdateReservationDto dto) {
        var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();

        // дедлайн (2 часа) — простой пример
        if (r.DateTimeStartUtc - DateTime.UtcNow < TimeSpan.FromHours(2))
            return BadRequest("RESERVATION_DEADLINE_EXCEEDED");

        // конфликты
        var sameTable = await _db.Reservations
            .Where(x => x.Id != id &&
                        x.TableNumber == dto.TableNumber &&
                        (x.Status == ReservationStatus.Pending || x.Status == ReservationStatus.Confirmed))
            .ToListAsync();

        if (sameTable.Any(x => Overlaps(dto.DateTimeStartUtc, dto.DurationMin, x.DateTimeStartUtc, x.DurationMin)))
            return Conflict("RESERVATION_CONFLICT");

        r.TableNumber = dto.TableNumber;
        r.DateTimeStartUtc = dto.DateTimeStartUtc;
        r.DurationMin = dto.DurationMin;
        r.PartySize = dto.PartySize;
        r.Comment = dto.Comment;

        // при изменении можно сбросить в Pending (если был Confirmed)
        if (r.Status == ReservationStatus.Confirmed)
            r.Status = ReservationStatus.Pending;

        await _db.SaveChangesAsync();
        return Ok(ToVm(r));
    }

    // DELETE /api/reservations/{id}  => Cancel
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel([FromRoute] int id) {
        var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();

        if (r.DateTimeStartUtc - DateTime.UtcNow < TimeSpan.FromHours(2))
            return BadRequest("RESERVATION_DEADLINE_EXCEEDED");

        r.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /api/reservations/{id}/confirm  (админский сценарий)
    [HttpPatch("{id:int}/confirm")]
    public async Task<IActionResult> Confirm([FromRoute] int id) {
        var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return NotFound();

        r.Status = ReservationStatus.Confirmed;
        await _db.SaveChangesAsync();
        return Ok(ToVm(r));
    }

    private static ReservationVm ToVm(Reservation r) => new() {
        Id = r.Id,
        UserId = r.UserId,
        TableNumber = r.TableNumber,
        DateTimeStartUtc = r.DateTimeStartUtc,
        DurationMin = r.DurationMin,
        PartySize = r.PartySize,
        Comment = r.Comment,
        Status = r.Status,
        CreatedAtUtc = r.CreatedAtUtc
    };
}