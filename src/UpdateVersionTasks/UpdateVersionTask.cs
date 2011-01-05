using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MattGriffith.UpdateVersion;
using System.IO;

namespace MattGriffith.UpdateVersion.Tasks
{
    public class UpdateVersion : Task
    {
        /// <summary>
        /// Stores the startdate property option.
        /// </summary>
        private string m_startDateOption;

        public string StartDate
        {
            set { m_startDateOption = value; }
        }

        /// <summary>
        /// Stores the build property option.
        /// </summary>
        private string m_buildOption;

        public string Build
        {
            set { m_buildOption = value; }
        }

        /// <summary>
        /// Stores the pin property option.
        /// </summary>
        private string m_pinOption;

        public string Pin
        {
            set { m_pinOption = value; }
        }

        /// <summary>
        /// Stores the revision property option.
        /// </summary>
        private string m_revisionOption;

        public string Revision
        {
            set { m_revisionOption = value; }
        }

        /// <summary>
        /// Stores the inputfile property option.
        /// </summary>
        private string m_inputFile;

        public string InputFile
        {
            set { m_inputFile = value; }
        }

        /// <summary>
        /// Stores the input if there is no inputfile specified 
        /// </summary>
        private string m_input;

        public string Input
        {
            set { m_input = value; }
        }

        /// <summary>
        /// Stores the outputfile property option.
        /// </summary>
        private string m_outputFile;

        public string OutputFile
        {
            set { m_outputFile = value; }
        }

        /// <summary>
        /// Stores the version property option.
        /// </summary>
        private string m_versionOption;

        public string Version
        {
            set { m_versionOption = value; }
        }

        /// <summary>
        /// Stores the output if there is no outputfile specified 
        /// </summary>
        private string m_updatedInput;

        [Output]
        public string UpdatedInput
        {
            get
            {
                return m_updatedInput;
            }

        }

        public override bool Execute()
        {
            List<String> buildoptions = new List<string>();
            Options options = null;
            string input = null;
            VersionUpdater updater = null;
            StringBuilder message = new StringBuilder();

            ///////////////////////////////////////////////////////////////////
            // validate the options
            try
            {
                if (string.IsNullOrEmpty(m_startDateOption) == false)
                {
                    buildoptions.Add("-s");
                    buildoptions.Add(m_startDateOption);
                }
                if (string.IsNullOrEmpty(m_buildOption) == false)
                {
                    buildoptions.Add("-b");
                    buildoptions.Add(m_buildOption);
                }
                if (string.IsNullOrEmpty(m_pinOption) == false)
                {
                    buildoptions.Add("-p");
                    buildoptions.Add(m_pinOption);
                }
                if (string.IsNullOrEmpty(m_revisionOption) == false)
                {
                    buildoptions.Add("-r");
                    buildoptions.Add(m_revisionOption);
                }
                if (string.IsNullOrEmpty(m_inputFile) == false)
                {
                    buildoptions.Add("-i");
                    buildoptions.Add(m_inputFile);
                }
                if (string.IsNullOrEmpty(m_outputFile) == false)
                {
                    buildoptions.Add("-o");
                    buildoptions.Add(m_outputFile);
                }
                if (string.IsNullOrEmpty(m_versionOption) == false)
                {
                    buildoptions.Add("-v");
                    buildoptions.Add(m_versionOption);
                }

                options = new Options(buildoptions.ToArray());
            }
            catch (Exception e)
            {

                message.AppendLine("Error validating options.");
                message.AppendLine(e.Message);
                message.AppendLine();
                message.AppendLine(Options.Usage);
                Log.LogError(message.ToString());
                return false;
            }

            ///////////////////////////////////////////////////////////////////
            // Get the input
            try
            {
                input = GetInput(options);
            }
            catch (Exception e)
            {
                message.AppendLine("Error reading input.");
                message.AppendLine(e.Message);
                Log.LogError(message.ToString());
                return false;
            }

            ///////////////////////////////////////////////////////////////////
            // Update the version number in the input
            try
            {
                updater = new VersionUpdater(input, options);
            }
            catch (Exception e)
            {
                message.AppendLine("Error updating version.");
                message.AppendLine(e.Message);
                Log.LogError(message.ToString());
                return false;
            }

            ///////////////////////////////////////////////////////////////////
            // Write the output
            try
            {
                m_updatedInput = updater.Output;

                if (String.IsNullOrEmpty(options.OutputFilename) == false)
                {
                    using (StreamWriter writer =
                          new StreamWriter(options.OutputFilename, false, Encoding.Default))
                    {
                        writer.Write(m_updatedInput);
                    }
                }
            }
            catch (Exception e)
            {
                message.AppendLine("Error writing output.");
                message.AppendLine(e.Message);
                Log.LogError(message.ToString());
                return false;
            }

            // Return normally
            return true;
        }

        /// <summary>
        /// Private helper method that gets the input string from the appropriate source.
        /// </summary>
        /// <param name="options">The property options.</param>
        /// <returns>
        /// Returns the input string.
        /// </returns>
        private static string GetInput(Options options)
        {
            string input = null;

            if (null == options.InputFilename)
            {
                // The input file name was not specified on the property wo we will
                // get the input from the standard input stream.
                input = options.Input;
            }
            else
            {
                // An input file was specified on the property. 
                input = ReadFile(options.InputFilename);
            }

            return input;
        }


        /// <summary>
        /// Private helper that reads the input string from a file.
        /// </summary>
        /// <param name="filename">The name of the file to read.</param>
        /// <returns>The string representing the data stored in the input file.</returns>
        private static string ReadFile(string filename)
        {
            string result = null;

            if (!File.Exists(filename))
                throw new ArgumentException("File does not exist.", "filename");

            using (FileStream stream = File.OpenRead(filename))
            {
                StreamReader reader = new StreamReader(stream, Encoding.Default, true);
                result = reader.ReadToEnd();
            }

            return result;
        }


    }
}
