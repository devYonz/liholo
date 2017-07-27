using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProfileFetch {
    private static string MEDIA_URL = "https://media.licdn.com/mpr/mpr/";
    private static string PROFILE_URL = "https://www.linkedin.com/voyager/api/identity/profiles/";
    private static string person;
    private static float timeout = 10.0f;


    public static IEnumerator fetchProfile(Person person, System.Action<string, Person> callback)
    {
        Debug.Log("Starting a request to get profile");
        string url = PROFILE_URL + person.name;

        var hdrs = new Dictionary<string, string>();
        hdrs.Add("accept", "application/json");
        // Fails because CSRF guard

        WWW personProfile = new WWW(url, null, hdrs);
 
        float elapsedTime = 0;
        while (!personProfile.isDone)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= ProfileFetch.timeout) break;
            yield return null;
        }
        // Do not call th ecall back if we run into errors
        if (!personProfile.isDone || !string.IsNullOrEmpty(personProfile.error))
        {
            if (!personProfile.isDone)
            {
                Debug.LogError("Timed out after 10 seconds");
                yield break;
            }
            Debug.LogError("Loading error: " + personProfile.error);
            Debug.LogWarning("Error response text: " + personProfile.text);
            yield break;
        }

        string jsonString = personProfile.text;
        Debug.Log("The response output is: ");
        Debug.LogWarning(jsonString);
        Debug.Log("The requested url: " + PROFILE_URL + person);
        callback(jsonString, person);
    }
}