using UnityEngine;

namespace NonStandard {
public class AxisRouteToUnityEvent : AxisRoute
{
	[System.Serializable]
	public class UnityEventFloat : UnityEngine.Events.UnityEvent<float> { }

	public UnityEventFloat unityEvent;
	private float lastValue = 0;
	public float outputMultiplier = 1;

	void Update()
	{
		if (IsModifiersSatisfied())
		{
			float value = UnityEngine.Input.GetAxis(axis);
			if (value != lastValue)
			{
				unityEvent.Invoke(value * outputMultiplier);
				lastValue = value;
			}
		}
	}
}
}