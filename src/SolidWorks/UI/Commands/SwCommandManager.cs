//*********************************************************************
//xCAD
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Enums;
using Xarial.XCad.SolidWorks.Exceptions;
using Xarial.XCad.SolidWorks.Services;
using Xarial.XCad.SolidWorks.UI.Commands.Exceptions;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Enums;
using Xarial.XCad.SolidWorks.UI.Commands.Toolkit.Structures;
using Xarial.XCad.SolidWorks.Utils;
using Xarial.XCad.UI;
using Xarial.XCad.UI.Commands;
using Xarial.XCad.UI.Commands.Enums;
using Xarial.XCad.UI.Commands.Structures;
using Xarial.XCad.Utils.Diagnostics;
using Xarial.XCad.Toolkit;
using Xarial.XCad.SolidWorks.Enums;
using System.Collections.Specialized;

namespace Xarial.XCad.SolidWorks.UI.Commands
{
    public interface ISwCommandManager : IXCommandManager, IDisposable
    {
        /// <summary>
        /// Pointer to command group which holding the add-in commands
        /// </summary>
        ICommandManager CmdMgr { get; }
    }

    /// <inheritdoc/>
    internal class SwCommandManager : ISwCommandManager
    {
        private class TabCommandInfo
        {
            internal int CommandId { get; }
            internal RibbonTabTextDisplay_e TextStyle { get; }

            internal TabCommandInfo(int commandId, RibbonTabTextDisplay_e textStyle)
            {
                CommandId = commandId;
                TextStyle = textStyle;
            }
        }

        private class TabCommandGroupInfo
        {
            internal SwCommandGroup Group { get; }
            internal List<TabCommandInfo> Commands { get; }

            internal TabCommandGroupInfo(SwCommandGroup group)
            {
                Group = group;
                Commands = new List<TabCommandInfo>();
            }
        }

        private class CommandInfo
        {
            internal SwCommandGroup Group { get; }
            internal CommandSpec Spec { get; }
            internal int CommandId { get; }

            internal CommandInfo(CommandSpec spec, SwCommandGroup parent, int commandId)
            {
                Spec = spec;
                Group = parent;
                CommandId = commandId;
            }
        }

        private class TabInfo
        {
            internal string TabName { get; }
            internal List<SwCommandGroup> CommandGroups { get; }

            internal TabInfo(string tabName) 
            {
                TabName = tabName;
                CommandGroups = new List<SwCommandGroup>();
            }
        }

        private const string SUB_GROUP_SEPARATOR = "\\";

        private readonly ISwApplication m_App;

        private readonly List<SwCommandGroup> m_CommandBars;

        private readonly Dictionary<string, CommandInfo> m_Commands;

        private readonly IXLogger m_Logger;

        /// <inheritdoc/>
        public ICommandManager CmdMgr { get; private set; }

        public IXCommandGroup[] CommandGroups => m_CommandBars.ToArray();

        private readonly IServiceProvider m_SvcProvider;

        private readonly ICommandGroupTabConfigurer m_TabConfigurer;

        internal SwCommandManager(ISwApplication app, int addinCookie, IServiceProvider svcProvider)
        {
            m_App = app;

            CmdMgr = m_App.Sw.GetCommandManager(addinCookie);

            m_SvcProvider = svcProvider;

            m_Logger = svcProvider.GetService<IXLogger>();
            m_TabConfigurer = svcProvider.GetService<ICommandGroupTabConfigurer>();
            m_Commands = new Dictionary<string, CommandInfo>();
            m_CommandBars = new List<SwCommandGroup>();
        }

        public IXCommandGroup AddCommandGroup(CommandGroupSpec cmdBar)
            => AddCommandGroupOrContextMenu(cmdBar, false, null);

        public IXCommandGroup AddContextMenu(CommandGroupSpec cmdBar, SelectType_e? owner)
        {
            swSelectType_e? selType = null;
            
            if (owner.HasValue) 
            {
                selType = (swSelectType_e)owner;
            }

            return AddCommandGroupOrContextMenu(cmdBar, true, selType);
        }

