//*********************************************************************
//xCAD
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.Reflection;
using Xarial.XCad.UI.Commands.Attributes;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands.Structures;
using Xarial.XCad.UI.Exceptions;
using Xarial.XCad.UI.Structures;

namespace Xarial.XCad.UI.Commands
{
    /// <summary>
    /// Additional methods for <see cref="IXCommandManager"/>
    /// </summary>
    public static class IXCommandManagerExtension
    {
        //TODO: think of a way to call Dispose on all wrapped enum groups

        internal class EnumCommandSpec<TEnum> : CommandSpec
            where TEnum : Enum
        {
            internal TEnum Value { get; }

            internal EnumCommandSpec(TEnum value, int userId) : base(userId)
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
            var id = GetEnumCommandGroupId(cmdMgr, typeof(TCmdEnum));

            var enumGrp = CreateEnumCommandGroup<TCmdEnum>(cmdMgr, GetEnumCommandGroupParent(cmdMgr, typeof(TCmdEnum)), id);

            var cmdGrp = cmdMgr.AddCommandGroup(enumGrp);

            return new EnumCommandGroup<TCmdEnum>(cmdGrp);
        }

        /// <summary>
        /// Adds context menu based on the enumeration
        /// </summary>
        /// <param name="owner">Context menu owner</param>
        ///<inheritdoc cref="AddCommandGroup{TCmdEnum}(IXCommandManager)"/>
        public static IEnumCommandGroup<TCmdEnum> AddContextMenu<TCmdEnum>(this IXCommandManager cmdMgr, SelectType_e? owner = null)
            where TCmdEnum : Enum
        {
            var id = GetEnumCommandGroupId(cmdMgr, typeof(TCmdEnum));

            var enumGrp = CreateEnumCommandGroup<TCmdEnum>(cmdMgr, GetEnumCommandGroupParent(cmdMgr, typeof(TCmdEnum)), id);

            var cmdGrp = cmdMgr.AddContextMenu(enumGrp, owner);

            return new EnumCommandGroup<TCmdEnum>(cmdGrp);
        }

        /// <summary>
        /// Creates spec from the enumeration
        /// </summary>
        /// <typeparam name="TCmdEnum">Enumeration type</typeparam>
        /// <param name="cmdMgr">Command group</param>
        /// <param name="parent">Parrent spec for this spec</param>
        /// <param name="id">Id or null to read the data from enum itself</param>
        /// <returns>Specification</returns>
        /// <exception cref="GroupUserIdNotAssignedException"/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static CommandGroupSpec CreateSpecFromEnum<TCmdEnum>(this IXCommandManager cmdMgr, CommandGroupSpec parent, int? id)
            where TCmdEnum : Enum
        {
            if (!id.HasValue)
            {
                if (TryGetUserAssignedGroupId(typeof(TCmdEnum), out int userId))
                {
                    id = userId;
                }
                else 
                {
                    throw new GroupUserIdNotAssignedException();
                }
            }

            return CreateEnumCommandGroup<TCmdEnum>(cmdMgr, parent, id.Value);
        }

        /// <param name="id">Id or -1 to automatically assign</param>
        private static EnumCommandGroupSpec CreateEnumCommandGroup<TCmdEnum>(IXCommandManager cmdMgr, CommandGroupSpec parent, int id)
                                    where TCmdEnum : Enum
        {
            var cmdGroupType = typeof(TCmdEnum);

            if (parent != null)
            {
                if (parent.Id == id)
                {
                    throw new ParentGroupCircularDependencyException($"{parent.Title} ({parent.Id})");
                }
            }

            var bar = new EnumCommandGroupSpec(cmdGroupType, id);
            bar.Parent = parent;

            bar.InitFromEnum<TCmdEnum>();

            var enumCmdUserId = 1;

            bar.Commands = Enum.GetValues(cmdGroupType).Cast<TCmdEnum>().Select(
                c => CreateEnumCommand(c, enumCmdUserId++)).ToArray();

            return bar;
        }

        private static CommandGroupSpec GetEnumCommandGroupParent(IXCommandManager cmdMgr, Type cmdGroupType)
        {
            CommandGroupSpec parent = null;

            CommandGroupParentAttribute grpParentAtt = null;

            if (cmdGroupType.TryGetAttribute<CommandGroupParentAttribute>(x => grpParentAtt = x))
            {
                var groups = cmdMgr.CommandGroups.Select(c => c.Spec);

                if (grpParentAtt.ParentGroupType != null)
                {
                    var parentGrpSpec = groups.OfType<EnumCommandGroupSpec>()
                        .FirstOrDefault(g => g.CmdGrpEnumType == grpParentAtt.ParentGroupType);

                    if (parentGrpSpec == null)
                    {
                        throw new ParentGroupNotFoundException(grpParentAtt.ParentGroupType.FullName, cmdGroupType.FullName);
                    }

                    if (grpParentAtt.ParentGroupType == cmdGroupType)
                    {
                        throw new ParentGroupCircularDependencyException(grpParentAtt.ParentGroupType.FullName);
                    }

                    parent = parentGrpSpec;
                }
                else
                {
                    var parentGrpSpec = groups.OfType<CommandGroupSpec>()
                        .FirstOrDefault(g => g.Id == grpParentAtt.ParentGroupUserId);

                    if (parentGrpSpec == null)
                    {
                        throw new ParentGroupNotFoundException(grpParentAtt.ParentGroupUserId.ToString(), cmdGroupType.FullName);
                    }
                    
                    parent = parentGrpSpec;
                }
            }

            return parent;
        }

        private static int GetEnumCommandGroupId(IXCommandManager cmdMgr, Type cmdGroupType)
        {
            if (!TryGetUserAssignedGroupId(cmdGroupType, out int id)) 
            {
                var nextGroupId = 1;

                if (cmdMgr.CommandGroups.Any())
                {
                    var usedIds = cmdMgr.CommandGroups.Select(g => g.Spec.Id).OrderBy(x => x).ToArray();

                    for (int i = 0; i < cmdMgr.CommandGroups.Length; i++)
                    {
                        if (usedIds[i] != nextGroupId)
                        {
                            break;
                        }

                        nextGroupId++;
                    }
                }

                id = nextGroupId;
            }
            
            return id;
        }

        private static bool TryGetUserAssignedGroupId(Type cmdGroupType, out int userId) 
        {
            CommandGroupInfoAttribute grpInfoAtt = null;

            if (cmdGroupType.TryGetAttribute<CommandGroupInfoAttribute>(x => grpInfoAtt = x))
            {
                if (grpInfoAtt.UserId != -1)
                {
                    userId = grpInfoAtt.UserId;
                    return true;
                }
            }

            userId = -1;
            return false;
        }

        private static EnumCommandSpec<TCmdEnum> CreateEnumCommand<TCmdEnum>(TCmdEnum cmdEnum, int userId)
            where TCmdEnum : Enum
        {
            var cmd = new EnumCommandSpec<TCmdEnum>(cmdEnum, userId);

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
    }
}