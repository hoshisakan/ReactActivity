namespace Application.Module
{
    public class FileTool
    {
        public static Func<string, bool> CheckFileExists = fullPath => System.IO.File.Exists(fullPath);
        public static Func<string, bool> CheckDirExists = path => System.IO.Directory.Exists(path);
        public static Action<string> CreateDirectory = (fullPath) => System.IO.Directory.CreateDirectory(fullPath);
        public static Action<string> RemoveFile = (fullPath) => System.IO.File.Delete(fullPath);
    

        public static void CheckFileExistsAndRemove(string fullPath)
        {
            if (CheckFileExists(fullPath))
            {
                RemoveFile(fullPath);
            }
        }

        public static void CheckAndCreateDirectory(string fullPath)
        {
            if (!CheckDirExists(fullPath))
            {
                CreateDirectory(fullPath);
            }
        }
    }
}