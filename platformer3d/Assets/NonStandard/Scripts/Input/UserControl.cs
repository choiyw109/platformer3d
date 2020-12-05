using UnityEngine;
using UnityEngine.EventSystems;

namespace NonStandard {
public class UserControl : MonoBehaviour {

	public static EventSystem CreateEventSystem()
	{
		EventSystem es = EventSystem.current;
		if (es == null)
		{
			GameObject evOb = new GameObject("EventSystem");
			es = evOb.AddComponent<EventSystem>();
			evOb.AddComponent<StandaloneInputModule>();
		}
		return es;
	}

	[System.Serializable]
	public struct KeyModifier
	{
		public KeyCode key;
		public bool notPressedExplicitly;
		public bool IsSatisfied() { return UnityEngine.Input.GetKey(key) != notPressedExplicitly; }
	}

	public static bool IsModifiersSatisfied(KeyModifier[] modifiers)
	{
		bool working = true;
		if (modifiers != null && modifiers.Length > 0)
		{
			for (int i = 0; i < modifiers.Length; ++i)
			{
				if (!modifiers[i].IsSatisfied()) { working = false; break; }
			}
		}
		return working;
	}

}
}