﻿using UnityEngine;
using UnityEngine.UI;



using System.Collections;

using TechTweaking.Bluetooth;
using System;

public static class ExtensionMethods
{
    public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}

public class PlayerController : MonoBehaviour {
	
	public float speed;
	private Rigidbody rb;
    private BluetoothDevice device;
    private Text debugText;


    // At the start of the game..
    void Start ()
	{
		rb = GetComponent<Rigidbody>();

        debugText = GameObject.Find("DebugText").GetComponent<Text>();

        BluetoothAdapter.enableBluetooth(); // Force Enabling Bluetooth
        device = new BluetoothDevice();
        device.Name = "HC-05"; // arduino needs to be paired with that name
        //device.MacAddress = "98:D3:32:31:1D:57";

        /*
        * 10 equals the char '\n' which is a "new Line" in Ascci representation, 
        * so the read() method will retun a packet thvisuat was ended by the byte 10. simply read() will read lines.
        * If you don't use the setEndByte() method, device.read() will return any available data (line or not), then you can order them as you want.
        */
        device.setEndByte((byte)';');
        device.connect();
        debugText.text = "Unreal is better";
    }


	void FixedUpdate ()
	{



#if UNITY_EDITOR
        debugText.text = "unityEditor";
        float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

        Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
        movement = Camera.main.transform.TransformDirection(movement);

#else
        Vector3 movement = ReadAxisFromBTDevice();
        NormelizeMovment(ref movement);
#endif
        movement.y = 0;
        Vector3 force = movement.normalized * speed;

        rb.AddForce (movement * speed);

        var tempVec = rb.velocity;
        for (int i = 0; i < 3; i++)
        {
            if (tempVec[i] > 10)
            {
                tempVec[i] = 10;
            }
            else if(tempVec[i] < -10)
            {
                tempVec[i] = -10;
            }
        }

        rb.velocity = tempVec;

        print(rb.velocity.ToString());
	}

    Vector3 ReadAxisFromBTDevice()
    {
        Vector3 axis = new Vector3();

        if (true ||device.IsDataAvailable)
        {
            byte[] msg = device.read();//because we called setEndByte(10)..read will always return a packet excluding the last byte 10.

            if (msg != null && msg.Length > 0)
            {
                string content = System.Text.ASCIIEncoding.ASCII.GetString(msg);
                //debugText.text = content; // print to debug text
                var splitContent = content.Split(',');

                for (int i = 0; i < 3; i++)
                {
                    axis[i] = float.Parse(splitContent[i]);
                }
            }
        }
        return axis;
    }

    void NormelizeMovment(ref Vector3 vec)
    {
        debugText.text = vec.ToString();
        Vector3 center = new Vector3(0, 750 ,-1500);
        vec -= center;

        for(int i = 0; i < 3; i++)
        {
            if( Math.Abs(vec[i]) < 750)
            {
                vec[i] = 0;
            }
        }

        vec.x = Remap(vec.y, -5000, 5000, -1, 1);
        vec.z = Remap(vec.z, -5000, 5000, -1, 1);
        vec.y = 0;
        debugText.text += " | " + vec.ToString();
    }

    public static float Remap(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        if(value < fromSource)
        {
            value = fromSource;
        }
        else if (value > toSource)
        {
            value = toSource;
        }
        return value / toSource;
    }
}
