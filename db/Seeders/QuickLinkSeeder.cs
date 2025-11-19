using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Db.Contexts;
using Scv.Db.Models;

namespace Scv.Db.Seeders;

public class QuickLinkSeeder(ILogger<QuickLinkSeeder> logger, IConfiguration config) : SeederBase<JasperDbContext>(logger)
{
    private readonly IConfiguration _config = config;

    public override int Order => 5;

    protected async override Task ExecuteAsync(JasperDbContext context)
    {
        // Get default quick links from env variable
        var defaultQuickLinksJson = _config["DEFAULT_QUICK_LINKS"];
        // Remove this line later
        Console.WriteLine(defaultQuickLinksJson);
        var quickLinksObj = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(defaultQuickLinksJson);
        var quickLinks = new List<QuickLink>();
        var order = 0;

        foreach (var quickLinkObj in quickLinksObj)
        {
            var newQuickLink = new QuickLink
            {
                Name = quickLinkObj["Name"],
                ParentId = null,
                IsMenu = false,
                URL = quickLinkObj["URL"],
                Order = order++,
                JudgeId = null
            };
            quickLinks.Add(newQuickLink);
        }

        this.Logger.LogInformation("\tUpdating quick links...");

        foreach (var link in quickLinks)
        {
            var ql = await context.QuickLinks.AsQueryable().FirstOrDefaultAsync(ql => ql.Name == link.Name);
            if (ql == null)
            {
                this.Logger.LogInformation("\t{name} does not exist, adding it...", link.Name);
                await context.QuickLinks.AddAsync(link);
            }
            else
            {
                this.Logger.LogInformation("\tUpdating fields for {name}...", link.Name);
                ql.URL = link.URL;
                ql.Order = link.Order;
            }
        }

        await context.SaveChangesAsync();
    }
}