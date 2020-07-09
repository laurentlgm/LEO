using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dropdown : MonoBehaviour
{
    public Airports airp;
    public Controller ctr;
    public StraightDist sd;
    public Cables cbl;


    public TMP_Dropdown dropdownStart;
    public TMP_Dropdown dropdownFinish;
    public TMP_Dropdown dropdownFiber;
    public TMP_Text textStraightDist;
    public TMP_Text textPingAvg;


    public void Dropdown_StartIndexChanged(int index)
    {
        if (index != 0)
        {
            if (dropdownStart.value == dropdownFinish.value)
            {
                dropdownFinish.value = 0;
                ctr.finish = "";
            }
            textStraightDist.text = dropdownStart.options[index].text + " to " + dropdownFinish.options[dropdownFinish.value].text;
            ctr.start = dropdownStart.options[index].text;

            if (dropdownFinish.value != 0)
            {
                ctr.UpdateEndpointEntities();
                sd.DrawSlerpCurve();
                UpdatePingAvg();
            }
        }
    }


    public void Dropdown_FinishIndexChanged(int index)
    {
        if (index != 0)
        {
            if (dropdownStart.value == dropdownFinish.value)
            {
                dropdownStart.value = 0;
                ctr.start = "";
            }
            textStraightDist.text = dropdownStart.options[dropdownStart.value].text + " to " + dropdownFinish.options[index].text;
            ctr.finish = dropdownFinish.options[index].text;

            if (dropdownStart.value != 0)
            {
                ctr.UpdateEndpointEntities();
                sd.DrawSlerpCurve();
                UpdatePingAvg();
            }
        }
    }


    public void Dropdown_FiberIndexChanged(int index)
    {
        if (index != 0)
        {
            cbl.ClearFiberCables();
            cbl.DrawFiberCable(dropdownFiber.options[dropdownFiber.value].text);
        }
        if (dropdownStart.value == 0 && dropdownFinish.value == 0)
        {
            textStraightDist.text = "Please select origin and destination.";
        }
    }


    void Start()
    {
        airp = GameObject.Find("Airports").GetComponent<Airports>();
        ctr = GameObject.Find("Input Controller").GetComponent<Controller>();
        sd = GameObject.Find("StraightDist").GetComponent<StraightDist>();
        cbl = GameObject.Find("FiberCables").GetComponent<Cables>();

        textStraightDist.text = "Please select origin and destination.\n" +
            "You can also select a submarine cable for comparison.\n" +
            "Visit www.submarinecablemap.com to browse cables.";
    }

    void UpdatePingAvg()
    {
        List<string> tempList = new List<string>();
        tempList.Add(dropdownStart.options[dropdownStart.value].text.ToLower().Replace(" ", string.Empty));
        tempList.Add(dropdownFinish.options[dropdownFinish.value].text.ToLower().Replace(" ", string.Empty));
        tempList.Sort();
        textPingAvg.text = "Average Internet ping: " + airp.pingAvg[tempList[0] + "/" + tempList[1]] + " ms";
    }
}