        internal SwCommandGroup AddCommandGroupOrContextMenu(CommandGroupSpec cmdBar,
            bool isContextMenu, swSelectType_e? contextMenuSelectType)
        {
            m_Logger.Log($"Creating command group: {cmdBar.Id}", LoggerMessageSeverity_e.Debug);

            if (m_CommandBars.FirstOrDefault(b => b.Spec.Id == cmdBar.Id) != null)
            {
                throw new GroupIdAlreadyExistsException(cmdBar);
            }

            var title = GetMenuPath(cmdBar);

            var cmdGroup = CreateCommandGroup(cmdBar.Id, title, cmdBar.Tooltip,
                cmdBar.Commands.Select(c => c.UserId).ToArray(), isContextMenu,
                contextMenuSelectType);

            var iconsConv = m_SvcProvider.GetService<IIconsCreator>();

            using (var mainIcon = CreateMainIcon(cmdBar, iconsConv))
            {
                using (var toolbarIcons = CreateToolbarIcons(cmdBar, iconsConv))
                {
                    SetCommandGroupIcons(cmdGroup, mainIcon, toolbarIcons);
                    
                    var bar = new SwCommandGroup(m_App, cmdBar, cmdGroup, isContextMenu);

                    CreateCommandItems(bar);

                    m_CommandBars.Add(bar);

                    return bar;
                }
            }
        }

        internal void HandleCommandClick(string cmdId)
        {
            m_Logger.Log($"Command clicked: {cmdId}", LoggerMessageSeverity_e.Debug);

            CommandInfo cmd;

            if (m_Commands.TryGetValue(cmdId, out cmd))
            {
                cmd.Group.RaiseCommandClick(cmd.Spec);
            }
            else
            {
                Debug.Assert(false, "All callbacks must be registered");
            }
        }

        internal int HandleCommandEnable(string cmdId)
        {
            CommandInfo cmd;

            if (m_Commands.TryGetValue(cmdId, out cmd))
            {
                return (int)cmd.Group.RaiseCommandEnable(cmd.Spec);
            }
            else
            {
                Debug.Assert(false, "All callbacks must be registered");
            }

            return (int)CommandItemEnableState_e.DeselectDisable;
        }

        internal void TryBuildCommandTabs()
        {
            try
            {
                var tabsData = GroupCommandsByTabs();

                foreach (var tabData in tabsData)
                {
                    TryCreateTab(tabData);
                }
            }
            catch (Exception ex) 
            {
                m_Logger.Log(ex);
            }
        }

        private CommandGroup CreateCommandGroup(int groupId, string title, string toolTip,
            int[] knownCmdIDs, bool isContextMenu, swSelectType_e? contextMenuSelectType)
        {
            int cmdGroupErr = 0;

            object registryIDs;

            var isChanged = false;

            if (CmdMgr.GetGroupDataFromRegistry(groupId, out registryIDs))
            {
                m_Logger.Log("Commands cached in the registry", LoggerMessageSeverity_e.Debug);

                isChanged = !CompareIDs(registryIDs as int[], knownCmdIDs);
            }

            m_Logger.Log($"Command ids changed: {isChanged}", LoggerMessageSeverity_e.Debug);

            CommandGroup cmdGroup;

            if (isContextMenu)
            {
                cmdGroup = CmdMgr.AddContextMenu(groupId, title);
                if (contextMenuSelectType.HasValue)
                {
                    cmdGroup.SelectType = (int)contextMenuSelectType;
                }
            }
            else
            {
                cmdGroup = CmdMgr.CreateCommandGroup2(groupId, title, toolTip,
                    toolTip, -1, isChanged, ref cmdGroupErr);

                m_Logger.Log($"Command group creation result: {(swCreateCommandGroupErrors)cmdGroupErr}", LoggerMessageSeverity_e.Debug);

                Debug.Assert(cmdGroupErr == (int)swCreateCommandGroupErrors.swCreateCommandGroup_Success);
            }

            return cmdGroup;
        }

