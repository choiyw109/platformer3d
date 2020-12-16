namespace NonStandard.TouchGui {
	public class TouchGuiButton : TouchColliderSensitive {
		public Inputs.KBind.EventSet eventSet;

		public override void PressDown(TouchCollider tc) {
			base.PressDown(tc);
			eventSet.DoPress();
		}
		public override void Hold(TouchCollider tc) {
			base.Hold(tc);
			eventSet.DoHold();
		}
		public override void Release(TouchCollider tc) {
			base.Release(tc);
			eventSet.DoRelease();
		}
	}
}