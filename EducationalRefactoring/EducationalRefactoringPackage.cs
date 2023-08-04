using System;
using System.IO;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;

namespace EducationalRefactoring
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    /// 
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(EducationalRefactoringPackage.PackageGuidString)]
    public sealed class EducationalRefactoringPackage : AsyncPackage
    {
        /// <summary>
        /// FirstPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "c60ba1b7-2667-40fa-852e-21b155757c66";

        private IVsUIShell uiShell;

        DetectSave detectSave;

        RunningDocumentTable docTable;

        OutputWindowPane outputWindow;

        bool metricPackageInit = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstPackage"/> class.
        /// </summary>
        public EducationalRefactoringPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            
            docTable = new RunningDocumentTable();

            detectSave = new DetectSave(docTable);
            detectSave.AfterSave += OnAfterSave;
            detectSave.BeforeSave += SetupMetricPackage;
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                0,
                ref clsid,
                "Initialisation Succeeded",
                string.Format(CultureInfo.CurrentCulture, "Inside {0}.Initialize()", this.GetType().FullName),
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_INFO,
                0,
                out result));

            CreatePane("Refactoring Assist");
        }

        /// <summary>
        /// Install/Setup for CodeMetrics Package. Setup happens the first time this function is run
        /// </summary>
        /// <param name="aDocument">The current opened document, this paramater is passed in through an event but not necessary within this function, it can be safely ignored</param>
        void SetupMetricPackage(Document aDocument)
        {
            // Only run this function on the first iteration then attempt to remove it from the event listener
            if (metricPackageInit)
            {
                detectSave.BeforeSave -= SetupMetricPackage;
                return;
            }

            // Try to download and install the CodeMetrics package dependancy
            try
            {
                EnvDTE.DTE _ObjDTE = (EnvDTE.DTE)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");
                var script = "Install-Package Microsoft.CodeAnalysis.Metrics -Version 3.3.3";
                _ObjDTE.ExecuteCommand("View.PackageManagerConsole");
                _ObjDTE.ExecuteCommand("View.PackageManagerConsole", script);
            }
            catch
            {
                Trace.TraceError("Failed to verify installation of 'Microsoft.CodeAnalysis.Metrics'...");
            }

            metricPackageInit = true;
        }

        /// <summary>
        /// Creates a new output window where the results of the artefact will be output
        /// </summary>
        /// <param name="title">The title to be used for the output window (Educational Refactoring)</param>
        void CreatePane(string title)
        {
            // Get the existing panes open in the OutputWindow
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;

            try
            {
                // If the pane exists already, write to it.
                outputWindow = panes.Item(title);
            }
            catch (ArgumentException)
            {
                // Create a new pane and write to it.
                outputWindow = panes.Add(title);
            }

            // Test the outputWindow to check it exists
            outputWindow.OutputString("\nEducational Refactoring is now active! Here to help!\n");
        }

        /// <summary>
        /// Triggers after a document has been saved through the DetectSave class
        /// </summary>
        /// <param name="sender">The event that triggered this function</param>
        /// <param name="aDocument">The open document that has been saved to</param>
        public void OnAfterSave(object sender, Document aDocument)
        {
            // Only run if the CodeMetrics package has been (/attempted to be) installed
            if(metricPackageInit)
                RunCodeMetrics(aDocument);
        }

        /// <summary>
        /// Run the CodeMetrics checks against the currently opened project
        /// </summary>
        /// <param name="aDocument">The open document that has been saved to</param>
        private void RunCodeMetrics(Document aDocument)
        {
            // This line is used to get the correct directory for the project solution
            string path = aDocument.ProjectItem.ContainingProject.FileName.Replace("\\" + aDocument.ProjectItem.ContainingProject.UniqueName, "");

            // The following code block opens a new cmd.exe window and attempts to open it as a Visual Studio Developer Console
            // This is to bypass any local user access level blockers for running msbuild (required to run code metrics programatically)
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C \"C:/Program Files (x86)/Microsoft Visual Studio/2019/Community/Common7/Tools/VsDevCmd.bat\" & cd " + path + " & msbuild /t:Metrics";
            process.StartInfo = startInfo;
            process.Start();

            // This line is used to get the correct file path for the newly processed CodeMetrics .xml file
            string newPath = aDocument.ProjectItem.ContainingProject.FileName.Replace(aDocument.ProjectItem.ContainingProject.UniqueName, aDocument.ProjectItem.ContainingProject.Name + ".Metrics.xml");
            ProcessCodeMetrics(newPath);
        }

        /// <summary>
        /// Check for specific values that occur in the code metrics report, if within a certain range, return them
        /// </summary>
        /// <param name="methodElements">All of the functions from the code metrics xml file to be checked</param>
        /// <param name="flagValueMin">The smallest value allowed to trigger a return</param>
        /// <param name="flagValueTrigger">The upper bound, values are required to be under this value to trigger a retur</param>
        /// <returns>A list of method names with the metric criteria that caused the flag (i.e. Maintainability)</returns>
        private IEnumerable<string>[] CheckForFlags(IEnumerable<XElement> methodElements, int flagValueTrigger, int flagValueMin)
        {
            // Linq to XML for processing the results from the code metric process
            IEnumerable<string> methodNames = from item in methodElements.Descendants("Members")
                                                   from method in item.Elements("Method")
                                                   from metrics in method.Elements("Metrics")
                                                   where (int)metrics.Element("Metric").Attribute("Value") < flagValueTrigger && (int)metrics.Element("Metric").Attribute("Value") > flagValueMin
                                                   select method.Attribute("Name").ToString();

            IEnumerable<string> metricNames = from item in methodElements.Descendants("Members")
                                                   from method in item.Elements("Method")
                                                   from metrics in method.Elements("Metrics")
                                                   where (int)metrics.Element("Metric").Attribute("Value") < flagValueTrigger && (int)metrics.Element("Metric").Attribute("Value") > flagValueMin
                                                   select metrics.Element("Metric").Attribute("Name").ToString();

            return new IEnumerable<string>[2] { methodNames, metricNames };
        }

        /// <summary>
        /// Processes the results of the code metrics, outputting any flags to the output window
        /// </summary>
        /// <param name="path">The path to the .xml output from the code metrics</param>
        private void ProcessCodeMetrics(string path)
        {
            if (!File.Exists(path))
            {
                Trace.TraceError("Error finding code metric file at path: " + path);
                return;
            }

            // Clears the output window
            outputWindow.Clear();
            // Then brings it to the front of the output screen
            outputWindow.Activate();

            // Loads the xml document into a variable of elements
            XElement codeMetrics = XElement.Load(path);

            // Makes a list of all functions/methods in the .xml file
            IEnumerable<XElement> methodElements = from item in codeMetrics.Descendants("Types")
                                                   select item.Element("NamedType");

            IEnumerable<string>[] flaggedDataMinor;
            IEnumerable<string>[] flaggedDataMajor;
            flaggedDataMinor = CheckForFlags(methodElements, 40, 20);
            flaggedDataMajor = CheckForFlags(methodElements, 20, -1);


            // The following code region outputs all of the results of the code metric scan to the output window, the messages are hard coded responses as a vertical slice

            #region OutputResultsToWindow
            // Outputs to the extension window for debugging
            Trace.WriteLine("Code Metric Results: ====================");
            Trace.WriteLine("Total of " + (flaggedDataMinor[0].Count() + flaggedDataMajor[0].Count()) + " methods found at path: " + path);

            if (flaggedDataMinor[0].Count() <= 0 && flaggedDataMajor[0].Count() <= 0)
                outputWindow.OutputString("\nCongratulations! You're code has no issues!");
            else 
            {
                string outputString,metric,method = "";
                if (flaggedDataMajor[0].Count() > 0)
                {
                    outputWindow.OutputString("\nMajor Problems detected in the code! Some of your functions may have been flagged with problematic code.");
                    outputWindow.OutputString("\nThe functions listed below are in need of code review. Some links are included that might help with understanding what to do next!");
                    outputWindow.OutputString("\n~=~=~~=~=~~=~=~~=~=~~=~=~~=~=~~=~=~\n");
                    outputWindow.OutputString("\nThe following functions have maintainability flags. \nThis likely means the function will still work but isn't readable and will be hard to fix/read later on in development. " +
                                "\nIt might also mean that the function is too specific to a task and won't be able to be re-purposed in another project or the function is very inefficient.\n\n");
                    for (int i = 0; i < flaggedDataMajor[1].Count(); i++)
                    {
                        outputString = "\n" + flaggedDataMajor[1].ElementAt(i);
                        metric = flaggedDataMajor[1].ElementAt(i);
                        method = flaggedDataMajor[0].ElementAt(i);
                        metric = metric.Replace("\"", "");
                        metric = metric.Remove(0, 5);
                        method = method.Remove(0, 5);
                        if (metric == "MaintainabilityIndex")
                        {
                            outputString = "Function " + (i + 1) + ") " + method;
                        }
                        outputWindow.OutputString(outputString);
                    }

                    outputWindow.OutputString("\n\n To help understand and make fixes to the above functions with maintainability flags, you should look into the following resources as independant study: \n");
                }
                else if (flaggedDataMinor[0].Count() > 0)
                {
                    outputWindow.OutputString("\nMinor Problems detected in the code! Some of your functions may have been flagged with problematic code.");
                    outputWindow.OutputString("\nYou might want to review some of the functions listed below. Links are included that might help with understanding what to do next!");
                    outputWindow.OutputString("\n~=~=~~=~=~~=~=~~=~=~~=~=~~=~=~~=~=~\n");
                    outputWindow.OutputString("The following functions have minor maintainability flags. \nThis likely means the function will still work but isn't easily readable. " +
                                "\nIt might also mean that the function is too specific to a task and won't be easy to re-purpose in another project.\n\n");
                    for (int i = 0; i < flaggedDataMinor[1].Count(); i++)
                    {
                        outputString = "\n" + flaggedDataMinor[1].ElementAt(i);
                        metric = flaggedDataMinor[1].ElementAt(i);
                        method = flaggedDataMinor[0].ElementAt(i);
                        metric = metric.Replace("\"", "");
                        metric = metric.Remove(0, 5);
                        method = method.Remove(0, 5);
                        if (metric == "MaintainabilityIndex")
                        {
                            outputString = "Function " + (i + 1) + ") " + method + "\n";
                        }
                        outputWindow.OutputString(outputString);
                    }

                    outputWindow.OutputString("\n\nTo help understand and make fixes to the above functions with minor maintainability flags, consider looking into the following resources: \n");
                    outputWindow.OutputString("http://www.red-gate.com/simple-talk/development/dotnet-development/writing-maintainable-code/");
                    outputWindow.OutputString("\n");
                    outputWindow.OutputString("http://www.crosscomm.com/resources/blog/six-coding-tips-junior-devs");
                }
            }

            #endregion
        }

        #endregion
    }


    /// <summary>
    /// DetectSave is used to run an event whenever a loaded document is saved in visual studio
    /// </summary>
    public class DetectSave : IVsRunningDocTableEvents3
    {
        private RunningDocumentTable mRunningDocumentTable;
        private DTE mDte;

        // Event declaration for running functions in other classes on save
        #region eventDeclarations
        public delegate void OnAfterSaveHandler(object sender, Document document);
        public event OnAfterSaveHandler AfterSave;

        public delegate void OnBeforeSaveHandler(Document document);
        public event OnBeforeSaveHandler BeforeSave;
        #endregion

        public DetectSave(RunningDocumentTable rdt)
        {
            mDte = (DTE)Package.GetGlobalService(typeof(DTE));

            mRunningDocumentTable = rdt;
            mRunningDocumentTable.Advise(this);
        }

        #region Private Methods

        /// <summary>
        /// Finds the open document by the cookie ID
        /// </summary>
        /// <param name="docCookie">The id for the document that was saved</param>
        /// <returns>The currently opened document that was saved</returns>
        private Document FindDocumentByCookie(uint docCookie)
        {
            var documentInfo = mRunningDocumentTable.GetDocumentInfo(docCookie);
            return mDte.Documents.Cast<Document>().FirstOrDefault(doc => doc.FullName == documentInfo.Moniker);
        }

        #endregion

        public int OnAfterSave(uint docCookie)
        {
            if (null == AfterSave)
                return VSConstants.S_OK;

            var document = FindDocumentByCookie(docCookie);
            if (null == document)
                return VSConstants.S_OK;

            // If the checks pass, run the event and pass the current document through
            AfterSave(this, FindDocumentByCookie(docCookie));
            return VSConstants.S_OK;
        }

        public int OnBeforeSave(uint docCookie)
        {
            if (null == BeforeSave)
                return VSConstants.S_OK;

            var document = FindDocumentByCookie(docCookie);
            if (null == document)
                return VSConstants.S_OK;

            // If the checks pass, run the event and pass the current document through
            BeforeSave(FindDocumentByCookie(docCookie));
            return VSConstants.S_OK;
        }

        // Required declarations for IVsRunningDocTableEvents3 (required dependency for save detection)
        #region IVsRunningDocTableEvents3 Declarations

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
