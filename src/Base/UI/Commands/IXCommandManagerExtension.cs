//*********************************************************************
//xCAD
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands.Structures;

namespace Xarial.XCad.UI.Commands
{
    public static class IXCommandManagerExtension
    {
        internal class EnumCommandSpec<TEnum> : CommandSpec
            where TEnum : Enum
        {
            internal TEnum Value { get; }

            internal EnumCommandSpec(TEnum value)
            {
                Value = value;
            }
        }

        public static IEnumCommandBar<TCmdEnum> AddCommandGroup<TCmdEnum>(this IXCommandManager cmdMgr)
            where TCmdEnum : Enum
        {
            int GetNextAvailableGroupId()
            {
                if (cmdMgr.CommandGroups.Any())
                {
                    return cmdMgr.CommandGroups.Max(g => g.Spec.Id) + 1;
                }
                else
                {
                    return 0;
                }
            }

            var barSpec = CreateCommandBar<TCmdEnum>(GetNextAvailableGroupId(), cmdMgr.CommandGroups.Select(c => c.Spec));

            var bar = cmdMgr.AddCommandGroup(barSpec);

            return new EnumCommandGroup<TCmdEnum>(bar);
        }

        private static EnumCommandSpec<TCmdEnum> CreateCommand<TCmdEnum>(TCmdEnum cmdEnum)
            where TCmdEnum : Enum
        {
            var cmd = new EnumCommandSpec<TCmdEnum>(cmdEnum);

            cmd.UserId = Convert.ToInt32(cmdEnum);

            if (!cmdEnum.TryGetAttribute<CommandItemInfoAttribute>(
                att =>
                {
                    cmd.HasMenu = att.HasMenu;
                    cmd.HasToolbar = att.HasToolbar;
                    cmd.SupportedWorkspace = att.SupportedWorkspaces;
                    cmd.HasTabBox = att.ShowInCommandTabBox;
                    cmd.TabBoxStyle = att.CommandTabBoxDisplayStyle;
                }))
            {
                cmd.HasMenu = true;
                cmd.HasToolbar = true;
                cmd.SupportedWorkspace = WorkspaceTypes_e.All;
                cmd.HasTabBox = false;
                cmd.TabBoxStyle = RibbonTabTextDisplay_e.TextBelow;
            }

            cmd.HasSpacer = cmdEnum.TryGetAttribute<CommandSpacerAttribute>(x => { });

            if (!cmdEnum.TryGetAttribute<DisplayNameAttribute>(
                att => cmd.Title = att.DisplayName))
            {
                cmd.Title = cmdEnum.ToString();
            }

            if (!cmdEnum.TryGetAttribute<DescriptionAttribute>(
                att => cmd.Tooltip = att.Description))
            {
                cmd.Tooltip = cmd.ToString();
            }

            if (!cmdEnum.TryGetAttribute<IconAttribute>(a => cmd.Icon = a.Icon))
            {
                cmd.Icon = Defaults.Icon;
            }

            return cmd;
        }

        private static EnumCommandGroupSpec CreateCommandBar<TCmdEnum>(int nextGroupId, IEnumerable<CommandGroupSpec> groups)
                                    where TCmdEnum : Enum
        {
            var cmdGroupType = typeof(TCmdEnum);

            var bar = new EnumCommandGroupSpec(cmdGroupType);

            CommandGroupInfoAttribute grpInfoAtt = null;

            if (cmdGroupType.TryGetAttribute<CommandGroupInfoAttribute>(x => grpInfoAtt = x))
            {
                if (grpInfoAtt.UserId != -1)
                {
                    bar.Id = grpInfoAtt.UserId;
                }
                else
                {
                    bar.Id = nextGroupId;
                }

                if (grpInfoAtt.ParentGroupType != null)
                {
                    var parentGrpSpec = groups.OfType<EnumCommandGroupSpec>()
                        .FirstOrDefault(g => g.CmdGrpEnumType == grpInfoAtt.ParentGroupType);

                    if (parentGrpSpec == null)
                    {
                        //TODO: create a specific exception
                        throw new NullReferenceException("Parent group is not created");
                    }

                    if (grpInfoAtt.ParentGroupType == cmdGroupType)
                    {
                        throw new InvalidOperationException("Group cannot be a parent of itself");
                    }

                    bar.Parent = parentGrpSpec;
                }
            }
            else
            {
                bar.Id = nextGroupId;
            }

            if (!cmdGroupType.TryGetAttribute<IconAttribute>(a => bar.Icon = a.Icon))
            {
                bar.Icon = Defaults.Icon;
            }

            if (!cmdGroupType.TryGetAttribute<DisplayNameAttribute>(a => bar.Title = a.DisplayName))
            {
                bar.Title = cmdGroupType.ToString();
            }

            if (!cmdGroupType.TryGetAttribute<DescriptionAttribute>(a => bar.Tooltip = a.Description))
            {
                bar.Tooltip = cmdGroupType.ToString();
            }

            bar.Commands = Enum.GetValues(cmdGroupType).Cast<TCmdEnum>().Select(
                c => CreateCommand(c)).ToArray();

            return bar;
        }
    }
}