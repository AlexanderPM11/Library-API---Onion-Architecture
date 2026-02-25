using LibraryAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(LibraryDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            // Apply migrations and create database if it doesn't exist
            await context.Database.MigrateAsync();

            await SeedBranchesAsync(context);
            await SeedRolesAndUsersAsync(roleManager, userManager, context);
            await SeedLibraryDataAsync(context);
        }

        private static async Task SeedBranchesAsync(LibraryDbContext context)
        {
            try
            {
                if (!await context.Branches.AnyAsync())
                {
                    var branches = new List<Branch>
                    {
                        new Branch { Name = "Sucursal Central", Address = "Av. Principal 123", PhoneNumber = "555-0101" },
                        new Branch { Name = "Sucursal Norte", Address = "Calle Norte 456", PhoneNumber = "555-0202" },
                        new Branch { Name = "Sucursal Sur", Address = "Av. Sur 789", PhoneNumber = "555-0303" }
                    };

                    await context.Branches.AddRangeAsync(branches);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            
        }

        private static async Task SeedRolesAndUsersAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, LibraryDbContext context)
        {
            // Seed Roles
            string[] roles = { "SuperAdmin", "Admin", "Empleado" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var centralBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Sucursal Central");

            // Seed SuperAdmin User
            var superAdminEmail = "superadmin@library.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdmin == null)
            {
                superAdmin = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "SuperAdmin",
                    BranchId = null // Global access
                };

                await userManager.CreateAsync(superAdmin, "Super123!");
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(superAdmin);
                await userManager.ResetPasswordAsync(superAdmin, token, "Super123!");
            }

            if (!await userManager.IsInRoleAsync(superAdmin, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }

            // Seed Admin User (Ex BranchAdmin)
            var adminEmail = "admin@library.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null && centralBranch != null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Central",
                    LastName = "Admin",
                    BranchId = centralBranch.Id
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed Empleado User
            var empleadoEmail = "empleado@library.com";
            if (await userManager.FindByEmailAsync(empleadoEmail) == null && centralBranch != null)
            {
                var empleadoUser = new ApplicationUser
                {
                    UserName = empleadoEmail,
                    Email = empleadoEmail,
                    EmailConfirmed = true,
                    FirstName = "Library",
                    LastName = "Empleado",
                    BranchId = centralBranch.Id
                };

                var result = await userManager.CreateAsync(empleadoUser, "Empleado123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(empleadoUser, "Empleado");
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

                    // Assign BranchId to existing books if not set
                    var centralBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Sucursal Central");
                    if (centralBranch != null)
                    {
                        foreach (var book in books)
                        {
                            book.BranchId = centralBranch.Id;
                        }
                        await context.SaveChangesAsync();
                    }

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
