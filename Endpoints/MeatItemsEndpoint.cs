using FreezerManager.Models;
using FreezerManager.Data;
using Microsoft.EntityFrameworkCore;

namespace FreezerManager.Endpoints
{
    public static class MeatItemEndpoints
    {
        public static void MapMeatItemEndpoints(this IEndpointRouteBuilder app)
        {
             app.MapGet("/api/MeatItems", async (AppDbContext db) => 
                    await db.MeatItems.ToListAsync());

            app.MapGet("api/MeatItems/{id}", async (int id, AppDbContext db) =>
                    await db.MeatItems.FindAsync(id) is MeatItem item ? Results.Ok(item) : Results.NotFound());

            app.MapPost("/api/meatitems", async (MeatItem meatItem, AppDbContext db) =>
            {
                db.MeatItems.Add(meatItem);
                await db.SaveChangesAsync();
                return Results.Created($"/api/meatitems/{meatItem.Id}", meatItem);
            });    

            //Update All, Decision: Only use for correcting errors?
            app.MapPut("/api/meatitems/{id}", async (int id, MeatItem updatedItem, AppDbContext db) =>
            {
               var meatItem = await db.MeatItems.FindAsync(id);
               if(meatItem is null) return Results.NotFound();

               meatItem.Type = updatedItem.Type;
               meatItem.Cut = updatedItem.Cut;
               meatItem.Weight = updatedItem.Weight;
               meatItem.Storage = updatedItem.Storage;

               await db.SaveChangesAsync();
               return Results.NoContent();

            });

            //Decision: Delete or change storange once used?
            app.MapDelete("api/meatitems/{id}", async (int id, AppDbContext db) =>
            { 
               var meatItem = await db.MeatItems.FindAsync(id);
               if(meatItem is null) return Results.NotFound();

               db.MeatItems.Remove(meatItem);
               await db.SaveChangesAsync();
               return Results.NoContent();
            });
        }
    }
}