        private void CreateCommandItems(SwCommandGroup parentGroup)
        {
            var cmdGrpSpec = parentGroup.Spec;
            var cmdGrp = parentGroup.CommandGroup;
            int groupId = cmdGrpSpec.Id;
            var cmds = cmdGrpSpec.Commands;

            var dupIds = cmds.Where(c => c.UserId > 0).GroupBy(c => c.UserId).Where(g => g.Count() > 1).ToArray();

            if (dupIds.Any()) 
            {
                throw new DuplicateCommandUserIdsException(cmdGrpSpec.Title, groupId, dupIds.Select(x => x.Key).ToArray());
            }

            var createdCmds = new List<Tuple<CommandSpec, int, string>>();

            var callbackMethodName = nameof(SwAddInEx.OnCommandClick);
            var enableMethodName = nameof(SwAddInEx.OnCommandEnable);

            for (int i = 0; i < cmds.Length; i++)
            {
                var cmd = cmds[i];

                swCommandItemType_e menuToolbarOpts = 0;

                if (cmd.HasMenu)
                {
                    menuToolbarOpts |= swCommandItemType_e.swMenuItem;
                }

                if (cmd.HasToolbar)
                {
                    menuToolbarOpts |= swCommandItemType_e.swToolbarItem;
                }

                if (menuToolbarOpts == 0)
                {
                    throw new InvalidMenuToolbarOptionsException(cmd);
                }

                var cmdName = $"{groupId}.{cmd.UserId}";

                var callbackFunc = $"{callbackMethodName}({cmdName})";
                var enableFunc = $"{enableMethodName}({cmdName})";

                if (cmd.HasSpacer)
                {
                    cmdGrp.AddSpacer2(-1, (int)menuToolbarOpts);
                }

                var cmdIndex = cmdGrp.AddCommandItem2(cmd.Title, -1, cmd.Tooltip,
                    cmd.Title, i, callbackFunc, enableFunc, cmd.UserId,
                    (int)menuToolbarOpts);

                createdCmds.Add(new Tuple<CommandSpec, int, string>(cmd, cmdIndex, cmdName));

                m_Logger.Log($"Created command {cmd.Title}:{cmdIndex} for {cmd.UserId}", LoggerMessageSeverity_e.Debug);
            }

            cmdGrp.HasToolbar = cmds.Any(c => c.HasToolbar);
            cmdGrp.HasMenu = cmds.Any(c => c.HasMenu);

            if (!cmdGrp.Activate()) 
            {
                m_Logger.Log("Command group activation failed", LoggerMessageSeverity_e.Error);
            }

            m_Logger.Log($"Command group-{groupId} Id: {(cmdGrp.HasToolbar ? cmdGrp.ToolbarId.ToString() : "No Toolbar")}", LoggerMessageSeverity_e.Debug);

            foreach (var createdCmd in createdCmds) 
            {
                var cmdId = cmdGrp.CommandID[createdCmd.Item2];
                var cmdInfo = new CommandInfo(createdCmd.Item1, parentGroup, cmdId);
                m_Commands.Add(createdCmd.Item3, cmdInfo);
            }   
        }

        private IImageCollection CreateMainIcon(CommandGroupSpec cmdBar, IIconsCreator iconsConv)
        {
            var mainIcon = cmdBar.Icon;

            if (mainIcon == null)
            {
                mainIcon = Defaults.Icon;
            }

            if (CompatibilityUtils.SupportsHighResIcons(m_App.Sw, CompatibilityUtils.HighResIconsScope_e.CommandManager))
            {
                return iconsConv.ConvertIcon(new CommandGroupHighResIcon(mainIcon));
            }
            else
            {
                return iconsConv.ConvertIcon(new CommandGroupIcon(mainIcon));
            }
        }

