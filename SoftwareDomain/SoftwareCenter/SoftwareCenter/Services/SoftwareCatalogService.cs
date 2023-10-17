using Microsoft.EntityFrameworkCore;

using SoftwareCenter.Data;
using SoftwareCenter.Models;

namespace SoftwareCenter.Services;

public class SoftwareCatalogService
{
    private readonly SoftwareDataContext _context;
    private readonly IPublishSoftwareMessages _publisher;

    public SoftwareCatalogService(SoftwareDataContext context, IPublishSoftwareMessages publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public IQueryable<SoftwareInventoryItemEntity> GetActiveTitles() => _context.GetActiveTitles();

    public async Task AddTitleAsync(TitleCreateModel request)
    {
        var titleToAdd = SoftwareInventoryItemEntity.From(request);
        _context.Titles.Add(titleToAdd);
        // call the portal and let them know there is new software
        await _context.SaveChangesAsync(); // save it to the local database
        await _publisher.PublishNewTitleAsync(titleToAdd); // publishing it to the broker.
    }

    public async Task RetireTitleAsync(int id)
    {

        // call the portal and let them know there is new software
        var title = await GetActiveTitles().SingleOrDefaultAsync(t => t.Id == id);
        if (title is null)
        {
            throw new ArgumentOutOfRangeException(nameof(RetireTitleAsync), "Title Not In Inventory to Retire");
        }
        else
        {
            title.Retired = true;
            await _context.SaveChangesAsync();


            await _publisher.RetireSoftwareTitleAsync(title);
        }
    }
}
