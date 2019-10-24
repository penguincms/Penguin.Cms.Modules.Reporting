using Penguin.Cms.Modules.Core.Models;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;
using Penguin.Reflection.Serialization.Objects;

namespace Penguin.Cms.Modules.Reporting.Areas.Admin.Models
{
    public class StoredProcedureDisplayModel
    {
        public string Name { get; set; } = string.Empty;
        public bool Optimized { get; set; }
        public MetaObject? Parameters { get; set; }

        public PagedListContainer<IMetaObject>? Results { get; set; }
    }
}