using EwlRealWorld.Library.DataAccess.Modification;

namespace EwlRealWorld.Library.DataAccess {
	public static class MainSequence {
		public static int GetNextValue() {
			return MainSequenceModification.InsertRow();
		}
	}
}