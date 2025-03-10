using System.Collections.Generic;
using System.Linq;

namespace Scv.Api.Models.Location;

public class Locations
{
    public List<Location> LocationList { get; private set; }

    private Locations(List<Location> locations)
    {
        LocationList = locations;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Locations"/> by merging two collections of <see cref="Location"/> objects.
    /// </summary>
    /// <param name="jcLocations">A collection of <see cref="Location"/> objects representing JC locations.</param>
    /// <param name="pcssLocations">A collection of <see cref="Location"/> objects representing PCSS locations.</param>
    /// <returns>A new instance of <see cref="Locations"/> containing the merged locations.</returns>
    /// <remarks>
    /// This method merges the JC and PCSS locations based on matching <see cref="Location.Code"/> and <see cref="Location.LocationId"/>.
    /// </remarks>
    public static Locations Create(ICollection<Location> jcLocations, ICollection<Location> pcssLocations)
    {
        var locations = jcLocations
            .Select(jc =>
            {
                var match = pcssLocations.SingleOrDefault(pcss => pcss.Code == jc.LocationId || pcss.Name == jc.Code);
                return Location.Create(jc.Name, jc.LocationId, match?.LocationId, jc.Active, match != null ? match.CourtRooms : jc.CourtRooms);
            })
            .Where(loc => loc.Active.GetValueOrDefault())
            .OrderBy(loc => loc.Name)
            .ToList();

        return new Locations(locations);
    }
}