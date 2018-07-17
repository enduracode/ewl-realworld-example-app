using System.Collections.Generic;
using System.Linq;

namespace EwlRealWorld.Library.DataAccess.TableRetrieval {
	partial class FavoritesTableRetrieval {
		public static ILookup<int, Row> ToArticleIdLookup( this IEnumerable<Row> rows ) => rows.ToLookup( i => i.ArticleId );
	}
}