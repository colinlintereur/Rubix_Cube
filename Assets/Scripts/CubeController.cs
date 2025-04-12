using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{
  // CONSTANTS
  public float CAMERA_SPEED = 2;
  public float ROTATE_SPEED = 2;
  public float SNAP_SPEED = 10;
  public int DIM = 3;
  public int SHUFFLE_STEPS = 50;
  public float AUTO_ROTATE_SPEED = 40;
  public float AUTO_ROTATE_SPEED_SCALE = 40;

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
    SOLVE,
    NOTATION
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
  private enum NotationState
  {
    UNSET,
    PICK,
    ROTATE
  }

  // STRUCTS
  private struct RemainingRotation
  {
    public RemainingRotation(GameObject sideToRotate, List<Transform> rotateGroup, Vector3 rotateAxis, float startingAngle)
    {
      this.sideToRotate = sideToRotate;
      this.rotateGroup = rotateGroup;
      this.rotateAxis = rotateAxis;
      this.startingAngle = startingAngle;
      this.remainingAngle = startingAngle;
    }

    public GameObject sideToRotate;
    public List<Transform> rotateGroup;
    public Vector3 rotateAxis;
    public float startingAngle;
    public float remainingAngle;

    public override readonly string ToString()
    {
      return $"rotateAxis: {rotateAxis}, startingAngle: {startingAngle}, remainingAngle: {remainingAngle}";
    }
  }
  private struct Rotation
  {
    public Rotation(GameObject quad, Vector3 rotateAxis, int CWRots)
    {
      this.quad = quad;
      this.rotateAxis = rotateAxis;
      this.CWRots = CWRots;
    }

    public GameObject quad;
    public Vector3 rotateAxis;
    public int CWRots;

    public override readonly string ToString()
    {
      return $"side: {quad.GetComponent<SideComponent>().side}, rotateAxis: {rotateAxis}, CWRots: {CWRots}";
    }
  }

  // SCENE OBJECTS
  private TextMeshProUGUI liftLMBText;
  private TextMeshProUGUI invalidInputText;
  private TMP_InputField notationInputField;
  private GameObject rubixCube;
  private GameObject notationContainer;
  private GameObject startingSide;
  private Button shuffleToggleButton;
  private Button shuffleOneTimeButton;
  private Button magicSolveButton;
  private Button notationContainerButton;
  private Scrollbar autoRotateSpeedScrollbar;

  // COLLECTIONS
  private readonly Transform[,,] cubeletIndexesToTransformArray = new Transform[3, 3, 3];
  private readonly Side[,] originalChildIndexesToSideArray = new Side[27, 6]; // ONLY SET ONCE
  private List<Transform> rotateGroup;
  private List<GameObject> quadList;
  private Stack<Rotation> rotationHistory;
  private Queue<Notation> notationList;
  private SortedDictionary<double, (Vector3 axis, Vector2 persp)> sorted;
  private Dictionary<Vector3, List<Side>> vectorToSideMap;
  private Dictionary<GameObject, Vector3Int> cubeletGameObjectToIndexesMap;
  private Dictionary<Side, List<Vector3>> sideToVectorMap;
  private Dictionary<GameObject, Vector3> originalCubeletGameObjectToPositionMap; // ONLY SET ONCE
  private Dictionary<Notation, Vector3Int> notationToCubeletIndexesMap;
  private Dictionary<GameObject, Dictionary<Side, GameObject>> cubeletToSideMapMap; // A little silly. Is this the best method?

  // OTHER GLOBALS
  private MouseState currentMouseState;
  private MouseState priorMouseState;
  private SimState simState;
  private ShuffleState shuffleState;
  private SolveState solveState;
  private NotationState notationState;
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
  private bool isToggleEnabled = false;

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
    notationToCubeletIndexesMap = new()
    {
      { Notation.F, new( 1, 1, 0 ) },
      { Notation.F_, new( 1, 1, 0 ) },
      { Notation.R, new( 2, 1, 1 ) },
      { Notation.R_, new( 2, 1, 1 ) },
      { Notation.U, new( 1, 2, 1 ) },
      { Notation.U_, new( 1, 2, 1 ) },
      { Notation.L, new( 0, 1, 1 ) },
      { Notation.L_, new( 0, 1, 1 ) },
      { Notation.B, new( 1, 1, 2 ) },
      { Notation.B_, new( 1, 1, 2 ) },
      { Notation.D, new( 1, 0, 1 ) },
      { Notation.D_, new( 1, 0, 1 ) },
      { Notation.M, new( 1, 1, 0 ) },
      { Notation.M_, new( 1, 1, 0 ) },
      { Notation.E, new( 1, 1, 0 ) },
      { Notation.E_, new( 1, 1, 0 ) },
      { Notation.S, new( 1, 2, 1 ) },
      { Notation.S_, new( 1, 2, 1 ) },
      { Notation.X, new(1, 1, 1) },
      { Notation.X_, new(1, 1, 1) },
      { Notation.Y, new(1, 1, 1) },
      { Notation.Y_, new(1, 1, 1) },
      { Notation.Z, new(1, 1, 1) },
      { Notation.Z_, new(1, 1, 1) },
    };
    cubeletGameObjectToIndexesMap = new Dictionary<GameObject, Vector3Int>();
    originalCubeletGameObjectToPositionMap = new Dictionary<GameObject, Vector3>();
    cubeletToSideMapMap = new Dictionary<GameObject, Dictionary<Side, GameObject>>();
    quadList = new();
    notationList = new();
    simState = SimState.MANUAL;
    rubixCube = GameObject.Find("Rubix Cube");
    liftLMBText = GameObject.Find("LiftLMBText").GetComponent<TextMeshProUGUI>();
    invalidInputText = GameObject.Find("InvalidInputText").GetComponent<TextMeshProUGUI>();
    notationInputField = GameObject.Find("NotationInput").GetComponent<TMP_InputField>();
    notationContainer = GameObject.Find("NotationContainer");
    shuffleToggleButton = GameObject.Find("ShuffleToggleButton").GetComponent<Button>();
    shuffleOneTimeButton = GameObject.Find("ShuffleOneTimeButton").GetComponent<Button>();
    magicSolveButton = GameObject.Find("MagicSolveButton").GetComponent<Button>();
    notationContainerButton = GameObject.Find("NotationContainerButton").GetComponent<Button>();
    autoRotateSpeedScrollbar = GameObject.Find("AutoRotateSpeedScrollbar").GetComponent<Scrollbar>();

    notationContainer.SetActive(false);
    autoRotateSpeedScrollbar.value = .5F;
    cubePos = rubixCube.transform.position;
    rotationHistory = new();
    invalidInputText.text = "";

    for (int i = 0; i < rubixCube.transform.childCount; i++)
    {
      GameObject child = rubixCube.transform.GetChild(i).gameObject;
      cubeletIndexesToTransformArray[i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3] = child.transform;
      cubeletGameObjectToIndexesMap[child] = new Vector3Int(i % 3, (int)Math.Floor((i / 3.0)) % 3, (int)Math.Floor((i / 9.0)) % 3);
      originalCubeletGameObjectToPositionMap[child] = child.transform.position;
      cubeletToSideMapMap[child] = new Dictionary<Side, GameObject>();
      for (int j = 0; j < child.transform.childCount; j++)
      {
        Side quadSide = child.transform.GetChild(j).gameObject.GetComponent<SideComponent>().side;
        originalChildIndexesToSideArray[i, j] = quadSide;
        if (quadSide != Side.INSIDE)
        {
          quadList.Add(child.transform.GetChild(j).gameObject);
          cubeletToSideMapMap[child][quadSide] = child.transform.GetChild(j).gameObject;
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

          Quaternion rotation = Quaternion.Euler(camY * CAMERA_SPEED, -camX * CAMERA_SPEED, 0);
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

          Quaternion rotation = Quaternion.Euler(camY * CAMERA_SPEED, -camX * CAMERA_SPEED, 0);
          rubixCube.transform.rotation = rotation * rubixCube.transform.rotation;
        }

        HandleSolveState();
        break;

      case SimState.NOTATION:
        if (currentMouseState == MouseState.RMB)
        {
          // Rotate whole cube with RMB
          float camX = Input.GetAxis("Mouse X");
          float camY = Input.GetAxis("Mouse Y");

          Quaternion rotation = Quaternion.Euler(camY * CAMERA_SPEED, -camX * CAMERA_SPEED, 0);
          rubixCube.transform.rotation = rotation * rubixCube.transform.rotation;
        }

        HandleNotationState();
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

        Quaternion rotation = Quaternion.Euler(camY * CAMERA_SPEED, -camX * CAMERA_SPEED, 0);
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
              FinishRemainingRotation(true);
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

          // The negative is due to the reported rotateAngle being opposite the rotation angle used by the game engine to rotate objects
          rotateAngle = -1 * Vector2.Dot(sorted[sortedKey].persp, new Vector2(dx, dy)) * ROTATE_SPEED;
          totRotateAngle += rotateAngle;

          foreach (var cubelet in rotateGroup)
          {
            // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
            cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * rotateAxis, rotateAngle);
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
        float percentToRot = anglePercent + 1;
        angleToRot = angleSign * Mathf.Min(percentToRot * Math.Abs(remainingRotation.startingAngle) * SNAP_SPEED * Time.deltaTime, Math.Abs(remainingRotation.remainingAngle));
      }
      else
      {
        // remainingRotation should be some multiple of 90
        int rotates = (int)Math.Abs(Math.Round(remainingRotation.startingAngle / 90));
        angleToRot = angleSign * Math.Min(Math.Abs(remainingRotation.startingAngle) * AUTO_ROTATE_SPEED / rotates * Time.deltaTime, Math.Abs(remainingRotation.remainingAngle));
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
    liftLMBText.color = lockLMB ? Color.red : Color.clear;
    AUTO_ROTATE_SPEED = AUTO_ROTATE_SPEED_SCALE * autoRotateSpeedScrollbar.value;
    if (isToggleEnabled)
    {
      shuffleOneTimeButton.interactable = false;
      switch (simState)
      {
        case SimState.SHUFFLE:
          magicSolveButton.interactable = false;
          notationContainerButton.interactable = false;
          break;
        case SimState.SOLVE:
          shuffleToggleButton.interactable = false;
          notationContainerButton.interactable = false;
          break;
        case SimState.NOTATION:
          magicSolveButton.interactable = false;
          shuffleToggleButton.interactable = false;
          break;
      }
    }
    else
    {
      shuffleToggleButton.interactable = true;
      shuffleOneTimeButton.interactable = true;
      magicSolveButton.interactable = true;
      notationContainerButton.interactable = true;
    }
    if (notationContainer.activeSelf)
    {
      notationContainerButton.GetComponentInChildren<TextMeshProUGUI>().text = "\\/";
    }
    else
    {
      notationContainerButton.GetComponentInChildren<TextMeshProUGUI>().text = "/\\";
    }
    return;
  }

  private void ResetController()
  {
    if (nonAxis != Vector3.zero)
    {
      float finalAngle = (float)Math.IEEERemainder(-1 * totRotateAngle, 90.0);
      if (totRotateAngle != 0)
      {
        int rotates = CWRotations(totRotateAngle + finalAngle);
        UpdateSides(startingSide, rotateGroup, rotateAxis, rotates, true);
      }

      remainingRotation = new RemainingRotation(startingSide, rotateGroup, rotateAxis, finalAngle);

      totRotateAngle = 0;
      rotateAxis = Vector3.zero;
      nonAxis = Vector3.zero;
    }
    // Maybe empty rotateGroup? Not really necessary
    return;
  }

  private void FinishRemainingRotation(bool storeHistory)
  {
    if (remainingRotation.remainingAngle != 0)
    {
      foreach (var cubelet in remainingRotation.rotateGroup)
      {
        // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
        cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * remainingRotation.rotateAxis, remainingRotation.remainingAngle);
      }

      int rotates = CWRotations(remainingRotation.startingAngle);
      UpdateSides(remainingRotation.sideToRotate, remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, storeHistory);
    }
    return;
  }

  private void UpdateSides(GameObject sideToRotate, List<Transform> rotGroup, Vector3 rotAxis, int rotates, bool addToHistory)
  {
    remainingRotation = new RemainingRotation();
    if (rotates == 0) return;
    if (addToHistory)
    {
      rotationHistory.Push(new Rotation(sideToRotate, rotAxis, rotates));
    }
    List<Side> sidesList = vectorToSideMap[rotAxis];
    int swap;

    if (rotates == 2 || rotates == -2)
    {
      foreach (var cubelet in rotGroup)
      {
        Vector3Int coords = cubeletGameObjectToIndexesMap[cubelet.gameObject];
        if (rotAxis == Vector3.right)
        {
          cubeletIndexesToTransformArray[coords.x, DIM - coords.y - 1, DIM - coords.z - 1] = cubelet;
          coords.y = DIM - coords.y - 1;
          coords.z = DIM - coords.z - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }

        else if (rotAxis == Vector3.up)
        {
          cubeletIndexesToTransformArray[DIM - coords.x - 1, coords.y, DIM - coords.z - 1] = cubelet;
          coords.z = DIM - coords.z - 1;
          coords.x = DIM - coords.x - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
        else if (rotAxis == Vector3.forward)
        {
          cubeletIndexesToTransformArray[DIM - coords.x - 1, DIM - coords.y - 1, coords.z] = cubelet;
          coords.x = DIM - coords.x - 1;
          coords.y = DIM - coords.y - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
      }
    }
    else if (rotates == 1)
    {
      foreach (var cubelet in rotGroup)
      {
        Vector3Int coords = cubeletGameObjectToIndexesMap[cubelet.gameObject];
        if (rotAxis == Vector3.right)
        {
          cubeletIndexesToTransformArray[coords.x, DIM - coords.z - 1, coords.y] = cubelet;
          swap = coords.z;
          coords.z = coords.y;
          coords.y = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
        else if (rotAxis == Vector3.up)
        {
          cubeletIndexesToTransformArray[coords.z, coords.y, DIM - coords.x - 1] = cubelet;
          swap = coords.x;
          coords.x = coords.z;
          coords.z = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
        else if (rotAxis == Vector3.forward)
        {
          cubeletIndexesToTransformArray[DIM - coords.y - 1, coords.x, coords.z] = cubelet;
          swap = coords.y;
          coords.y = coords.x;
          coords.x = DIM - swap - 1;
          cubeletGameObjectToIndexesMap[cubelet.gameObject] = coords;
        }
      }
    }
    else if (rotates == -1)
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
      cubeletToSideMapMap[cubelet.gameObject].Clear();
      for (int i = 0; i < cubelet.transform.childCount; i++)
      {
        Side side = cubelet.transform.GetChild(i).GetComponent<SideComponent>().side;
        int index = sidesList.IndexOf(side);

        if (side == Side.INSIDE)
        {
          continue;
        }
        else if (index == -1)
        {
          cubeletToSideMapMap[cubelet.gameObject][side] = cubelet.transform.GetChild(i).gameObject;
        }
        else
        {
          side = sidesList[(index - rotates + 4) % 4];
          cubelet.transform.GetChild(i).GetComponent<SideComponent>().side = side;

          cubeletToSideMapMap[cubelet.gameObject][side] = cubelet.transform.GetChild(i).gameObject;
        }
      }
    }
  }

  private List<Transform> InititalizeGroups(GameObject quad, Vector3 axis)
  {
    Side s = quad.GetComponent<SideComponent>().side;
    Vector3Int index = cubeletGameObjectToIndexesMap[quad.transform.parent.gameObject];
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
          else if (axis == Vector3.forward) // NON_AXIS
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
      case Side.LEFT:
      case Side.RIGHT:
        {
          if (axis == Vector3.right) // NON_AXIS
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
          else if (axis == Vector3.up) // NON_AXIS
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
      // This is the case for whole-cube rotations
      case Side.INSIDE:
        {
          for (int i = 0; i <= 2; i++)
          {
            for (int j = 0; j <= 2; j++)
            {
              for (int k = 0; k <= 2; k++)
              {
                transformGroup.Add(cubeletIndexesToTransformArray[i, j, k]);
              }
            }
          }
          break;
        }
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
        int randRotateNum = UnityEngine.Random.Range(-2, 3);

        GameObject sideToRotate = quadList[randSideIndex];
        if (sideToRotate == null) Debug.Log("sideToRotate is null!");
        Vector3 rotAxis = sideToVectorMap[sideToRotate.GetComponent<SideComponent>().side][randRotateAxis];
        List<Transform> rotGroup = InititalizeGroups(sideToRotate, rotAxis);

        remainingRotation = new RemainingRotation(sideToRotate, rotGroup, rotAxis, 90 * randRotateNum);
        shuffleState = ShuffleState.ROTATE;
        break;
      case ShuffleState.ROTATE:
        ContinueRemainingRotation();
        if (remainingRotation.remainingAngle == 0)
        {
          int rotates = CWRotations(remainingRotation.startingAngle);
          UpdateSides(remainingRotation.sideToRotate, remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, true);
          shuffleState = ShuffleState.PICK;
        }
        break;
      case ShuffleState.UNSET:
        break;
    }
  }

  private void HandleSolveState()
  {
    switch (solveState)
    {
      case SolveState.PICK:
        if (rotationHistory.Count == 0)
        {
          simState = SimState.MANUAL;
          solveState = SolveState.UNSET;
          isToggleEnabled = false;
          break;
        }
        Rotation rotation = rotationHistory.Pop();

        List<Transform> rotGroup = InititalizeGroups(rotation.quad, rotation.rotateAxis);
        remainingRotation = new RemainingRotation(rotation.quad, rotGroup, rotation.rotateAxis, -90 * rotation.CWRots);
        solveState = SolveState.ROTATE;
        break;
      case SolveState.ROTATE:
        ContinueRemainingRotation();
        if (remainingRotation.remainingAngle == 0)
        {
          int rotates = CWRotations(remainingRotation.startingAngle);
          UpdateSides(remainingRotation.sideToRotate, remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, false);
          solveState = SolveState.PICK;
        }
        break;
      case SolveState.UNSET:
        break;
    }
  }

  private void HandleNotationState()
  {
    switch (notationState)
    {
      case NotationState.PICK:
        if (notationList.Count == 0)
        {
          simState = SimState.NOTATION;
          notationState = NotationState.UNSET;
          break;
        }
        Notation notation = notationList.Dequeue();

        NotationToRotation(notation);
        notationState = NotationState.ROTATE;
        break;
      case NotationState.ROTATE:
        ContinueRemainingRotation();
        if (remainingRotation.remainingAngle == 0)
        {
          int rotates = CWRotations(remainingRotation.startingAngle);
          UpdateSides(remainingRotation.sideToRotate, remainingRotation.rotateGroup, remainingRotation.rotateAxis, rotates, true);
          notationState = NotationState.PICK;
        }
        break;
      case NotationState.UNSET:
        break;
    }
  }

  // Called by the ShuffleToggleButton.onClick()
  public void ShuffleCubeToggle()
  {
    if (!canClickButtons) return;
    isToggleEnabled = (isToggleEnabled == false);
    canClickButtons = false;
    shuffleToggleButton.interactable = false;
    simState = (simState == SimState.SHUFFLE) ? SimState.MANUAL : SimState.SHUFFLE;
    shuffleState = (shuffleState == ShuffleState.UNSET) ? ShuffleState.PICK : ShuffleState.UNSET;
    FinishRemainingRotation(true);
    shuffleToggleButton.interactable = true;
    canClickButtons = true;
  }

  // Called by the ShuffleOneTimeButton.onClick()
  public void ShuffleCubeOneTime()
  {
    if (!canClickButtons) return;
    canClickButtons = false;
    simState = SimState.SHUFFLE;
    FinishRemainingRotation(true);
    for (int i = 0; i < SHUFFLE_STEPS; i++)
    {
      int randSideIndex = UnityEngine.Random.Range(0, quadList.Count);
      int randRotateAxis = UnityEngine.Random.Range(0, 2);
      int randRotateNum = UnityEngine.Random.Range(-2, 3);

      GameObject sideToRotate = quadList[randSideIndex];
      if (sideToRotate == null) Debug.Log("sideToRotate is null!");
      Vector3 rotAxis = sideToVectorMap[sideToRotate.GetComponent<SideComponent>().side][randRotateAxis];

      RotateGroup(sideToRotate, rotAxis, randRotateNum);
    }
    simState = SimState.MANUAL;
    canClickButtons = true;
  }

  // Called by the ResetSimButton.onClick()
  public void ResetCube()
  {
    canClickButtons = false;
    isToggleEnabled = false;
    FinishRemainingRotation(false);
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
    notationContainer.SetActive(false);
    canClickButtons = true;
  }

  // Called by the MagicSolveButton.onClick()
  public void MagicSolveToggle()
  {
    if (!canClickButtons) return;
    isToggleEnabled = (isToggleEnabled == false);
    canClickButtons = false;
    magicSolveButton.interactable = false;
    simState = (simState == SimState.SOLVE) ? SimState.MANUAL : SimState.SOLVE;
    solveState = (solveState == SolveState.UNSET) ? SolveState.PICK : SolveState.UNSET;
    FinishRemainingRotation(false);
    magicSolveButton.interactable = true;
    canClickButtons = true;
  }

  // Called by the NotationContainerButton.onClick()
  public void NotationContainerButton()
  {
    notationContainerButton.interactable = false;
    isToggleEnabled = (isToggleEnabled == false);
    simState = (simState == SimState.NOTATION) ? SimState.MANUAL : SimState.NOTATION;
    FinishRemainingRotation(true);
    notationContainer.SetActive(notationContainer.activeSelf == false);
    notationContainerButton.interactable = true;
    invalidInputText.text = "";
  }

  public void NotationRotate(Notation notation)
  {
    notationList.Enqueue(notation);
    simState = SimState.NOTATION;
    if (notationState != NotationState.ROTATE)
    {
      notationState = NotationState.PICK;
    }
  }

  private void NotationToRotation(Notation notation)
  {
    FinishRemainingRotation(true);
    Vector3Int cubeletIndex = notationToCubeletIndexesMap[notation];
    GameObject cubelet = cubeletIndexesToTransformArray[cubeletIndex.x, cubeletIndex.y, cubeletIndex.z].gameObject;
    GameObject quad;
    if (notation.IsCubeRotation())
    {
      quad = cubelet.transform.GetChild(0).gameObject;
    }
    else
    {
      quad = cubeletToSideMapMap[cubelet][notation.GetSide()];
    }
    List<Transform> rotGroup = InititalizeGroups(quad, notation.GetRotateAxis());
    remainingRotation = new RemainingRotation(quad, rotGroup, notation.GetRotateAxis(), 90 * notation.GetRotationSign());
  }

  // Called by the SubmitInputButton.onClick()
  public void SubmitInputButton()
  {
    invalidInputText.text = "";
    String rotationInput = notationInputField.text;
    if (rotationInput == null || rotationInput == "") return;
    notationInputField.SetTextWithoutNotify("");

    char delimiter = ' ';
    if (rotationInput.Contains(','))
    {
      delimiter = ',';
    }

    String[] rotations = rotationInput.Split(delimiter);
    foreach (String rotation in rotations)
    {
      String trimmedRotation = rotation.Trim();
      Notation notation = Notations.ConvertString(trimmedRotation);
      if (notation == Notation.UNSET)
      {
        invalidInputText.text = $"Invalid input: '{trimmedRotation}'";
        notationList.Clear();
        return;
      }
      notationList.Enqueue(notation);
    }

    simState = SimState.NOTATION;
    if (notationState != NotationState.ROTATE)
    {
      notationState = NotationState.PICK;
    }
  }

  public void ClearInvalidInputText()
  {
    invalidInputText.text = "";
  }

  private void RotateGroup(GameObject sideToRotate, Vector3 rotAxis, int numRotates)
  {
    List<Transform> rotGroup = InititalizeGroups(sideToRotate, rotAxis);
    foreach (var cubelet in rotGroup)
    {
      // rotateAxis needs to be rotated to match cube rotation (this function rotates on world axis)
      cubelet.transform.RotateAround(cubePos, rubixCube.transform.rotation * rotAxis, 90 * numRotates);
    }
    UpdateSides(sideToRotate, rotGroup, rotAxis, numRotates, true);
  }

  // +rotation is CCW, -rotation is CW (rotation axis goes into screen)
  // angleRotated must be a multiple 90
  // 
  private int CWRotations(float angleRotated)
  {
    return (int)Math.IEEERemainder((int)angleRotated / 90, 4.0);
  }
}

// +rotation is CCW (rotation axis goes into screen)
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