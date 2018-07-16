using System.Collections.Generic;
using System.Linq;
using EwlRealWorld.Library.DataAccess.CommandConditions;

namespace EwlRealWorld.Library.DataAccess.TableRetrieval {
	partial class ArticleTagsTableRetrieval {
		public static IEnumerable<Row> GetRowsLinkedToArticle( int articleId ) => GetRows( new ArticleTagsTableEqualityConditions.ArticleId( articleId ) );
		public static IEnumerable<Row> OrderByTagId( this IEnumerable<Row> rows ) => rows.OrderBy( i => i.TagId );
		public static ILookup<int, Row> ToArticleIdLookup( this IEnumerable<Row> rows ) => rows.ToLookup( i => i.ArticleId );
	}
}