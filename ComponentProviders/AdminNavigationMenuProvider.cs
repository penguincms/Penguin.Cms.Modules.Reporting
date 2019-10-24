﻿using Penguin.Cms.Modules.Core.ComponentProviders;
using Penguin.Cms.Modules.Core.Navigation;
using Penguin.Navigation.Abstractions;
using Penguin.Security.Abstractions;
using Penguin.Security.Abstractions.Interfaces;
using System.Collections.Generic;
using SecurityRoles = Penguin.Security.Abstractions.Constants.RoleNames;

namespace Penguin.Cms.Modules.Reporting.ComponentProviders
{
    public class AdminNavigationMenuProvider : NavigationMenuProvider
    {
        public override INavigationMenu GenerateMenuTree()
        {
            return new NavigationMenu()
            {
                Name = "Admin",
                Text = "Admin",
                Children = new List<INavigationMenu>() {
                         new NavigationMenu()
                         {
                             Text = "Reporting",
                             Icon = "list",
                             Href = "/Admin/Reporting/Index",
                             Permissions = new List<ISecurityGroupPermission>()
                             {
                                 this.CreatePermission(SecurityRoles.SysAdmin, PermissionTypes.Read | PermissionTypes.Write)
                             }
                         }
                    }
            };
        }
    }
}