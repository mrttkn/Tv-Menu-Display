using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MenuDisplayApp.Dtos;
using MenuDisplayApp.Models;
using MenuDisplayApp.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace MenuDisplayApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;


        public MenuController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("grouped")]
        public async Task<IActionResult> GetGroupedMenuItems()
        {
            // Veritabanından SQL sorgusuyla veri çek
            var rawMenuItems = await _context.MenuItems
                .FromSqlRaw(@"
            SELECT 
                gmg.MenuGroupID,
                gm.MenuItemID,
                gmg.MenuGroupText, 
                gm.MenuItemText, 
                CAST(gm.DefaultUnitPrice AS DECIMAL(10, 2)) as DefaultUnitPrice,
                gm.CustomField10
            FROM GlobalMenuItems gm
            INNER JOIN GlobalMenuGroups gmg ON gmg.MenuGroupID = gm.MenuGroupID
            WHERE gm.MenuItemActive = 1
            ORDER BY gmg.MenuGroupText, gm.MenuItemText
        ")
                .ToListAsync();

            // Veri gruplama ve DTO'ya dönüştürme
            var groupedMenuItems = rawMenuItems
                .GroupBy(item => new { item.MenuGroupID, item.MenuGroupText })
                .Select(group => new MenuGroupDto
                {
                    GroupID = group.Key.MenuGroupID,
                    GroupName = group.Key.MenuGroupText,
                    Items = group.Select(item => new MenuItemDto
                    {
                        MenuItemText = item.MenuItemText,
                        DefaultUnitPrice = item.DefaultUnitPrice
                    }).ToList()
                }).ToList();

            // JSON olarak istemciye döndür
            return Ok(groupedMenuItems);
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProductsByGroupIDs([FromQuery] List<int> groupIDs)
        {
            if (groupIDs == null || !groupIDs.Any())
            {
                return BadRequest("En az bir groupID göndermelisiniz.");
            }

            using var connection = new SqlConnection(_connectionString);

            const string query = @"
        SELECT 
            gm.MenuItemText, 
            gm.MenuItemID,
            CAST(gm.DefaultUnitPrice AS DECIMAL(10, 2)) AS DefaultUnitPrice,
            gm.MenuGroupID
        FROM GlobalMenuItems gm
        WHERE gm.MenuGroupID IN @GroupIDs AND gm.MenuItemActive = 1
        ORDER BY gm.MenuGroupID, gm.MenuItemText";

            var products = await connection.QueryAsync(query, new { GroupIDs = groupIDs });

            if (!products.Any())
            {
                return NotFound("Belirtilen kategorilere ait ürün bulunamadı.");
            }

            return Ok(products);
        }

        // Grup ID'ye göre ürünleri listeleme
        [HttpGet("grouped/{groupID}")]
        public async Task<IActionResult> GetProductsByGroup(int groupID)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                SELECT 
                    gm.MenuItemText, 
                    gm.MenuItemID,
                    CAST(gm.DefaultUnitPrice AS DECIMAL(10, 2)) AS DefaultUnitPrice,
                    gm.CustomField10
                FROM GlobalMenuItems gm
                WHERE gm.MenuGroupID = @groupID AND gm.MenuItemActive = 1
                ORDER BY gm.MenuItemText";

            var products = await connection.QueryAsync(query, new { groupID });

            if (!products.Any())
            {
                return NotFound("Bu kategoriye ait ürün bulunamadı.");
            }

            return Ok(products);
        }
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateMenuItems([FromBody] List<int> menuItemIDs)
        {
            if (menuItemIDs == null || !menuItemIDs.Any())
            {
                return BadRequest("Aktif edilecek en az bir MenuItemID gönderiniz.");
            }

            using var connection = new SqlConnection(_connectionString);

            const string query = @"
            UPDATE GlobalMenuItems 
            SET CustomField10 = '1'
            WHERE MenuItemID IN @MenuItemIDs";

            var rowsAffected = await connection.ExecuteAsync(query, new { MenuItemIDs = menuItemIDs });

            if (rowsAffected == 0)
            {
                return NotFound("Hiçbir ürün güncellenemedi.");
            }

            return Ok($"{rowsAffected} ürün aktif hale getirildi.");
        }

        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateMenuItems([FromBody] List<int> menuItemIDs)
        {
            if (menuItemIDs == null || !menuItemIDs.Any())
            {
                return BadRequest("Pasif edilecek en az bir MenuItemID gönderiniz.");
            }

            using var connection = new SqlConnection(_connectionString);

            const string query = @"
            UPDATE GlobalMenuItems 
            SET CustomField10 = '0'
            WHERE MenuItemID IN @MenuItemIDs";

            var rowsAffected = await connection.ExecuteAsync(query, new { MenuItemIDs = menuItemIDs });

            if (rowsAffected == 0)
            {
                return NotFound("Hiçbir ürün güncellenemedi.");
            }

            return Ok($"{rowsAffected} ürün pasif hale getirildi.");
        }

    }
}