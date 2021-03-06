﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Omu.ValueInjecter;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using VirtoCommerce.Platform.Data.Model;

namespace VirtoCommerce.Platform.Data.Security
{
    public static class SecurityConverters
    {
        public static RoleEntity ToDataModel(this Role source)
        {
            var result = new RoleEntity
            {
                Name = source.Name,
                Description = source.Description,
            };

            if (source.Id != null)
                result.Id = source.Id;

            result.RolePermissions = new NullCollection<RolePermissionEntity>();

            if (source.Permissions != null)
            {
                result.RolePermissions = new ObservableCollection<RolePermissionEntity>(source.Permissions.Select(p => new RolePermissionEntity { PermissionId = p.Id }));
            }

            return result;
        }

        public static PermissionEntity ToDataModel(this Permission source)
        {
            var result = new PermissionEntity
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description
            };

            return result;
        }

        public static Role ToCoreModel(this RoleEntity source)
        {
            var result = new Role
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
            };

            if (source.RolePermissions != null)
            {
                result.Permissions = source.RolePermissions.Select(rp => rp.Permission.ToCoreModel()).ToArray();
            }

            return result;
        }

        public static Permission ToCoreModel(this PermissionEntity source)
        {
            var result = new Permission
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
            };

            return result;
        }

        public static Permission ToCoreModel(this ModulePermission source, string moduleId, string groupName)
        {
            return new Permission
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                ModuleId = moduleId,
                GroupName = groupName,
            };
        }

        public static void Patch(this RoleEntity source, RoleEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var patchInjection = new PatchInjection<RoleEntity>(x => x.Name, x => x.Description);
            target.InjectFrom(patchInjection, source);

            if (!source.RolePermissions.IsNullCollection())
            {
                var comparer = AnonymousComparer.Create((RolePermissionEntity rp) => rp.PermissionId);
                source.RolePermissions.Patch(target.RolePermissions, comparer, (sourceItem, targetItem) => { });
            }
        }

        public static void Patch(this PermissionEntity source, PermissionEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var patchInjection = new PatchInjection<PermissionEntity>(x => x.Name, x => x.Description);
            target.InjectFrom(patchInjection, source);
        }

        public static void Patch(this RoleAssignmentEntity source, RoleAssignmentEntity target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            var patchInjection = new PatchInjection<RoleAssignmentEntity>(x => x.RoleId, x => x.AccountId);
            target.InjectFrom(patchInjection, source);
        }
    }
}
