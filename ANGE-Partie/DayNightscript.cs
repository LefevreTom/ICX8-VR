using UnityEngine;

public class DayNightScript : MonoBehaviour
{
    [Header("Configuration du cycle")]
    public Light directionalLight;             // Lumière principale de la scène (soleil/lune)
    public float cycleDurationInSeconds = 120f; // Durée d’un cycle complet (jour+nuit) en secondes

    [Header("Ambiance")]
    public Gradient lightColor;                // Couleur de la lumière selon le moment du cycle
    public Gradient ambientColor;              // Couleur de la lumière ambiante selon le moment du cycle

    private float timeOfDay = 0f;              // 0 = minuit, 0.5 = midi
    private float rotationSpeed;               // Vitesse de rotation calculée

    void Start()
    {
        if (directionalLight == null)
        {
            directionalLight = GetComponent<Light>();
        }
        rotationSpeed = 360f / cycleDurationInSeconds;
    }

    void Update()
    {
        // Avancer le temps
        timeOfDay += Time.deltaTime / cycleDurationInSeconds;
        timeOfDay %= 1f; // Boucle 0-1

        // Rotation continue de la lumière (simulateur soleil/lune)
        directionalLight.transform.rotation = Quaternion.Euler((timeOfDay * 360f) - 90f, 170f, 0f);

        // Changer la couleur et l’intensité selon l’heure
        directionalLight.color = lightColor.Evaluate(timeOfDay);
        RenderSettings.ambientLight = ambientColor.Evaluate(timeOfDay);

        // Facultatif : Ajouter skybox ou autres effets via Gradient ici
        // if (RenderSettings.skybox.HasProperty("_Tint")) {
        //     RenderSettings.skybox.SetColor("_Tint", skyboxColor.Evaluate(timeOfDay));
        // }
    }
}
