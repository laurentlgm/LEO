    using UnityEngine;

public class SatOrbit : MonoBehaviour
{
    public Controller ctr;

    private GameObject planet; // Would be useful if Earth awas not always at (0,0,0)

    // Orbital Period Equation (Newton's form of Keplers third law):
    // T= SQRT[(4 • pi2 • R3) / (G * Mcentral)] = SQRT[(4 • pi2 • R3) / (G * Mcentral)]
    // T = SQRT [(4 • (3.1415)2 • (6.47 x 106 m)3) / (6.673 x 10-11 N m2/kg2) • (5.98x1024 kg) ]
    // To simplify Angular sat speed in the sky is 0.07/s:

    private float speed = 0.07f;

    void Awake()
    {   // "Find" is slow but used only once for every satellite since they are created dinamically:
        planet = GameObject.Find("Earth");
        ctr = GameObject.Find("Input Controller").GetComponent<Controller>();
    }

    void Update()
    {
        transform.RotateAround(planet.transform.position, -transform.up, speed * ctr.speedMultiplier * Time.deltaTime);
    }
}
