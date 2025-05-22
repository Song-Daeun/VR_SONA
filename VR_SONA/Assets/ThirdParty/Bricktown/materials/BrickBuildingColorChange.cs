using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickBuildingColorChange : MonoBehaviour
{
    private Renderer myObj;
	[SerializeField] private Color Brick1;
	// [SerializeField] private Color Brick2;
	// [SerializeField] private Color Brick3;
	// [SerializeField] private Color Brick4;
	[SerializeField] private Color Concrete_1;
	[SerializeField] private Color Concrete_2;
	[SerializeField] private Color Roof;
	
    void Start()
    {
        myObj = GetComponent<Renderer>();
		
		myObj.material.SetColor("_Color_7",Brick1);
		myObj.material.SetColor("_Color_1",Brick1);
		myObj.material.SetColor("_Color_3",Brick1);
		myObj.material.SetColor("_Color_2",Brick1);
		myObj.material.SetColor("_Color_4",Concrete_1);
		myObj.material.SetColor("Color_5",Concrete_2);
		myObj.material.SetColor("_Color_6",Roof);
		
		
		
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
