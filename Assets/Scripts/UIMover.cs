using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMover : MonoBehaviour
{

    public Vector3 moveSpeed;
    bool moving = false;

    float startingLifetime = 3;
    float lifeTime = 3;

    Vector3 startingPos;

    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.localPosition;
        Debug.Log($"Starting pos {startingPos} {transform.position}");
        lifeTime = startingLifetime;
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + moveSpeed, Time.fixedDeltaTime * 10);
            //transform.position = 
            lifeTime -= Time.deltaTime;
            Fade();
        }
    }

    public void StartMoving()
    {
        moving = true;
        gameObject.SetActive(true);
    }

    void Fade()
    {
        if (lifeTime < 0.6f)
        {
            moving = false;
            Reset();
            return;
        }
        Color currentColour = GetComponent<Image>().color;
        float t = lifeTime / (startingLifetime*0.5f);
        GetComponent<Image>().color = new Color(currentColour.r, currentColour.g, currentColour.b, (t*t));
        
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        //transform.position = startingPos;
        lifeTime = startingLifetime;

    }
}
