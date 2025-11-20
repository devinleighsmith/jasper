using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Db.Contexts;
using Scv.Db.Models;

namespace Scv.Db.Seeders;

public class QuickLinkSeeder(ILogger<QuickLinkSeeder> logger, IConfiguration config, IMapper mapper) : SeederBase<JasperDbContext>(logger)
{
    private readonly IConfiguration _config = config;
    private readonly IMapper _mapper = mapper;

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
                ParentName = quickLinkObj["ParentName"],
                IsMenu = bool.Parse(quickLinkObj["IsMenu"]),
                URL = quickLinkObj["URL"],
                Order = int.Parse(quickLinkObj["Order"]),
                JudgeId = quickLinkObj["JudgeId"]
            };
            quickLinks.Add(newQuickLink);
        }

        Logger.LogInformation("\tUpdating quick links...");

        foreach (var link in quickLinks)
        {
            var ql = await context.QuickLinks.AsQueryable().FirstOrDefaultAsync(ql => ql.Name == link.Name);
            if (ql == null)
            {
                Logger.LogInformation("\t{name} does not exist, adding it...", link.Name);
                await context.QuickLinks.AddAsync(link);
            }
            else
            {
                Logger.LogInformation("\tUpdating fields for {name}...", link.Name);

                // Use mapster instead
                
                // ql.URL = link.URL;
                // ql.Order = link.Order;
                // ql.ParentName = link.ParentName;
                // ql.IsMenu = link.IsMenu;
                // ql.JudgeId = link.JudgeId;
                _mapper.Map(link, ql);

            }
        }

        await context.SaveChangesAsync();
    }
}