using UnityEngine;

public static class GlobalHelper 
{
   public static string GenerateUniqueID(GameObject obj)
    {
        //Get the Object name, pos to create a ID to save
        return $"{obj.scene.name}_{obj.transform.position.x}_{obj.transform.position.y}";
    }
}
