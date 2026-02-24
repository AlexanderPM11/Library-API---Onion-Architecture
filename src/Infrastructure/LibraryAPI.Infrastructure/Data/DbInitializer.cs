using LibraryAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAPI.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LibraryDbContext context, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            // Apply migrations and create database if it doesn't exist
            await context.Database.MigrateAsync();

            await SeedRolesAndUsersAsync(roleManager, userManager);
            await SeedLibraryDataAsync(context);
        }

        private static async Task SeedRolesAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            // Seed Roles
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            var adminEmail = "admin@library.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Regular User
            var userEmail = "user@library.com";
            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var regularUser = new IdentityUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }
        }

        private static async Task SeedLibraryDataAsync(LibraryDbContext context)
        {
            // Seed Authors if none exist
            if (!await context.Authors.AnyAsync())
            {
                var authors = new List<Author>
                {
                    new Author { FirstName = "George", LastName = "Orwell", Biography = "English novelist, essayist, journalist and critic." },
                    new Author { FirstName = "J.K.", LastName = "Rowling", Biography = "British author, best known for the Harry Potter series." },
                    new Author { FirstName = "Isaac", LastName = "Asimov", Biography = "American writer and professor of biochemistry, known for science fiction." }
                };

                await context.Authors.AddRangeAsync(authors);
                await context.SaveChangesAsync();

                // Seed Books associated with these authors
                var fictionCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Fiction");
                var scienceCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Science");

                if (fictionCategory != null && scienceCategory != null)
                {
                    var books = new List<Book>
                    {
                        new Book 
                        { 
                            Title = "1984", 
                            Isbn = "9780451524935", 
                            Description = "Dystopian social science fiction novel.", 
                            PublicationYear = 1949, 
                            Stock = 10, 
                            CategoryId = fictionCategory.Id 
                        },
                        new Book 
                        { 
                            Title = "Foundation", 
                            Isbn = "9780553293357", 
                            Description = "Science fiction novel by Isaac Asimov.", 
                            PublicationYear = 1951, 
                            Stock = 5, 
                            CategoryId = scienceCategory.Id 
                        }
                    };

                    await context.Books.AddRangeAsync(books);
                    await context.SaveChangesAsync();

                    // Link Authors to Books (Many-to-Many)
                    var orwell = authors[0];
                    var asimov = authors[2];
                    var b1984 = books[0];
                    var bFoundation = books[1];

                    context.BookAuthors.AddRange(
                        new BookAuthor { BookId = b1984.Id, AuthorId = orwell.Id },
                        new BookAuthor { BookId = bFoundation.Id, AuthorId = asimov.Id }
                    );
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
