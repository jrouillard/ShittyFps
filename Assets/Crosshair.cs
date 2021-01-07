using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Crosshair : MonoBehaviour {
    public GameObject cross;
    
    public Transform maincamera;
    private RaycastHit hit;
    
    // Start is called before the first frame update
    void Start() {
        cross.gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update() {
        if (Physics.Raycast(maincamera.position, maincamera.forward, 300)) {
            cross.GetComponent<Image>().material.SetColor("_Color", Color.red);
        } else {
            cross.GetComponent<Image>().material.SetColor("_Color", Color.white);
        }
    }
}
