using Penguin.Cms.Modules.Core.Models;
using Penguin.Persistence.Database.Serialization.Extensions.Meta;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;

namespace Penguin.Cms.Modules.Reporting.Areas.Admin.Models
{
    public class StoredProcedureDisplayModel
    {
        public string Name { get; set; } = string.Empty;
        public bool Optimized { get; set; }
        public DbMetaObject? Parameters { get; set; }

        public PagedListContainer<IMetaObject>? Results { get; set; }
    }
}