        private IImageCollection CreateToolbarIcons(CommandGroupSpec cmdBar, IIconsCreator iconsConv)
        {
            IXImage[] iconList = null;

            if (cmdBar.Commands != null)
            {
                iconList = cmdBar.Commands.Select(c => c.Icon ?? Defaults.Icon).ToArray();
            }

            if (CompatibilityUtils.SupportsHighResIcons(m_App.Sw, CompatibilityUtils.HighResIconsScope_e.CommandManager))
            {   
                if (iconList != null && iconList.Any())
                {
                    return iconsConv.ConvertIconsGroup(iconList.Select(i => new CommandGroupHighResIcon(i)).ToArray());
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (iconList != null && iconList.Any())
                {
                    return iconsConv.ConvertIconsGroup(iconList.Select(i => new CommandGroupIcon(i)).ToArray());
                }
                else
                {
                    return null;
                }
            }
        }

        private void SetCommandGroupIcons(CommandGroup cmdGroup, IImageCollection mainIcon, IImageCollection toolbarIcons)
        {
            //NOTE: if commands are not used, main icon will fail if toolbar commands image list is not specified, so it is required to specify it explicitly
            if (CompatibilityUtils.SupportsHighResIcons(m_App.Sw, CompatibilityUtils.HighResIconsScope_e.CommandManager))
            {
                cmdGroup.MainIconList = mainIcon.FilePaths;
                cmdGroup.IconList = toolbarIcons?.FilePaths;
            }
            else
            {
                var mainIconPath = mainIcon?.FilePaths ?? new string[] { null, null };

                var smallIcon = mainIconPath[0];
                var largeIcon = mainIconPath[1];

                cmdGroup.SmallMainIcon = smallIcon;
                cmdGroup.LargeMainIcon = largeIcon;

                var iconListPath = toolbarIcons?.FilePaths ?? new string[] { null, null };
                var smallIconList = iconListPath[0];
                var largeIconList = iconListPath[1];

                cmdGroup.SmallIconList = smallIconList;
                cmdGroup.LargeIconList = largeIconList;
            }
        }

        private bool CompareIDs(IEnumerable<int> storedIDs, IEnumerable<int> addinIDs)
            => storedIDs.OrderBy(x => x).SequenceEqual(addinIDs.OrderBy(x => x));

        private string GetMenuPath(CommandGroupSpec cmdBar)
        {
            var title = new StringBuilder();

            var parent = cmdBar.Parent;

            while (parent != null)
            {
                title.Insert(0, parent.Title + SUB_GROUP_SEPARATOR);
                parent = parent.Parent;
            }

            title.Append(cmdBar.Title);

            return title.ToString();
        }

        private TabInfo[] GroupCommandsByTabs()
        {
            var tabs = new List<TabInfo>();

            foreach (var cmdGrp in m_CommandBars)
            {
                if (!cmdGrp.IsContextMenu)
                {
                    var tabName = cmdGrp.Spec.RibbonTabName;

                    if (string.IsNullOrEmpty(tabName))
                    {
                        var rootTabGroupSpec = cmdGrp.Spec;

                        while (rootTabGroupSpec.Parent != null)
                        {
                            rootTabGroupSpec = rootTabGroupSpec.Parent;
                        }

                        tabName = rootTabGroupSpec.Title;
                    }

                    var config = new CommandGroupTabConfiguration()
                    {
                        Include = true,
                        TabName = tabName
                    };

                    m_TabConfigurer.ConfigureTab(cmdGrp.Spec, config);

                    if (config.Include)
                    {
                        tabName = config.TabName;

                        var tabData = tabs.FirstOrDefault(t => string.Equals(t.TabName, tabName, StringComparison.CurrentCultureIgnoreCase));

                        if (tabData == null)
                        {
                            tabData = new TabInfo(tabName);
                            tabs.Add(tabData);
                        }

                        tabData.CommandGroups.Add(cmdGrp);
                    }
                }
            }

            return tabs.ToArray();
        }

        private void TryCreateTab(TabInfo tabInfo)
        {
            try
            {
                var tabGroups = GroupTabCommandsByWorkspace(tabInfo.CommandGroups);

                foreach (var tabGroup in tabGroups)
                {
                    try
                    {
                        CreateTabInWorkspace(tabInfo.TabName, tabGroup.Key, tabGroup.Value);
                    }
                    catch (Exception ex)
                    {
                        m_Logger.Log(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
            }
        }

        private void CreateTabInWorkspace(string tabName, WorkspaceTypes_e workspace, TabCommandGroupInfo[] tabGroups)
        {
            var cmdTab = GetTab(tabName, workspace, tabGroups, out bool matches);

            if (!matches)
            {
                foreach (var tabGroup in tabGroups)
                {
                    var tabBox = cmdTab.AddCommandTabBox();

                    var cmdIds = tabGroup.Commands.Select(c => c.CommandId).ToArray();
                    var txtTypes = tabGroup.Commands.Select(c => (int)ConvertTextDisplay(c.TextStyle)).ToArray();

                    if (!tabBox.AddCommands(cmdIds, txtTypes))
                    {
                        m_Logger.Log($"Failed to add commands to commands tab box {tabName} for document type {workspace}", LoggerMessageSeverity_e.Error);
                    }
                }
            }
        }

        private CommandTab GetTab(string tabName, WorkspaceTypes_e workspace, TabCommandGroupInfo[] tabGroups, out bool matches)
        {
            swDocumentTypes_e docType;

            switch (workspace)
            {
                case WorkspaceTypes_e.Part:
                    docType = swDocumentTypes_e.swDocPART;
                    break;

                case WorkspaceTypes_e.Assembly:
                    docType = swDocumentTypes_e.swDocASSEMBLY;
                    break;

                case WorkspaceTypes_e.Drawing:
                    docType = swDocumentTypes_e.swDocDRAWING;
                    break;

                default:
                    throw new NotSupportedException();
            }

            var cmdTab = CmdMgr.GetCommandTab((int)docType, tabName);

            if (cmdTab != null)
            {
                if (!IsCommandTabContainingGroups(cmdTab, tabGroups))
                {
                    m_Logger.Log($"Tab '{tabName}' in {workspace} is changed", LoggerMessageSeverity_e.Debug);

                    if (!TryClearTab(tabName, docType, ref cmdTab))
                    {
                        throw new Exception($"Failed to remove tab '{tabName}' in {workspace}");
                    }

                    matches = false;
                }
                else
                {
                    m_Logger.Log($"Tab '{tabName}' in {workspace} is not changed", LoggerMessageSeverity_e.Debug);

                    matches = true;
                }
            }
            else
            {
                m_Logger.Log($"Tab '{tabName}' in {workspace} does not exist", LoggerMessageSeverity_e.Debug);

                matches = false;
                cmdTab = CmdMgr.AddCommandTab((int)docType, tabName);
            }

            if (cmdTab == null) 
            {
                throw new Exception($"Failed to create tab '{tabName}' in {workspace}");
            }

            return cmdTab;
        }

        private bool TryClearTab(string tabName, swDocumentTypes_e docType, ref CommandTab cmdTab)
        {
            var cmdTabBoxes = (object[])cmdTab.CommandTabBoxes();

            if (cmdTabBoxes?.Any() == true)
            {
                foreach (CommandTabBox cmdTabBox in cmdTabBoxes)
                {
                    cmdTab.RemoveCommandTabBox(cmdTabBox);
                }

                if (cmdTab.GetCommandTabBoxCount() > 0)
                {
                    m_Logger.Log($"Failed to clear command tab boxes in '{tabName}' in {docType}", LoggerMessageSeverity_e.Debug);

                    var removeTabRes = CmdMgr.RemoveCommandTab(cmdTab);

                    if (!removeTabRes) //NOTE: sometimes API returns false despite the tab been removed correctly
                    {
                        removeTabRes = CmdMgr.GetCommandTab((int)docType, tabName) == null;
                    }

                    if (removeTabRes)
                    {
                        cmdTab = CmdMgr.AddCommandTab((int)docType, tabName);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else 
            {
                return true;
            }
        }

        private Dictionary<WorkspaceTypes_e, TabCommandGroupInfo[]> GroupTabCommandsByWorkspace(List<SwCommandGroup> groups)
        {
            var tabCommands = new Dictionary<WorkspaceTypes_e, List<TabCommandGroupInfo>>();

            var cmdIds = m_Commands.Values.ToDictionary(x => x.Spec, x => x.CommandId);

            foreach (var group in groups)
            {
                if (group.Spec.Commands != null)
                {
                    foreach (var cmd in group.Spec.Commands)
                    {
                        if (cmd.HasRibbon)
                        {
                            var cmdId = cmdIds[cmd];
                            var textType = ConvertTextDisplay(cmd.RibbonTextStyle);

                            foreach (var worspaceType in new WorkspaceTypes_e[] { WorkspaceTypes_e.Part, WorkspaceTypes_e.Assembly, WorkspaceTypes_e.Drawing }) 
                            {
                                if (cmd.SupportedWorkspace.HasFlag(worspaceType))
                                {
                                    if (!tabCommands.TryGetValue(worspaceType, out List<TabCommandGroupInfo> tabCmdGrps))
                                    {
                                        tabCmdGrps = new List<TabCommandGroupInfo>();
                                        tabCommands.Add(worspaceType, tabCmdGrps);
                                    }

                                    var tabCmdGrp = tabCmdGrps.FirstOrDefault(g => g.Group == group);

                                    if (tabCmdGrp == null) 
                                    {
                                        tabCmdGrp = new TabCommandGroupInfo(group);
                                        tabCmdGrps.Add(tabCmdGrp);
                                    }

                                    tabCmdGrp.Commands.Add(new TabCommandInfo(cmdId, cmd.RibbonTextStyle));
                                }
                            }
                        }
                    }
                }
            }

            return tabCommands.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }
        
        private bool IsCommandTabContainingGroups(ICommandTab cmdTab, TabCommandGroupInfo[] tabGroups) 
        {
            var tabBoxes = (object[])cmdTab.CommandTabBoxes();

            if (tabBoxes != null && tabBoxes.Length == tabGroups.Length)
            {
                for (int i = 0; i < tabBoxes.Length; i++)
                {
                    if (!IsCommandTabBoxMatchingGroup((ICommandTabBox)tabBoxes[i], tabGroups[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsCommandTabBoxMatchingGroup(ICommandTabBox cmdTabBox, TabCommandGroupInfo groupInfo)
        {
            object existingCmds;
            object existingTextStyles;
            cmdTabBox.GetCommands(out existingCmds, out existingTextStyles);

            if (existingCmds != null && existingTextStyles != null)
            {
                var cmdIds = groupInfo.Commands.Select(c => c.CommandId).ToArray();

                if (((int[])existingCmds).SequenceEqual(cmdIds)) 
                {
                    var txtTypes = groupInfo.Commands.Select(c => (int)ConvertTextDisplay(c.TextStyle)).ToArray();

                    if (((int[])existingTextStyles).SequenceEqual(txtTypes))
                    {
                        return true;
                    }
                }
                    
            }

            return false;
        }
        
        private swCommandTabButtonTextDisplay_e ConvertTextDisplay(RibbonTabTextDisplay_e style)
        {
            switch (style)
            {
                case RibbonTabTextDisplay_e.NoText:
                    return swCommandTabButtonTextDisplay_e.swCommandTabButton_NoText;

                case RibbonTabTextDisplay_e.TextBelow:
                    return swCommandTabButtonTextDisplay_e.swCommandTabButton_TextBelow;

                case RibbonTabTextDisplay_e.TextHorizontal:
                    return swCommandTabButtonTextDisplay_e.swCommandTabButton_TextHorizontal;

                default:
                    return swCommandTabButtonTextDisplay_e.swCommandTabButton_TextHorizontal;
            }
        }

        public void Dispose()
            => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var grp in m_CommandBars)
                {
                    m_Logger.Log($"Removing group: {grp.Spec.Id}", LoggerMessageSeverity_e.Debug);

                    bool removeRes;

                    if (m_App.IsVersionNewerOrEqual(SwVersion_e.Sw2011))
                    {
                        var res = (swRemoveCommandGroupErrors)CmdMgr.RemoveCommandGroup2(grp.Spec.Id, true);
                        removeRes = res == swRemoveCommandGroupErrors.swRemoveCommandGroup_Success;
                    }
                    else
                    {
                        removeRes = CmdMgr.RemoveCommandGroup(grp.Spec.Id);
                    }

                    if (!removeRes)
                    {
                        m_Logger.Log($"Failed to remove group: {grp.Spec.Id}", LoggerMessageSeverity_e.Warning);
                    }
                }

                m_CommandBars.Clear();
            }

            if (CmdMgr != null)
            {
                if (Marshal.IsComObject(CmdMgr))
                {
                    Marshal.ReleaseComObject(CmdMgr);
                }

                CmdMgr = null;
            }
        }
    }
}