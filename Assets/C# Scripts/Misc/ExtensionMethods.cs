using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;


public static class ExtensionMethods
{
    #region Invoke

    /// <summary>
    /// Call function after a delay
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="f">Function to call.</param>
    /// <param name="delay">Wait time before calling function.</param>
    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }

    public static void Invoke<T>(this MonoBehaviour mb, Action<T> f, T param, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, param, delay));
    }

    private static IEnumerator InvokeRoutine(Action f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f();
    }

    private static IEnumerator InvokeRoutine<T>(Action<T> f, T param, float delay)
    {
        yield return new WaitForSeconds(delay);
        f(param);
    }

    #endregion


    #region SetParent

    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot)
    {
        if (parent == null)
        {
            Debug.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
    }
    public static void SetParent(this Transform trans, Transform parent, bool keepLocalPos, bool keepLocalRot, bool keepLocalScale)
    {
        if (parent == null)
        {
            Debug.LogWarning("You are trying to set a transform to a parent that doesnt exist, this is not allowed");
            return;
        }

        trans.SetParent(parent);
        if (!keepLocalPos)
        {
            trans.localPosition = Vector3.zero;
        }
        if (!keepLocalRot)
        {
            trans.localRotation = Quaternion.identity;
        }
        if (!keepLocalScale)
        {
            trans.localScale = Vector3.one;
        }
    }

    #endregion


    #region TryGetComponent(s)

    public static bool TryGetComponentInChildren<T>(this Transform trans, out T component, bool includeInactive = false) where T : Component
    {
        component = trans.GetComponentInChildren<T>(includeInactive);
        return component != null;
    }

    public static bool TryGetComponentsInChildren<T>(this Transform trans, out T[] components, bool includeInactive = false) where T : Component
    {
        components = trans.GetComponentsInChildren<T>(includeInactive);

        return components.Length > 0;
    }

    public static bool TryGetComponentInParent<T>(this Transform trans, out T component) where T : Component
    {
        component = trans.GetComponentInParent<T>();
        return component != null;
    }

    public static bool TryGetComponentsInParent<T>(this Transform trans, out T[] component) where T : Component
    {
        component = trans.GetComponentsInParent<T>();
        return component != null;
    }

    public static bool TryFindObjectOfType<T>(this UnityEngine.Object obj, out T component, bool includeInactive = false) where T : Component
    {
        component = UnityEngine.Object.FindObjectOfType<T>(includeInactive);
        return component != null;
    }

    #endregion


    #region HasComponent

    public static bool HasComponent<T>(this Transform trans) where T : Component
    {
        return trans.GetComponent<T>() != null;
    }

    public static bool HasComponentInChildren<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInChildren<T>(includeInactive) != null;
    }

    public static bool HasComponentInParent<T>(this Transform trans, bool includeInactive = false) where T : Component
    {
        return trans.GetComponentInParent<T>(includeInactive) != null;
    }

    #endregion
}


public static class VectorLogic
{
    /// <summary>
    /// Instantly move a vector3 towards the new Vector3, up to maxDistance
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="maxDist"></param>
    /// <returns>The new Position</returns>
    public static Vector3 InstantMoveTowards(Vector3 from, Vector3 to, float maxDist)
    {
        // Calculate the direction vector and its magnitude
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        // If the distance is less than or equal to maxDist, move directly to the target
        if (distance <= maxDist)
        {
            return to;
        }

        // Normalize the direction and scale by maxDist
        Vector3 move = direction.normalized * maxDist;

        // Return the new position
        return from + move;
    }


    public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = math.clamp(value.x, min.x, max.x);
        value.y = math.clamp(value.y, min.y, max.y);
        value.z = math.clamp(value.z, min.z, max.z);

        return value;
    }


    public static Vector3 ClampDirection(this Vector3 value, Vector3 clamp)
    {
        // Calculate the scale factors for each axis
        float scaleX = math.abs(value.x) > clamp.x ? math.abs(clamp.x / value.x) : 1f;
        float scaleY = math.abs(value.y) > clamp.y ? math.abs(clamp.y / value.y) : 1f;
        float scaleZ = math.abs(value.z) > clamp.z ? math.abs(clamp.z / value.z) : 1f;

        // Use the smallest scale factor to preserve direction
        float scale = math.min(scaleX, math.min(scaleY, scaleZ));

        // Scale the vector uniformly
        return value * scale;
    }
}


public static class EzRandom
{
    [ThreadStatic]
    private static Unity.Mathematics.Random random;


    // Initialize the random instance
    static EzRandom()
    {
        // Generate a seed for the random using the current time
        random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
    }


    public static void ReSeed()
    {
        random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
    }
    public static void ReSeed(uint seed)
    {
        random = new Unity.Mathematics.Random(seed);
    }


    public static int Range(int min, int max)
    {
        EnsureInitialized();

        return random.NextInt(min, max);
    }

    public static uint Range(uint min, uint max)
    {
        EnsureInitialized();

        return random.NextUInt(min, max);
    }

    public static float Range(float min, float max)
    {
        EnsureInitialized();

        return random.NextFloat(min, max);
    }

    public static Vector3 Range(Vector3 min, Vector3 max)
    {
        EnsureInitialized();

        Vector3 vec;
        vec.x = Range(min.x, max.x);
        vec.y = Range(min.y, max.y);
        vec.z = Range(min.z, max.z);


        return vec;
    }

    public static float3 Range(float3 min, float3 max)
    {
        EnsureInitialized();

        float3 vec;
        vec.x = Range(min.x, max.x);
        vec.y = Range(min.y, max.y);
        vec.z = Range(min.z, max.z);


        return vec;
    }


    public static int3 Range(int3 min, int3 max)
    {
        EnsureInitialized();

        int3 vec;
        vec.x = Range(min.x, max.x);
        vec.y = Range(min.y, max.y);
        vec.z = Range(min.z, max.z);


        return vec;
    }

    public static Color RandomColor(bool randomizeAlpha = false)
    {
        EnsureInitialized();

        Color color;
        color.r = random.NextFloat();
        color.g = random.NextFloat();
        color.b = random.NextFloat();
        color.a = randomizeAlpha ? random.NextFloat() : 1;


        return color;
    }

    private static void EnsureInitialized()
    {
        if (random.Equals(default(Unity.Mathematics.Random)))
        {
            ReSeed();
        }
    }
}