using Ecommerce_Utilidades;
using Ecommerce_Datos.Datos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            IServiceProvider services,
            IHostEnvironment environment,
            CancellationToken cancellationToken = default)
        {
            if (!environment.IsDevelopment())
            {
                return;
            }

            using IServiceScope scope = services.CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IConfiguration config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<IdentityUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger("SeedData");

            int timeoutSeconds = config.GetValue<int?>("SeedAdmin:OperationTimeoutSeconds") ?? 10;
            timeoutSeconds = Math.Clamp(timeoutSeconds, 5, 120);
            TimeSpan operationTimeout = TimeSpan.FromSeconds(timeoutSeconds);

            logger.LogInformation("Starting seed initialization with timeout of {TimeoutSeconds}s per operation.", timeoutSeconds);

            try
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                await EnsureRoleAsync(roleManager, WC.AdminRole, operationTimeout, logger, cancellationToken);
                await EnsureRoleAsync(roleManager, WC.ClienteRole, operationTimeout, logger, cancellationToken);

                string adminEmail = config["SeedAdmin:Email"] ?? "admin@ecommerce.local";
                string adminPassword = config["SeedAdmin:Password"] ?? "Admin123!";

                IdentityUser? existingUser = await WaitWithTimeoutAsync(
                    userManager.FindByEmailAsync(adminEmail),
                    operationTimeout,
                    $"Find admin user '{adminEmail}'",
                    cancellationToken);

                if (existingUser is null)
                {
                    IdentityUser adminUser = new()
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };

                    IdentityResult createResult = await WaitWithTimeoutAsync(
                        userManager.CreateAsync(adminUser, adminPassword),
                        operationTimeout,
                        "Create admin user",
                        cancellationToken);

                    if (!createResult.Succeeded)
                    {
                        logger.LogError("No se pudo crear el usuario admin de desarrollo: {Errors}", string.Join("; ", createResult.Errors.Select(e => e.Description)));
                        return;
                    }

                    existingUser = adminUser;
                    logger.LogInformation("Admin de desarrollo creado: {Email}", adminEmail);
                }

                bool adminRoleAssigned = await WaitWithTimeoutAsync(
                    userManager.IsInRoleAsync(existingUser, WC.AdminRole),
                    operationTimeout,
                    "Check admin role assignment",
                    cancellationToken);

                if (!adminRoleAssigned)
                {
                    await WaitWithTimeoutAsync(
                        userManager.AddToRoleAsync(existingUser, WC.AdminRole),
                        operationTimeout,
                        "Assign admin role",
                        cancellationToken);
                }

                logger.LogInformation("Seed initialization completed.");
            }
            catch (OperationCanceledException)
            {
                logger.LogError("Seed initialization canceled.");
            }
            catch (TimeoutException ex)
            {
                logger.LogError(ex, "Seed initialization timed out.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while executing seed initialization.");
            }
        }

        private static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName,
            TimeSpan operationTimeout,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            bool exists = await WaitWithTimeoutAsync(
                roleManager.RoleExistsAsync(roleName),
                operationTimeout,
                $"Check role '{roleName}' existence",
                cancellationToken);

            if (!exists)
            {
                IdentityResult result = await WaitWithTimeoutAsync(
                    roleManager.CreateAsync(new IdentityRole(roleName)),
                    operationTimeout,
                    $"Create role '{roleName}'",
                    cancellationToken);

                if (!result.Succeeded)
                {
                    logger.LogWarning("Role '{RoleName}' could not be created: {Errors}", roleName, string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task<T> WaitWithTimeoutAsync<T>(
            Task<T> task,
            TimeSpan timeout,
            string operationName,
            CancellationToken cancellationToken)
        {
            Task completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationToken));
            if (completedTask != task)
            {
                throw new TimeoutException($"Seed operation timed out: {operationName} ({timeout.TotalSeconds}s).");
            }

            return await task;
        }

        private static async Task WaitWithTimeoutAsync(
            Task task,
            TimeSpan timeout,
            string operationName,
            CancellationToken cancellationToken)
        {
            Task completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationToken));
            if (completedTask != task)
            {
                throw new TimeoutException($"Seed operation timed out: {operationName} ({timeout.TotalSeconds}s).");
            }

            await task;
        }
    }
}
