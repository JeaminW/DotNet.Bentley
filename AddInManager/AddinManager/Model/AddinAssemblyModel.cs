using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace AddinManager.Model
{
    /// <summary>
    /// AddinAssemblyModel
    /// </summary>
    class AddInAssemblyModel
    {
        /// <summary>
        /// 路径
        /// </summary>

        public string Path { get; set; }

        /// <summary>
        /// 类型
        /// </summary>

        public List<string> Types { get; set; }

        public AddInAssemblyModel()
        {
            this.Types = new List<string>();
        }

        /// <summary>
        /// Writes the file.
        /// </summary>
        public static void WriteModels(List<AddInAssemblyModel> model)
        {
            if (model.Count == 0)
            {
                File.WriteAllText(Helper.GlobalHelper.AddInManagerAssemblyFile, "");
                return;
            }

            var array = new string[model.Count];

            for (int i = 0; i < model.Count; i++)
            {
                var builder = new System.Text.StringBuilder();

                builder.Append(model[i].Path);

                foreach (var type in model[i].Types)
                {
                    builder.Append(string.Format(";{0}", type));
                }

                array[i] = builder.ToString();
            }

            File.WriteAllLines(Helper.GlobalHelper.AddInManagerAssemblyFile, array);
        }

        /// <summary>
        /// Reads the models.
        /// </summary>
        /// <returns></returns>
        public static List<AddInAssemblyModel> ReadModels()
        {
            var result = new List<AddInAssemblyModel>();

            if (!File.Exists(Helper.GlobalHelper.AddInManagerAssemblyFile))
            {
                return result;
            }

            var array = File.ReadAllLines(Helper.GlobalHelper.AddInManagerAssemblyFile);

            if (array.Length == 0)
            {
                return result;
            }

            foreach (var item in array)
            {
                var sp = item.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (sp.Length < 2)
                {
                    continue;
                }

                var model = new AddInAssemblyModel() { Path = sp[0].Trim() };

                for (int i = 1; i < sp.Length; i++)
                {
                    model.Types.Add(sp[i].Trim());
                }

                result.Add(model);
            }

            return result;
        }

        /// <summary>
        /// Converters the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns></returns>
        public static List<AddInAssemblyModel> Converter(IEnumerable<AddInModel> models)
        {
            var result = new List<AddInAssemblyModel>();

            using (var eum = models.GetEnumerator())
            {
                while (eum.MoveNext())
                {
                    var model = new AddInAssemblyModel() { Path = eum.Current.Path };

                    if (eum.Current.Childs != null)
                    {
                        foreach (var item in eum.Current.Childs)
                        {
                            model.Types.Add(item.Name);
                        }
                    }

                    if (model.Types.Count > 0)
                    {
                        result.Add(model);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Converters the specified models.
        /// </summary>
        /// <param name="models">The models.</param>
        /// <returns></returns>
        public static ObservableCollection<AddInModel> Converter(IList<AddInAssemblyModel> models)
        {
            var result = new ObservableCollection<AddInModel>();

            using (var eum = models.GetEnumerator())
            {
                while (eum.MoveNext())
                {
                    if (File.Exists(eum.Current.Path))
                    {
                        var model = new AddInModel() { Path = eum.Current.Path, Name = System.IO.Path.GetFileNameWithoutExtension(eum.Current.Path) };

                        if (eum.Current.Types != null)
                        {
                            foreach (var item in eum.Current.Types)
                            {
                                model.Childs.Add(new AddInModel()
                                {
                                    Name = item,
                                    Parent = model,
                                    Path = eum.Current.Path
                                });
                            }
                        }

                        result.Add(model);
                    }
                }
            }
            return result;
        }
    }
}
