using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Text;

namespace $safeprojectname$
{
    public class CustomTask : Task
    {
        [Required] 
        public string StringParameter { get; set; }

        public ITaskItem[] FilesParameter { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Hello World task! {0}", StringParameter);
            if(FilesParameter != null && FilesParameter.Length > 0)
            {
                foreach (var item in FilesParameter)
                {
                    var bld = new StringBuilder($"spec: '{item.ItemSpec}'");
                    foreach (string name in item.MetadataNames)
                    {
                        bld.AppendFormat("; {0}: {1}", name, item.GetMetadata(name));
                    }
                    Log.LogMessage(MessageImportance.High, "\tFile Parameter {0}", bld);
                }
            } else
            {
                Log.LogMessage(MessageImportance.High, "\t No File Parameter!");
            }
            return true;
        }
    }
}
