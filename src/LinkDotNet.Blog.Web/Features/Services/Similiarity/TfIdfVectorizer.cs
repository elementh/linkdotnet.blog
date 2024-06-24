using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace LinkDotNet.Blog.Web.Features.Services.Similiarity;

internal sealed class TfIdfVectorizer
{
    private readonly IReadOnlyCollection<IReadOnlyCollection<string>> documents;
    private readonly Dictionary<string, double> idfScores;

    private const double TitleWeight = 3.0;
    private const double ShortDescriptionWeight = 2.0;
    private const double TagWeight = 1.5;

    public TfIdfVectorizer(IReadOnlyCollection<IReadOnlyCollection<string>> documents)
    {
        this.documents = documents;
        idfScores = CalculateIdfScores();
    }

    public Dictionary<string, double> ComputeTfIdfVector(IReadOnlyCollection<string> targetDocument)
    {
        ArgumentNullException.ThrowIfNull(targetDocument);

        var termFrequency = targetDocument.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        var tfidfVector = new Dictionary<string, double>();

        foreach (var term in termFrequency.Keys)
        {
            var tf = termFrequency[term] / (double)targetDocument.Count;
            var idf = idfScores.TryGetValue(term, out var score) ? score : 0;
            tfidfVector[term] = tf * idf * GetTermWeight(term, [..targetDocument]);
        }

        return tfidfVector;
    }

    private Dictionary<string, double> CalculateIdfScores()
    {
        var termDocumentFrequency = new Dictionary<string, int>();
        var scores = new Dictionary<string, double>();

        foreach (var term in documents.Select(document => document.Distinct()).SelectMany(terms => terms))
        {
            termDocumentFrequency.TryGetValue(term, out var value);
            termDocumentFrequency[term] = ++value;
        }

        foreach (var term in termDocumentFrequency.Keys)
        {
            scores[term] = Math.Log(documents.Count / (double)termDocumentFrequency[term]);
        }

        return scores;
    }

    private static double GetTermWeight(string term, IList<string> document)
    {
        var titleStartIndex = document.IndexOf("|title|");
        var shortDescriptionStartIndex = document.IndexOf("|short_description|");
        var tagsStartIndex = document.IndexOf("|tags|");

        if (titleStartIndex >= 0 && shortDescriptionStartIndex >= 0 && tagsStartIndex >= 0)
        {
            var termIndex = document.ToList().IndexOf(term);
            if (termIndex > titleStartIndex && termIndex < shortDescriptionStartIndex)
            {
                return TitleWeight;
            }
            if (termIndex > shortDescriptionStartIndex && termIndex < tagsStartIndex)
            {
                return ShortDescriptionWeight;
            }
            if (termIndex > tagsStartIndex)
            {
                return TagWeight;
            }
        }
        return 1.0;
    }
}
