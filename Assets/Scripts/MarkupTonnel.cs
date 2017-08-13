using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkupTonnel : MonoBehaviour
{
    public GameObject TonnelMouseoverMarkup;
    public GameObject TonnelMarkup;

    // Use this for initialization
    void Start()
    {
        TonnelMouseoverMarkup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000.0f) &&
            (hit.transform.tag.Equals(GameManager.ENVIRONMENT_TAG) || hit.transform.tag.Equals(GameManager.MARKUP_TAG)))
        {
            Vector3 blockPosition = hit.point + hit.normal * -1f;
            
            blockPosition.x = (float) Math.Round(blockPosition.x, MidpointRounding.AwayFromZero);
            blockPosition.y = (float) Math.Round(blockPosition.y, MidpointRounding.AwayFromZero);
            blockPosition.z = (float) Math.Round(blockPosition.z, MidpointRounding.AwayFromZero);

            TonnelMouseoverMarkup.transform.position = blockPosition;
            TonnelMouseoverMarkup.SetActive(true);
        }
        else
        {
            TonnelMouseoverMarkup.SetActive(false);
        }
        
//        if (Input.GetMouseButtonDown(0))
//        {
//            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//            if (Physics.Raycast(ray, out hit, 1000.0f))
//            {
//                Vector3 blockPosition = hit.point + hit.normal / 2.0f;
//                blockPosition.x = (float) Math.Round(blockPosition.x, MidpointRounding.AwayFromZero);
//                blockPosition.y = (float) Math.Round(blockPosition.y, MidpointRounding.AwayFromZero);
//                blockPosition.z = (float) Math.Round(blockPosition.z, MidpointRounding.AwayFromZero);
//
//                Instantiate(TonnelMarkup, blockPosition, Quaternion.identity);
//            }
//        }
    }
}