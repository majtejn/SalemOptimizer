using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalemOptimizer
{
	public static class InspirationalDatabase
	{
		private static readonly Lazy<List<Inspirational>> inspirationals = new Lazy<List<Inspirational>>(Load);

		public static List<Inspirational> Inspirationals { get { return inspirationals.Value; } }

		static List<Inspirational> Load()
		{
			return
				File.ReadAllLines("Inspirationals.tab")
					.Select(row => row.Split('\t'))
					.Select
					(
						(cols, index) =>
							new Inspirational
							(
								cols[0],
								int.Parse(cols[18]),
								cols
									.Select((val, idx) => new { Index = idx, Value = val })
									.Where(i => i.Index >= 2 && i.Index <= 16)
									.OrderBy(i => i.Index)
									.Select(i => string.IsNullOrWhiteSpace(i.Value) ? 0 : int.Parse(i.Value))
									.ToArray(),
								int.Parse(cols[19])
							)
					)
					.Where(i => i.Proficiencies.Length > 0)
					.Select((inspirational, index) => { inspirational.Id = index; return inspirational; })
					.ToList();
		}

		public static string InspToLine(Inspirational insp)
		{
			string line = String.Concat(insp.Name, "\t\t");
			
			foreach (int i in insp.Proficiencies)
			{
				if(i>0)
					line = String.Concat(line, i.ToString());
				line = String.Concat(line, "\t");
			}
			line = String.Concat(line, insp.Inspiration.ToString(),"\t");
			line = String.Concat(line, insp.Uses.ToString(),"\t");
			line = String.Concat(line, insp.Diff.ToString());
			return line;

		}

		public static void SaveToFile(string filename)
		{
			string[] list = new string[Inspirationals.Count];
			int i = 0;
			foreach (Inspirational insp in Inspirationals)
			{
				list[i] = InspToLine(insp);
				i++;
			}
			File.WriteAllLines(filename, list);
		}
	}
}
