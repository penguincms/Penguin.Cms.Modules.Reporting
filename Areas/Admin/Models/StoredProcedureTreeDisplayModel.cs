using Penguin.Persistence.Database;
using System.Collections.Generic;

namespace Penguin.Cms.Modules.Reporting.Areas.Admin.Models
{
    public class StoredProcedureTreeDisplayModel
    {
        public string Name { get; set; } = string.Empty;

        public List<SQLParameterInfo> Parameters { get; } = new List<SQLParameterInfo>();
    }
}