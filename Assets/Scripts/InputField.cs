using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputField : MonoBehaviour
{
    public Controller ctr;
    public SatScript scr;
    public OrbitPaths orb;

    public TMP_InputField inputPlanes;
    public TMP_InputField inputSats;
    public TMP_InputField inputInclination;
    public TMP_InputField inputPhase;
    public TMP_InputField inputAltitude;
    public TMP_InputField inputElevation;

    private int planes;
    private int satellites;
    private float inclination;
    private int f;
    private float altitude;
    private float elevation;

    public void PlanesChangeCheck()
    {
        int.TryParse(inputPlanes.text, out planes);
        if (planes < 1 || planes > 99)
        {
            inputPlanes.textComponent.color = Color.red;
        }
        else
        {
            inputPlanes.textComponent.color = Color.black;
        }
    }

    public void PlanesChangeCommit()
    {
        if ( planes > 1 && planes < 100)
        {
            PhaseChangeCheck();
            ctr.planes = planes;
            orb.ResetOrbits();
            scr.ResetConstellation();
            ctr.UpdateEndpointEntities();  // ResetConstelation = Destroy Entities
            ctr.UpdateWalkerDescription();
        }
    }

        public void SatellitesChangeCheck()
    {
        int.TryParse(inputSats.text, out satellites);
        if (satellites < 1 || satellites > 99)
        {
            inputSats.textComponent.color = Color.red;
        }
        else
        {
            inputSats.textComponent.color = Color.black;
        }
    }


    public void SatellitesChangeCommit()
    {
        if (satellites > 0 && satellites < 99)
        {
            ctr.satellites = satellites;
            scr.ResetConstellation();
            ctr.UpdateEndpointEntities();
            ctr.UpdateWalkerDescription();
        }
    }


    public void InclinationChangeCheck()
    {
        float.TryParse(inputInclination.text, out inclination);
        if (inclination < 1 || inclination > 90)
        {
            inputInclination.textComponent.color = Color.red;
        }
        else
        {
            inputInclination.textComponent.color = Color.black;
        }
    }


    public void InclinationChangeCommit()
    {
        if (inclination >= 1 && inclination <= 90)
        {
            ctr.inclination = inclination;
            orb.ResetOrbits();
            scr.ResetConstellation();
            ctr.UpdateEndpointEntities();
            ctr.UpdateWalkerDescription();
        }
    }


    public void PhaseChangeCheck()
    {
        int.TryParse(inputPhase.text, out f);
        int.TryParse(inputPlanes.text, out planes);
        if (f < 0 || f > planes - 1)
        {
            inputPhase.textComponent.color = Color.red;
        }
        else
        {
            inputPhase.textComponent.color = Color.black;
        }
    }


    public void PhaseChangeCommit()
    {
        if (f >= 0 && f <= planes - 1)
        {
            ctr.f= f;
            scr.ResetConstellation();
            ctr.UpdateEndpointEntities();
            ctr.UpdateWalkerDescription();
        }
    }

    public void AltitudeChangeCheck()
    {
        // Assuming LEO altitude is between 160 and 2000km.
        float.TryParse(inputAltitude.text, out altitude);
        if (altitude < 160 || altitude > 2000) 
        {
            inputAltitude.textComponent.color = Color.red;
        }
        else
        {
            inputAltitude.textComponent.color = Color.black;
        }
    }

    public void AltitudeChangeCommit()
    {
        if (altitude >= 160 && altitude <= 2000)
        {
            ctr.altitude = altitude;
            orb.ResetOrbits();
            scr.ResetConstellation();
            ctr.UpdateEndpointEntities();
        }
    }

    public void ElevationChangeCheck()
    {
        // Elevation is between 0 and 90°.
        float.TryParse(inputElevation.text, out elevation);
        if (elevation < 0 || elevation > 90)
        {
            inputElevation.textComponent.color = Color.red;
        }
        else
        {
            inputElevation.textComponent.color = Color.black;
        }
    }

    public void ElevationChangeCommit()
    {
        if (elevation >= 0 && elevation <= 90)
        {
            ctr.elevation = elevation;
            ctr.UpdateSatFootprint();
            scr.ResetConstellation();
            ctr.UpdateEndpointEntities();
        }
    }
}
