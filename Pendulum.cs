using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Pendulum : MonoBehaviour {


    Vector3 bobRestPosition;

    float rodLength = 1f;

    Vector3 velocity = new Vector3();
    Vector3 bobPosition;

    public GameObject Bob;
    public Button StartButton;
    public GameObject Pivot;
    public Button RadiansButton;

    void Start () {
        this.bobRestPosition = this.Bob.transform.position;
        this.StartButton.onClick.AddListener(SetUpPendulum);
        //this.RadiansButton.onClick.AddListener(AngleUnit);
    }

    float dt = 0.001f;
    float currentTime = 0f;
    float timeSinceLastFrame = 0f;
    float timeBetweenFrames = 0f;

    void Update() {
        // Calculate time difference between Frames (how often Update() is called)
        this.timeBetweenFrames = Time.time - currentTime;
        this.currentTime = Time.time;
        this.timeSinceLastFrame += timeBetweenFrames;

        // Only update the pendulums position if a milisecond has passed
        while (this.timeSinceLastFrame >= this.dt) {
            // Store the pendulum position and update it
            this.bobPosition = this.MovePendulum(this.bobPosition, this.dt);
            timeSinceLastFrame -= this.dt;
            updateOutput();
        }

        Vector3 positionAfterFrame = this.bobPosition*this.timeSinceLastFrame/this.dt + this.bobPosition*(1f-this.timeSinceLastFrame/this.dt);
        this.Bob.transform.position = positionAfterFrame;
    }

    void SetUpPendulum() {
        //sets Initial position of the bob depending on the value of the sliders
        Bob.transform.position = new Vector3(
            GameObject.Find("Canvas/RodSlider").GetComponent <Slider>().value*Mathf.Sin(GameObject.Find("Canvas/AngleSlider").GetComponent <Slider>().value), 
            5-GameObject.Find("Canvas/RodSlider").GetComponent <Slider>().value*Mathf.Cos(GameObject.Find("Canvas/AngleSlider").GetComponent <Slider>().value), 
            0);
        //works out the rod length from the distance between the bob and pivot
        this.rodLength = Vector3.Distance(Pivot.transform.position, Bob.transform.position);
        this.velocity = Vector3.zero;

        this.bobPosition = this.Bob.transform.position;
    }

    // Method: return a 3D vector for the new location of the bob
   Vector3 MovePendulum(Vector3 bobPosition, float time) {
        //get infomation about Gravity
        float gravity = GameObject.Find("Canvas/GravitySlider").GetComponent <Slider>().value;
        this.velocity += Physics.gravity.normalized * gravity * time;

        //find out where the pivot is in relation to the bob
        Vector3 pivotPosition = this.Pivot.transform.position;

        //using Newtons equations, find the downwards force acting on the pendulum
        float gravitationalDisplacement = Vector3.Distance(pivotPosition, this.bobPosition + this.velocity * time);

        //using Newtons equations, find out the force acting on the pendulum 
        if(gravitationalDisplacement > this.rodLength || (gravitationalDisplacement == this.rodLength)) {
            
            //convert it from polar to Cartisan format
            float inclinationAngle = Vector3.Angle(this.bobPosition-pivotPosition, Physics.gravity.normalized);

            float tension = gravity * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
            float bobCentripetalForce = calcualteCentripetalForce(this.velocity, this.rodLength);
            tension += bobCentripetalForce;

            //transfer the force into a velocity and apply to pendulum
            this.velocity += (pivotPosition - this.bobPosition).normalized * tension * time;
        }
        //apply the velocity to the current position to get the new position
        return newVectorLocationOfPivot(pivotPosition, time);
    }

    void updateOutput() {
        var gravityValue = GameObject.Find("Canvas/GravitySlider").GetComponent <Slider>().value;
        var angle = GameObject.Find("Canvas/AngleSlider").GetComponent <Slider>().value;
        var rod = GameObject.Find("Canvas/RodSlider").GetComponent <Slider>().value;
        var angleRatio = calcualteAngleRatio(angle);
        GameObject.Find("Canvas/Output").GetComponent <TMP_Text>().text = "x = " + getXOutput(rodLength, gravityValue, angleRatio) + "\ny = " + getYOutput(rodLength, gravityValue, angleRatio);
        GameObject.Find("Canvas/Gravity").GetComponent <TMP_Text>().text = "Gravity: " + gravityValue;
        GameObject.Find("Canvas/InitialAngle").GetComponent <TMP_Text>().text = "Inital Angle: " + putNumberTo2DP(angle/3.1416f) + "π";
        GameObject.Find("Canvas/RodLength").GetComponent <TMP_Text>().text = "Inital Rod Length: " + rod;
    }

    public float calcualteAngleRatio(float angle) {
		return angle/(0.5f*3.1416f);
	}

    public float putNumberTo2DP(float number) {
		return Mathf.Round(number*100)/100;
	}

    public string getXOutput(float rodLength, float gravityValue, float angleRatio) {
        return rodLength + "*sin(" + gravityValue + "t)*" + putNumberTo2DP(angleRatio);
    }

    public string getYOutput(float rodLength, float gravityValue, float angleRatio) {
        return "-" + rodLength +  " + " + rodLength + "*cos(" + gravityValue + "t)*" + putNumberTo2DP(angleRatio);
    }

    public Vector3 newVectorLocationOfPivot(Vector3 start, float time) {

        // distance = velocity * time
        Vector3 positionAfterFrame = Vector3.zero;
        positionAfterFrame += this.velocity * time;
        float distanceFromOldPostion = Vector3.Distance(start, this.bobPosition + positionAfterFrame);

        Vector3 end = this.bobPosition + positionAfterFrame;

        // if it has moved more than the rod length, use the rod length
        var distanceMoved = distanceFromOldPostion <= this.rodLength ? distanceFromOldPostion : this.rodLength;
        return start + (distanceMoved * Vector3.Normalize(end - start));
    }

    public float calcualteCentripetalForce(Vector3 velocity, float rodLength) {
		return ((Mathf.Pow(velocity.magnitude, 2))/rodLength);
	}
}