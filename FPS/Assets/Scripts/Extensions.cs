using UnityEngine;

public static class Extensions
{
    public static GameObject CreateCopyAsGameObject(this AudioSource source, Transform parent)
    {
        var newSound = new GameObject
        {
            name = $"{source.gameObject.name}AudioSourceCopy"
        };
        newSound.transform.parent = parent;
        newSound.transform.localPosition = Vector3.zero;
        var newAS = newSound.AddComponent<AudioSource>();
        newAS.playOnAwake = source.playOnAwake;
        newAS.clip = source.clip;
        newAS.volume = source.volume;
        newAS.maxDistance = source.maxDistance;
        newAS.minDistance = source.minDistance;
        newAS.spatialBlend = source.spatialBlend;
        newAS.pitch = source.pitch;

        return newSound;
    }
}