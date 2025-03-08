using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CubeController : MonoBehaviour
{
  public float CAMERA_SCALE = 2;
  public float ROTATE_SCALE = 2;
  public float SNAP_SCALE = 1;
  public float CUBELET_GAP = 1.05F; // MUST BE EQUAL TO THE GAP BETWEEN CUBELETS!!
  public int DIM = 3;
  private GameObject rubixCube;
  private TextMeshProUGUI liftLMB;
  private Transform[,,] cubeletIndexesToTransformMap = new Transform[3, 3, 3];
  private List<Transform> rotateGroup;
  private Vector3 rotateAxis = Vector3.zero;
  private Vector3 nonAxis = Vector3.zero;
  private GameObject startingSide;
  private float rotateAngle = 0;
  private float totRotateAngle = 0;
  private SortedDictionary<double, (Vector3 axis, Vector2 persp)> sorted;
  private double sortedKey;
  private Dictionary<Vector3, List<Side>> sides;
  private Dictionary<GameObject, Vector3Int> cubeletGameObjectToIndexesMap;
  private enum MouseState
  {
    UNSET,
    LMB,
    RMB,
    BOTH,
    NEITHER
  }
  private MouseState currentMouseState;
  private MouseState priorMouseState;
  private struct RemainingRotation
  {
    public RemainingRotation(List<Transform> rotateGroup, Vector3 rotateAxis, float startingAngle)
    {
      this.rotateGroup = rotateGroup;
      this.rotateAxis = rotateAxis;
      this.startingAngle = startingAngle;
      this.remainingAngle = startingAngle;
    }

    public List<Transform> rotateGroup;
    public Vector3 rotateAxis;
    public float startingAngle;
    public float remainingAngle;

    public override readonly string ToString() => $"({rotateGroup}, {rotateAxis}, {startingAngle}, {remainingAngle})";
  }
  private RemainingRotation remainingRotation;
  private bool lockLMB = false;
  private bool rotateLMB = false;

  // Start is called before the first frame update
  void Start()
  {
    sides = new Dictionary<Vector3, List<Side>>
        {
            { Vector3.right, new List<Side>() { Side.FRONT, Side.DOWN, Side.BACK, Side.UP } },
            { Vector3.up, new List<Side>() { Side.FRONT, Side.RIGHT, Side.BACK, Side.LEFT } },
            { Vector3.forward, new List<Side>() { Side.RIGHT, Side.DOWN, Side.LEFT, Side.UP } }
        };
    cubeletGameObjectToIndexesMap = new Dictionary<GameObject, Vector3Int>();

    rubixCube = GameObject.Find("Rubix Cube");
    liftLMB = GameObject.Find("LiftLMB").GetComponent<TextMeshProUGUI>();
    for (int i = 0; i < rubixCube.transform.childCount; i++)
    {
      GameObject child = rubixCube.transform.GetChild(i).gameObject;
      //Debug.Log(child + ", worldPos: " + child.position);
      cubeletIndexesToTransformMap[i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3] = child.transform;
      cubeletGameObjectToIndexesMap[child] = new Vector3Int(i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3);
    }
  }

  // Update is called once per frame
  void Update()
  {
    SetMouseState();
    UpdateText();

    switch (currentMouseState)
    {
      case MouseState.BOTH:
        ResetController();
        // This protects against the case where the user LMB drags their mouse outside the cube bounds, clicks and then unclicks RMB.
        // Maybe this can detected and handled "gracefully" in the future.
        // The LMB can only start moving the cube again once it is unclicked.
        if (priorMouseState == MouseState.LMB)
        {
          lockLMB = true;
        }
        break;
      case MouseState.RMB:
        if (priorMouseState == MouseState.LMB)
        {
          Debug.Log("WOW, Very impressive mouse work!");
          FinishRotation();
        }
        lockLMB = false;

        // Rotate whole cube with RMB
        float camX = Input.GetAxis("Mouse X");
        float camY = Input.GetAxis("Mouse Y");

        Quaternion rotation = Quaternion.Euler(camY * CAMERA_SCALE, -camX * CAMERA_SCALE, 0);
        rubixCube.transform.rotation = rotation * rubixCube.transform.rotation;
        break;
      case MouseState.LMB:
        if (lockLMB) break;

        if (!rotateLMB)
        {
          // Logic for rotating sides of the cube with LMB
          Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
          if (Physics.Raycast(ray, out RaycastHit hit, 100))
          {
            if (hit.transform.gameObject.GetComponent<SideComponent>().side != Side.INSIDE)
            {
              startingSide = hit.transform.gameObject;
              nonAxis = startingSide.GetComponent<SideComponent>().side.GetAxis();

              // Finish any current rotation
              SnapRotation();
              rotateLMB = true;
            }
          }
        }

        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        if (rotateAxis == Vector3.zero)
        {
          Vector2 mouse = new(dx, dy);
          if (mouse == Vector2.zero) break;

          Vector3 pointX_Axis = rubixCube.transform.rotation * Vector3.right;
          Vector3 pointY_Axis = rubixCube.transform.rotation * Vector3.up;
          Vector3 pointZ_Axis = rubixCube.transform.rotation * Vector3.forward;

          Vector2 pointX_Persp = new(Vector3.Dot(Vector3.up, pointX_Axis), -1 * Vector3.Dot(Vector3.right, pointX_Axis));
          Vector2 pointY_Persp = new(Vector3.Dot(Vector3.up, pointY_Axis), -1 * Vector3.Dot(Vector3.right, pointY_Axis));
          Vector2 pointZ_Persp = new(Vector3.Dot(Vector3.up, pointZ_Axis), -1 * Vector3.Dot(Vector3.right, pointZ_Axis));

          float xDot = Math.Abs(Vector2.Dot(pointX_Persp, mouse));
          float yDot = Math.Abs(Vector2.Dot(pointY_Persp, mouse));
          float zDot = Math.Abs(Vector2.Dot(pointZ_Persp, mouse));

          sorted = new SortedDictionary<double, (Vector3, Vector2)>
          {
            [xDot] = (Vector3.right, pointX_Persp),
            [yDot] = (Vector3.up, pointY_Persp),
            [zDot] = (Vector3.forward, pointZ_Persp)
          };

          foreach (var kvp in sorted)
          {
            if (kvp.Value.axis == nonAxis) continue;
            rotateAxis = kvp.Value.axis;
            sortedKey = kvp.Key;
          }

          rotateGroup = InititalizeGroups(startingSide, rotateAxis);
          totRotateAngle = 0;
        }

        rotateAngle = Vector2.Dot(sorted[sortedKey].persp, new Vector2(dx, dy)) * ROTATE_SCALE;
        totRotateAngle += rotateAngle;

        foreach (var cubelet in rotateGroup)
        {
          // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
          cubelet.transform.RotateAround(Vector3.zero, rubixCube.transform.rotation * rotateAxis, -rotateAngle);
        }
        break;
      case MouseState.NEITHER:
        FinishRotation();
        break;
      case MouseState.UNSET:
        throw new Exception("MouseState is UNSET!");
    }
  }

  private void FinishRotation()
  {
    lockLMB = false;
    rotateLMB = false;
    ResetController();
    if (remainingRotation.remainingAngle != 0)
    {
      int angleSign = Math.Sign(remainingRotation.remainingAngle);
      float anglePercent = remainingRotation.remainingAngle / remainingRotation.startingAngle;
      float percentToRot = (.01F * anglePercent) + .01F;
      float angleToRot = angleSign * Mathf.Min(percentToRot * Math.Abs(remainingRotation.startingAngle) * SNAP_SCALE, Math.Abs(remainingRotation.remainingAngle));
      remainingRotation.remainingAngle = remainingRotation.remainingAngle - angleToRot;

      foreach (var cubelet in remainingRotation.rotateGroup)
      {
        // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
        cubelet.transform.RotateAround(Vector3.zero, rubixCube.transform.rotation * remainingRotation.rotateAxis, angleToRot);
      }
    }
  }

  private void UpdateText()
  {
    liftLMB.color = lockLMB ? Color.red : Color.clear;
    return;
  }

  private void ResetController()
  {
    if (nonAxis != Vector3.zero)
    {
      float finalAngle = (float)Math.IEEERemainder(totRotateAngle, 90.0);
      if (totRotateAngle != 0)
      {
        UpdateSides(finalAngle);
      }

      remainingRotation = new RemainingRotation(rotateGroup, rotateAxis, finalAngle);

      totRotateAngle = 0;
      rotateAxis = Vector3.zero;
      nonAxis = Vector3.zero;
    }
    // Maybe empty rotateGroup? Not really necessary
    return;
  }

  private void SnapRotation()
  {
    if (remainingRotation.remainingAngle != 0)
    {
      foreach (var cubelet in remainingRotation.rotateGroup)
      {
        cubelet.transform.RotateAround(Vector3.zero, rubixCube.transform.rotation * remainingRotation.rotateAxis, remainingRotation.remainingAngle); // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
      }
      remainingRotation = new RemainingRotation();
    }
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
      foreach (var cubelet in rotateGroup)
      {
        Vector3Int coords = cubeletGameObjectToIndexesMap[cubelet.gameObject];
        if (rotateAxis == Vector3.right)
        {
          cubeletIndexesToTransformMap[coords.x, coords.z, DIM - coords.y - 1] = cubelet;
          swap = coords.y;
          coords.y = coords.z;
          coords.z = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }

        else if (rotateAxis == Vector3.up)
        {
          cubeletIndexesToTransformMap[DIM - coords.z - 1, coords.y, coords.x] = cubelet;
          swap = coords.z;
          coords.z = coords.x;
          coords.x = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
        else if (rotateAxis == Vector3.forward)
        {
          cubeletIndexesToTransformMap[coords.y, DIM - coords.x - 1, coords.z] = cubelet;
          swap = coords.x;
          coords.x = coords.y;
          coords.y = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
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

  private List<Transform> InititalizeGroups(GameObject side, Vector3 axis)
  {
    Side s = side.GetComponent<SideComponent>().side;
    Vector3Int index = cubeletGameObjectToIndexesMap[side.transform.parent.gameObject];
    List<Transform> transformGroup = new List<Transform>();
    switch (s)
    {
      case Side.FRONT:
      case Side.BACK:
        {
          if (axis == Vector3.right)
          {
            for (int j = 0; j <= 2; j++)
            {
              for (int k = 0; k <= 2; k++)
              {
                // Formerly (int)(hit.transform.parent.localPosition.x / CUBELET_GAP) + 1  // USING LOCALPOSITION MIGHT BE A PROBLEM
                transformGroup.Add(cubeletIndexesToTransformMap[index.x, j, k]);
              }
            }
          }
          else if (axis == Vector3.up)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int k = 0; k <= 2; k++)
              {
                transformGroup.Add(cubeletIndexesToTransformMap[i, index.y, k]);
              }
            }
          }
          break;
        }
      case Side.LEFT:
      case Side.RIGHT:
        {
          if (axis == Vector3.up)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int k = 0; k <= 2; k++)
              {
                transformGroup.Add(cubeletIndexesToTransformMap[i, index.y, k]);
              }
            }
          }
          else if (axis == Vector3.forward)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int j = 0; j <= 2; j++)
              {
                transformGroup.Add(cubeletIndexesToTransformMap[i, j, index.z]);
              }
            }
          }
          break;
        }
      case Side.UP:
      case Side.DOWN:
        {
          if (axis == Vector3.right)
          {
            for (int j = 0; j <= 2; j++)
            {
              for (int k = 0; k <= 2; k++)
              {
                transformGroup.Add(cubeletIndexesToTransformMap[index.x, j, k]);
              }
            }
          }
          else if (axis == Vector3.forward)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int j = 0; j <= 2; j++)
              {
                transformGroup.Add(cubeletIndexesToTransformMap[i, j, index.z]);
              }
            }
          }
          break;
        }
      case Side.INSIDE:
      default: break;
    }

    return transformGroup;
  }

  // This method detemines the priority of different mouse states:
  // BOTH > RMB > LMB > NEITHER
  private void SetMouseState()
  {
    if (currentMouseState != MouseState.UNSET) priorMouseState = currentMouseState;

    if (Input.GetMouseButton(0) && Input.GetMouseButton(1)) currentMouseState = MouseState.BOTH;
    else if (!Input.GetMouseButton(0) && Input.GetMouseButton(1)) currentMouseState = MouseState.RMB;
    else if (Input.GetMouseButton(0) && !Input.GetMouseButton(1)) currentMouseState = MouseState.LMB;
    else if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1)) currentMouseState = MouseState.NEITHER;
    else currentMouseState = MouseState.UNSET;
    return;
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