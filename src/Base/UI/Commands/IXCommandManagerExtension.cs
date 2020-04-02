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
using Xarial.XCad.UI.Structures;

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

        /// <summary>
        /// Adds command group based on the enumeration where each enumeration field represents the command button
        /// </summary>
        /// <typeparam name="TCmdEnum">Enumeration with commands</typeparam>
        /// <param name="cmdMgr">Command manager</param>
        /// <returns>Created command group</returns>
        /// <remarks>Decorate enumeration and fields with <see cref="TitleAttribute"/>, <see cref="IconAttribute"/>, <see cref="DescriptionAttribute"/>, <see cref="CommandItemInfoAttribute"/> to customized look and feel of commands</remarks>
        public static IEnumCommandGroup<TCmdEnum> AddCommandGroup<TCmdEnum>(this IXCommandManager cmdMgr)
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
                cmd.HasTabBox = true;
                cmd.TabBoxStyle = RibbonTabTextDisplay_e.TextBelow;
            }

            cmd.HasSpacer = cmdEnum.TryGetAttribute<CommandSpacerAttribute>(x => { });

            cmd.InitFromEnum(cmdEnum);

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

            bar.InitFromEnum<TCmdEnum>();

            bar.Commands = Enum.GetValues(cmdGroupType).Cast<TCmdEnum>().Select(
                c => CreateCommand(c)).ToArray();

            return bar;
        }
    }
}