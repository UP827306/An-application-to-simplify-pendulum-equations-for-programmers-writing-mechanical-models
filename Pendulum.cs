using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Pendulum : MonoBehaviour {

    public GameObject Pivot;
    public GameObject Bob;
    public Button StartButton;
    public Button RadiansButton;

    Vector3 bobRestPosition;
    bool bobRestPositionSet = false;

    private float tensionForce = 0f;
    private float gravityForce = 0f;
    float rodLength = 1f;

    Vector3 currentVelocity = new Vector3();
    Vector3 currentStatePosition;
    private Vector3 gravitationPullDirection;
    private Vector3 rodTensionPullDirection;

    void Start () {
        this.bobRestPosition = this.Bob.transform.position;
        this.bobRestPositionSet = true;

        this.StartButton.onClick.AddListener(PendulumInit);
        //this.RadiansButton.onClick.AddListener(AngleUnit);
    }

    float dt = 0.01f;
    float currentTime = 0f;
    float timeBetweenFrames = 0f;

    void Update() {
        // Calculate time difference between timeBetweenFrames (how often Update() is called)
        float timeBetweenFrames = Time.time - currentTime;
        this.currentTime = Time.time;
        this.timeBetweenFrames += timeBetweenFrames;

        // Only update the pendulums position if 0.01 seconds have passed
        while (this.timeBetweenFrames >= this.dt) {
            // Store the pendulum position and update it
            this.currentStatePosition = this.PendulumUpdate(this.currentStatePosition, this.dt);
            timeBetweenFrames -= this.dt;
            updateOutput();
        }

        Vector3 newPosition = this.currentStatePosition*this.timeBetweenFrames/this.dt + this.currentStatePosition*(1f-this.timeBetweenFrames/this.dt);
        this.Bob.transform.position = newPosition;
    }

    [ContextMenu("Reset Pendulum Position")]
    void ResetPendulumPosition() {
        if(this.bobRestPositionSet)
            this.MoveBob(this.bobRestPosition);
        else
            this.PendulumInit();
    }

    [ContextMenu("Reset Pendulum Forces")]
    void ResetPendulumForces() {
        this.currentVelocity = Vector3.zero;

        this.currentStatePosition = this.Bob.transform.position;
    }

    void PendulumInit() {
        //sets Initial position of the bob depending on the value of the sliders
        Bob.transform.position = new Vector3(
            GameObject.Find("Canvas/RodSlider").GetComponent <Slider>().value*Mathf.Sin(GameObject.Find("Canvas/AngleSlider").GetComponent <Slider>().value), 
            5-GameObject.Find("Canvas/RodSlider").GetComponent <Slider>().value*Mathf.Cos(GameObject.Find("Canvas/AngleSlider").GetComponent <Slider>().value), 
            0);
        //works out the rod length from the distance between the bob and pivot
        this.rodLength = Vector3.Distance(Pivot.transform.position, Bob.transform.position);
        this.ResetPendulumForces();
    }

    void MoveBob(Vector3 resetBobPosition) {
        this.Bob.transform.position = resetBobPosition;
        this.currentStatePosition = resetBobPosition;
    }

    // Method: return a 3D vector for the new location of the bob
   Vector3 PendulumUpdate(Vector3 currentStatePosition, float time) {
        //get infomation about Gravity
        this.gravityForce = GameObject.Find("Canvas/GravitySlider").GetComponent <Slider>().value;
        this.gravitationPullDirection = Physics.gravity.normalized;
        this.currentVelocity += this.gravitationPullDirection * this.gravityForce * time;

        //find out where the bob is in relation to the pivot
        Vector3 bobPostion = this.currentStatePosition;  
        Vector3 pivotPostion = this.Pivot.transform.position;

        //using Newtons equations, find the downwards force acting on the pendulum
        float distanceAfterGravity = Vector3.Distance(pivotPostion, bobPostion + this.currentVelocity * time);

        //using Newtons equations, find out the force acting on the pendulum
        if(distanceAfterGravity > this.rodLength || (distanceAfterGravity == this.rodLength)) {

            this.rodTensionPullDirection = (pivotPostion - bobPostion).normalized;
            
            //convert it from polar to Cartisan format
            float inclinationAngle = Vector3.Angle(bobPostion-pivotPostion, this.gravitationPullDirection);

            this.tensionForce = GameObject.Find("Canvas/GravitySlider").GetComponent <Slider>().value * Mathf.Cos(Mathf.Deg2Rad * inclinationAngle);
            float centripetalForce = calcualteCentripetalForce(this.currentVelocity, this.rodLength);
            this.tensionForce += centripetalForce;

            //transfer the force into a velocity
            this.currentVelocity += this.rodTensionPullDirection * this.tensionForce * time;
        }
        //apply the velocity to the current position to get the new position
        return newVectorLocationOfPivot(pivotPostion, time);
    }

    void updateOutput() {
        var gravity = GameObject.Find("Canvas/GravitySlider").GetComponent <Slider>().value;
        var angle = GameObject.Find("Canvas/AngleSlider").GetComponent <Slider>().value;
        var rod = GameObject.Find("Canvas/RodSlider").GetComponent <Slider>().value;
        var angleRatio = calcualteAngleRatio(angle);
        GameObject.Find("Canvas/Output").GetComponent <TMP_Text>().text = "x = " + getXOutput(rodLength, gravity, angleRatio) + "\ny = " + getYOutput(rodLength, gravity, angleRatio);
        GameObject.Find("Canvas/Gravity").GetComponent <TMP_Text>().text = "Gravity: " + gravity;
        GameObject.Find("Canvas/InitialAngle").GetComponent <TMP_Text>().text = "Inital Angle: " + putNumberTo2DP(angle/3.1416f) + "π";
        GameObject.Find("Canvas/RodLength").GetComponent <TMP_Text>().text = "Inital Rod Length: " + rod;
    }

    public float calcualteAngleRatio(float angle) {
		return angle/(0.5f*3.1416f);
	}

    public float putNumberTo2DP(float number) {
		return Mathf.Round(number*100)/100;
	}

    public string getXOutput(float rodLength, float gravity, float angleRatio) {
        return rodLength + "*sin(" + gravity + "t)*" + putNumberTo2DP(angleRatio);
    }

    public string getYOutput(float rodLength, float gravity, float angleRatio) {
        return "-" + rodLength +  " + " + rodLength + "*cos(" + gravity + "t)*" + putNumberTo2DP(angleRatio);
    }

    public Vector3 newVectorLocationOfPivot(Vector3 start, float time) {

        // distance = velocity * time
        Vector3 newPosition = Vector3.zero;
        newPosition += this.currentVelocity * time;
        float distanceFromOldPostion = Vector3.Distance(start, this.currentStatePosition + newPosition);

        Vector3 end = this.currentStatePosition + newPosition;

        // if it has moved more than the rod length, use the rod length
        var distanceMoved = distanceFromOldPostion <= this.rodLength ? distanceFromOldPostion : this.rodLength;
        return start + (distanceMoved * Vector3.Normalize(end - start));
    }

    public float calcualteCentripetalForce(Vector3 currentVelocity, float rodLength) {
		return ((Mathf.Pow(currentVelocity.magnitude, 2))/rodLength);
	}
}