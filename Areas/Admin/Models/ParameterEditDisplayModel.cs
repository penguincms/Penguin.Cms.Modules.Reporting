using Penguin.Cms.Reporting;
using Penguin.Persistence.Database;

namespace Penguin.Cms.Modules.Reporting.Areas.Admin.Models
{
    public class ParameterEditDisplayModel
    {
        public ParameterInfo? ParameterInfo { get; set; }

        public SQLParameterInfo? SQLParameterInfo { get; set; }
    }
}