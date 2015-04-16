﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using System.Windows.Forms;

namespace Zinc.CarbonCopy
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCarbonCopyPkgString)]
    [ProvideAutoLoad(UIContextGuids80.Debugging)]
    public sealed class CarbonCopyPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CarbonCopyPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidCarbonCopyCmdSet, (int)PkgCmdIDList.cmdidCopyDeclaration);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );

                //CommandID projectMenuCommandID = new CommandID(GuidList.Interactive_WindowCmdSet, (int)PkgCmdIDList.cmdidLoadUI);
                //OleMenuCommand projectmenuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                //projectmenuItem.BeforeQueryStatus += menuCommand_BeforeQueryStatus;
           
                mcs.AddCommand( menuItem );
                
            }
        }
        #endregion

        private void menuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            MessageBox.Show("test");
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            CopyDeclaration();
        }

        private void CopyDeclaration()
        {
            string variableName = null;
            try
            {
                variableName = GetSelectedVariable();
            }
            catch (InvalidExpressionException)
            {
                MessageBox.Show("Make sure the object name is fully selected.", "Invalid object selected");
                return;
            }

            string declaration = GenerateDeclaration(variableName);

            Clipboard.SetText(declaration);
        }

        private string GetSelectedVariable()
        {
            var dteInstance = (DTE)GetService(typeof(SDTE));
            var textDocument = (EnvDTE.TextDocument)dteInstance.ActiveDocument.Object("");
            var selectedVariable = textDocument.Selection.Text;

            EnvDTE.Expression expression = dteInstance.Debugger.GetExpression(selectedVariable);

            if (!expression.IsValidValue)
            {
                throw new InvalidExpressionException(selectedVariable);
            }

            return selectedVariable;
        }

        private string GenerateDeclaration(string variableName)
        {
            var dteInstance = (DTE)GetService(typeof(SDTE));

            var replicateProvider = new ReplicateProvider(dteInstance.Debugger);

            var replicate = replicateProvider.CreateReplicate(variableName);

            var replicator = new Replication.Replicator();

            return replicator.GenerateDeclaration(replicate);
        }
    }
}
