using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalemOptimizer
{
    class Skill
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Skill(string name,int[] proficiencies)
        {
            this.Name = name;
           
            this.Proficiencies = proficiencies;
        }

        public int[] Proficiencies;

    }
    class SkillDatabase
    {
        private static readonly Lazy<List<Skill>> skills = new Lazy<List<Skill>>(Load);

        public static List<Skill> Skills { get { return skills.Value; } }

        static List<Skill> Load()
        {
            return
                File.ReadAllLines("Skills.tab")
                    .Select(row => row.Split('\t'))
                    .Select
                    (
                        (cols, index) =>
                            new Skill
                            (
                                cols[0],
                                
                                cols
                                    .Select((val, idx) => new { Index = idx, Value = val })
                                    .Where(i => i.Index >= 1 && i.Index <= 15)
                                    .OrderBy(i => i.Index)
                                    .Select(i => string.IsNullOrWhiteSpace(i.Value) ? 0 : int.Parse(i.Value))
                                    .ToArray()
                            )
                    )
                    .Where(i => i.Proficiencies.Length > 0)
                    .Select((inspirational, index) => { inspirational.Id = index; return inspirational; })
                    .ToList();
        }
    }
}
