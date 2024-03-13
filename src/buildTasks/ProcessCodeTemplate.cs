using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System;

namespace NatCruise.Build.Tasks
{
    // takes a template file (.cst), replaces placeholder values with
    // corresponding build property/env variable

    // see Directory.Build.targets for definition of build task that
    // utilizes this class
    public class ProcessCodeTemplate : Microsoft.Build.Utilities.Task
    {
        private ITaskItem[] _templateFiles;
        private Regex _elementRegex;

        public ProcessCodeTemplate()
        {
            _elementRegex = new Regex(@"\$\(([a-z|0-9|_]+)\)");
        }

        [Required]
        public ITaskItem[] TemplateFiles
        {
            get { return _templateFiles; }
            set { _templateFiles = value; }
        }

        public override bool Execute()
        {
            using (var xmlReader = XmlReader.Create(BuildEngine.ProjectFileOfTaskNode))
            {
                var project = new Project(xmlReader);

                foreach (var templateFile in TemplateFiles)
                {
                    var templatePath = templateFile.ItemSpec;
                    var fileDir = System.IO.Path.GetDirectoryName(templatePath);
                    var fileName = Path.GetFileNameWithoutExtension(templatePath);
                    var ouputFileName = fileName + ".local.cs";
                    var outputPath = Path.Combine(fileDir, ouputFileName);

                    ProcessTemplate(templatePath, outputPath, project);
                }

                return !Log.HasLoggedErrors;
            }
        }

        public void ProcessTemplate(string templatePath, string outputPath, Project project)
        {
            try
            {
                var fileContent = File.ReadAllText(templatePath);

                fileContent = _elementRegex.Replace(fileContent, (Match x) =>
                {
                    var itemName = x.Groups[1].Value;
                    var itemValue = project.GetPropertyValue(itemName) ?? Environment.GetEnvironmentVariable(itemName);
                    Log.LogMessage(itemName + ":" + itemValue);
                    return itemValue;
                });

                File.WriteAllText(outputPath, fileContent);
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }
        }
    }
}