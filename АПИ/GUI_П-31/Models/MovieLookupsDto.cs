using System.Collections.Generic;

namespace GuiP31.Models
{
    public class MovieLookupsDto
    {
        public List<LookupItemDto> Directors { get; set; } = new List<LookupItemDto>();

        public List<LookupItemDto> Genres { get; set; } = new List<LookupItemDto>();

        public List<LookupItemDto> Actors { get; set; } = new List<LookupItemDto>();

        public List<LookupItemDto> Curators { get; set; } = new List<LookupItemDto>();
    }
}
