using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeController : MonoBehaviour
{
    public float CAMERA_SCALE = 2;
    public float ROTATE_SCALE = 2;
    public float SNAP_SCALE = 1;
    public float CUBELET_GAP = 1.05F; // MUST BE EQUAL TO THE GAP BETWEEN CUBELETS!!
    public int DIM = 3;
    private GameObject rubixCube;
    private Transform[,,] transforms = new Transform[3, 3, 3];
    private List<Transform> rotateGroup;
    private Vector3 rotateAxis = Vector3.zero;
    private Vector3 nonAxis = Vector3.zero;
    private float rotateAngle = 0;
    private float totRotateAngle = 0;
    private SortedDictionary<double, (Vector3 axis, Vector2 persp)> sorted;
    private double sortedKey;
    private Dictionary<Vector3, List<Side>> sides;
    private Dictionary<Transform, Vector3Int> indices;
    //private Dictionary<Vector3, List<Transform>> groups;

    // Start is called before the first frame update
    void Start()
    {
        sides = new Dictionary<Vector3, List<Side>>
        {
            { Vector3.right, new List<Side>() { Side.FRONT, Side.DOWN, Side.BACK, Side.UP } },
            { Vector3.up, new List<Side>() { Side.FRONT, Side.RIGHT, Side.BACK, Side.LEFT } },
            { Vector3.forward, new List<Side>() { Side.RIGHT, Side.DOWN, Side.LEFT, Side.UP } }
        };
        indices = new Dictionary<Transform, Vector3Int>();

        rubixCube = GameObject.Find("Rubix Cube");
        for (int i = 0; i < rubixCube.transform.childCount; i++) // Cubelets are ordered. Could potentially flatten 3D array to 1D
        {
            Transform child = rubixCube.transform.GetChild(i);
            //Debug.Log(child) + ", coords: " + i % 3 + ", " + (int)Math.Floor((i / 3.0)) % 3 + ", " + (int)Math.Floor((i / 9.0)) % 3);
            Debug.Log(child + ", worldPos: " + child.position);
            transforms[i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3] = child;
            indices[child] = new Vector3Int(i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If both LMB and RMB are pressed, reset and return (maybe change this later to make it so the key pressed before pressing both takes priority)
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) { 
            ResetController(); 
            return;
        }
        // If neither LMB or RMB or pressed, reset and return
        if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
        {
            ResetController();
            return;
        }

        // RMB take priority over LMB
        if (Input.GetMouseButton(1))
        {
            // Rotate whole cube with RMB
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            //Debug.Log($"dx: {dx}, dy: {dy}");

            Quaternion rotation = Quaternion.Euler(dy * CAMERA_SCALE, -dx * CAMERA_SCALE, 0);
            rubixCube.transform.rotation = rotation * rubixCube.transform.rotation;
        }
        else if (Input.GetMouseButton(0))
        {
            // Logic for rotating sides of the cube with LMB
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                nonAxis = hit.transform.gameObject.GetComponent<SideComponent>().side.GetAxis();
                //Debug.Log(hit.transform.gameObject.name + ", " +  hit.transform.parent.name + ", " + hit.transform.gameObject.GetComponent<Renderer>().material.name);
                //Debug.Log(hit.transform.gameObject.name + ", " + hit.transform.gameObject.GetComponent<SideComponent>().side + ", " + hit.transform.gameObject.GetComponent<SideComponent>().side.GetAxis());
            }

            //float totX = 0;
            //float totY = 0;
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            if (nonAxis != Vector3.zero)
            {
                if (rotateAxis == Vector3.zero)
                {
                    Vector2 mouse = new Vector2(dx, dy);
                    if (mouse == Vector2.zero) return;

                    Vector3 pointX_Axis = rubixCube.transform.rotation * Vector3.right;
                    Vector3 pointY_Axis = rubixCube.transform.rotation * Vector3.up;
                    Vector3 pointZ_Axis = rubixCube.transform.rotation * Vector3.forward;

                    Vector2 pointX_Persp = new Vector2(Vector3.Dot(Vector3.up, pointX_Axis), -1 * Vector3.Dot(Vector3.right, pointX_Axis));
                    Vector2 pointY_Persp = new Vector2(Vector3.Dot(Vector3.up, pointY_Axis), -1 * Vector3.Dot(Vector3.right, pointY_Axis));
                    Vector2 pointZ_Persp = new Vector2(Vector3.Dot(Vector3.up, pointZ_Axis), -1 * Vector3.Dot(Vector3.right, pointZ_Axis));

                    float xDot = Math.Abs(Vector2.Dot(pointX_Persp, mouse));
                    float yDot = Math.Abs(Vector2.Dot(pointY_Persp, mouse));
                    float zDot = Math.Abs(Vector2.Dot(pointZ_Persp, mouse));

                    //Debug.Log($"xDot = {xDot}, (Vector3.right, pointX_Persp) = {(Vector3.right, pointX_Persp)}");
                    //Debug.Log($"yDot = {yDot}, (Vector3.up, pointY_Persp) = {(Vector3.up, pointY_Persp)}");
                    //Debug.Log($"zDot = {zDot}, (Vector3.forward, pointZ_Persp) = {(Vector3.forward, pointZ_Persp)}");

                    sorted = new SortedDictionary<double, (Vector3, Vector2)>
                    {
                        [xDot] = (Vector3.right, pointX_Persp),
                        [yDot] = (Vector3.up, pointY_Persp),
                        [zDot] = (Vector3.forward, pointZ_Persp)
                    };

                    foreach (var kvp in sorted)
                    {
                        //Debug.Log($"Key = {kvp.Key}, Value = {kvp.Value}");
                        if (kvp.Value.axis == nonAxis) continue;
                        rotateAxis = kvp.Value.axis;
                        sortedKey = kvp.Key;
                        //Debug.Log($"sortedKey: {sortedKey}, rotateAxis: {rotateAxis}");
                    }

                    rotateGroup = new List<Transform>();
                    InititalizeGroups(hit);
                    totRotateAngle = 0;
                }

                //totX += dx;
                //totY += dy;
                //rotateAngle = Vector2.Dot(sorted[sortedKey].persp, new Vector2(totX, totY));
                //Debug.Log($"sortedKey: {sortedKey}");
                rotateAngle = Vector2.Dot(sorted[sortedKey].persp, new Vector2(dx, dy)) * ROTATE_SCALE;
                totRotateAngle += rotateAngle;

                foreach (var cubelet in rotateGroup)
                {
                    cubelet.transform.RotateAround(Vector3.zero, rubixCube.transform.rotation * rotateAxis, -rotateAngle); // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
                }

            }
        }
    }

    private void ResetController()
    {
        if (nonAxis != Vector3.zero)
        {
            float finalAngle = (float)Math.IEEERemainder(totRotateAngle, 90.0);
            foreach (var cubelet in rotateGroup)
            {
                // Maybe add snapping animation, scaled by SNAP_SCALE
                cubelet.transform.RotateAround(Vector3.zero, rubixCube.transform.rotation * rotateAxis, finalAngle); // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
                //AnimatedRotation animatedRotation = cubelet.transform.GetComponent<AnimatedRotation>();
                //animatedRotation.SetDirection() // set to the desired direction (https://kylewbanks.com/blog/animating-rotations-through-code-in-unity)
            }

            if (totRotateAngle != 0)
            {
                Debug.Log($"updating sides; totRotateAngle: {totRotateAngle}, finalAngle: {finalAngle}");
                UpdateSides(finalAngle);
            }
            totRotateAngle = 0;
            rotateAxis = Vector3.zero;
            nonAxis = Vector3.zero;
        }
        // Maybe empty rotateGroup? Not really necessary
        return;
    }

    private void UpdateSides(float finalAngle)
    {
        List<Side> sidesList = sides[rotateAxis];
        // AXES GO THROUGH SCREEN (1 rotation is CW)
        int rotates = ((int)Math.IEEERemainder((double)((int)(totRotateAngle - finalAngle) / 90), 4.0) + 4) % 4;
        int swap;

        for (int index = 0; index < rotates; index++)
        {
            Debug.Log($"rotate: {index + 1}/{rotates}");
            foreach (var cubelet in rotateGroup)
            {
                Vector3Int coords = indices[cubelet];
                if (rotateAxis == Vector3.right)
                {
                    transforms[coords.x, coords.z, DIM - coords.y - 1] = cubelet;
                    swap = coords.y;
                    coords.y = coords.z;
                    coords.z = DIM - swap - 1;
                    indices[cubelet] = coords;
                }
                
                else if (rotateAxis == Vector3.up)
                {
                    transforms[DIM - coords.z - 1, coords.y, coords.x] = cubelet;
                    swap = coords.z;
                    coords.z = coords.x;
                    coords.x = DIM - swap - 1;
                    indices[cubelet] = coords;
                }
                else if (rotateAxis == Vector3.forward)
                {
                    transforms[coords.y, DIM - coords.x - 1, coords.z] = cubelet;
                    swap = coords.x;
                    coords.x = coords.y;
                    coords.y = DIM - swap - 1;
                    indices[cubelet] = coords;
                }
            }
        }

        foreach (var cubelet in rotateGroup)
        {
            for (int i = 0; i < cubelet.transform.childCount; i++)
            {
                int index = sidesList.IndexOf(cubelet.transform.GetChild(i).GetComponent<SideComponent>().side);
                if (index != -1)
                {
                    cubelet.transform.GetChild(i).GetComponent<SideComponent>().side = sidesList[(index + rotates) % 4];
                }
            }
        }
    }

    private void InititalizeGroups(RaycastHit hit)
    {
        Side side = hit.transform.gameObject.GetComponent<SideComponent>().side;
        Vector3Int index = indices[hit.transform.parent];
        switch (side)
        {
            case Side.FRONT:
            case Side.BACK:
                {
                    if (rotateAxis == Vector3.right)
                    {
                        for (int j = 0; j <= 2; j++)
                        {
                            for (int k = 0; k <= 2; k++)
                            {
                                // Formerly (int)(hit.transform.parent.localPosition.x / CUBELET_GAP) + 1  // USING LOCALPOSITION MIGHT BE A PROBLEM
                                rotateGroup.Add(transforms[index.x, j, k]);
                            }
                        }
                    }
                    else if (rotateAxis == Vector3.up)
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            for (int k = 0; k <= 2; k++)
                            {
                                rotateGroup.Add(transforms[i, index.y, k]);
                            }
                        }
                    }
                    break;
                }
            case Side.LEFT:
            case Side.RIGHT:
                {
                    if (rotateAxis == Vector3.up)
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            for (int k = 0; k <= 2; k++)
                            {
                                rotateGroup.Add(transforms[i, index.y, k]);
                            }
                        }
                    }
                    else if (rotateAxis == Vector3.forward)
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            for (int j = 0; j <= 2; j++)
                            {
                                rotateGroup.Add(transforms[i, j, index.z]);
                            }
                        }
                    }
                    break;
                }
            case Side.UP:
            case Side.DOWN:
                {
                    if (rotateAxis == Vector3.right)
                    {
                        for (int j = 0; j <= 2; j++)
                        {
                            for (int k = 0; k <= 2; k++)
                            {
                                rotateGroup.Add(transforms[index.x, j, k]);
                            }
                        }
                    }
                    else if (rotateAxis == Vector3.forward)
                    {
                        for (int i = 0; i <= 2; i++)
                        {
                            for (int j = 0; j <= 2; j++)
                            {
                                rotateGroup.Add(transforms[i, j, index.z]);
                            }
                        }
                    }
                    break;
                }
            case Side.INSIDE:
            default: break;
        }
    }
}

// AXES GO THROUGH SCREEN (1 rotation is CW)
// Z_AXIS
//    c c c 
//    c c c 
// |  c c c
// j i->

//   c(0, 0, 0) -> c(0, 2, 0)
//   c(1, 2, 1) -> c(2, 1, 1)

// Y_AXIS
// k i->
// |  c c c 
//    c c c 
//    c c c

// c(0, 0, 0) -> c(2, 0, 0)
// c(1, 0, 2) -> c(0, 0, 1)

// X_AXIS
//    c c c 
//    c c c  
//    c c c |
//      <-k j

// c(0, 0, 0) -> c(0, 0, 2)
// c(1, 1, 2) -> c(1, 2, 1)