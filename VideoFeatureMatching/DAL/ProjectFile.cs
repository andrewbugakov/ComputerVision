using System;

namespace VideoFeatureMatching.DAL
{
    public class ProjectFile<TModel>
    {
        public ProjectFile(TModel model)
        {
            if (model == null) throw new ArgumentNullException("model");
            Model = model;
        }

        public ProjectFile(TModel model, string path)
            : this(model)
        {
            Path = path;
        }

        public TModel Model { get; private set; }
        public string Path { get; private set; }
        public bool IsSaved { get; private set; }

        public event EventHandler ProjectSaved;
    }
}