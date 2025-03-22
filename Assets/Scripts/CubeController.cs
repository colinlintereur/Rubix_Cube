using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{
  // CONSTANTS
  public float CAMERA_SCALE = 2;
  public float ROTATE_SCALE = 2;
  public float SNAP_SCALE = 1;
  public int DIM = 3;
  public int SHUFFLE_STEPS = 50;
  public float AUTO_ROTATE_SPEED = 1;

  // ENUMS
  private enum MouseState
  {
    UNSET,
    LMB,
    RMB,
    BOTH,
    NEITHER
  }
  private enum SimState
  {
    UNSET,
    MANUAL,
    SHUFFLE,
    SOLVE
  }
  private enum ShuffleState
  {
    UNSET,
    PICK,
    ROTATE
  }
  private enum SolveState
  {
    UNSET,
    PICK,
    ROTATE
  }

  // STRUCTS
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

    public override string ToString()
    {
      return $"rotateAxis: {rotateAxis}, startingAngle: {startingAngle}, remainingAngle: {remainingAngle}";
    }
  }
  private struct Rotation
  {
    public Rotation(GameObject side, Vector3 rotateAxis, int CWRots)
    {
      this.side = side;
      this.rotateAxis = rotateAxis;
      this.CWRots = CWRots;
    }

    public GameObject side;
    public Vector3 rotateAxis;
    public int CWRots;

    public override string ToString()
    {
      return $"rotateAxis: {rotateAxis}, CWRots: {CWRots}";
    }
  }

  // SCENE OBJECTS
  private GameObject rubixCube;
  private TextMeshProUGUI liftLMB;
  private Button shuffleToggleButton;
  private Button magicSolveButton;
  private Scrollbar autoRotateSpeedScrollbar;

  // COLLECTIONS
  private readonly Transform[,,] cubeletIndexesToTransformArray = new Transform[3, 3, 3];
  private readonly Side[,] originalChildIndexesToSideArray = new Side[27, 6]; // ONLY SET ONCE
  private List<Transform> rotateGroup;
  private List<GameObject> quadList;
  private List<Rotation> rotationHistory;
  private SortedDictionary<double, (Vector3 axis, Vector2 persp)> sorted;
  private Dictionary<Vector3, List<Side>> vectorToSideMap;
  private Dictionary<GameObject, Vector3Int> cubeletGameObjectToIndexesMap;
  private Dictionary<Side, List<Vector3>> sideToVectorMap;
  private Dictionary<GameObject, Vector3> originalCubeletGameObjectToPositionMap; // ONLY SET ONCE

  // OTHER GLOBALS
  private MouseState currentMouseState;
  private MouseState priorMouseState;
  private SimState simState;
  private ShuffleState shuffleState;
  private SolveState solveState;
  private RemainingRotation remainingRotation;
  private Vector3 rotateAxis = Vector3.zero;
  private Vector3 nonAxis = Vector3.zero;
  private Vector3 cubePos = Vector3.zero;
  private double sortedKey;
  private float rotateAngle = 0;
  private float totRotateAngle = 0;
  private bool lockLMB = false;
  private bool rotateLMB = false;
  private bool canClickButtons = false;
  private GameObject startingSide;

  // Start is called before the first frame update
  void Start()
  {
    vectorToSideMap = new()
    {
      { Vector3.right, new() { Side.FRONT, Side.DOWN, Side.BACK, Side.UP } },
      { Vector3.up, new() { Side.FRONT, Side.RIGHT, Side.BACK, Side.LEFT } },
      { Vector3.forward, new() { Side.RIGHT, Side.DOWN, Side.LEFT, Side.UP } }
    };
    sideToVectorMap = new()
    {
      [Side.FRONT] = new() { Vector3.right, Vector3.up },
      [Side.BACK] = new() { Vector3.right, Vector3.up },
      [Side.LEFT] = new() { Vector3.up, Vector3.forward },
      [Side.RIGHT] = new() { Vector3.up, Vector3.forward },
      [Side.UP] = new() { Vector3.forward, Vector3.right },
      [Side.DOWN] = new() { Vector3.forward, Vector3.right },
    };
    cubeletGameObjectToIndexesMap = new Dictionary<GameObject, Vector3Int>();
    originalCubeletGameObjectToPositionMap = new Dictionary<GameObject, Vector3>();
    quadList = new();
    simState = SimState.MANUAL;
    rubixCube = GameObject.Find("Rubix Cube");
    liftLMB = GameObject.Find("LiftLMB").GetComponent<TextMeshProUGUI>();
    shuffleToggleButton = GameObject.Find("ShuffleToggleButton").GetComponent<Button>();
    magicSolveButton = GameObject.Find("MagicSolveButton").GetComponent<Button>();
    autoRotateSpeedScrollbar = GameObject.Find("AutoRotateSpeedScrollbar").GetComponent<Scrollbar>();
    autoRotateSpeedScrollbar.value = AUTO_ROTATE_SPEED;
    cubePos = rubixCube.transform.position;
    rotationHistory = new();

    for (int i = 0; i < rubixCube.transform.childCount; i++)
    {
      GameObject child = rubixCube.transform.GetChild(i).gameObject;
      cubeletIndexesToTransformArray[i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3] = child.transform;
      cubeletGameObjectToIndexesMap[child] = new Vector3Int(i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3);
      originalCubeletGameObjectToPositionMap[child] = child.transform.position;
      for (int j = 0; j < child.transform.childCount; j++)
      {
        Side quadSide = child.transform.GetChild(j).gameObject.GetComponent<SideComponent>().side;
        originalChildIndexesToSideArray[i, j] = quadSide;
        if (quadSide != Side.INSIDE)
        {
          quadList.Add(child.transform.GetChild(j).gameObject);
        }
      }
    }
    canClickButtons = true;
  }

  // Update is called once per frame
  void Update()
  {
    SetMouseState();
    GameObjectUpdates();
    switch (simState)
    {
      case SimState.MANUAL:
        HandleManualState();
        break;

      case SimState.SHUFFLE:
        if (currentMouseState == MouseState.RMB)
        {
          // Rotate whole cube with RMB
          float camX = Input.GetAxis("Mouse X");
          float camY = Input.GetAxis("Mouse Y");

          Quaternion rotation = Quaternion.Euler(camY * CAMERA_SCALE, -camX * CAMERA_SCALE, 0);
          rubixCube.transform.rotation = rotation * rubixCube.transform.rotation;
        }

        HandleShuffleState();
        break;

      case SimState.SOLVE:
        if (currentMouseState == MouseState.RMB)
        {
          // Rotate whole cube with RMB
          float camX = Input.GetAxis("Mouse X");
          float camY = Input.GetAxis("Mouse Y");

          Quaternion rotation = Quaternion.Euler(camY * CAMERA_SCALE, -camX * CAMERA_SCALE, 0);
          rubixCube.transform.rotation = rotation * rubixCube.transform.rotation;
        }

        HandleSolveState();
        break;

      default:
        break;
    }
  }

  private void HandleManualState()
  {
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
        }
        ContinueRemainingRotation();
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
          if (Physics.Raycast(ray, out RaycastHit hit, 50))
          {
            if (hit.transform.gameObject == null)
            {
              break;
            }
            else if (hit.transform.gameObject.GetComponent<SideComponent>().side != Side.INSIDE)
            {
              startingSide = hit.transform.gameObject;
              nonAxis = startingSide.GetComponent<SideComponent>().side.GetNonAxis();

              // Finish any current rotation
              FinishRemainingRotation();
              rotateLMB = true;
            }
          }
        }

        if (rotateLMB)
        {
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
            cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * rotateAxis, -rotateAngle);
          }
        }
        break;
      case MouseState.NEITHER:
        ContinueRemainingRotation();
        break;
      case MouseState.UNSET:
        throw new Exception("MouseState is UNSET!");
    }
  }

  private void ContinueRemainingRotation()
  {
    lockLMB = false;
    rotateLMB = false;
    ResetController();
    if (remainingRotation.remainingAngle != 0)
    {
      int angleSign = Math.Sign(remainingRotation.remainingAngle);
      float anglePercent = remainingRotation.remainingAngle / remainingRotation.startingAngle;
      float angleToRot;
      if (simState == SimState.MANUAL)
      {
        float percentToRot = (.01F * anglePercent) + .01F;
        angleToRot = angleSign * Mathf.Min(percentToRot * Math.Abs(remainingRotation.startingAngle) * SNAP_SCALE, Math.Abs(remainingRotation.remainingAngle));
        //Debug.Log($"MANUAL::ContinueRemainingRotation(): angleToRot: {angleToRot}, startingAngle: {remainingRotation.startingAngle}");
      }
      else
      {
        // remainingRotation should be some multiple of 90
        int rotates = (int)Math.Abs(Math.Round(remainingRotation.startingAngle / 90));
        angleToRot = angleSign * Math.Min(Math.Abs(remainingRotation.startingAngle) * AUTO_ROTATE_SPEED * .02F / rotates, Math.Abs(remainingRotation.remainingAngle));
        //Debug.Log($"AUTO::ContinueRemainingRotation(): angleToRot: {angleToRot}, rotates: {rotates}, remainingRotation: {remainingRotation}");
      }
      remainingRotation.remainingAngle -= angleToRot;

      foreach (var cubelet in remainingRotation.rotateGroup)
      {
        // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
        cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * remainingRotation.rotateAxis, angleToRot);
      }
    }
  }

  private void GameObjectUpdates()
  {
    liftLMB.color = lockLMB ? Color.red : Color.clear;
    AUTO_ROTATE_SPEED = autoRotateSpeedScrollbar.value;
    return;
  }

  private void ResetController()
  {
    if (nonAxis != Vector3.zero)
    {
      float finalAngle = (float)Math.IEEERemainder(totRotateAngle, 90.0);
      if (totRotateAngle != 0)
      {
        int rotates = CWRotations(totRotateAngle - finalAngle);
        UpdateSides(rotateGroup, rotateAxis, rotates, true);
      }

      remainingRotation = new RemainingRotation(rotateGroup, rotateAxis, finalAngle);

      totRotateAngle = 0;
      rotateAxis = Vector3.zero;
      nonAxis = Vector3.zero;
    }
    // Maybe empty rotateGroup? Not really necessary
    return;
  }

  private void FinishRemainingRotation()
  {
    if (remainingRotation.remainingAngle != 0)
    {
      foreach (var cubelet in remainingRotation.rotateGroup)
      {
        // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
        cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * remainingRotation.rotateAxis, remainingRotation.remainingAngle);
      }

      int rotates = CWRotations(-remainingRotation.startingAngle);
      UpdateSides(remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, true);
      remainingRotation = new RemainingRotation();
    }
    return;
  }

  private void UpdateSides(List<Transform> rotGroup, Vector3 rotAxis, int rotates, bool addToHistory)
  {
    if ((rotates != 0) && addToHistory)
    {
      rotationHistory.Add(new Rotation(startingSide, rotAxis, rotates));
    }
    List<Side> sidesList = vectorToSideMap[rotAxis];
    int swap;

    for (int index = 0; index < rotates; index++)
    {
      foreach (var cubelet in rotGroup)
      {
        Vector3Int coords = cubeletGameObjectToIndexesMap[cubelet.gameObject];
        if (rotAxis == Vector3.right)
        {
          cubeletIndexesToTransformArray[coords.x, coords.z, DIM - coords.y - 1] = cubelet;
          swap = coords.y;
          coords.y = coords.z;
          coords.z = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }

        else if (rotAxis == Vector3.up)
        {
          cubeletIndexesToTransformArray[DIM - coords.z - 1, coords.y, coords.x] = cubelet;
          swap = coords.z;
          coords.z = coords.x;
          coords.x = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
        else if (rotAxis == Vector3.forward)
        {
          cubeletIndexesToTransformArray[coords.y, DIM - coords.x - 1, coords.z] = cubelet;
          swap = coords.x;
          coords.x = coords.y;
          coords.y = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
      }
    }

    foreach (var cubelet in rotGroup)
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
    List<Transform> transformGroup = new();
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
                transformGroup.Add(cubeletIndexesToTransformArray[index.x, j, k]);
              }
            }
          }
          else if (axis == Vector3.up)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int k = 0; k <= 2; k++)
              {
                transformGroup.Add(cubeletIndexesToTransformArray[i, index.y, k]);
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
                transformGroup.Add(cubeletIndexesToTransformArray[i, index.y, k]);
              }
            }
          }
          else if (axis == Vector3.forward)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int j = 0; j <= 2; j++)
              {
                transformGroup.Add(cubeletIndexesToTransformArray[i, j, index.z]);
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
                transformGroup.Add(cubeletIndexesToTransformArray[index.x, j, k]);
              }
            }
          }
          else if (axis == Vector3.forward)
          {
            for (int i = 0; i <= 2; i++)
            {
              for (int j = 0; j <= 2; j++)
              {
                transformGroup.Add(cubeletIndexesToTransformArray[i, j, index.z]);
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

  private void HandleShuffleState()
  {
    switch (shuffleState)
    {
      case ShuffleState.PICK:
        int randSideIndex = UnityEngine.Random.Range(0, quadList.Count);
        int randRotateAxis = UnityEngine.Random.Range(0, 2);
        int randRotateNum = UnityEngine.Random.Range(1, 4);

        GameObject sideToRotate = quadList[randSideIndex];
        if (sideToRotate == null) Debug.Log("sideToRotate is null!");
        Vector3 rotAxis = sideToVectorMap[sideToRotate.GetComponent<SideComponent>().side][randRotateAxis];
        List<Transform> rotGroup = InititalizeGroups(sideToRotate, rotAxis);

        remainingRotation = new RemainingRotation(rotGroup, rotAxis, -90 * randRotateNum);
        shuffleState = ShuffleState.ROTATE;
        break;
      case ShuffleState.ROTATE:
        ContinueRemainingRotation();
        if (remainingRotation.remainingAngle == 0)
        {
          int rotates = CWRotations(-remainingRotation.startingAngle);
          UpdateSides(remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, true);
          shuffleState = ShuffleState.PICK;
        }
        break;
      case ShuffleState.UNSET:
        break;
    }
  }

  private void HandleSolveState()
  {
    //Debug.Log($"HandleSolveState(): {rotationHistory.Count}");
    switch (solveState)
    {
      case SolveState.PICK:
        if (rotationHistory.Count == 0)
        {
          simState = SimState.MANUAL;
          solveState = SolveState.UNSET;
          break;
        }
        Rotation rotation = rotationHistory[rotationHistory.Count - 1];
        rotationHistory.RemoveAt(rotationHistory.Count - 1);
        //Debug.Log($"HandleSolveState(): {rotation}");

        List<Transform> rotGroup = InititalizeGroups(rotation.side, rotation.rotateAxis);
        remainingRotation = new RemainingRotation(rotGroup, rotation.rotateAxis, 90 * rotation.CWRots);
        solveState = SolveState.ROTATE;
        break;
      case SolveState.ROTATE:
        ContinueRemainingRotation();
        if (remainingRotation.remainingAngle == 0)
        {
          int rotates = CWRotations(-remainingRotation.startingAngle);
          //Debug.Log($"HandleSolveState(): rotates: {rotates}, startingAngle: {remainingRotation.startingAngle}");
          UpdateSides(remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, false);
          solveState = SolveState.PICK;
        }
        break;
      case SolveState.UNSET:
        break;
    }
  }

  // Called by the ShuffleToggleButton.onClick()
  public void ShuffleCubeToggle()
  {
    if (!canClickButtons) return;
    canClickButtons = false;
    shuffleToggleButton.interactable = false;
    simState = (simState == SimState.SHUFFLE) ? SimState.MANUAL : SimState.SHUFFLE;
    shuffleState = (shuffleState == ShuffleState.UNSET) ? ShuffleState.PICK : ShuffleState.UNSET;
    FinishRemainingRotation();
    shuffleToggleButton.interactable = true;
    canClickButtons = true;
  }

  // Called by the ShuffleOneTimeButton.onClick()
  public void ShuffleCubeOneTime()
  {
    if (!canClickButtons) return;
    canClickButtons = false;
    simState = SimState.SHUFFLE;
    FinishRemainingRotation();
    List<Transform> rotGroup;
    for (int i = 0; i < SHUFFLE_STEPS; i++)
    {
      int randSideIndex = UnityEngine.Random.Range(0, quadList.Count);
      int randRotateAxis = UnityEngine.Random.Range(0, 2);
      int randRotateNum = UnityEngine.Random.Range(1, 4);

      GameObject sideToRotate = quadList[randSideIndex];
      if (sideToRotate == null) Debug.Log("sideToRotate is null!");
      // This is needed for the magic solve. This feels wrong and should be fixed.
      startingSide = sideToRotate;
      Vector3 rotAxis = sideToVectorMap[sideToRotate.GetComponent<SideComponent>().side][randRotateAxis];

      rotGroup = InititalizeGroups(sideToRotate, rotAxis);
      RotateGroup(rotGroup, rotAxis, randRotateNum);
    }
    simState = SimState.MANUAL;
    canClickButtons = true;
  }

  // Called by the ResetSimButton.onClick()
  public void ResetCube()
  {
    canClickButtons = false;
    FinishRemainingRotation();
    simState = SimState.MANUAL;
    shuffleState = ShuffleState.UNSET;
    solveState = SolveState.UNSET;
    rotationHistory.Clear();

    rubixCube.transform.rotation = Quaternion.identity;
    for (int i = 0; i < rubixCube.transform.childCount; i++)
    {
      GameObject child = rubixCube.transform.GetChild(i).gameObject;

      child.transform.SetPositionAndRotation(originalCubeletGameObjectToPositionMap[child], Quaternion.identity);
      cubeletIndexesToTransformArray[i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3] = child.transform;
      cubeletGameObjectToIndexesMap[child] = new Vector3Int(i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3);

      for (int j = 0; j < child.transform.childCount; j++)
      {
        child.transform.GetChild(j).GetComponent<SideComponent>().side = originalChildIndexesToSideArray[i, j];
      }
    }
    canClickButtons = true;
  }

  // Called by the MagicSolveButton.onClick()
  public void MagicSolveToggle()
  {
    if (!canClickButtons) return;
    canClickButtons = false;
    magicSolveButton.interactable = false;
    simState = (simState == SimState.SOLVE) ? SimState.MANUAL : SimState.SOLVE;
    solveState = (solveState == SolveState.UNSET) ? SolveState.PICK : SolveState.UNSET;
    //Debug.Log($"MagicSolveToggle(): simState: {simState}, solveState: {solveState}");
    FinishRemainingRotation();
    magicSolveButton.interactable = true;
    canClickButtons = true;
  }

  private void RotateGroup(List<Transform> rotateGroup, Vector3 rotAxis, int numRotates)
  {
    foreach (var cubelet in rotateGroup)
    {
      // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
      cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * rotAxis, -90 * numRotates);
    }
    UpdateSides(rotateGroup, rotAxis, numRotates, true);
  }

  // rotation is CW (rotation axis goes into screen)
  private int CWRotations(float angleRotated)
  {
    return ((int)Math.IEEERemainder((int)angleRotated / 90, 4.0) + 4) % 4;
  }
}

// rotation is CW (rotation axis goes into screen)
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