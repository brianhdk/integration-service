using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.RavenDB.Extensions
{
	public static class RavenDbExtensions
	{
        public static string Id<T>(this IDocumentSession session, string id)
        {
            return session.DocumentStore().Id<T>(id);
        }

        public static string Id<T>(this IDocumentStore store, string id)
        {
            if (id == null) throw new ArgumentNullException("id");

            return store.Conventions.DefaultFindFullDocumentKeyFromNonStringIdentifier(id, typeof(T), false);
        }

        public static IDocumentStore DocumentStore(this IDocumentSession session)
        {
            return session.Advanced.DocumentStore;
        }

	    public static T DontTrack<T>(this T subject, IDocumentSession session)
            where T : class
	    {
	        if (subject == null)
	            return null;

	        session.Advanced.Evict(subject);

	        return subject;
	    }

        public static IRavenQueryable<T> If<T>(this IRavenQueryable<T> source, bool condition, Func<IRavenQueryable<T>, IRavenQueryable<T>> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            return condition ? action(source) : source;
        }

        public static IRavenQueryable<T> Expose<T>(this IRavenQueryable<T> source, Action<IRavenQueryable<T>> exposer)
        {
            if (exposer != null)
                exposer(source);

            return source;
        }

	    private static readonly Regex TermReplacer = new Regex(@"(?:\[\[|\]\])");

	    public static string Term(this string searchTerm)
	    {
	        return String.Concat("[[", TermReplacer.Replace(searchTerm ?? String.Empty, String.Empty), "]]");
	    }

        public static string Wildcard(this string searchTerm)
        {
            return searchTerm
                .EmptyIfNull()
                .TrimStart('*')
                .IfNotThere().Append("*");
        }

        public static string Fuzzy(this string term, decimal tolerance)
        {
            if (String.IsNullOrWhiteSpace(term))
                return term;

            return String.Concat(term, "~", tolerance.ToString(CultureInfo.InvariantCulture));
        }

        public static void DeleteAll<TIndexCreator>(this IDocumentSession session, string query = null) where TIndexCreator : AbstractIndexCreationTask, new()
        {
            session.DocumentStore().DeleteAll<TIndexCreator>(query);
        }

        public static void DeleteAll<TIndexCreator>(this IDocumentStore store, string query = null) where TIndexCreator : AbstractIndexCreationTask, new()
        {
            var index = new TIndexCreator();

            DeleteByIndex(store, index.IndexName, new IndexQuery { Query = query });
        }

        private static void DeleteByIndex(IDocumentStore store, string indexName, IndexQuery query)
        {
            store.DatabaseCommands.DeleteByIndex(
                indexName,
                query,
                options: new BulkOperationOptions { AllowStale = false })
            .WaitForCompletion();
        }
	}
}