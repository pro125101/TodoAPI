﻿using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace TodoApi;

internal static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this RouteGroupBuilder group)
    {
        group.WithTags("Todos");

        group.MapGet("/", async (TodoDbContext db, UserId owner) =>
        {
            return await db.Todos.Where(todo => todo.OwnerId == owner.Id).ToListAsync();
        });

        group.MapGet("/{id}", async (TodoDbContext db, int id, UserId owner) =>
        {
            return await db.Todos.FindAsync(id) switch
            {
                Todo todo when todo.OwnerId == owner.Id => Results.Ok(todo),
                _ => Results.NotFound()
            };
        })
        .Produces<Todo>()
        .Produces(Status404NotFound);

        group.MapPost("/", async (TodoDbContext db, NewTodo newTodo, UserId owner) =>
        {
            var todo = new Todo
            {
                Title = newTodo.Title,
                OwnerId = owner.Id
            };

            await db.Todos.AddAsync(todo);
            await db.SaveChangesAsync();

            return Results.Created($"/todos/{todo.Id}", todo);
        })
       .Produces(Status201Created);

        group.MapPut("/{id}", async (TodoDbContext db, int id, Todo todo, UserId owner) =>
        {
            if (id != todo.Id)
            {
                return Results.BadRequest();
            }

            if (!await db.Todos.AnyAsync(x => x.Id == id && x.OwnerId != owner.Id))
            {
                return Results.NotFound();
            }

            db.Update(todo);
            await db.SaveChangesAsync();

            return Results.Ok();
        })
        .Produces(Status400BadRequest)
        .Produces(Status404NotFound)
        .Produces(Status200OK);

        group.MapDelete("/{id}", async (TodoDbContext db, int id, UserId owner) =>
        {
            var todo = await db.Todos.FindAsync(id);
            if (todo is null || todo.OwnerId != owner.Id)
            {
                return Results.NotFound();
            }

            db.Todos.Remove(todo);
            await db.SaveChangesAsync();

            return Results.Ok();
        })
        .Produces(Status400BadRequest)
        .Produces(Status404NotFound)
        .Produces(Status200OK);

        return group;
    }
}
