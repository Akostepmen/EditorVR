#if UNITY_EDITOR
using UnityEditor.Experimental.EditorVR.Utilities;
using UnityEngine;

namespace UnityEditor.Experimental.EditorVR.Tools
{ 
    static class AudioComponentUtils
    {
        public static void Add(GameObject go, AudioComponentType type)
        {
            switch (type)
            {
                case AudioComponentType.Source:
                    AddSource(go);
                    break;
                case AudioComponentType.ReverbZone:
                    AddReverbZone(go);
                    break;
                case AudioComponentType.ReverbFilter:
                    AddReverbFilter(go);
                    break;
                case AudioComponentType.LowPassFilter:
                    AddLowPassFilter(go);
                    break;
                case AudioComponentType.HighPassFilter:
                    AddHighPassFilter(go);
                    break;
                case AudioComponentType.ChorusFilter:
                    AddChorusFilter(go);
                    break;
                case AudioComponentType.DistortionFilter:
                    AddDistortionFilter(go);
                    break;
                case AudioComponentType.EchoFilter:
                    AddEchoFilter(go);
                    break;
            }
        }

        public static AudioSource AddSource(GameObject go)
        {
            return go.TryAddComponent<AudioSource>();
        }

        public static AudioReverbZone AddReverbZone(GameObject go)
        {
            return go.TryAddComponent<AudioReverbZone>();
        }

        public static AudioReverbFilter AddReverbFilter(GameObject go)
        {
            return TryAddFilter<AudioReverbFilter>(go);
        }

        public static AudioLowPassFilter AddLowPassFilter(GameObject go)
        {
            return TryAddFilter<AudioLowPassFilter>(go);
        }

        public static AudioHighPassFilter AddHighPassFilter(GameObject go)
        {
            return TryAddFilter<AudioHighPassFilter>(go);
        }

        public static AudioChorusFilter AddChorusFilter(GameObject go)
        {
            return TryAddFilter<AudioChorusFilter>(go);
        }

        public static AudioDistortionFilter AddDistortionFilter(GameObject go)
        {
            return TryAddFilter<AudioDistortionFilter>(go);
        }

        public static AudioEchoFilter AddEchoFilter(GameObject go)
        {
            return TryAddFilter<AudioEchoFilter>(go);
        }

        static T TryAddFilter<T>(GameObject go) where T: Component
        {
            if (HasSourceOrListener(go))
            {
                return go.TryAddComponent<T>();
            }
            else
            {
                // considering putting in an ifdef for performance reasons
                Debug.LogWarning("Filters need an Source or Listener on the object!");
                return null;
            }
        }

        static bool HasSourceOrListener(GameObject go)
        {
            if (go.GetComponent<AudioSource>() != null)
                return true;
            else if (go.GetComponent<AudioListener>() != null)
                return true;
            
            return false;
        }
    }
}
#endif
