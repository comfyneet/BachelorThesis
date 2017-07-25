using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RiceDoctor.DatabaseManager;
using RiceDoctor.OntologyManager;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    public class OntologyMiner
    {
        [NotNull] private readonly IDictionary<Article, IReadOnlyDictionary<string, int>> _articles;

        [NotNull]
        private readonly IDictionary<IAnalyzable, Tuple<IReadOnlyCollection<string>, IReadOnlyCollection<string>>>
            _entities;

        [NotNull] private IList<string> _terms;

        public OntologyMiner([NotNull] IOntologyManager manager, [NotNull] IReadOnlyCollection<Article> articles)
        {
            Check.NotNull(manager, nameof(manager));
            Check.NotNull(articles, nameof(articles));

            _articles = new Dictionary<Article, IReadOnlyDictionary<string, int>>();
            foreach (var article in articles)
                _articles.Add(article, null);
            _entities = new Dictionary<IAnalyzable, Tuple<IReadOnlyCollection<string>, IReadOnlyCollection<string>>>();

            foreach (var cls in manager.GetSubClasses("Thing", OntologyManager.GetType.GetAll))
            {
                var synonyms = new List<string>();
                if (cls.Label != null) synonyms.Add(Trim(cls.Label));
                _entities.Add(cls, new Tuple<IReadOnlyCollection<string>, IReadOnlyCollection<string>>(synonyms, null));
            }

            foreach (var individual in manager.GetIndividuals())
            {
                var classTerms = individual.GetAllClasses().Select(c => Trim(c.ToString())).ToList();
                var synonyms = new List<string>();
                var nearSynonyms = new List<string>();
                if (individual.GetNames() != null)
                    foreach (var name in individual.GetNames())
                    {
                        var trimmedName = Trim(name);
                        string shortTrimmedName = null;
                        foreach (var classTerm in classTerms)
                        {
                            if (!trimmedName.StartsWith(classTerm)) continue;
                            shortTrimmedName = trimmedName.Substring(classTerm.Length).Trim();
                            break;
                        }
                        if (!synonyms.Contains(trimmedName)) synonyms.Add(trimmedName);
                        if (shortTrimmedName != null && !synonyms.Contains(shortTrimmedName))
                            synonyms.Add(shortTrimmedName);
                    }
                if (individual.GetTerms() != null)
                    foreach (var term in individual.GetTerms())
                    {
                        var trimmedTerm = Trim(term);
                        string shortTrimmedTerm = null;
                        foreach (var classTerm in classTerms)
                        {
                            if (!trimmedTerm.StartsWith(classTerm)) continue;
                            shortTrimmedTerm = trimmedTerm.Substring(classTerm.Length).Trim();
                            break;
                        }
                        if (!synonyms.Contains(trimmedTerm) && !nearSynonyms.Contains(trimmedTerm))
                            nearSynonyms.Add(trimmedTerm);
                        if (shortTrimmedTerm != null && !synonyms.Contains(shortTrimmedTerm) &&
                            !nearSynonyms.Contains(shortTrimmedTerm)) nearSynonyms.Add(shortTrimmedTerm);
                    }

                _entities.Add(individual,
                    new Tuple<IReadOnlyCollection<string>, IReadOnlyCollection<string>>(synonyms, nearSynonyms));
            }

            _terms = new List<string>();
            foreach (var entity in _entities)
            {
                if (entity.Value.Item1 != null)
                    foreach (var synonym in entity.Value.Item1)
                        if (!_terms.Contains(synonym)) _terms.Add(synonym);
                if (entity.Value.Item2 != null)
                    foreach (var nearSynonym in entity.Value.Item2)
                        if (!_terms.Contains(nearSynonym)) _terms.Add(nearSynonym);
            }
            _terms = _terms.OrderBy(t => t).ToList();
        }

        public void Train()
        {
            // Windows
            var batFile = Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Dependencies\UETsegmenter\train.bat");
            var trainningFile = Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Dependencies\UETsegmenter\trainning-set\data.txt");

            File.WriteAllLines(trainningFile, _terms);

            var p = new Process {StartInfo = {FileName = batFile}};
            p.Start();
            p.WaitForExit();
        }

        public void Segment()
        {
            var articles = _articles.Keys.ToList();
            Parallel.ForEach(articles, article => { _articles[article] = SegmentArticle(article); });

            foreach (var terms in _articles.Values)
            foreach (var term in terms)
                if (!_terms.Contains(term.Key)) _terms.Add(term.Key);
            _terms = _terms.OrderBy(t => t).ToList();

            var c = new int[_terms.Count, _terms.Count];
            foreach (var k in _articles)
                for (var i = 0; i < _terms.Count; ++i)
                {
                    var f_i_k = k.Value.ContainsKey(_terms[i]) ? k.Value[_terms[i]] : 0;
                    for (var j = 0; j < _terms.Count; ++j)
                    {
                        var f_j_k = k.Value.ContainsKey(_terms[j]) ? k.Value[_terms[j]] : 0;
                        c[i, j] += f_i_k * f_j_k;
                    }
                }

            var s = new double[_terms.Count, _terms.Count];
            for (var i = 0; i < _terms.Count; ++i)
            for (var j = 0; j < _terms.Count; ++j)
            {
                var denominator = c[i, i] + c[j, j] - c[i, j];
                if (denominator == 0) s[i, j] = 0;
                else s[i, j] = (double) c[i, j] / denominator;
            }

            var matrixFile = Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\Resources\association-matrix.txt");
            using (var fs = File.Open(matrixFile, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    for (var i = 0; i < _terms.Count; ++i)
                    for (var j = 0; j < _terms.Count; ++j)
                    {
                        if (i > j) continue;

                        sw.WriteLine($"{_terms[i]}\t{_terms[j]}\t{s[i, j]}");
                        sw.Flush();
                    }
                }
            }
        }

        [NotNull]
        private string[] GetSegmentedTerms([NotNull] string outFile)
        {
            Check.NotEmpty(outFile, nameof(outFile));

            var data = File.ReadAllText(outFile).Trim();
            var terms = data.Split(' ');
            terms = terms.Select(t => t.Replace('_', ' ')).ToArray();

            return terms;
        }

        private IReadOnlyDictionary<string, int> SegmentArticle([NotNull] Article article)
        {
            Check.NotNull(article, nameof(article));

            var title = article.Title == "" ? "" : Trim(article.Title);
            var content = article.Content == "" ? "" : Trim(article.Content);

            var guid = Guid.NewGuid().ToString();
            var batName = $"{guid}.bat";
            var inName = $"{guid}_in.txt";
            var outName = $"{guid}_out.txt";

            var batFile = Path.Combine(AppContext.BaseDirectory,
                $@"..\..\..\..\Dependencies\UETsegmenter\{batName}");
            var inFile = Path.Combine(AppContext.BaseDirectory,
                $@"..\..\..\..\Dependencies\UETsegmenter\{inName}");
            var outFile = Path.Combine(AppContext.BaseDirectory,
                $@"..\..\..\..\Dependencies\UETsegmenter\{outName}");

            var command = $@"cd /d %0\..
java -jar uetsegmenter.jar -r seg -m trained-models -i {inName} -o {outName}  %*";

            File.WriteAllText(batFile, command);
            File.WriteAllText(inFile, "");
            File.WriteAllText(outFile, "");

            var segmentedTerms = new Dictionary<string, int>();

            if (title != "")
            {
                File.WriteAllText(inFile, title);

                var p = new Process {StartInfo = {FileName = batFile}};
                p.Start();
                p.WaitForExit();

                foreach (var term in GetSegmentedTerms(outFile))
                    if (segmentedTerms.ContainsKey(term)) segmentedTerms[term]++;
                    else segmentedTerms[term] = 1;
            }

            if (content != "")
            {
                File.WriteAllText(inFile, content);

                var p = new Process {StartInfo = {FileName = batFile}};
                p.Start();
                p.WaitForExit();

                foreach (var term in GetSegmentedTerms(outFile))
                    if (segmentedTerms.ContainsKey(term)) segmentedTerms[term]++;
                    else segmentedTerms[term] = 1;
            }

            File.Delete(batFile);
            File.Delete(inFile);
            File.Delete(outFile);

            return segmentedTerms;
        }

        [NotNull]
        private string Trim([NotNull] string text)
        {
            Check.NotEmpty(text, nameof(text));

            return text.ToLower().RemoveNonWordChars();
        }
    }
}