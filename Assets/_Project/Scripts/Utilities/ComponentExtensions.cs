using UnityEngine;

namespace JingHongLu.Utilities
{
    public static class ComponentExtensions
    {
        public static bool TryGetInSelfOrParent<T>(this Component component, out T result)
            where T : Component
        {
            result = null;

            if (component == null)
            {
                return false;
            }

            if (component.TryGetComponent(out result))
            {
                return true;
            }

            result = component.GetComponentInParent<T>();
            return result != null;
        }

        public static bool TryGetInSelfOrChildren<T>(
            this Component component,
            out T result,
            bool includeInactive = true)
            where T : Component
        {
            result = null;

            if (component == null)
            {
                return false;
            }

            if (component.TryGetComponent(out result))
            {
                return true;
            }

            result = component.GetComponentInChildren<T>(includeInactive);
            return result != null;
        }

        public static bool TryGetInSelfOrParent<T>(this GameObject gameObject, out T result)
            where T : Component
        {
            result = null;

            return gameObject != null
                && gameObject.transform.TryGetInSelfOrParent(out result);
        }

        public static bool TryGetInSelfOrChildren<T>(
            this GameObject gameObject,
            out T result,
            bool includeInactive = true)
            where T : Component
        {
            result = null;

            return gameObject != null
                && gameObject.transform.TryGetInSelfOrChildren(out result, includeInactive);
        }
    }
}
