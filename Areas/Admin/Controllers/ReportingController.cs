using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Penguin.Cms.Modules.Admin.Areas.Admin.Controllers;
using Penguin.Cms.Modules.Core.Models;
using Penguin.Cms.Modules.Reporting.Areas.Admin.Models;
using Penguin.Cms.Modules.Reporting.Constants.Strings;
using Penguin.Cms.Reporting;
using Penguin.Cms.Reporting.Extensions;
using Penguin.Configuration.Abstractions.Extensions;
using Penguin.Configuration.Abstractions.Interfaces;
using Penguin.Persistence.Abstractions.Attributes.Validation;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Database;
using Penguin.Persistence.Database.Extensions;
using Penguin.Persistence.Database.Objects;
using Penguin.Persistence.Database.Serialization.Extensions;
using Penguin.Persistence.Database.Serialization.Extensions.Meta;
using Penguin.Reflection.Dynamic;
using Penguin.Reflection.Serialization.Abstractions.Interfaces;
using Penguin.Reflection.Serialization.Abstractions.Wrappers;
using Penguin.Reflection.Serialization.Constructors;
using Penguin.Reflection.Serialization.Objects;
using Penguin.Security.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Penguin.Cms.Modules.Reporting.Areas.Admin.Controllers
{
    public class ReportingController : AdminController
    {
        private readonly DatabaseInstance ReportingDatabase;
        protected IProvideConfigurations ConfigurationService { get; set; }

        protected IRepository<ParameterInfo> ReportingParameterRepository { get; set; }

        public ReportingController(IProvideConfigurations configurationService, IRepository<ParameterInfo> reportingParameterRepository, System.IServiceProvider serviceProvider, IUserSession userSession) : base(serviceProvider, userSession)
        {
            this.ConfigurationService = configurationService;
            this.ReportingDatabase = new DatabaseInstance(this.ConfigurationService.ConnectionStringOrConfiguration(ConfigurationNames.CONNECTION_STRINGS_REPORTING));
            this.ReportingParameterRepository = reportingParameterRepository;
        }

        [HttpGet]
        public ActionResult EditParameter(string Name, string Parameter)
        {
            ParameterEditDisplayModel model = new ParameterEditDisplayModel()
            {
                ParameterInfo = this.ReportingParameterRepository.GetByProcedureAndName(Name, Parameter) ?? new ParameterInfo()
                {
                    ParameterName = Parameter,
                    ProcedureName = Name
                },
                SQLParameterInfo = this.ReportingDatabase.GetParameters(Name).First(p => p.PARAMETER_NAME == Parameter)
            };

            return this.View(model);
        }

        [HttpPost]
        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        public ActionResult EditParameter(ParameterEditDisplayModel model)
        {
            if (model is null)
            {
                throw new System.ArgumentNullException(nameof(model));
            }

            if (model.ParameterInfo is null)
            {
                throw new NullReferenceException("ParameterInfo can not be null");
            }

            using (this.ReportingParameterRepository.WriteContext())
            {
                model.SQLParameterInfo = this.ReportingDatabase.GetParameters(model.ParameterInfo.ProcedureName).First(p => p.PARAMETER_NAME == model.ParameterInfo.ParameterName);

                ParameterInfo existing = this.ReportingParameterRepository.GetByProcedureAndName(model.ParameterInfo.ProcedureName, model.ParameterInfo.ParameterName);

                if (existing is null)
                {
                    existing = model.ParameterInfo;
                }
                else
                {
                    existing.MinValue = model.ParameterInfo.MinValue;
                    existing.MaxValue = model.ParameterInfo.MaxValue;
                    existing.Default = model.ParameterInfo.Default;
                }

                this.ReportingParameterRepository.AddOrUpdate(existing);
            }

            return this.View(model);
        }

        public string? GetParamConstraint(ParameterConstraint source, System.Type type)
        {
            if (source == null || !source.Enabled)
            {
                return null;
            }

            if (source.Type == ParameterConstraint.ConstraintType.Static)
            {
                return source.Value;
            }
            else if (source.Type == ParameterConstraint.ConstraintType.SQL)
            {
                return this.ReportingDatabase.ExecuteToTable(source.Value).GetSingle<string>();
            }
            else
            {
                return CodeGenerator.Execute(source.Value, type)?.ToString();
            }
        }

        public ActionResult RunStoredProcedure(string Name, string json, int count = 20, int page = 0)
        {
            List<SQLParameterInfo> parameters = this.ReportingDatabase.GetParameters(Name);

            DbMetaObject toRender = parameters.ToMetaObject();

            StoredProcedureDisplayModel model = new StoredProcedureDisplayModel
            {
                Name = Name,
                Parameters = toRender,
                Optimized = parameters.Any(p => p.PARAMETER_NAME == "count") && parameters.Any(p => p.PARAMETER_NAME == "page")
            };

            if (parameters.Any())
            {
                JObject jObject = JObject.Parse(json);

                foreach (JProperty jtok in jObject.Properties())
                {
                    model.Parameters[jtok.Name].Value = jtok.Value.ToString();
                }
            }

            model.Results = new PagedListContainer<IMetaObject>()
            {
                Count = count,
                Page = page
            };

            if (model.Parameters.HasProperty("@page") && model.Parameters.HasProperty("@count"))
            {
                model.Parameters["@page"].Value = $"{page}";
                model.Parameters["@count"].Value = $"{count}";
                model.Results.Items.AddRange(this.ReportingDatabase.ExecuteStoredProcedureToTable(Name, model.Parameters.ToSqlParameters()).ToMetaObject().Cast<IMetaObject>());
            }
            else
            {
                List<IMetaObject> results = this.ReportingDatabase.ExecuteStoredProcedureToTable(Name, model.Parameters.ToSqlParameters()).ToMetaObject().Cast<IMetaObject>().ToList();
                model.Results.TotalCount = results.Count;
                model.Results.Items.AddRange(results.Skip(page * count).Take(count));
            }

            return this.View(model);
        }

        [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
        public ActionResult StoredProcedure(string Name)
        {
            List<SQLParameterInfo> parameters = this.ReportingDatabase.GetParameters(Name);

            DbMetaObject toRender = parameters.ToMetaObject();

            foreach (SQLParameterInfo thisParam in parameters)
            {
                ParameterInfo DBInstance = this.ReportingParameterRepository.GetByProcedureAndName(Name, thisParam.PARAMETER_NAME);
                System.Type netType = TypeConverter.ToNetType(thisParam.DATA_TYPE);

                if (DBInstance != null)
                {
                    if (DBInstance.Default != null)
                    {
                        toRender[thisParam.PARAMETER_NAME].Value = this.GetParamConstraint(DBInstance.Default, netType);
                    }

                    if (DBInstance.MinValue is null)
                    {
                        throw new NullReferenceException("DbInstance MinValue can not be null");
                    }

                    if (DBInstance.MaxValue is null)
                    {
                        throw new NullReferenceException("DbInstance MaxValue can not be null");
                    }

                    if (DBInstance.MinValue.Enabled || DBInstance.MaxValue.Enabled)
                    {
                        string? Min = null;
                        string? Max = null;

                        if (DBInstance.MinValue.Enabled)
                        {
                            Min = this.GetParamConstraint(DBInstance.MinValue, netType);
                        }

                        if (DBInstance.MaxValue.Enabled)
                        {
                            Max = this.GetParamConstraint(DBInstance.MaxValue, netType);
                        }

                        RangeAttribute range = new RangeAttribute(netType, Min, Max);

                        MetaConstructor c = new MetaConstructor();

                        DbMetaObject instance = new DbMetaObject()
                        {
                            Properties = new List<DbMetaObject>()
                            {
                                new DbMetaObject()
                                {
                                    Value = range.Minimum.ToString(),
                                    Property = new DbMetaProperty()
                                }
                            }
                        };

                        MetaAttributeHolder toAdd = new MetaAttributeHolder(range, false);

                        instance.Type = toAdd.Type;

                        List<IMetaAttribute> existingAttributes = toRender[thisParam.PARAMETER_NAME].Property.Attributes.ToList();

                        existingAttributes.Add(toAdd);

                        toRender[thisParam.PARAMETER_NAME].Property.Attributes = existingAttributes;
                    }
                }
            }

            StoredProcedureDisplayModel model = new StoredProcedureDisplayModel
            {
                Name = Name,
                Parameters = toRender,
                Optimized = parameters.Any(p => p.PARAMETER_NAME == "count") && parameters.Any(p => p.PARAMETER_NAME == "page")
            };

            return this.View(model);
        }
    }
}