using Microsoft.Win32;
using VideoFeatureMatching.L10n;

namespace VideoFeatureMatching.DAL
{
    public abstract class BaseFileAccessor<TModel>
    {
        public void Save(ProjectFile<TModel> projectFile)
        {
            if (!projectFile.IsSaved)
            {
                SaveAs(projectFile);
            }
            else
            {
                SaveTo(projectFile.Model, projectFile.Path);
                projectFile.IsSaved = true;
            }
        }

        public void SaveAs(ProjectFile<TModel> projectFile)
        {
            var fileDialog = new SaveFileDialog();
            fileDialog.Filter = Strings.ProjectExtenstion;

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                projectFile.Path = fileDialog.FileName;
                SaveTo(projectFile.Model, projectFile.Path);
                projectFile.IsSaved = true;
            }
        }

        public ProjectFile<TModel> Open()
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = Strings.ProjectExtenstion;

            var result = fileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var path = fileDialog.FileName;
                var model = ReadFrom(path);
                return new ProjectFile<TModel>(model, path);
            }
            else
            {
                return null;
            }
        }

        protected abstract void SaveTo(TModel model, string path);
        protected abstract TModel ReadFrom(string path);
    }
}