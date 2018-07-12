using System.Collections.Generic;
using EwlRealWorld.Library.DataAccess.CommandConditions;

namespace EwlRealWorld.Library.DataAccess.TableRetrieval {
	partial class ArticleTagsTableRetrieval {
		public static IEnumerable<Row> GetRowsLinkedToArticle( int articleId ) => GetRows( new ArticleTagsTableEqualityConditions.ArticleId( articleId ) );
	}
}