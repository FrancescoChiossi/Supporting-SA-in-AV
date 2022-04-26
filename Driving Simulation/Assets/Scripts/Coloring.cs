using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coloring : MonoBehaviour
{
    //changes car color

    private Color carColor;
    private int colorProb;
    private Material m_mat;


    void Awake()
    {
        m_mat = GetComponent<Renderer>().material;
        changeColor(m_mat);

    }

    private void changeColor(Material carMat)
    {
        colorProb = Random.Range(1, 10); //probability of car having a certain color
        if (colorProb < 4)
        { //black 3/10
            carMat.color = Color.black;
            carMat.SetColor("_ReflectColor", Color.black);
        }
        else if (colorProb < 5)
        { //white 1/10
            carMat.color = Color.white;
            carMat.SetColor("_ReflectColor", Color.white / 2);
        }
        else if (colorProb < 7)
        { //grey 1/5
            carMat.color = Color.grey;
            carMat.SetColor("_ReflectColor", Color.grey / 2);
        }
        else
        { //random color 2/5
            carMat.color = Random.ColorHSV(0f, 1f);
            carColor = carMat.color / 2f;
            carMat.SetColor("_ReflectColor", carColor);
        }
    }
}
